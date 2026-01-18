using Microsoft.AspNetCore.Http;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public interface IContractService
    {
        Task<List<Contract>> GetByEstablishmentAsync(int establishmentId, ContractType? type = null, string? search = null);
        Task<Contract?> GetByIdAsync(int id);
        Task<string?> CreateAsync(ContractDto dto, IFormFile? file);
        Task<string?> UpdateAsync(int id, ContractDto dto, IFormFile? file);
        Task<string?> AddAmendmentAsync(ContractAmendmentDto dto, IFormFile? file);
        Task<(byte[] Content, string FileName)?> DownloadContractAsync(int id);
        Task<(byte[] Content, string FileName)?> DownloadAmendmentAsync(int id);
        Task<ContractExpirationStatsDto> GetStatsAsync();
        Task<List<ContractAmendment>> GetRenewalHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, string? search = null);
        Task<string?> RevertRenewalAsync(int amendmentId);
        Task<decimal> GetMonthlyValueStatsAsync();
        Task<(byte[] Content, string FileName)?> GetReportAsync(int establishmentId);
        Task<ContractReportResultDto> GetReportDataAsync(ContractReportFilterDto filter);
    }
}