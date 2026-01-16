using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public interface IIncidentService
    {
        Task<string?> CreateInternalAsync(CreateInternalIncidentDto dto);
        Task<PaginatedResult<Incident>> GetMyIncidentsAsync(int page = 1, int pageSize = 5, IncidentStatus? status = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<PaginatedResult<Incident>> GetExternalIncidentsAsync(int page = 1, int pageSize = 10, IncidentStatus? status = null, DateTime? startDate = null, DateTime? endDate = null, string? search = null);
        Task<Dictionary<string, int>> GetMyIncidentStatisticsAsync();
        Task<Incident?> GetByIdAsync(int id);
        Task<string?> RespondAsync(int id, string response);
        Task<string?> AssignAsync(int id, int targetId);
    }
}