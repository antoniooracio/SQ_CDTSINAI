using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using SQ.CDT_SINAI.Web.Models.ViewModels;
using SQ.CDT_SINAI.Web.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Controllers
{
    public class EstablishmentController : Controller
    {
        private readonly IEstablishmentService _service;
        private readonly IBrandService _brandService;
        private readonly IEstablishmentTypeService _typeService;
        private readonly ILegalizationService _legalizationService;

        public EstablishmentController(IEstablishmentService service, IBrandService brandService, IEstablishmentTypeService typeService, ILegalizationService legalizationService)
        {
            _service = service;
            _brandService = brandService;
            _typeService = typeService;
            _legalizationService = legalizationService;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            ViewData["CurrentFilter"] = search;
            var list = await _service.GetAllAsync(page, 10, search);
            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = (await _brandService.GetAllAsync(1, 100)).Items;
            ViewBag.Types = (await _typeService.GetAllAsync(1, 100)).Items;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(EstablishmentDto model)
        {
            if (ModelState.IsValid)
            {
                var error = await _service.CreateAsync(model);
                if (error == null) return RedirectToAction(nameof(Index));
                ModelState.AddModelError(string.Empty, error);
            }
            ViewBag.Brands = (await _brandService.GetAllAsync(1, 100)).Items;
            ViewBag.Types = (await _typeService.GetAllAsync(1, 100)).Items;
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var establishment = await _service.GetByIdAsync(id);
            if (establishment == null) return NotFound();

            var dto = new EstablishmentDto
            {
                Id = establishment.Id,
                Name = establishment.Name,
                Address = establishment.Address,
                Neighborhood = establishment.Neighborhood,
                City = establishment.City,
                State = establishment.State,
                ZipCode = establishment.ZipCode,
                Regional = establishment.Regional,
                BrandIds = establishment.Brands.Select(b => b.Id).ToList(),
                EstablishmentTypeIds = establishment.EstablishmentTypes.Select(t => t.Id).ToList()
            };

            ViewBag.Brands = (await _brandService.GetAllAsync(1, 100)).Items;
            ViewBag.Types = (await _typeService.GetAllAsync(1, 100)).Items;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EstablishmentDto model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var error = await _service.UpdateAsync(id, model);
                if (error == null) return RedirectToAction(nameof(Index));
                ModelState.AddModelError(string.Empty, error);
            }
            ViewBag.Brands = (await _brandService.GetAllAsync(1, 100)).Items;
            ViewBag.Types = (await _typeService.GetAllAsync(1, 100)).Items;
            return View(model);
        }

        public async Task<IActionResult> Legalization(int id, int page = 1, string? search = null, DocumentStatus? statusFilter = null)
        {
            var establishment = await _service.GetByIdAsync(id);
            if (establishment == null) return NotFound();

            var allTypes = await _legalizationService.GetDocumentTypesAsync();
            var docs = await _legalizationService.GetDocumentsByEstablishmentAsync(id);

            // Lógica de Junção e Filtro em Memória
            var query = allTypes.Select(type => {
                var doc = docs.FirstOrDefault(d => d.DocumentTypeId == type.Id);
                var status = doc?.Status ?? DocumentStatus.Pending;
                
                // Recalcula status visual (Vencido por data)
                var isExpired = status == DocumentStatus.Expired || (doc?.ExpirationDate.HasValue == true && doc.ExpirationDate < System.DateTime.Today);
                var effectiveStatus = isExpired ? DocumentStatus.Expired : status;

                return new { Type = type, Status = effectiveStatus };
            });

            // Filtro de Texto
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Type.Name.Contains(search, System.StringComparison.OrdinalIgnoreCase) || 
                                         x.Type.ResponsibleArea.Contains(search, System.StringComparison.OrdinalIgnoreCase));
            }

            // Filtro de Status
            if (statusFilter.HasValue)
            {
                query = query.Where(x => x.Status == statusFilter.Value);
            }

            var filteredList = query.ToList();
            var totalCount = filteredList.Count;
            var pageSize = 10;
            var pagedItems = filteredList.Skip((page - 1) * pageSize).Take(pageSize).Select(x => x.Type).ToList();

            var paginatedTypes = new PaginatedResult<DocumentType>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };

            var viewModel = new EstablishmentLegalizationViewModel
            {
                Establishment = establishment,
                DocumentTypes = paginatedTypes,
                ExistingDocuments = docs
            };

            ViewData["CurrentSearch"] = search;
            ViewData["CurrentStatus"] = statusFilter;

            return View(viewModel);
        }

        [HttpPost]
        // Permite envio de HTML gigante (Base64) no formulário
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = 209715200)]
        public async Task<IActionResult> UploadDocument(UpdateDocumentStatusDto dto, IFormFile? file)
        {
            var error = await _legalizationService.SaveDocumentAsync(dto, file);
            if (error != null)
                TempData["Error"] = error;
            else
                TempData["Success"] = "Documento atualizado com sucesso!";

            return RedirectToAction(nameof(Legalization), new { id = dto.EstablishmentId });
        }

        public async Task<IActionResult> DownloadDocument(int id)
        {
            var result = await _legalizationService.DownloadDocumentAsync(id);
            if (result == null) return NotFound();
            return File(result.Value.Content, "application/octet-stream", result.Value.FileName);
        }

        public async Task<IActionResult> GenerateReport(int id)
        {
            var result = await _legalizationService.GetReportAsync(id);
            if (result == null) return NotFound("Não foi possível gerar o relatório.");
            return File(result.Value.Content, "application/pdf", result.Value.FileName);
        }
    }
}