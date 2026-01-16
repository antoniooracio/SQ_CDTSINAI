using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using SQ.CDT_SINAI.Web.Services;
using System;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Controllers
{
    public class IncidentController : Controller
    {
        private readonly IIncidentService _incidentService;
        private readonly ICollaboratorService _collaboratorService;

        public IncidentController(IIncidentService incidentService, ICollaboratorService collaboratorService)
        {
            _incidentService = incidentService;
            _collaboratorService = collaboratorService;
        }

        public async Task<IActionResult> Create()
        {
            // Carrega colaboradores para o dropdown de "Alvo"
            var collaborators = await _collaboratorService.GetAllAsync(1, 1000);
            ViewBag.Collaborators = collaborators.Items;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateInternalIncidentDto model)
        {
            if (ModelState.IsValid)
            {
                var error = await _incidentService.CreateInternalAsync(model);
                if (error == null)
                    return RedirectToAction("Index", "Home"); // Volta para o Dashboard
                
                ModelState.AddModelError(string.Empty, error);
            }
            
            var collaborators = await _collaboratorService.GetAllAsync(1, 1000);
            ViewBag.Collaborators = collaborators.Items;
            return View(model);
        }

        public async Task<IActionResult> Respond(int id)
        {
            var incident = await _incidentService.GetByIdAsync(id);
            if (incident == null) return NotFound();
            return View(incident);
        }

        [HttpPost]
        public async Task<IActionResult> Respond(int id, string response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                ModelState.AddModelError("", "A resposta é obrigatória.");
                var incident = await _incidentService.GetByIdAsync(id);
                return View(incident);
            }

            var error = await _incidentService.RespondAsync(id, response);
            if (error == null)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError(string.Empty, error);
            var incidentReload = await _incidentService.GetByIdAsync(id);
            return View(incidentReload);
        }

        public async Task<IActionResult> ExternalIndex(int page = 1, IncidentStatus? status = null, DateTime? startDate = null, DateTime? endDate = null, string? search = null)
        {
            var result = await _incidentService.GetExternalIncidentsAsync(page, 10, status, startDate, endDate, search);
            
            // Passa os filtros para a View manter o estado
            ViewData["Status"] = status;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["Search"] = search;
            
            return View(result);
        }

        public async Task<IActionResult> Assign(int id)
        {
            var incident = await _incidentService.GetByIdAsync(id);
            if (incident == null) return NotFound();
            
            var collaborators = await _collaboratorService.GetAllAsync(1, 1000);
            ViewBag.Collaborators = collaborators.Items;
            
            return View(incident);
        }

        [HttpPost]
        public async Task<IActionResult> Assign(int id, int targetId)
        {
            var error = await _incidentService.AssignAsync(id, targetId);
            if (error == null)
                return RedirectToAction(nameof(ExternalIndex));

            ModelState.AddModelError(string.Empty, error);
            // Recarrega dados em caso de erro
            return await Assign(id);
        }
    }
}