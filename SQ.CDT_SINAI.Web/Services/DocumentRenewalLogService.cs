using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public class DocumentRenewalLogService : IDocumentRenewalLogService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DocumentRenewalLogService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<PaginatedResult<DocumentRenewalLog>> GetLogsAsync(int page = 1, int pageSize = 20, DateTime? startDate = null, DateTime? endDate = null, string? search = null)
        {
            AddAuthorizationHeader();
            var url = $"api/documentrenewallog?page={page}&pageSize={pageSize}";
            
            if (startDate.HasValue) url += $"&startDate={startDate.Value:yyyy-MM-dd}";
            if (endDate.HasValue) url += $"&endDate={endDate.Value:yyyy-MM-dd}";
            if (!string.IsNullOrEmpty(search)) url += $"&search={System.Net.WebUtility.UrlEncode(search)}";

            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro na API ({response.StatusCode}): {error}");
            }
            return await response.Content.ReadFromJsonAsync<PaginatedResult<DocumentRenewalLog>>() ?? new PaginatedResult<DocumentRenewalLog>();
        }

        public async Task<string?> RevertRenewalAsync(int id)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsync($"api/documentrenewallog/revert/{id}", null);
            if (response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsStringAsync();
        }
    }
}