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
    public class RoleService : IRoleService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoleService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<PaginatedResult<Role>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            AddAuthorizationHeader();
            var url = $"api/role?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(search)) url += $"&search={System.Net.WebUtility.UrlEncode(search)}";
            return await _httpClient.GetFromJsonAsync<PaginatedResult<Role>>(url) ?? new PaginatedResult<Role>();
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<Role>($"api/role/{id}");
        }

        public async Task<bool> CreateAsync(Role role)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/role", role);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(Role role)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/role/{role.Id}", role);
            return response.IsSuccessStatusCode;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"api/role/{id}");
            if (response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsStringAsync();
        }
    }
}