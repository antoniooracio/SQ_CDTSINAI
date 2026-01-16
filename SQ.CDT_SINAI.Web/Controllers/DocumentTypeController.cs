using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Shared.Models;
using SQ.CDT_SINAI.Web.Services;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Controllers
{
    public class DocumentTypeController : Controller
    {
        private readonly IDocumentTypeService _service;

        public DocumentTypeController(IDocumentTypeService service)
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
        public async Task<IActionResult> Create(DocumentType model)
        {
            if (ModelState.IsValid)
            {
                if (await _service.CreateAsync(model))
                    return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var docType = await _service.GetByIdAsync(id);
            if (docType == null) return NotFound();
            return View(docType);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, DocumentType model)
        {
            if (id != model.Id) return BadRequest();
            if (ModelState.IsValid && await _service.UpdateAsync(model))
                return RedirectToAction(nameof(Index));
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var error = await _service.DeleteAsync(id);
            if (error != null)
                TempData["Error"] = error;
            else
                TempData["Success"] = "Tipo de documento excluído com sucesso.";

            return RedirectToAction(nameof(Index));
        }
    }
}