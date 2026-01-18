using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQ.CDT_SINAI.API.Data;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
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
        public async Task<ActionResult<List<Contract>>> GetByEstablishment(int establishmentId)
        {
            if (!await UserHasAccessToEstablishment(establishmentId)) return Forbid();

            return await _context.Contracts
                .Include(c => c.Amendments)
                .Where(c => c.EstablishmentId == establishmentId)
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
                VendorName = dto.VendorName,
                ObjectDescription = dto.ObjectDescription,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                MonthlyValue = dto.MonthlyValue,
                AutomaticRenewal = dto.AutomaticRenewal,
                RenewalMonths = dto.RenewalMonths,
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
            // Soma o valor mensal de todos os contratos ativos
            var totalValue = await _context.Contracts
                .Where(c => c.Status == ContractStatus.Active)
                .SumAsync(c => c.MonthlyValue);

            return totalValue;
        }

        // GET: api/contract/stats
        [HttpGet("stats")]
        public async Task<ActionResult<ContractExpirationStatsDto>> GetStats()
        {
            var today = DateTime.Today;
            
            // Filtra contratos ativos
            var query = _context.Contracts
                .Where(c => c.Status == ContractStatus.Active);

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
            if (userRole == "Administrador" || userRole == "Coordenador") return true;

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
    }
}