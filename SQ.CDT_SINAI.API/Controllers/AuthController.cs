using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SQ.CDT_SINAI.API.Data;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SQ.CDT_SINAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCollaboratorDto dto)
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
                RoleId = dto.RoleId > 0 ? dto.RoleId : 3, // Padrão: Colaborador (3)
                Establishments = establishments,
                
                Cpf = dto.Cpf,
                Rg = dto.Rg,
                BirthDate = dto.BirthDate,
                PhoneNumber = dto.PhoneNumber,
                AddressStreet = dto.AddressStreet,
                AddressNumber = dto.AddressNumber,
                AddressNeighborhood = dto.AddressNeighborhood,
                AddressCity = dto.AddressCity,
                AddressState = dto.AddressState,
                AddressZipCode = dto.AddressZipCode,
                ProfessionalLicense = dto.ProfessionalLicense,
                JobTitle = dto.JobTitle,
                AdmissionDate = dto.AdmissionDate
            };

            _context.Collaborators.Add(collaborator);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Colaborador cadastrado com sucesso!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Collaborators
                .Include(c => c.Specializations)
                .Include(c => c.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Password == dto.Password);

            if (user == null || !user.Active)
                return Unauthorized("Email ou senha inválidos.");

            var token = GenerateJwtToken(user);
            return Ok(new { token, user = new { user.Name, user.Email, user.Specializations } });
        }

        private string GenerateJwtToken(Collaborator user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role?.Name ?? "Colaborador") // Adiciona o Perfil ao Token
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}