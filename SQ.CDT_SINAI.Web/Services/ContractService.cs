using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public class ContractService : IContractService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContractService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            var baseUrl = configuration["ApiSettings:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl)) _httpClient.BaseAddress = new System.Uri(baseUrl);
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<Contract>> GetByEstablishmentAsync(int establishmentId, ContractType? type = null, string? search = null)
        {
            AddAuthorizationHeader();
            var url = $"api/contract/establishment/{establishmentId}";
            var query = new List<string>();
            if (type.HasValue) query.Add($"type={type.Value}");
            if (!string.IsNullOrEmpty(search)) query.Add($"search={search}");
            
            if (query.Count > 0) url += "?" + string.Join("&", query);
            
            return await _httpClient.GetFromJsonAsync<List<Contract>>(url) ?? new List<Contract>();
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<Contract>($"api/contract/{id}");
        }

        public async Task<string?> CreateAsync(ContractDto dto, IFormFile? file)
        {
            AddAuthorizationHeader();
            using var content = new MultipartFormDataContent();
            
            content.Add(new StringContent(dto.EstablishmentId.ToString()), nameof(dto.EstablishmentId));
            content.Add(new StringContent(dto.ContractNumber), nameof(dto.ContractNumber));
            content.Add(new StringContent(((int)dto.Type!.Value).ToString()), nameof(dto.Type));
            content.Add(new StringContent(dto.VendorName), nameof(dto.VendorName));
            if (!string.IsNullOrEmpty(dto.ObjectDescription)) content.Add(new StringContent(dto.ObjectDescription), nameof(dto.ObjectDescription));
            content.Add(new StringContent(dto.StartDate!.Value.ToString("yyyy-MM-dd")), nameof(dto.StartDate));
            content.Add(new StringContent(dto.EndDate!.Value.ToString("yyyy-MM-dd")), nameof(dto.EndDate));
            content.Add(new StringContent(dto.MonthlyValue!.Value.ToString()), nameof(dto.MonthlyValue));
            content.Add(new StringContent(((int)dto.PaymentFrequency).ToString()), nameof(dto.PaymentFrequency));
            if (dto.InstallmentCount.HasValue) content.Add(new StringContent(dto.InstallmentCount.Value.ToString()), nameof(dto.InstallmentCount));
            if (dto.TotalContractValue.HasValue) content.Add(new StringContent(dto.TotalContractValue.Value.ToString()), nameof(dto.TotalContractValue));
            content.Add(new StringContent(dto.AutomaticRenewal.ToString()), nameof(dto.AutomaticRenewal));
            content.Add(new StringContent((dto.RenewalMonths ?? 0).ToString()), nameof(dto.RenewalMonths));

            if (file != null)
            {
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.FileName);
            }

            var response = await _httpClient.PostAsync("api/contract", content);
            if (response.IsSuccessStatusCode) return null;
            var error = await response.Content.ReadAsStringAsync();
            return error.Trim('"');
        }

        public async Task<string?> UpdateAsync(int id, ContractDto dto, IFormFile? file)
        {
            AddAuthorizationHeader();
            using var content = new MultipartFormDataContent();
            
            content.Add(new StringContent(dto.Id.ToString()), nameof(dto.Id));
            content.Add(new StringContent(dto.EstablishmentId.ToString()), nameof(dto.EstablishmentId));
            content.Add(new StringContent(dto.ContractNumber), nameof(dto.ContractNumber));
            content.Add(new StringContent(((int)dto.Type!.Value).ToString()), nameof(dto.Type));
            content.Add(new StringContent(dto.VendorName), nameof(dto.VendorName));
            if (!string.IsNullOrEmpty(dto.ObjectDescription)) content.Add(new StringContent(dto.ObjectDescription), nameof(dto.ObjectDescription));
            content.Add(new StringContent(dto.StartDate!.Value.ToString("yyyy-MM-dd")), nameof(dto.StartDate));
            content.Add(new StringContent(dto.EndDate!.Value.ToString("yyyy-MM-dd")), nameof(dto.EndDate));
            content.Add(new StringContent(dto.MonthlyValue!.Value.ToString()), nameof(dto.MonthlyValue));
            content.Add(new StringContent(((int)dto.PaymentFrequency).ToString()), nameof(dto.PaymentFrequency));
            if (dto.InstallmentCount.HasValue) content.Add(new StringContent(dto.InstallmentCount.Value.ToString()), nameof(dto.InstallmentCount));
            if (dto.TotalContractValue.HasValue) content.Add(new StringContent(dto.TotalContractValue.Value.ToString()), nameof(dto.TotalContractValue));
            content.Add(new StringContent(dto.AutomaticRenewal.ToString()), nameof(dto.AutomaticRenewal));
            content.Add(new StringContent((dto.RenewalMonths ?? 0).ToString()), nameof(dto.RenewalMonths));
            content.Add(new StringContent(((int)dto.Status).ToString()), nameof(dto.Status));

            if (file != null)
            {
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.FileName);
            }

            var response = await _httpClient.PutAsync($"api/contract/{id}", content);
            if (response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string?> AddAmendmentAsync(ContractAmendmentDto dto, IFormFile? file)
        {
            AddAuthorizationHeader();
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.ContractId.ToString()), nameof(dto.ContractId));
            content.Add(new StringContent(dto.AmendmentNumber), nameof(dto.AmendmentNumber));
            content.Add(new StringContent(((int)dto.Type).ToString()), nameof(dto.Type));
            content.Add(new StringContent(dto.SignatureDate.ToString("yyyy-MM-dd")), nameof(dto.SignatureDate));
            if (dto.NewEndDate.HasValue) content.Add(new StringContent(dto.NewEndDate.Value.ToString("yyyy-MM-dd")), nameof(dto.NewEndDate));
            if (dto.NewMonthlyValue.HasValue) content.Add(new StringContent(dto.NewMonthlyValue.Value.ToString()), nameof(dto.NewMonthlyValue));
            if (!string.IsNullOrEmpty(dto.Description)) content.Add(new StringContent(dto.Description), nameof(dto.Description));

            if (file != null)
            {
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.FileName);
            }

            var response = await _httpClient.PostAsync("api/contract/amendment", content);
            if (response.IsSuccessStatusCode) return null;
            var error = await response.Content.ReadAsStringAsync();
            return error.Trim('"');
        }

        public async Task<(byte[] Content, string FileName)?> DownloadContractAsync(int id)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"api/contract/download/{id}");
            if (response.IsSuccessStatusCode)
            {
                var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "contrato.pdf";
                var content = await response.Content.ReadAsByteArrayAsync();
                return (content, fileName);
            }
            return null;
        }

        public async Task<(byte[] Content, string FileName)?> DownloadAmendmentAsync(int id)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"api/contract/amendment/download/{id}");
            if (response.IsSuccessStatusCode)
            {
                var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "aditivo.pdf";
                var content = await response.Content.ReadAsByteArrayAsync();
                return (content, fileName);
            }
            return null;
        }

        public async Task<ContractExpirationStatsDto> GetStatsAsync()
        {
            AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<ContractExpirationStatsDto>("api/contract/stats") ?? new ContractExpirationStatsDto();
        }

        public async Task<List<ContractAmendment>> GetRenewalHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, string? search = null)
        {
            AddAuthorizationHeader();
            var query = new List<string>();
            if (startDate.HasValue) query.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue) query.Add($"endDate={endDate.Value:yyyy-MM-dd}");
            if (!string.IsNullOrEmpty(search)) query.Add($"search={search}");
            
            var queryString = query.Any() ? "?" + string.Join("&", query) : "";
            return await _httpClient.GetFromJsonAsync<List<ContractAmendment>>($"api/contract/renewal-history{queryString}") ?? new List<ContractAmendment>();
        }

        public async Task<string?> RevertRenewalAsync(int amendmentId)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsync($"api/contract/revert-renewal/{amendmentId}", null);
            if (response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<decimal> GetMonthlyValueStatsAsync()
        {
            AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<decimal>("api/contract/monthly-value");
        }

        public async Task<(byte[] Content, string FileName)?> GetReportAsync(int establishmentId)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"api/contract/report/{establishmentId}");
            if (response.IsSuccessStatusCode)
            {
                var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? $"Relatorio_Contratos_{establishmentId}.pdf";
                var content = await response.Content.ReadAsByteArrayAsync();
                return (content, fileName);
            }
            return null;
        }

        public async Task<ContractReportResultDto> GetReportDataAsync(ContractReportFilterDto filter)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/contract/report-data", filter);
            return await response.Content.ReadFromJsonAsync<ContractReportResultDto>() ?? new ContractReportResultDto();
        }
    }
}