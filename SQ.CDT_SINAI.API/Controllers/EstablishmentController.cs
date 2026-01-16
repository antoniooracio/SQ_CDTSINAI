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
    public class EstablishmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EstablishmentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Establishment>>> GetEstablishments([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var query = _context.Establishments
                .Include(e => e.Brands)
                .Include(e => e.EstablishmentTypes)
                .Include(e => e.Documents)
                .AsQueryable();

            // Filtro de Segurança: Se não for Admin, vê apenas os seus
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && userRole != "Administrador")
            {
                var userId = int.Parse(userIdClaim.Value);
                query = query.Where(e => e.Collaborators.Any(c => c.Id == userId));
            }

            if (!string.IsNullOrEmpty(search)) query = query.Where(e => e.Name.Contains(search) || e.City.Contains(search));

            var totalCount = await query.CountAsync();
            var items = await query.OrderBy(e => e.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResult<Establishment> { Items = items, TotalCount = totalCount, PageNumber = page, PageSize = pageSize };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Establishment>> GetEstablishment(int id)
        {
            var establishment = await _context.Establishments
                .Include(e => e.Brands)
                .Include(e => e.EstablishmentTypes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (establishment == null) return NotFound();
            return establishment;
        }

        [HttpPost]
        public async Task<ActionResult<Establishment>> PostEstablishment(EstablishmentDto dto)
        {
            var brands = await _context.Brands.Where(b => dto.BrandIds.Contains(b.Id)).ToListAsync();
            var types = await _context.EstablishmentTypes.Where(t => dto.EstablishmentTypeIds.Contains(t.Id)).ToListAsync();

            var establishment = new Establishment
            {
                Name = dto.Name,
                Cnpj = dto.Cnpj,
                UnitCode = dto.UnitCode,
                Address = dto.Address,
                Neighborhood = dto.Neighborhood,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode,
                Regional = dto.Regional,
                Brands = brands,
                EstablishmentTypes = types
            };

            _context.Establishments.Add(establishment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEstablishment), new { id = establishment.Id }, establishment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEstablishment(int id, EstablishmentDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var establishment = await _context.Establishments
                .Include(e => e.Brands)
                .Include(e => e.EstablishmentTypes)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (establishment == null) return NotFound();

            establishment.Name = dto.Name;
            establishment.Cnpj = dto.Cnpj;
            establishment.UnitCode = dto.UnitCode;
            establishment.Address = dto.Address;
            establishment.Neighborhood = dto.Neighborhood;
            establishment.City = dto.City;
            establishment.State = dto.State;
            establishment.ZipCode = dto.ZipCode;
            establishment.Regional = dto.Regional;

            establishment.Brands.Clear();
            establishment.Brands.AddRange(await _context.Brands.Where(b => dto.BrandIds.Contains(b.Id)).ToListAsync());

            establishment.EstablishmentTypes.Clear();
            establishment.EstablishmentTypes.AddRange(await _context.EstablishmentTypes.Where(t => dto.EstablishmentTypeIds.Contains(t.Id)).ToListAsync());

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}