using System;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public interface IDocumentRenewalLogService
    {
        Task<PaginatedResult<DocumentRenewalLog>> GetLogsAsync(int page = 1, int pageSize = 20, DateTime? startDate = null, DateTime? endDate = null, string? search = null);
        Task<string?> RevertRenewalAsync(int id);
    }
}