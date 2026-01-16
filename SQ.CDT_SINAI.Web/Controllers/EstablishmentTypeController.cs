using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using SQ.CDT_SINAI.Web.Services;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Controllers
{
    public class EstablishmentTypeController : Controller
    {
        private readonly IEstablishmentTypeService _service;
        private readonly IDocumentTypeService _documentService;

        public EstablishmentTypeController(IEstablishmentTypeService service, IDocumentTypeService documentService)
        {
            _service = service;
            _documentService = documentService;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            ViewData["CurrentFilter"] = search;
            var list = await _service.GetAllAsync(page, 10, search);
            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.DocumentTypes = (await _documentService.GetAllAsync(1, 1000)).Items;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(EstablishmentTypeDto model)
        {
            if (ModelState.IsValid)
                if (await _service.CreateAsync(model))
                    return RedirectToAction(nameof(Index));
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var type = await _service.GetByIdAsync(id);
            if (type == null) return NotFound();

            var dto = new EstablishmentTypeDto
            {
                Id = type.Id,
                Name = type.Name,
                ServiceLocationType = type.ServiceLocationType,
                NecessaryDocumentIds = type.NecessaryDocuments.Select(d => d.Id).ToList(),
                ClosingDocumentIds = type.ClosingDocuments.Select(d => d.Id).ToList()
            };
            ViewBag.DocumentTypes = (await _documentService.GetAllAsync(1, 1000)).Items;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EstablishmentTypeDto model)
        {
            if (id != model.Id) return BadRequest();
            if (ModelState.IsValid && await _service.UpdateAsync(model))
                return RedirectToAction(nameof(Index));
            return View(model);
        }
    }
}