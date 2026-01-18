using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
    public class CollaboratorController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CollaboratorController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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

            var phone = dto.PhoneNumber?.Trim();
            if (!string.IsNullOrEmpty(phone) && await _context.Collaborators.AnyAsync(u => u.PhoneNumber == phone))
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
                Rg = dto.Rg ?? string.Empty,
                BirthDate = dto.BirthDate,
                PhoneNumber = phone ?? string.Empty,
                AddressStreet = dto.AddressStreet ?? string.Empty,
                AddressNumber = dto.AddressNumber ?? string.Empty,
                AddressNeighborhood = dto.AddressNeighborhood ?? string.Empty,
                AddressCity = dto.AddressCity ?? string.Empty,
                AddressState = dto.AddressState ?? string.Empty,
                AddressZipCode = dto.AddressZipCode ?? string.Empty,
                ProfessionalLicense = dto.ProfessionalLicense ?? string.Empty,
                JobTitle = dto.JobTitle ?? string.Empty,
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

            var phone = dto.PhoneNumber?.Trim();
            if (!string.IsNullOrEmpty(phone) && await _context.Collaborators.AnyAsync(c => c.PhoneNumber == phone && c.Id != id))
                return BadRequest("Telefone já cadastrado para outro colaborador.");

            // Atualiza propriedades simples
            collaborator.Name = dto.Name;
            collaborator.Email = dto.Email;
            collaborator.Cpf = dto.Cpf;
            collaborator.Rg = dto.Rg ?? string.Empty;
            collaborator.BirthDate = dto.BirthDate;
            collaborator.PhoneNumber = phone ?? string.Empty;
            collaborator.JobTitle = dto.JobTitle ?? string.Empty;
            collaborator.ProfessionalLicense = dto.ProfessionalLicense ?? string.Empty;
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

        [HttpPost("{id}/photo")]
        public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
        {
            var collaborator = await _context.Collaborators.FindAsync(id);
            if (collaborator == null) return NotFound();

            if (file == null || file.Length == 0)
                return BadRequest("Nenhum arquivo enviado.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Formato de imagem inválido.");

            var uploadFolder = Path.Combine(_environment.ContentRootPath, "Uploads", "Profiles");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            // Remove foto antiga se existir
            if (!string.IsNullOrEmpty(collaborator.ProfilePictureUrl))
            {
                var oldPath = Path.Combine(uploadFolder, collaborator.ProfilePictureUrl);
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            var uniqueFileName = $"{id}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            collaborator.ProfilePictureUrl = uniqueFileName;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Foto atualizada com sucesso.", url = uniqueFileName });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);

            var user = await _context.Collaborators.FindAsync(userId);
            if (user == null) return NotFound();

            if (user.Password != dto.CurrentPassword)
                return BadRequest("Senha atual incorreta.");

            user.Password = dto.NewPassword;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Senha alterada com sucesso." });
        }

        [HttpGet("{id}/photo")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var collaborator = await _context.Collaborators.FindAsync(id);
            if (collaborator == null || string.IsNullOrEmpty(collaborator.ProfilePictureUrl))
                return NotFound();

            var filePath = Path.Combine(_environment.ContentRootPath, "Uploads", "Profiles", collaborator.ProfilePictureUrl);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = extension == ".png" ? "image/png" : 
                              extension == ".gif" ? "image/gif" : "image/jpeg";

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(fileStream, contentType);
        }
    }
}