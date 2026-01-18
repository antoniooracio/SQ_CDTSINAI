using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using SQ.CDT_SINAI.Web.Attributes;
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
        [RequiresPermission("Contract", "View")]
        public async Task<IActionResult> Index(int establishmentId, ContractType? type, string? search)
        {
            var establishment = await _establishmentService.GetByIdAsync(establishmentId);
            if (establishment == null) return NotFound();

            var contracts = await _contractService.GetByEstablishmentAsync(establishmentId, type, search);
            
            ViewBag.Establishment = establishment;
            ViewData["CurrentType"] = type;
            ViewData["CurrentSearch"] = search;
            return View(contracts);
        }

        // Detalhes do Contrato (Linha do Tempo)
        [RequiresPermission("Contract", "View")]
        public async Task<IActionResult> Details(int id)
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null) return NotFound();
            return View(contract);
        }

        [HttpPost]
        [RequiresPermission("Contract", "Create")]
        public async Task<IActionResult> Create(ContractDto model, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                var error = await _contractService.CreateAsync(model, file);
                if (error == null)
                    return RedirectToAction(nameof(Index), new { establishmentId = model.EstablishmentId });
                
                ModelState.AddModelError(string.Empty, error);
            }
            
            // Se chegou aqui, houve erro. Recarrega a lista para exibir a view Index novamente
            var establishment = await _establishmentService.GetByIdAsync(model.EstablishmentId);
            var contracts = await _contractService.GetByEstablishmentAsync(model.EstablishmentId, model.Type, null);
            
            ViewBag.Establishment = establishment;
            ViewData["CurrentType"] = model.Type;
            ViewData["CurrentSearch"] = null;
            ViewBag.ShowCreateModal = true; // Flag para reabrir o modal
            
            return View(nameof(Index), contracts);
        }

        [RequiresPermission("Contract", "Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null) return NotFound();

            var dto = new ContractDto
            {
                Id = contract.Id,
                EstablishmentId = contract.EstablishmentId,
                ContractNumber = contract.ContractNumber,
                Type = contract.Type,
                VendorName = contract.VendorName,
                ObjectDescription = contract.ObjectDescription,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                MonthlyValue = contract.MonthlyValue,
                PaymentFrequency = contract.PaymentFrequency,
                InstallmentCount = contract.InstallmentCount,
                TotalContractValue = contract.TotalContractValue,
                AutomaticRenewal = contract.AutomaticRenewal,
                RenewalMonths = contract.RenewalMonths,
                Status = contract.Status
            };

            return View(dto);
        }

        [HttpPost]
        [RequiresPermission("Contract", "Edit")]
        public async Task<IActionResult> Edit(int id, ContractDto model, IFormFile? file)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var error = await _contractService.UpdateAsync(id, model, file);
                if (error == null)
                    return RedirectToAction(nameof(Index), new { establishmentId = model.EstablishmentId });
                
                ModelState.AddModelError(string.Empty, error);
            }
            
            return View(model);
        }

        [HttpPost]
        [RequiresPermission("Contract", "Edit")]
        public async Task<IActionResult> AddAmendment(ContractAmendmentDto model, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                var error = await _contractService.AddAmendmentAsync(model, file);
                if (error != null) TempData["Error"] = error;
                else TempData["Success"] = "Aditivo adicionado com sucesso!";
            }
            
            // Se chegou aqui, houve erro ou sucesso com redirect.
            // Se ModelState for inválido, precisamos recarregar a view Details
            var contract = await _contractService.GetByIdAsync(model.ContractId);
            ViewBag.ShowAmendmentModal = true;
            
            return View(nameof(Details), contract);
        }

        [RequiresPermission("Contract", "View")]
        public async Task<IActionResult> Download(int id)
        {
            var result = await _contractService.DownloadContractAsync(id);
            if (result == null) return NotFound();
            return File(result.Value.Content, "application/pdf", result.Value.FileName);
        }

        [RequiresPermission("Contract", "View")]
        public async Task<IActionResult> DownloadAmendment(int id)
        {
            var result = await _contractService.DownloadAmendmentAsync(id);
            if (result == null) return NotFound();
            return File(result.Value.Content, "application/pdf", result.Value.FileName);
        }

        [RequiresPermission("Contract", "View")]
        public async Task<IActionResult> GenerateReport(int establishmentId)
        {
            var result = await _contractService.GetReportAsync(establishmentId);
            if (result == null) return NotFound("Não foi possível gerar o relatório.");
            return File(result.Value.Content, "application/pdf", result.Value.FileName);
        }
    }
}