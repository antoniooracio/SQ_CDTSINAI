using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Web.Models.DTOs;
using SQ.CDT_SINAI.Web.Models.ViewModels;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace SQ.CDT_SINAI.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                _httpClient.BaseAddress = new Uri(baseUrl);
            }
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginViewModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { model.Email, model.Password });

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                }
            }
            catch (Exception)
            {
                // Em produção, logar o erro aqui
            }

            return null;
        }

        public async Task<string?> RegisterAsync(RegisterCollaboratorDto model)
        {
            // Adiciona o Token no Header (caso o cadastro exija autenticação futuramente)
            AddAuthorizationHeader();

            var response = await _httpClient.PostAsJsonAsync("api/auth/register", model);
            
            if (response.IsSuccessStatusCode)
                return null;

            var error = await response.Content.ReadAsStringAsync();
            // Remove aspas extras que a API pode retornar em strings JSON
            return error.Trim('"');
        }

        public void Logout()
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete("jwt_token");
        }

        // Método auxiliar para recuperar o Cookie e anexar ao HttpClient
        private void AddAuthorizationHeader()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                var token = context.Request.Cookies["jwt_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }
    }
}