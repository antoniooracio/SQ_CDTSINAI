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
    public class ContractController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ContractController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/contract/establishment/{id}
        [HttpGet("establishment/{establishmentId}")]
        public async Task<ActionResult<List<Contract>>> GetByEstablishment(int establishmentId, [FromQuery] ContractType? type = null, [FromQuery] string? search = null)
        {
            if (!await UserHasAccessToEstablishment(establishmentId)) return Forbid();

            var query = _context.Contracts
                .Include(c => c.Amendments)
                .Where(c => c.EstablishmentId == establishmentId);

            if (type.HasValue)
                query = query.Where(c => c.Type == type.Value);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.ContractNumber.Contains(search) || 
                                         c.VendorName.Contains(search) || 
                                         c.ObjectDescription.Contains(search) ||
                                         c.MonthlyValue.ToString().Contains(search));
            }

            return await query
                .OrderByDescending(c => c.EndDate)
                .ToListAsync();
        }

        // GET: api/contract/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Contract>> GetById(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Amendments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null) return NotFound();
            if (!await UserHasAccessToEstablishment(contract.EstablishmentId)) return Forbid();

            return contract;
        }

        // POST: api/contract
        [HttpPost]
        public async Task<ActionResult<Contract>> Create([FromForm] ContractDto dto, IFormFile? file)
        {
            if (!await UserHasAccessToEstablishment(dto.EstablishmentId)) return Forbid();

            var contract = new Contract
            {
                EstablishmentId = dto.EstablishmentId,
                ContractNumber = dto.ContractNumber,
                Type = dto.Type!.Value,
                VendorName = dto.VendorName,
                ObjectDescription = dto.ObjectDescription,
                StartDate = dto.StartDate!.Value,
                EndDate = dto.EndDate!.Value,
                MonthlyValue = dto.MonthlyValue!.Value,
                PaymentFrequency = dto.PaymentFrequency,
                InstallmentCount = dto.InstallmentCount,
                TotalContractValue = dto.TotalContractValue,
                AutomaticRenewal = dto.AutomaticRenewal,
                RenewalMonths = dto.RenewalMonths ?? 0,
                Status = ContractStatus.Active
            };

            if (file != null && file.Length > 0)
            {
                var (fileName, filePath) = await SaveFileAsync(file);
                contract.FileName = fileName;
                contract.FilePath = filePath;
            }

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
        }

        // PUT: api/contract/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] ContractDto dto, IFormFile? file)
        {
            if (id != dto.Id) return BadRequest("ID do contrato não confere.");

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound("Contrato não encontrado.");
            
            if (!await UserHasAccessToEstablishment(contract.EstablishmentId)) return Forbid();

            // Atualiza os campos
            contract.ContractNumber = dto.ContractNumber;
            contract.Type = dto.Type!.Value;
            contract.VendorName = dto.VendorName;
            contract.ObjectDescription = dto.ObjectDescription;
            contract.StartDate = dto.StartDate!.Value;
            contract.EndDate = dto.EndDate!.Value;
            contract.MonthlyValue = dto.MonthlyValue!.Value;
            contract.PaymentFrequency = dto.PaymentFrequency;
            contract.InstallmentCount = dto.InstallmentCount;
            contract.TotalContractValue = dto.TotalContractValue;
            contract.AutomaticRenewal = dto.AutomaticRenewal;
            contract.RenewalMonths = dto.RenewalMonths ?? 0;
            contract.Status = dto.Status;

            if (file != null && file.Length > 0)
            {
                var (fileName, filePath) = await SaveFileAsync(file);
                contract.FileName = fileName;
                contract.FilePath = filePath;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/contract/amendment
        [HttpPost("amendment")]
        public async Task<IActionResult> AddAmendment([FromForm] ContractAmendmentDto dto, IFormFile? file)
        {
            var contract = await _context.Contracts.FindAsync(dto.ContractId);
            if (contract == null) return NotFound("Contrato não encontrado.");
            
            if (!await UserHasAccessToEstablishment(contract.EstablishmentId)) return Forbid();

            var amendment = new ContractAmendment
            {
                ContractId = dto.ContractId,
                AmendmentNumber = dto.AmendmentNumber,
                Type = dto.Type,
                SignatureDate = dto.SignatureDate,
                NewEndDate = dto.NewEndDate,
                NewMonthlyValue = dto.NewMonthlyValue,
                Description = dto.Description
            };

            if (file != null && file.Length > 0)
            {
                var (fileName, filePath) = await SaveFileAsync(file);
                amendment.FileName = fileName;
                amendment.FilePath = filePath;
            }

            // === Lógica de Negócio: Atualização do Contrato Pai ===
            
            // Se o aditivo altera o prazo (Prorrogação), atualiza a data fim do contrato
            if (dto.NewEndDate.HasValue)
            {
                contract.EndDate = dto.NewEndDate.Value;
            }

            // Se o aditivo altera o valor (Reajuste), atualiza o valor mensal do contrato
            if (dto.NewMonthlyValue.HasValue)
            {
                contract.MonthlyValue = dto.NewMonthlyValue.Value;
            }

            _context.ContractAmendments.Add(amendment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Aditivo registrado com sucesso." });
        }

        // GET: api/contract/download/{id}
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || string.IsNullOrEmpty(contract.FilePath)) return NotFound();
            
            if (!await UserHasAccessToEstablishment(contract.EstablishmentId)) return Forbid();

            var filePath = Path.Combine(_environment.ContentRootPath, "Uploads", "Contracts", contract.FilePath);
            if (!System.IO.File.Exists(filePath)) return NotFound("Arquivo não encontrado no servidor.");

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, "application/pdf", contract.FileName ?? "contrato.pdf");
        }

        // GET: api/contract/amendment/download/{id}
        [HttpGet("amendment/download/{id}")]
        public async Task<IActionResult> DownloadAmendment(int id)
        {
            var amendment = await _context.ContractAmendments.Include(a => a.Contract).FirstOrDefaultAsync(a => a.Id == id);
            if (amendment == null || string.IsNullOrEmpty(amendment.FilePath)) return NotFound();

            if (amendment.Contract != null && !await UserHasAccessToEstablishment(amendment.Contract.EstablishmentId)) return Forbid();

            var filePath = Path.Combine(_environment.ContentRootPath, "Uploads", "Contracts", amendment.FilePath);
            if (!System.IO.File.Exists(filePath)) return NotFound("Arquivo não encontrado no servidor.");

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, "application/pdf", amendment.FileName ?? "aditivo.pdf");
        }

        // GET: api/contract/monthly-value
        [HttpGet("monthly-value")]
        public async Task<ActionResult<decimal>> GetMonthlyValueStats()
        {
            var query = _context.Contracts.AsQueryable();
            
            // Aplica filtro de segurança (Filiais do usuário)
            query = ApplySecurityFilter(query);

            // Busca dados necessários para cálculo em memória
            var activeContracts = await query
                .Where(c => c.Status == ContractStatus.Active)
                .Select(c => new { 
                    c.PaymentFrequency, 
                    c.MonthlyValue, 
                    c.StartDate, 
                    c.InstallmentCount 
                })
                .ToListAsync();

            decimal totalValue = 0;
            var today = DateTime.Today;
            var currentMonthStart = new DateTime(today.Year, today.Month, 1);
            var nextMonthStart = currentMonthStart.AddMonths(1);

            foreach (var contract in activeContracts)
            {
                if (contract.PaymentFrequency == PaymentFrequency.Monthly)
                {
                    totalValue += contract.MonthlyValue;
                }
                else if (contract.PaymentFrequency == PaymentFrequency.Installments)
                {
                    // Soma apenas se ainda houver parcelas a pagar (Data Atual < Data Início + Meses Parcelas)
                    var installmentsEndDate = contract.StartDate.AddMonths(contract.InstallmentCount ?? 0);
                    if (currentMonthStart < installmentsEndDate)
                        totalValue += contract.MonthlyValue;
                }
                else if (contract.PaymentFrequency == PaymentFrequency.Single)
                {
                    // Soma apenas se o contrato iniciou neste mês (Custo pontual do mês)
                    if (contract.StartDate >= currentMonthStart && contract.StartDate < nextMonthStart)
                        totalValue += contract.MonthlyValue;
                }
            }

            return totalValue;
        }

        // GET: api/contract/stats
        [HttpGet("stats")]
        public async Task<ActionResult<ContractExpirationStatsDto>> GetStats()
        {
            var today = DateTime.Today;
            
            var query = _context.Contracts.AsQueryable();

            // Aplica filtro de segurança (Filiais do usuário)
            query = ApplySecurityFilter(query);

            query = query.Where(c => c.Status == ContractStatus.Active);

            var contracts = await query.Select(c => c.EndDate).ToListAsync();

            return new ContractExpirationStatsDto
            {
                Expired = contracts.Count(d => d < today),
                ExpiresIn5Days = contracts.Count(d => d >= today && d <= today.AddDays(5)),
                ExpiresIn15Days = contracts.Count(d => d > today.AddDays(5) && d <= today.AddDays(15)),
                ExpiresIn30Days = contracts.Count(d => d > today.AddDays(15) && d <= today.AddDays(30))
            };
        }

        // GET: api/contract/renewal-history
        [HttpGet("renewal-history")]
        public async Task<ActionResult<List<ContractAmendment>>> GetRenewalHistory([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? search)
        {
            // Filtra aditivos que são do tipo TermExtension e contêm "Renovação Automática" no número
            var query = _context.ContractAmendments
                .Include(a => a.Contract)
                .ThenInclude(c => c.Establishment) // Inclui o estabelecimento para mostrar na tela
                .Where(a => a.Type == AmendmentType.TermExtension && a.AmendmentNumber.Contains("Renovação Automática"));

            // Filtro de Segurança para o Histórico
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Administrador")
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    var userId = int.Parse(userIdClaim.Value);
                    query = query.Where(a => _context.Collaborators.Where(col => col.Id == userId).SelectMany(col => col.Establishments).Any(e => e.Id == a.Contract.EstablishmentId));
                }
            }

            if (startDate.HasValue)
                query = query.Where(a => a.SignatureDate >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(a => a.SignatureDate <= endDate.Value);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.Contract.Establishment.Name.Contains(search) || a.Contract.ContractNumber.Contains(search) || a.Contract.VendorName.Contains(search));
            }

            return await query.OrderByDescending(a => a.SignatureDate).ToListAsync();
        }

        // POST: api/contract/process-renewals
        [HttpPost("process-renewals")]
        [AllowAnonymous] // Deve ser protegido por chave de API ou IP em produção
        public async Task<IActionResult> ProcessRenewals()
        {
            var today = DateTime.Today;
            var contractsToRenew = await _context.Contracts
                .Where(c => c.Status == ContractStatus.Active && c.AutomaticRenewal && c.EndDate <= today)
                .ToListAsync();

            foreach (var contract in contractsToRenew)
            {
                var newEndDate = contract.EndDate.AddMonths(contract.RenewalMonths);
                
                var amendment = new ContractAmendment
                {
                    ContractId = contract.Id,
                    AmendmentNumber = $"Renovação Automática - {today:dd/MM/yyyy}",
                    Type = AmendmentType.TermExtension,
                    SignatureDate = today,
                    NewEndDate = newEndDate,
                    Description = $"Renovação automática de {contract.RenewalMonths} meses processada pelo sistema."
                };

                contract.EndDate = newEndDate;
                _context.ContractAmendments.Add(amendment);
            }

            if (contractsToRenew.Any())
                await _context.SaveChangesAsync();

            return Ok(new { message = $"{contractsToRenew.Count} contratos renovados." });
        }

        // POST: api/contract/revert-renewal/{amendmentId}
        [HttpPost("revert-renewal/{amendmentId}")]
        public async Task<IActionResult> RevertRenewal(int amendmentId)
        {
            var amendment = await _context.ContractAmendments
                .Include(a => a.Contract)
                .FirstOrDefaultAsync(a => a.Id == amendmentId);

            if (amendment == null) return NotFound("Aditivo não encontrado.");
            
            if (!await UserHasAccessToEstablishment(amendment.Contract!.EstablishmentId)) return Forbid();

            // Verifica se é uma renovação automática
            if (amendment.Type != AmendmentType.TermExtension || !amendment.AmendmentNumber.Contains("Renovação Automática"))
            {
                return BadRequest("Apenas renovações automáticas podem ser revertidas por aqui.");
            }

            // Verifica se é o último aditivo (para garantir consistência cronológica)
            var lastAmendment = await _context.ContractAmendments
                .Where(a => a.ContractId == amendment.ContractId)
                .OrderByDescending(a => a.SignatureDate)
                .FirstOrDefaultAsync();

            if (lastAmendment != null && lastAmendment.Id != amendment.Id)
            {
                return BadRequest("Não é possível reverter esta renovação pois existem aditivos posteriores.");
            }

            // Reverte a data do contrato (subtrai os meses renovados ou volta para a data anterior)
            // Simplificação: Como sabemos que foi uma extensão, podemos tentar restaurar a data anterior.
            // A data anterior seria a data de assinatura deste aditivo (que é a data que venceu)
            amendment.Contract.EndDate = amendment.SignatureDate;

            _context.ContractAmendments.Remove(amendment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Renovação revertida com sucesso." });
        }

        // GET: api/contract/report/{establishmentId}
        [HttpGet("report/{establishmentId}")]
        public async Task<IActionResult> GetReport(int establishmentId)
        {
            if (!await UserHasAccessToEstablishment(establishmentId)) return Forbid();

            var establishment = await _context.Establishments.FindAsync(establishmentId);
            if (establishment == null) return NotFound("Estabelecimento não encontrado.");

            var contracts = await _context.Contracts
                .Where(c => c.EstablishmentId == establishmentId)
                .OrderByDescending(c => c.EndDate)
                .ToListAsync();

            // Gerar PDF com QuestPDF
            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape()); // Paisagem para caber mais colunas
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text($"Relatório de Contratos - {establishment.Name}")
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
                                    columns.RelativeColumn(2); // Número
                                    columns.RelativeColumn(3); // Fornecedor
                                    columns.RelativeColumn(4); // Objeto
                                    columns.RelativeColumn(2); // Vigência
                                    columns.RelativeColumn(2); // Valor
                                    columns.RelativeColumn(2); // Status
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Número").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Fornecedor").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Objeto").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Vigência").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Valor").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Status").SemiBold();
                                });

                                foreach (var contract in contracts)
                                {
                                    var statusText = contract.Status switch {
                                        ContractStatus.Active => "Vigente",
                                        ContractStatus.Expired => "Vencido",
                                        ContractStatus.Canceled => "Cancelado",
                                        _ => "Rascunho"
                                    };

                                    var valueText = contract.PaymentFrequency == PaymentFrequency.Monthly 
                                        ? $"{contract.MonthlyValue:C2} (Mensal)" 
                                        : $"{contract.MonthlyValue:C2} ({(contract.PaymentFrequency == PaymentFrequency.Single ? "Único" : "Parcela")})";

                                    table.Cell().Element(CellStyle).Text(contract.ContractNumber);
                                    table.Cell().Element(CellStyle).Text(contract.VendorName);
                                    table.Cell().Element(CellStyle).Text(contract.ObjectDescription);
                                    table.Cell().Element(CellStyle).Text($"{contract.StartDate:dd/MM/yy} a {contract.EndDate:dd/MM/yy}");
                                    table.Cell().Element(CellStyle).Text(valueText);
                                    table.Cell().Element(CellStyle).Text(statusText);
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

            return File(pdf, "application/pdf", $"Relatorio_Contratos_{establishment.Name}.pdf");
        }

        // POST: api/contract/report-data
        [HttpPost("report-data")]
        public async Task<ActionResult<ContractReportResultDto>> GetReportData(ContractReportFilterDto filter)
        {
            var query = _context.Contracts
                .Include(c => c.Establishment).ThenInclude(e => e.Brands)
                .AsQueryable();

            // Aplica filtro de segurança
            query = ApplySecurityFilter(query);

            // Filtros do Relatório
            if (filter.EstablishmentIds != null && filter.EstablishmentIds.Any())
                query = query.Where(c => filter.EstablishmentIds.Contains(c.EstablishmentId));

            if (filter.BrandIds != null && filter.BrandIds.Any())
                query = query.Where(c => c.Establishment.Brands.Any(b => filter.BrandIds.Contains(b.Id)));

            if (!string.IsNullOrEmpty(filter.Regional))
                query = query.Where(c => c.Establishment.Regional == filter.Regional);

            if (filter.Types != null && filter.Types.Any())
                query = query.Where(c => filter.Types.Contains(c.Type));

            if (filter.Statuses != null && filter.Statuses.Any())
                query = query.Where(c => filter.Statuses.Contains(c.Status));

            if (filter.StartDate.HasValue)
                query = query.Where(c => c.StartDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(c => c.EndDate <= filter.EndDate.Value);

            var contracts = await query.ToListAsync();

            // Separação de Valores (Apenas Ativos)
            var activeContracts = contracts.Where(c => c.Status == ContractStatus.Active).ToList();

            var recurringValue = activeContracts
                .Where(c => c.PaymentFrequency == PaymentFrequency.Monthly)
                .Sum(c => c.MonthlyValue);

            var nonRecurringValue = activeContracts
                .Where(c => c.PaymentFrequency != PaymentFrequency.Monthly)
                .Sum(c => c.TotalContractValue ?? (c.MonthlyValue * (c.InstallmentCount ?? 1)));

            // Processamento dos Gráficos
            var result = new ContractReportResultDto
            {
                TotalContractsFound = contracts.Count,
                TotalRecurringValue = recurringValue,
                TotalNonRecurringValue = nonRecurringValue,
                // Mantemos o TotalMonthlyValue como a soma do fluxo estimado do mês (lógica anterior) se necessário, 
                // ou podemos removê-lo da view se os novos campos forem suficientes.
                TotalMonthlyValue = recurringValue // Simplificação para não quebrar compatibilidade imediata
            };

            // Helper para criar datasets
            ContractChartData BuildChart(IEnumerable<IGrouping<string, Contract>> groups)
            {
                var chart = new ContractChartData();
                chart.Labels = groups.Select(g => g.Key).OrderBy(k => k).ToList();

                var statuses = new[] { ContractStatus.Active, ContractStatus.Expired, ContractStatus.Canceled, ContractStatus.Draft };
                var colors = new[] { "#28a745", "#dc3545", "#6c757d", "#ffc107" }; // Verde, Vermelho, Cinza, Amarelo
                var labels = new[] { "Vigente", "Vencido", "Cancelado", "Rascunho" };

                for (int i = 0; i < statuses.Length; i++)
                {
                    var status = statuses[i];
                    var dataset = new ContractChartDataset { Label = labels[i], BackgroundColor = colors[i] };
                    
                    foreach (var label in chart.Labels)
                    {
                        var count = groups.First(g => g.Key == label).Count(x => x.Status == status);
                        dataset.Data.Add(count);
                    }
                    chart.Datasets.Add(dataset);
                }
                return chart;
            }

            result.ContractsByType = BuildChart(contracts.GroupBy(c => c.Type.ToString()));
            
            // Gráfico por Regional (Valor Total)
            var regionalGroups = contracts.Where(c => c.Status == ContractStatus.Active)
                                          .GroupBy(c => c.Establishment?.Regional ?? "N/A")
                                          .OrderBy(g => g.Key)
                                          .ToList();

            var regionalChart = new ContractChartData();
            regionalChart.Labels = regionalGroups.Select(g => g.Key).ToList();
            
            // Dataset 1: Mensal (Recorrente)
            var dsMonthly = new ContractChartDataset 
            { 
                Label = "Mensal (Recorrente)", 
                BackgroundColor = "#28a745" // Verde
            };
            dsMonthly.Data = regionalGroups.Select(g => (int)g
                .Where(c => c.PaymentFrequency == PaymentFrequency.Monthly)
                .Sum(c => c.MonthlyValue)).ToList();
            regionalChart.Datasets.Add(dsMonthly);

            // Dataset 2: Único
            var dsSingle = new ContractChartDataset 
            { 
                Label = "Pagamento Único", 
                BackgroundColor = "#17a2b8" // Azul
            };
            dsSingle.Data = regionalGroups.Select(g => (int)g
                .Where(c => c.PaymentFrequency == PaymentFrequency.Single)
                .Sum(c => c.TotalContractValue ?? c.MonthlyValue)).ToList();
            regionalChart.Datasets.Add(dsSingle);

            // Dataset 3: Parcelado
            var dsInstallments = new ContractChartDataset 
            { 
                Label = "Parcelado (Total)", 
                BackgroundColor = "#ffc107" // Amarelo
            };
            dsInstallments.Data = regionalGroups.Select(g => (int)g
                .Where(c => c.PaymentFrequency == PaymentFrequency.Installments)
                .Sum(c => c.TotalContractValue ?? (c.MonthlyValue * (c.InstallmentCount ?? 1)))).ToList();
            regionalChart.Datasets.Add(dsInstallments);

            // Dataset 4: Outros/Indefinido (Legado)
            var dsOther = new ContractChartDataset 
            { 
                Label = "Outros/Indefinido", 
                BackgroundColor = "#6c757d" // Cinza
            };
            dsOther.Data = regionalGroups.Select(g => (int)g
                .Where(c => (int)c.PaymentFrequency == 0 || c.PaymentFrequency == (PaymentFrequency)99) // 0 = Default do banco antigo
                .Sum(c => c.TotalContractValue ?? c.MonthlyValue)).ToList();
            
            if (dsOther.Data.Sum() > 0) regionalChart.Datasets.Add(dsOther);

            result.ContractsByRegional = regionalChart;

            return result;
        }

        private async Task<(string FileName, string FilePath)> SaveFileAsync(IFormFile file)
        {
            var uploadFolder = Path.Combine(_environment.ContentRootPath, "Uploads", "Contracts");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return (file.FileName, uniqueFileName);
        }

        private async Task<bool> UserHasAccessToEstablishment(int establishmentId)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Administrador") return true; // Apenas Admin tem acesso global

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return false;
            var userId = int.Parse(userIdClaim.Value);

            // Verifica se o usuário tem vínculo com o estabelecimento
            // Nota: Isso assume que a tabela de junção CollaboratorEstablishment existe e está mapeada
            // Se não estiver mapeada diretamente no DbContext, pode ser necessário ajustar a query
            return await _context.Collaborators
                .Where(c => c.Id == userId)
                .AnyAsync(c => c.Establishments.Any(e => e.Id == establishmentId));
        }

        // Helper para aplicar filtro de segurança em queries de Contrato
        private IQueryable<Contract> ApplySecurityFilter(IQueryable<Contract> query)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Administrador") return query;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return query.Where(x => false); // Sem usuário, sem acesso
            
            var userId = int.Parse(userIdClaim.Value);

            // Filtra contratos onde o estabelecimento está na lista de estabelecimentos do usuário
            return query.Where(c => _context.Collaborators
                .Where(col => col.Id == userId)
                .SelectMany(col => col.Establishments)
                .Any(e => e.Id == c.EstablishmentId));
        }
    }
}