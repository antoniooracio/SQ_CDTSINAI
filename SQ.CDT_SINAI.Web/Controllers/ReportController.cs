using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Web.Services;
using SQ.CDT_SINAI.Web.Models.ViewModels;
using System;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ILegalizationService _legalizationService;
        private readonly IEstablishmentService _establishmentService;
        private readonly IBrandService _brandService;

        public ReportController(IReportService reportService, ILegalizationService legalizationService, IEstablishmentService establishmentService, IBrandService brandService)
        {
            _reportService = reportService;
            _legalizationService = legalizationService;
            _establishmentService = establishmentService;
            _brandService = brandService;
        }

        public IActionResult Index()
        {
            // Default last 30 days
            return View(new ReportFilterDto { StartDate = DateTime.Today.AddDays(-30), EndDate = DateTime.Today });
        }

        [HttpPost]
        public async Task<IActionResult> Display(ReportFilterDto filter)
        {
            var data = await _reportService.GetIncidentReportAsync(filter);
            ViewBag.Filter = filter;
            return View(data);
        }

        public async Task<IActionResult> Legalization()
        {
            ViewBag.Establishments = (await _establishmentService.GetAllAsync(1, 1000)).Items;
            ViewBag.Brands = (await _brandService.GetAllAsync(1, 1000)).Items;
            
            return View(new LegalizationReportViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Legalization(LegalizationReportViewModel model)
        {
            var data = await _legalizationService.GetReportDataAsync(model.Filter);
            model.Result = data;

            ViewBag.Establishments = (await _establishmentService.GetAllAsync(1, 1000)).Items;
            ViewBag.Brands = (await _brandService.GetAllAsync(1, 1000)).Items;

            return View(model);
        }
    }
}