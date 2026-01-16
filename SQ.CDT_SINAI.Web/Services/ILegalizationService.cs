using Microsoft.AspNetCore.Http;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public interface ILegalizationService
    {
        Task<List<DocumentType>> GetDocumentTypesAsync();
        Task<List<EstablishmentDocument>> GetDocumentsByEstablishmentAsync(int establishmentId);
        Task<string?> SaveDocumentAsync(UpdateDocumentStatusDto dto, IFormFile? file);
        Task<(byte[] Content, string FileName)?> DownloadDocumentAsync(int id);
        Task<(byte[] Content, string FileName)?> GetReportAsync(int establishmentId);
        Task<DocumentExpirationStatsDto> GetStatsAsync();
        Task<LegalizationReportResultDto> GetReportDataAsync(LegalizationReportFilterDto filter);
    }
}
