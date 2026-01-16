using SQ.CDT_SINAI.Shared.DTOs;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public interface IReportService
    {
        Task<ReportResultDto> GetIncidentReportAsync(ReportFilterDto filter);
    }
}