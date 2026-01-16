using Microsoft.Extensions.Configuration;
using SQ.CDT_SINAI.Web.Models.DTOs;
using SQ.CDT_SINAI.Web.Models.ViewModels;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            
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
    }
}