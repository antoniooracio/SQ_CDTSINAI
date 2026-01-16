using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Shared.Models;
using SQ.CDT_SINAI.Web.Services;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Controllers
{
    public class RoleController : Controller
    {
        private readonly IRoleService _service;

        public RoleController(IRoleService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            ViewData["CurrentFilter"] = search;
            var list = await _service.GetAllAsync(page, 10, search);
            return View(list);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Role model)
        {
            if (ModelState.IsValid)
                if (await _service.CreateAsync(model))
                    return RedirectToAction(nameof(Index));
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var role = await _service.GetByIdAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Role model)
        {
            if (id != model.Id) return BadRequest();
            if (ModelState.IsValid && await _service.UpdateAsync(model))
                return RedirectToAction(nameof(Index));
            return View(model);
        }

        // Implementar Delete se necessário
    }
}