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
    public class EstablishmentTypeService : IEstablishmentTypeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EstablishmentTypeService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<PaginatedResult<EstablishmentType>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            AddAuthorizationHeader();
            var url = $"api/establishmenttype?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(search)) url += $"&search={System.Net.WebUtility.UrlEncode(search)}";
            return await _httpClient.GetFromJsonAsync<PaginatedResult<EstablishmentType>>(url) ?? new PaginatedResult<EstablishmentType>();
        }

        public async Task<EstablishmentType?> GetByIdAsync(int id)
        {
            AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<EstablishmentType>($"api/establishmenttype/{id}");
        }

        public async Task<bool> CreateAsync(EstablishmentTypeDto type)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/establishmenttype", type);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(EstablishmentTypeDto type)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/establishmenttype/{type.Id}", type);
            return response.IsSuccessStatusCode;
        }
    }
}