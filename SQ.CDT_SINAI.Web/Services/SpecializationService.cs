using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public class SpecializationService : ISpecializationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SpecializationService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<PaginatedResult<Specialization>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            AddAuthorizationHeader();
            var url = $"api/specialization?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(search))
            {
                url += $"&search={System.Net.WebUtility.UrlEncode(search)}";
            }
            return await _httpClient.GetFromJsonAsync<PaginatedResult<Specialization>>(url) ?? new PaginatedResult<Specialization>();
        }

        public async Task<bool> CreateAsync(Specialization specialization)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/specialization", specialization);
            return response.IsSuccessStatusCode;
        }

        public async Task<Specialization?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<Specialization>($"api/specialization/{id}");
        }

        public async Task<bool> UpdateAsync(Specialization specialization)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/specialization/{specialization.Id}", specialization);
            return response.IsSuccessStatusCode;
        }
    }
}