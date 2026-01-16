using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQ.CDT_SINAI.API.Data;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace SQ.CDT_SINAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PermissionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckPermission([FromQuery] string role, [FromQuery] string module, [FromQuery] string action)
        {
            if (role == "Administrador") return Ok(true);

            var hasPermission = await _context.RolePermissions
                .AnyAsync(p => p.Role.Name == role && p.Module == module && p.Action == action);

            return Ok(hasPermission);
        }

        [HttpGet("matrix")]
        public async Task<ActionResult<List<PermissionMatrixDto>>> GetMatrix()
        {
            try
            {
                var roles = await _context.Roles.ToListAsync();
                var permissions = await _context.RolePermissions.ToListAsync();

                var matrix = roles.Select(r => new PermissionMatrixDto
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    Permissions = permissions.Where(p => p.RoleId == r.Id)
                                             .Select(p => $"{p.Module}.{p.Action}")
                                             .ToList()
                }).ToList();

                return matrix;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao buscar matriz: {ex.Message}");
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdatePermissions(UpdateRolePermissionsDto dto)
        {
            // Remove permissões existentes do perfil
            var existing = await _context.RolePermissions.Where(p => p.RoleId == dto.RoleId).ToListAsync();
            _context.RolePermissions.RemoveRange(existing);

            // Adiciona as novas
            foreach (var permString in dto.Permissions)
            {
                var parts = permString.Split('.');
                if (parts.Length == 2)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = dto.RoleId,
                        Module = parts[0],
                        Action = parts[1]
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("copy")]
        public async Task<IActionResult> CopyPermissions([FromQuery] int sourceRoleId, [FromQuery] int targetRoleId)
        {
            if (sourceRoleId == targetRoleId) return BadRequest("Origem e destino devem ser diferentes.");

            var sourcePermissions = await _context.RolePermissions
                .Where(p => p.RoleId == sourceRoleId)
                .ToListAsync();

            // Remove permissões existentes do destino
            var targetExisting = await _context.RolePermissions
                .Where(p => p.RoleId == targetRoleId)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(targetExisting);

            // Adiciona as novas
            foreach (var perm in sourcePermissions)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = targetRoleId,
                    Module = perm.Module,
                    Action = perm.Action
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}