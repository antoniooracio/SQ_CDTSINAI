using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Web.Attributes;
using SQ.CDT_SINAI.Web.Services;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Controllers
{
    public class ContractRenewalLogController : Controller
    {
        private readonly IContractService _contractService;

        public ContractRenewalLogController(IContractService contractService)
        {
            _contractService = contractService;
        }

        [RequiresPermission("ContractRenewalLog", "View")]
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string? search)
        {
            var history = await _contractService.GetRenewalHistoryAsync(startDate, endDate, search);
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["Search"] = search;
            return View(history);
        }

        [HttpPost]
        [RequiresPermission("ContractRenewalLog", "Revert")]
        public async Task<IActionResult> Revert(int id)
        {
            var error = await _contractService.RevertRenewalAsync(id);
            if (error != null)
                TempData["Error"] = error;
            else
                TempData["Success"] = "Renovação revertida com sucesso!";
            
            return RedirectToAction(nameof(Index));
        }
    }
}