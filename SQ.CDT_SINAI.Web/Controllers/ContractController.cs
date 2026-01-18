using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Web.Services;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Controllers
{
    public class ContractController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IEstablishmentService _establishmentService;

        public ContractController(IContractService contractService, IEstablishmentService establishmentService)
        {
            _contractService = contractService;
            _establishmentService = establishmentService;
        }

        // Lista de Contratos de um Estabelecimento
        public async Task<IActionResult> Index(int establishmentId)
        {
            var establishment = await _establishmentService.GetByIdAsync(establishmentId);
            if (establishment == null) return NotFound();

            var contracts = await _contractService.GetByEstablishmentAsync(establishmentId);
            
            ViewBag.Establishment = establishment;
            return View(contracts);
        }

        // Detalhes do Contrato (Linha do Tempo)
        public async Task<IActionResult> Details(int id)
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null) return NotFound();
            return View(contract);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ContractDto model, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                var error = await _contractService.CreateAsync(model, file);
                if (error == null)
                    return RedirectToAction(nameof(Index), new { establishmentId = model.EstablishmentId });
                
                TempData["Error"] = error;
            }
            else
            {
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["Error"] = $"Não foi possível salvar: {errors}";
            }
            return RedirectToAction(nameof(Index), new { establishmentId = model.EstablishmentId });
        }

        [HttpPost]
        public async Task<IActionResult> AddAmendment(ContractAmendmentDto model, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                var error = await _contractService.AddAmendmentAsync(model, file);
                if (error != null) TempData["Error"] = error;
                else TempData["Success"] = "Aditivo adicionado com sucesso!";
            }
            else
            {
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["Error"] = $"Não foi possível salvar o aditivo: {errors}";
            }
            return RedirectToAction(nameof(Details), new { id = model.ContractId });
        }

        public async Task<IActionResult> Download(int id)
        {
            var result = await _contractService.DownloadContractAsync(id);
            if (result == null) return NotFound();
            return File(result.Value.Content, "application/pdf", result.Value.FileName);
        }

        public async Task<IActionResult> DownloadAmendment(int id)
        {
            var result = await _contractService.DownloadAmendmentAsync(id);
            if (result == null) return NotFound();
            return File(result.Value.Content, "application/pdf", result.Value.FileName);
        }
    }
}