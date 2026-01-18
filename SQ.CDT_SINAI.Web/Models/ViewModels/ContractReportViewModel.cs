using SQ.CDT_SINAI.Shared.DTOs;

namespace SQ.CDT_SINAI.Web.Models.ViewModels
{
    public class ContractReportViewModel
    {
        public ContractReportFilterDto Filter { get; set; } = new();
        public ContractReportResultDto? Result { get; set; }
    }
}