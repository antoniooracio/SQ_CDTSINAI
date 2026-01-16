using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Shared.Models;
using SQ.CDT_SINAI.Web.Models.ViewModels;
using SQ.CDT_SINAI.Web.Services;
using System.Threading.Tasks;
using System;

namespace SQ.CDT_SINAI.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICollaboratorService _collaboratorService;
        private readonly ISpecializationService _specializationService;
        private readonly IIncidentService _incidentService;
        private readonly ILegalizationService _legalizationService;

        public HomeController(ICollaboratorService collaboratorService, ISpecializationService specializationService, IIncidentService incidentService, ILegalizationService legalizationService)
        {
            _collaboratorService = collaboratorService;
            _specializationService = specializationService;
            _incidentService = incidentService;
            _legalizationService = legalizationService;
        }

        public async Task<IActionResult> Index(int page = 1, IncidentStatus status = IncidentStatus.Open, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Verifica se está logado para buscar dados
            if (Request.Cookies["jwt_token"] == null)
                return RedirectToAction("Login", "Account");

            var collaborators = await _collaboratorService.GetAllAsync(1, 1);
            var specializations = await _specializationService.GetAllAsync(1, 1);
            
            // Busca incidentes com o filtro e paginação
            var myIncidents = await _incidentService.GetMyIncidentsAsync(page, 5, status, startDate, endDate);
            var stats = await _incidentService.GetMyIncidentStatisticsAsync();
            var docStats = await _legalizationService.GetStatsAsync();

            var viewModel = new DashboardViewModel
            {
                TotalCollaborators = collaborators.TotalCount,
                TotalSpecializations = specializations.TotalCount,
                Incidents = myIncidents,
                IncidentStats = stats,
                CurrentFilter = status,
                StartDate = startDate,
                EndDate = endDate,
                DocumentStats = docStats
            };

            return View(viewModel);
        }
    }
}