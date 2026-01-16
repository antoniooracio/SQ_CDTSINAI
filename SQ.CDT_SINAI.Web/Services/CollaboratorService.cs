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
    public class CollaboratorService : ICollaboratorService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CollaboratorService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<PaginatedResult<Collaborator>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            AddAuthorizationHeader();
            var url = $"api/collaborator?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(search))
            {
                url += $"&search={System.Net.WebUtility.UrlEncode(search)}";
            }
            return await _httpClient.GetFromJsonAsync<PaginatedResult<Collaborator>>(url) ?? new PaginatedResult<Collaborator>();
        }

        public async Task<Collaborator?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<Collaborator>($"api/collaborator/{id}");
        }

        public async Task<string?> CreateAsync(RegisterCollaboratorDto dto)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/collaborator", dto);
            
            if (response.IsSuccessStatusCode)
                return null;

            var error = await response.Content.ReadAsStringAsync();
            return error.Trim('"');
        }

        public async Task<string?> UpdateAsync(int id, UpdateCollaboratorDto dto)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/collaborator/{id}", dto);
            
            if (response.IsSuccessStatusCode)
                return null;

            var error = await response.Content.ReadAsStringAsync();
            return error.Trim('"');
        }
    }
}