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
    public class DocumentTypeService : IDocumentTypeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DocumentTypeService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<PaginatedResult<DocumentType>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            AddAuthorizationHeader();
            var url = $"api/documenttype?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(search)) url += $"&search={System.Net.WebUtility.UrlEncode(search)}";
            return await _httpClient.GetFromJsonAsync<PaginatedResult<DocumentType>>(url) ?? new PaginatedResult<DocumentType>();
        }

        public async Task<DocumentType?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<DocumentType>($"api/documenttype/{id}");
        }

        public async Task<bool> CreateAsync(DocumentType documentType)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/documenttype", documentType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(DocumentType documentType)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/documenttype/{documentType.Id}", documentType);
            return response.IsSuccessStatusCode;
        }

        public async Task<string?> DeleteAsync(int id)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"api/documenttype/{id}");
            
            if (response.IsSuccessStatusCode) return null;

            var error = await response.Content.ReadAsStringAsync();
            return error.Trim('"');
        }
    }
}