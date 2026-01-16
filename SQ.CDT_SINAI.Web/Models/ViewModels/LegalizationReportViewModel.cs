using SQ.CDT_SINAI.Shared.DTOs;

namespace SQ.CDT_SINAI.Web.Models.ViewModels
{
    public class LegalizationReportViewModel
    {
        public LegalizationReportFilterDto Filter { get; set; } = new();
        public LegalizationReportResultDto? Result { get; set; }
    }
}