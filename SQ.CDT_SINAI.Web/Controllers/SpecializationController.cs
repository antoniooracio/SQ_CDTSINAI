using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Shared.Models;
using SQ.CDT_SINAI.Web.Services;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Controllers
{
    public class SpecializationController : Controller
    {
        private readonly ISpecializationService _service;

        public SpecializationController(ISpecializationService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            ViewData["CurrentFilter"] = search;
            var list = await _service.GetAllAsync(page, 10, search);
            return View(list);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Specialization model)
        {
            if (ModelState.IsValid)
                if (await _service.CreateAsync(model))
                    return RedirectToAction(nameof(Index));
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var spec = await _service.GetByIdAsync(id);
            if (spec == null) return NotFound();
            return View(spec);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Specialization model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                if (await _service.UpdateAsync(model))
                    return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}