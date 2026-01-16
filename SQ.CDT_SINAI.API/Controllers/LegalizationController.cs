using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQ.CDT_SINAI.API.Data;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Claims;

namespace SQ.CDT_SINAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LegalizationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public LegalizationController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // POST: api/legalization/save
        [HttpPost("save")]
        public async Task<IActionResult> SaveDocument([FromForm] UpdateDocumentStatusDto dto, IFormFile? file)
        {
            // Validação de Acesso
            if (!await UserHasAccessToEstablishment(dto.EstablishmentId))
                return Forbid();

            if (dto.Cost.HasValue && dto.Cost.Value < 0)
                return BadRequest("O custo não pode ser negativo.");

            // 1. Busca documento existente ou cria um novo
            var document = await _context.EstablishmentDocuments
                .FirstOrDefaultAsync(d => d.EstablishmentId == dto.EstablishmentId && d.DocumentTypeId == dto.DocumentTypeId);

            if (document == null)
            {
                document = new EstablishmentDocument
                {
                    EstablishmentId = dto.EstablishmentId,
                    DocumentTypeId = dto.DocumentTypeId
                };
                _context.EstablishmentDocuments.Add(document);
            }

            // 2. Atualiza os dados
            document.Status = dto.Status;
            document.EmissionDate = dto.EmissionDate;
            document.ExpirationDate = dto.ExpirationDate;
            document.ProtocolNumber = dto.ProtocolNumber;
            document.Cost = dto.Cost;
            document.Justification = dto.Justification;
            document.AutomaticRenewal = dto.AutomaticRenewal;
            document.RenewalMonths = dto.RenewalMonths;
            document.LastUpdated = DateTime.Now;

            // 3. Processa o Upload do Arquivo (se enviado)
            if (file != null && file.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".jpg", ".jpeg", ".png", ".txt" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest("Tipo de arquivo não permitido.");

                // Define o caminho de salvamento: API/Uploads/Legalization
                var uploadFolder = Path.Combine(_environment.ContentRootPath, "Uploads", "Legalization");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                // Gera nome único para evitar conflitos
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                // Salva no disco
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Atualiza referência no banco
                document.FilePath = uniqueFileName;
                document.FileName = file.FileName;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Documento salvo com sucesso.", documentId = document.Id });
        }

        // GET: api/legalization/establishment/{id}
        [HttpGet("establishment/{establishmentId}")]
        public async Task<ActionResult<List<EstablishmentDocument>>> GetDocumentsByEstablishment(int establishmentId)
        {
            // Validação de Acesso
            if (!await UserHasAccessToEstablishment(establishmentId))
                return Forbid();

            var documents = await _context.EstablishmentDocuments
                .Include(d => d.DocumentType)
                .Where(d => d.EstablishmentId == establishmentId)
                .ToListAsync();

            return documents;
        }

        // GET: api/legalization/download/{id}
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var docCheck = await _context.EstablishmentDocuments.FindAsync(id);
            if (docCheck != null && !await UserHasAccessToEstablishment(docCheck.EstablishmentId))
                return Forbid();

            var document = await _context.EstablishmentDocuments.FindAsync(id);
            if (document == null || string.IsNullOrEmpty(document.FilePath))
                return NotFound("Documento ou arquivo não encontrado.");

            var filePath = Path.Combine(_environment.ContentRootPath, "Uploads", "Legalization", document.FilePath);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Arquivo físico não encontrado no servidor.");

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var contentType = "application/octet-stream"; // Genérico para download
            
            return File(bytes, contentType, document.FileName ?? "documento");
        }

        // GET: api/legalization/types
        [HttpGet("types")]
        public async Task<ActionResult<List<DocumentType>>> GetDocumentTypes()
        {
            return await _context.DocumentTypes.Where(t => t.Active).OrderBy(t => t.Name).ToListAsync();
        }

        // GET: api/legalization/report/{establishmentId}
        [HttpGet("report/{establishmentId}")]
        public async Task<IActionResult> GetReport(int establishmentId)
        {
            // Validação de Acesso
            if (!await UserHasAccessToEstablishment(establishmentId))
                return Forbid();

            var establishment = await _context.Establishments.FindAsync(establishmentId);
            if (establishment == null) return NotFound("Estabelecimento não encontrado.");

            var documents = await _context.EstablishmentDocuments
                .Include(d => d.DocumentType)
                .Where(d => d.EstablishmentId == establishmentId)
                .ToListAsync();
            
            var allTypes = await _context.DocumentTypes.Where(t => t.Active).OrderBy(t => t.Name).ToListAsync();

            // Gerar PDF com QuestPDF
            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text($"Relatório de Legalização - {establishment.Name}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().Text($"Regional: {establishment.Regional}");
                            x.Item().Text($"CNPJ: {establishment.Cnpj}");
                            x.Item().Text($"Data de Geração: {DateTime.Now:dd/MM/yyyy HH:mm}");
                            x.Item().PaddingVertical(0.5f, Unit.Centimetre).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4); // Documento
                                    columns.RelativeColumn(3); // Área
                                    columns.RelativeColumn(2); // Status
                                    columns.RelativeColumn(2); // Validade
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Documento").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Área").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Status").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Validade").SemiBold();
                                });

                                foreach (var type in allTypes)
                                {
                                    var doc = documents.FirstOrDefault(d => d.DocumentTypeId == type.Id);
                                    var status = doc?.Status ?? DocumentStatus.Pending;
                                    var statusText = status switch {
                                        DocumentStatus.Active => "Emitido",
                                        DocumentStatus.Pending => "Pendente",
                                        DocumentStatus.Expired => "Vencido",
                                        DocumentStatus.Protocol => "Protocolo",
                                        DocumentStatus.Exempt => "Isento",
                                        _ => "Outro"
                                    };
                                    
                                    var expiration = doc?.ExpirationDate?.ToString("dd/MM/yyyy") ?? "-";

                                    table.Cell().Element(CellStyle).Text(type.Name);
                                    table.Cell().Element(CellStyle).Text(type.ResponsibleArea);
                                    table.Cell().Element(CellStyle).Text(statusText);
                                    table.Cell().Element(CellStyle).Text(expiration);
                                }

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                }
                            });
                        });

                    page.Footer().AlignCenter().Text(x => { x.Span("Página "); x.CurrentPageNumber(); });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"Relatorio_Legalizacao_{establishment.Name}.pdf");
        }

        // GET: api/legalization/stats
        [HttpGet("stats")]
        public async Task<ActionResult<DocumentExpirationStatsDto>> GetStats()
        {
            var today = DateTime.Today;
            
            // Filtra documentos que têm data de validade e não são isentos
            var query = _context.EstablishmentDocuments
                .Where(d => d.Status != DocumentStatus.Exempt && d.ExpirationDate.HasValue);

            var docs = await query.Select(d => d.ExpirationDate!.Value).ToListAsync();

            return new DocumentExpirationStatsDto
            {
                Expired = docs.Count(d => d < today),
                ExpiresIn5Days = docs.Count(d => d >= today && d <= today.AddDays(5)),
                ExpiresIn15Days = docs.Count(d => d > today.AddDays(5) && d <= today.AddDays(15)),
                ExpiresIn30Days = docs.Count(d => d > today.AddDays(15) && d <= today.AddDays(30))
            };
        }

        // POST: api/legalization/report
        [HttpPost("report")]
        public async Task<ActionResult<LegalizationReportResultDto>> GetReport(LegalizationReportFilterDto filter)
        {
            // 1. Busca Estabelecimentos com seus Tipos e Documentos
            var query = _context.Establishments
                .Include(e => e.Brands)
                .Include(e => e.EstablishmentTypes).ThenInclude(t => t.NecessaryDocuments)
                .Include(e => e.EstablishmentTypes).ThenInclude(t => t.ClosingDocuments)
                .Include(e => e.Documents).ThenInclude(d => d.DocumentType)
                .AsQueryable();

            // Filtros
            if (filter.EstablishmentIds != null && filter.EstablishmentIds.Any()) 
                query = query.Where(e => filter.EstablishmentIds.Contains(e.Id));
            
            if (filter.BrandIds != null && filter.BrandIds.Any()) 
                query = query.Where(e => e.Brands.Any(b => filter.BrandIds.Contains(b.Id)));
            
            if (!string.IsNullOrEmpty(filter.Regional)) 
                query = query.Where(e => e.Regional == filter.Regional);

            if (filter.ServiceLocationTypes != null && filter.ServiceLocationTypes.Any())
                query = query.Where(e => e.EstablishmentTypes.Any(t => filter.ServiceLocationTypes.Contains(t.ServiceLocationType)));

            // Filtro de Segurança (Usuário vê apenas o que tem acesso)
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && userRole != "Administrador" && userRole != "Coordenador")
            {
                var userId = int.Parse(userIdClaim.Value);
                query = query.Where(e => e.Collaborators.Any(c => c.Id == userId));
            }

            var establishments = await query.ToListAsync();

            // 2. Processamento em Memória para calcular Status (incluindo Pendentes)
            var reportData = new List<(string Area, string DocName, string Status)>();
            var today = DateTime.Today;

            foreach (var est in establishments)
            {
                // Identifica todos os tipos de documentos necessários para este estabelecimento
                // (União dos documentos necessários e de encerramento de todos os tipos vinculados)
                var requiredDocTypes = est.EstablishmentTypes
                    .SelectMany(t => t.NecessaryDocuments.Concat(t.ClosingDocuments))
                    .DistinctBy(d => d.Id)
                    .ToList();

                foreach (var docType in requiredDocTypes)
                {
                    // Filtro de Compliance (se marcado, ignora documentos que não são de compliance)
                    if (filter.ComplianceOnly && !docType.IsCompliance)
                        continue;

                    var doc = est.Documents.FirstOrDefault(d => d.DocumentTypeId == docType.Id);
                    
                    string status = "Pendente"; // Padrão se não existir

                    if (doc != null)
                    {
                        if (doc.Status == DocumentStatus.Expired || (doc.ExpirationDate.HasValue && doc.ExpirationDate < today))
                            status = "Vencido";
                        else if (doc.Status == DocumentStatus.Active)
                            status = "Emitido";
                        else if (doc.Status == DocumentStatus.Protocol)
                            status = "Protocolo";
                        else if (doc.Status == DocumentStatus.Provisional)
                            status = "Provisório";
                        else if (doc.Status == DocumentStatus.Exempt)
                            status = "Isento";
                    }

                    if (filter.Statuses != null && filter.Statuses.Any())
                    {
                        var allowedStatusStrings = filter.Statuses.Select(s => s switch
                        {
                            DocumentStatus.Active => "Emitido",
                            DocumentStatus.Pending => "Pendente",
                            DocumentStatus.Expired => "Vencido",
                            DocumentStatus.Protocol => "Protocolo",
                            DocumentStatus.Provisional => "Provisório",
                            DocumentStatus.Exempt => "Isento",
                            _ => s.ToString()
                        }).ToHashSet();

                        if (!allowedStatusStrings.Contains(status))
                            continue;
                    }

                    reportData.Add((docType.ResponsibleArea, docType.Name, status));
                }
            }

            // 3. Agrupa para os Gráficos
            var result = new LegalizationReportResultDto
            {
                TotalEstablishmentsFound = establishments.Count
            };

            // Helper para criar datasets
            StackedChartData BuildChart(IEnumerable<IGrouping<string, (string Area, string DocName, string Status)>> groups)
            {
                var chart = new StackedChartData();
                chart.Labels = groups.Select(g => g.Key).OrderBy(k => k).ToList();

                var statuses = new[] { "Emitido", "Provisório", "Protocolo", "Vencido", "Pendente" };
                var colors = new[] { "#28a745", "#17a2b8", "#ffc107", "#dc3545", "#6c757d" };

                for (int i = 0; i < statuses.Length; i++)
                {
                    var status = statuses[i];
                    var dataset = new ChartDataset { Label = status, BackgroundColor = colors[i] };
                    
                    foreach (var label in chart.Labels)
                    {
                        var count = groups.First(g => g.Key == label).Count(x => x.Status == status);
                        dataset.Data.Add(count);
                    }
                    chart.Datasets.Add(dataset);
                }
                return chart;
            }

            result.DocumentsByArea = BuildChart(reportData.GroupBy(x => x.Area));
            result.DocumentsByType = BuildChart(reportData.GroupBy(x => x.DocName));

            return result;
        }

        // Método Auxiliar de Segurança
        private async Task<bool> UserHasAccessToEstablishment(int establishmentId)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            // 1. Se for Admin ou Coordenador, tem acesso total
            if (userRole == "Administrador" || userRole == "Coordenador")
                return true;

            // 2. Se for Colaborador, verifica se está vinculado ao estabelecimento
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return false;
            
            var userId = int.Parse(userIdClaim.Value);

            var hasAccess = await _context.Collaborators
                .Where(c => c.Id == userId)
                .AnyAsync(c => c.Establishments.Any(e => e.Id == establishmentId));

            return hasAccess;
        }
    }
}