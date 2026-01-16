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
    public class IncidentService : IIncidentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IncidentService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                _httpClient.BaseAddress = new System.Uri(baseUrl);
            }
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<string?> CreateInternalAsync(CreateInternalIncidentDto dto)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/incident/internal", dto);
            
            if (response.IsSuccessStatusCode)
                return null;

            var error = await response.Content.ReadAsStringAsync();
            return error.Trim('"');
        }

        public async Task<PaginatedResult<Incident>> GetMyIncidentsAsync(int page = 1, int pageSize = 5, IncidentStatus? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            AddAuthorizationHeader();
            var url = $"api/incident/my-incidents?page={page}&pageSize={pageSize}";
            if (status.HasValue)
                url += $"&status={(int)status.Value}";
            if (startDate.HasValue)
                url += $"&startDate={startDate.Value:yyyy-MM-dd}";
            if (endDate.HasValue)
                url += $"&endDate={endDate.Value:yyyy-MM-dd}";
                
            return await _httpClient.GetFromJsonAsync<PaginatedResult<Incident>>(url) ?? new PaginatedResult<Incident>();
        }

        public async Task<PaginatedResult<Incident>> GetExternalIncidentsAsync(int page = 1, int pageSize = 10, IncidentStatus? status = null, DateTime? startDate = null, DateTime? endDate = null, string? search = null)
        {
            AddAuthorizationHeader();
            var url = $"api/incident/external?page={page}&pageSize={pageSize}";
            if (status.HasValue)
                url += $"&status={(int)status.Value}";
            if (startDate.HasValue)
                url += $"&startDate={startDate.Value:yyyy-MM-dd}";
            if (endDate.HasValue)
                url += $"&endDate={endDate.Value:yyyy-MM-dd}";
            if (!string.IsNullOrEmpty(search))
                url += $"&search={System.Net.WebUtility.UrlEncode(search)}";
                
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro na API ({response.StatusCode}): {error}");
            }
            return await response.Content.ReadFromJsonAsync<PaginatedResult<Incident>>() ?? new PaginatedResult<Incident>();
        }

        public async Task<Dictionary<string, int>> GetMyIncidentStatisticsAsync()
        {
            AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<Dictionary<string, int>>("api/incident/my-stats") ?? new Dictionary<string, int>();
        }

        public async Task<Incident?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<Incident>($"api/incident/{id}");
        }

        public async Task<string?> RespondAsync(int id, string response)
        {
            AddAuthorizationHeader();
            var res = await _httpClient.PutAsJsonAsync($"api/incident/{id}/respond", response);
            
            if (res.IsSuccessStatusCode) return null;
            
            return await res.Content.ReadAsStringAsync();
        }

        public async Task<string?> AssignAsync(int id, int targetId)
        {
            AddAuthorizationHeader();
            var res = await _httpClient.PutAsJsonAsync($"api/incident/{id}/assign", targetId);
            
            if (res.IsSuccessStatusCode) return null;
            return await res.Content.ReadAsStringAsync();
        }
    }
}