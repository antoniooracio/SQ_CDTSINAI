using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Web.Attributes;
using SQ.CDT_SINAI.Web.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Controllers
{
    [RequiresPermission("Role", "Edit")] // Apenas quem pode editar perfis acessa a matriz
    public class PermissionController : Controller
    {
        private readonly IPermissionService _service;

        public PermissionController(IPermissionService service)
        {
            _service = service;
        }

        // Lista de Permissões Disponíveis no Sistema (Linhas da Matriz)
        public static readonly List<(string Module, string Action, string Label)> SystemPermissions = new()
        {
            ("Collaborator", "View", "Colaboradores - Visualizar"),
            ("Collaborator", "Create", "Colaboradores - Criar"),
            ("Collaborator", "Edit", "Colaboradores - Editar"),
            
            ("Role", "View", "Perfis - Visualizar"),
            ("Role", "Create", "Perfis - Criar"),
            ("Role", "Edit", "Perfis - Editar"),
            
            ("Establishment", "View", "Estabelecimentos - Visualizar"),
            ("Establishment", "Create", "Estabelecimentos - Criar"),
            ("Establishment", "Edit", "Estabelecimentos - Editar"),
            
            ("Incident", "View", "Ocorrências - Visualizar"),
            ("Incident", "Create", "Ocorrências - Criar"),
            ("Incident", "Respond", "Ocorrências - Responder"),
            
            ("Legalization", "View", "Legalização - Visualizar"),
            ("Legalization", "Upload", "Legalização - Upload/Editar")
        };

        public async Task<IActionResult> Index()
        {
            var matrix = await _service.GetMatrixAsync();
            ViewBag.SystemPermissions = SystemPermissions;
            return View(matrix);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateRolePermissionsDto model)
        {
            await _service.UpdatePermissionsAsync(model);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Copy(int sourceRoleId, int targetRoleId)
        {
            await _service.CopyPermissionsAsync(sourceRoleId, targetRoleId);
            return RedirectToAction(nameof(Index));
        }
    }
}