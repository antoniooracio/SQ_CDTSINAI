using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SQ.CDT_SINAI.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;

        public PermissionService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            var baseUrl = configuration["ApiSettings:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl)) _httpClient.BaseAddress = new System.Uri(baseUrl);
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<bool> HasPermissionAsync(string role, string module, string action)
        {
            if (role == "Administrador") return true;

            // Verifica Cache
            var cacheKey = $"perm_{role}_{module}_{action}";
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                return cachedResult;
            }

            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"api/permission/check?role={role}&module={module}&action={action}");
            
            bool result = false;
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadFromJsonAsync<bool>();
            }

            // Salva no Cache por 10 minutos
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

            return result;
        }

        public async Task<List<PermissionMatrixDto>> GetMatrixAsync()
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync("api/permission/matrix");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro na API ({response.StatusCode}): {error}");
            }
            return await response.Content.ReadFromJsonAsync<List<PermissionMatrixDto>>() ?? new List<PermissionMatrixDto>();
        }

        public async Task<bool> UpdatePermissionsAsync(UpdateRolePermissionsDto dto)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/permission/update", dto);
            
            // Se atualizou, idealmente deveríamos limpar o cache, mas como a chave inclui o Role, 
            // e o cache é curto (10min), podemos deixar expirar ou implementar limpeza complexa depois.
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CopyPermissionsAsync(int sourceRoleId, int targetRoleId)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsync($"api/permission/copy?sourceRoleId={sourceRoleId}&targetRoleId={targetRoleId}", null);
            return response.IsSuccessStatusCode;
        }
    }
}
