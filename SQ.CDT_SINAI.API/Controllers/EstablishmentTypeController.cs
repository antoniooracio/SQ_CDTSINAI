using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQ.CDT_SINAI.API.Data;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;

namespace SQ.CDT_SINAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EstablishmentTypeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EstablishmentTypeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<EstablishmentType>>> GetTypes([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var query = _context.EstablishmentTypes.AsQueryable();
            if (!string.IsNullOrEmpty(search)) query = query.Where(t => t.Name.Contains(search));

            var totalCount = await query.CountAsync();
            var items = await query.OrderBy(t => t.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResult<EstablishmentType> { Items = items, TotalCount = totalCount, PageNumber = page, PageSize = pageSize };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EstablishmentType>> GetType(int id)
        {
            var type = await _context.EstablishmentTypes
                .Include(t => t.NecessaryDocuments)
                .Include(t => t.ClosingDocuments)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (type == null) return NotFound();
            return type;
        }

        [HttpPost]
        public async Task<ActionResult<EstablishmentType>> PostType(EstablishmentTypeDto dto)
        {
            var necessaryDocs = await _context.DocumentTypes.Where(d => dto.NecessaryDocumentIds.Contains(d.Id)).ToListAsync();
            var closingDocs = await _context.DocumentTypes.Where(d => dto.ClosingDocumentIds.Contains(d.Id)).ToListAsync();

            var type = new EstablishmentType
            {
                Name = dto.Name,
                ServiceLocationType = dto.ServiceLocationType,
                NecessaryDocuments = necessaryDocs,
                ClosingDocuments = closingDocs
            };

            _context.EstablishmentTypes.Add(type);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetType), new { id = type.Id }, type);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutType(int id, EstablishmentTypeDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var type = await _context.EstablishmentTypes
                .Include(t => t.NecessaryDocuments)
                .Include(t => t.ClosingDocuments)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (type == null) return NotFound();

            type.Name = dto.Name;
            type.ServiceLocationType = dto.ServiceLocationType;

            type.NecessaryDocuments.Clear();
            type.NecessaryDocuments.AddRange(await _context.DocumentTypes.Where(d => dto.NecessaryDocumentIds.Contains(d.Id)).ToListAsync());

            type.ClosingDocuments.Clear();
            type.ClosingDocuments.AddRange(await _context.DocumentTypes.Where(d => dto.ClosingDocumentIds.Contains(d.Id)).ToListAsync());

            try 
            { 
                await _context.SaveChangesAsync(); 
            }
            catch (DbUpdateConcurrencyException) 
            { 
                if (!_context.EstablishmentTypes.Any(e => e.Id == id)) return NotFound(); 
                else throw; 
            }
            return NoContent();
        }
    }
}