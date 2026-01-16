using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQ.CDT_SINAI.API.Data;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentRenewalLogController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DocumentRenewalLogController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<DocumentRenewalLog>>> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.DocumentRenewalLogs
                    .Include(l => l.EstablishmentDocument).ThenInclude(d => d.DocumentType)
                    .Include(l => l.EstablishmentDocument).ThenInclude(d => d.Establishment)
                    .AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(l => l.RenewalDate >= startDate.Value);
                
                if (endDate.HasValue)
                    query = query.Where(l => l.RenewalDate < endDate.Value.AddDays(1));

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(l => l.EstablishmentDocument.Establishment.Name.Contains(search) || l.EstablishmentDocument.DocumentType.Name.Contains(search));

                query = query.OrderByDescending(l => l.RenewalDate);

                var totalCount = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                return new PaginatedResult<DocumentRenewalLog> { Items = items, TotalCount = totalCount, PageNumber = page, PageSize = pageSize };
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar logs: {ex.Message}");
            }
        }

        [HttpPost("revert/{id}")]
        public async Task<IActionResult> RevertRenewal(int id)
        {
            var log = await _context.DocumentRenewalLogs
                .Include(l => l.EstablishmentDocument)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (log == null) return NotFound("Registro de renovação não encontrado.");
            if (log.EstablishmentDocument == null) return BadRequest("Documento associado não encontrado.");

            // Reverte a data para a anterior
            log.EstablishmentDocument.ExpirationDate = log.OldExpirationDate;
            log.EstablishmentDocument.LastUpdated = DateTime.Now;

            // Remove o log para evitar reversão duplicada e limpar histórico
            _context.DocumentRenewalLogs.Remove(log);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Renovação revertida com sucesso." });
        }
    }
}