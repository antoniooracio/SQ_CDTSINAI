using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQ.CDT_SINAI.API.Data;
using SQ.CDT_SINAI.Shared.Models;
using SQ.CDT_SINAI.Shared.DTOs;

namespace SQ.CDT_SINAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Exige Token JWT
    public class SpecializationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SpecializationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Specialization>>> GetSpecializations([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var query = _context.Specializations.Where(s => s.Active);
            
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.Name.Contains(search));
            }
            
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResult<Specialization> { Items = items, TotalCount = totalCount, PageNumber = page, PageSize = pageSize };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Specialization>> GetSpecialization(int id)
        {
            var specialization = await _context.Specializations.FindAsync(id);
            if (specialization == null)
                return NotFound();

            return specialization;
        }

        [HttpPost]
        public async Task<ActionResult<Specialization>> PostSpecialization(Specialization specialization)
        {
            _context.Specializations.Add(specialization);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSpecializations), new { id = specialization.Id }, specialization);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpecialization(int id, Specialization specialization)
        {
            if (id != specialization.Id)
                return BadRequest();

            _context.Entry(specialization).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) { if (!_context.Specializations.Any(e => e.Id == id)) return NotFound(); else throw; }

            return NoContent();
        }
    }
}