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
    public class CollaboratorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CollaboratorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Collaborator>>> GetCollaborators([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var query = _context.Collaborators
                .Include(c => c.Specializations)
                .Include(c => c.Role)
                .Where(c => c.Active);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var items = await query.OrderBy(c => c.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResult<Collaborator> { Items = items, TotalCount = totalCount, PageNumber = page, PageSize = pageSize };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Collaborator>> GetCollaborator(int id)
        {
            var collaborator = await _context.Collaborators
                .Include(c => c.Specializations)
                .Include(c => c.Establishments)
                .Include(c => c.Role)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (collaborator == null)
                return NotFound();

            return collaborator;
        }

        [HttpPost]
        public async Task<ActionResult<Collaborator>> PostCollaborator(RegisterCollaboratorDto dto)
        {
            if (await _context.Collaborators.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email já cadastrado.");

            if (await _context.Collaborators.AnyAsync(u => u.Cpf == dto.Cpf))
                return BadRequest("CPF já cadastrado.");

            if (await _context.Collaborators.AnyAsync(u => u.PhoneNumber == dto.PhoneNumber))
                return BadRequest("Telefone já cadastrado.");

            // Busca as especializações pelos IDs informados
            var specializations = await _context.Specializations
                .Where(s => dto.SpecializationIds.Contains(s.Id))
                .ToListAsync();

            var establishments = await _context.Establishments
                .Where(e => dto.EstablishmentIds.Contains(e.Id))
                .ToListAsync();

            var collaborator = new Collaborator
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password, // TODO: Implementar Hash de senha
                Specializations = specializations,
                RoleId = dto.RoleId,
                Establishments = establishments,
                
                Cpf = dto.Cpf,
                Rg = dto.Rg,
                BirthDate = dto.BirthDate,
                PhoneNumber = dto.PhoneNumber ?? string.Empty,
                AddressStreet = dto.AddressStreet ?? string.Empty,
                AddressNumber = dto.AddressNumber ?? string.Empty,
                AddressNeighborhood = dto.AddressNeighborhood ?? string.Empty,
                AddressCity = dto.AddressCity ?? string.Empty,
                AddressState = dto.AddressState ?? string.Empty,
                AddressZipCode = dto.AddressZipCode ?? string.Empty,
                ProfessionalLicense = dto.ProfessionalLicense,
                JobTitle = dto.JobTitle,
                AdmissionDate = dto.AdmissionDate
            };

            _context.Collaborators.Add(collaborator);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCollaborator), new { id = collaborator.Id }, collaborator);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCollaborator(int id, UpdateCollaboratorDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var collaborator = await _context.Collaborators
                .Include(c => c.Specializations)
                .Include(c => c.Establishments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (collaborator == null)
                return NotFound();

            // Validações de unicidade (excluindo o próprio usuário da verificação)
            if (await _context.Collaborators.AnyAsync(c => c.Cpf == dto.Cpf && c.Id != id))
                return BadRequest("CPF já cadastrado para outro colaborador.");

            if (await _context.Collaborators.AnyAsync(c => c.Email == dto.Email && c.Id != id))
                return BadRequest("Email já cadastrado para outro colaborador.");

            if (await _context.Collaborators.AnyAsync(c => c.PhoneNumber == dto.PhoneNumber && c.Id != id))
                return BadRequest("Telefone já cadastrado para outro colaborador.");

            // Atualiza propriedades simples
            collaborator.Name = dto.Name;
            collaborator.Email = dto.Email;
            collaborator.Cpf = dto.Cpf;
            collaborator.Rg = dto.Rg;
            collaborator.BirthDate = dto.BirthDate;
            collaborator.PhoneNumber = dto.PhoneNumber ?? string.Empty;
            collaborator.JobTitle = dto.JobTitle;
            collaborator.ProfessionalLicense = dto.ProfessionalLicense;
            collaborator.Active = dto.Active;
            collaborator.RoleId = dto.RoleId;
            
            collaborator.AddressStreet = dto.AddressStreet ?? string.Empty;
            collaborator.AddressNumber = dto.AddressNumber ?? string.Empty;
            collaborator.AddressNeighborhood = dto.AddressNeighborhood ?? string.Empty;
            collaborator.AddressCity = dto.AddressCity ?? string.Empty;
            collaborator.AddressState = dto.AddressState ?? string.Empty;
            collaborator.AddressZipCode = dto.AddressZipCode ?? string.Empty;

            // Atualiza Especializações (Many-to-Many)
            collaborator.Specializations.Clear();
            var specializations = await _context.Specializations
                .Where(s => dto.SpecializationIds.Contains(s.Id))
                .ToListAsync();
            collaborator.Specializations.AddRange(specializations);

            // Atualiza Estabelecimentos
            collaborator.Establishments.Clear();
            var establishments = await _context.Establishments
                .Where(e => dto.EstablishmentIds.Contains(e.Id))
                .ToListAsync();
            collaborator.Establishments.AddRange(establishments);

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}