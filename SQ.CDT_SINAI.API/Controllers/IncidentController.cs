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
    public class IncidentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IncidentController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/incident/external (Público - Ocorrência Externa)
        [HttpPost("external")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateExternal(CreateExternalIncidentDto dto)
        {
            var incident = new Incident
            {
                Type = IncidentType.External,
                Description = dto.Description,
                TargetAreaOrProfessional = dto.TargetAreaOrProfessional,
                ReporterName = string.IsNullOrEmpty(dto.ReporterName) ? "Anônimo" : dto.ReporterName,
                ReporterContact = dto.ReporterContact,
                Severity = dto.Severity,
                ClientType = dto.ClientType,
                ContactMethod = dto.ContactMethod,
                Category = dto.Category,
                
                // Novos Campos
                ProtocolNumber = dto.ProtocolNumber,
                ClientCpf = dto.ClientCpf,
                Cip = dto.Cip,
                HealthInsurance = dto.HealthInsurance,
                ClientName = dto.ClientName,
                ClientPhone = dto.ClientPhone,
                ClientEmail = dto.ClientEmail,
                ClientAddress = dto.ClientAddress,
                SecondaryContactName = dto.SecondaryContactName,
                ClientBirthDate = dto.ClientBirthDate,
                DoctorCrm = dto.DoctorCrm,
                InvolvedRegional = dto.InvolvedRegional, // Reuso do campo existente
                DoctorName = dto.DoctorName,
                DoctorEmail = dto.DoctorEmail,
                DoctorPhone1 = dto.DoctorPhone1,
                DoctorPhone2 = dto.DoctorPhone2,

                CreatedAt = DateTime.Now,
                Deadline = CalculateDeadline(dto.Severity)
            };

            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ocorrência registrada com sucesso. Protocolo: " + incident.Id });
        }

        // GET: api/incident/external (Listagem para Gestores)
        [HttpGet("external")]
        [Authorize]
        public async Task<ActionResult<PaginatedResult<Incident>>> GetExternalIncidents([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] IncidentStatus? status = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Incidents
                    .Include(i => i.Target) // Inclui o responsável atual, se houver
                    .Where(i => i.Type == IncidentType.External);

                if (status.HasValue) query = query.Where(i => i.Status == status.Value);
                if (startDate.HasValue) query = query.Where(i => i.CreatedAt >= startDate.Value);
                if (endDate.HasValue) query = query.Where(i => i.CreatedAt < endDate.Value.AddDays(1));

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(i => (i.ClientName != null && i.ClientName.Contains(search)) || 
                                             (i.ProtocolNumber != null && i.ProtocolNumber.Contains(search)) ||
                                             i.Description.Contains(search));
                }

                var totalCount = await query.CountAsync();
                var items = await query.OrderByDescending(i => i.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                return new PaginatedResult<Incident> { Items = items, TotalCount = totalCount, PageNumber = page, PageSize = pageSize };
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar ocorrências: {ex.Message}");
            }
        }

        // Método auxiliar para calcular os "3 Tempos"
        private DateTime CalculateDeadline(IncidentSeverity severity)
        {
            return severity switch
            {
                IncidentSeverity.Low => DateTime.Now.AddHours(72),    // 3 Dias
                IncidentSeverity.Medium => DateTime.Now.AddHours(48), // 2 Dias
                IncidentSeverity.High => DateTime.Now.AddHours(24),   // 1 Dia
                _ => DateTime.Now.AddHours(48)
            };
        }

        // POST: api/incident/internal
        [HttpPost("internal")]
        [Authorize]
        public async Task<IActionResult> CreateInternal(CreateInternalIncidentDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            
            var userId = int.Parse(userIdClaim.Value);

            var incident = new Incident
            {
                Type = IncidentType.Internal,
                Description = dto.Description,
                ReporterId = userId,
                TargetId = dto.TargetId,
                Severity = dto.Severity,
                Category = dto.Category,
                InvolvedArea = dto.InvolvedArea,
                LocationOrUnit = dto.LocationOrUnit,
                TargetArea = dto.TargetArea,
                InvolvedRegional = dto.InvolvedRegional,
                InvolvedBrand = dto.InvolvedBrand,
                ClientName = dto.ClientName,
                ProtocolNumber = dto.ProtocolNumber,
                ClientCpf = dto.ClientCpf,
                CreatedAt = DateTime.Now,
                Deadline = CalculateDeadline(dto.Severity)
            };

            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ocorrência interna registrada com sucesso." });
        }

        // GET: api/incident/my-incidents
        [HttpGet("my-incidents")]
        [Authorize]
        public async Task<ActionResult<PaginatedResult<Incident>>> GetMyIncidents([FromQuery] int page = 1, [FromQuery] int pageSize = 5, [FromQuery] IncidentStatus? status = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var query = _context.Incidents
                .Include(i => i.Reporter)
                .Where(i => i.TargetId == userId);

            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);

            if (startDate.HasValue)
                query = query.Where(i => i.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(i => i.CreatedAt < endDate.Value.AddDays(1)); // Menor que o dia seguinte para cobrir o dia todo

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(i => i.Deadline)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Incident> { Items = items, TotalCount = totalCount, PageNumber = page, PageSize = pageSize };
        }

        // GET: api/incident/my-stats
        [HttpGet("my-stats")]
        [Authorize]
        public async Task<ActionResult<Dictionary<string, int>>> GetMyIncidentStatistics()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);

            return await _context.Incidents
                .Where(i => i.TargetId == userId && i.Status == IncidentStatus.Open)
                .GroupBy(i => i.Severity)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Severity.ToString(), v => v.Count);
        }

        // GET: api/incident/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Incident>> GetIncident(int id)
        {
            var incident = await _context.Incidents
                .Include(i => i.Reporter)
                .Include(i => i.Target)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (incident == null) return NotFound();
            return incident;
        }

        // PUT: api/incident/{id}/respond
        [HttpPut("{id}/respond")]
        [Authorize]
        public async Task<IActionResult> RespondIncident(int id, [FromBody] string response)
        {
            var incident = await _context.Incidents.FindAsync(id);
            if (incident == null) return NotFound();

            incident.Response = response;
            incident.ResponseDate = DateTime.Now;
            incident.Status = IncidentStatus.Responded;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Resposta registrada com sucesso." });
        }

        // PUT: api/incident/{id}/assign
        [HttpPut("{id}/assign")]
        [Authorize]
        public async Task<IActionResult> AssignIncident(int id, [FromBody] int targetId)
        {
            var incident = await _context.Incidents.FindAsync(id);
            if (incident == null) return NotFound();

            // Atualiza o responsável (Target)
            incident.TargetId = targetId;
            
            await _context.SaveChangesAsync();
            return Ok(new { message = "Ocorrência atribuída com sucesso." });
        }
    }
}