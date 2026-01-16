using System;
using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Web.Services;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Controllers
{
    public class DocumentRenewalLogController : Controller
    {
        private readonly IDocumentRenewalLogService _service;

        public DocumentRenewalLogController(IDocumentRenewalLogService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(int page = 1, DateTime? startDate = null, DateTime? endDate = null, string? search = null)
        {
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentSearch"] = search;

            var logs = await _service.GetLogsAsync(page, 20, startDate, endDate, search);
            return View(logs);
        }

        [HttpPost]
        public async Task<IActionResult> Revert(int id)
        {
            var error = await _service.RevertRenewalAsync(id);
            if (error != null)
                TempData["Error"] = error;
            else
                TempData["Success"] = "Renovação revertida com sucesso.";
            
            return RedirectToAction(nameof(Index));
        }
    }
}