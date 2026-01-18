using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public class LegalizationService : ILegalizationService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LegalizationService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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

        public async Task<List<DocumentType>> GetDocumentTypesAsync()
        {
            AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<List<DocumentType>>("api/legalization/types") ?? new List<DocumentType>();
        }

        public async Task<List<EstablishmentDocument>> GetDocumentsByEstablishmentAsync(int establishmentId)
        {
            AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<List<EstablishmentDocument>>($"api/legalization/establishment/{establishmentId}") ?? new List<EstablishmentDocument>();
        }

        public async Task<string?> SaveDocumentAsync(UpdateDocumentStatusDto dto, IFormFile? file)
        {
            AddAuthorizationHeader();
            using var content = new MultipartFormDataContent();
            
            content.Add(new StringContent(dto.EstablishmentId.ToString()), nameof(dto.EstablishmentId));
            content.Add(new StringContent(dto.DocumentTypeId.ToString()), nameof(dto.DocumentTypeId));
            content.Add(new StringContent(((int)dto.Status).ToString()), nameof(dto.Status));
            
            if (dto.EmissionDate.HasValue) content.Add(new StringContent(dto.EmissionDate.Value.ToString("yyyy-MM-dd")), nameof(dto.EmissionDate));
            if (dto.ExpirationDate.HasValue) content.Add(new StringContent(dto.ExpirationDate.Value.ToString("yyyy-MM-dd")), nameof(dto.ExpirationDate));
            if (!string.IsNullOrEmpty(dto.ProtocolNumber)) content.Add(new StringContent(dto.ProtocolNumber), nameof(dto.ProtocolNumber));
            if (dto.Cost.HasValue) content.Add(new StringContent(dto.Cost.Value.ToString()), nameof(dto.Cost));
            if (!string.IsNullOrEmpty(dto.Justification)) content.Add(new StringContent(dto.Justification), nameof(dto.Justification));
            content.Add(new StringContent(dto.AutomaticRenewal.ToString()), nameof(dto.AutomaticRenewal));
            content.Add(new StringContent(dto.RenewalMonths.ToString()), nameof(dto.RenewalMonths));
            if (!string.IsNullOrEmpty(dto.ContentHtml)) content.Add(new StringContent(dto.ContentHtml), nameof(dto.ContentHtml));
            content.Add(new StringContent(dto.IsPermanent.ToString()), nameof(dto.IsPermanent));
            if (!string.IsNullOrEmpty(dto.Tags)) content.Add(new StringContent(dto.Tags), nameof(dto.Tags));

            if (file != null)
            {
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.FileName);
            }

            var response = await _httpClient.PostAsync("api/legalization/save", content);
            if (response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<(byte[] Content, string FileName)?> DownloadDocumentAsync(int id)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"api/legalization/download/{id}");
            if (response.IsSuccessStatusCode)
            {
                var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "documento";
                var content = await response.Content.ReadAsByteArrayAsync();
                return (content, fileName);
            }
            return null;
        }

        public async Task<(byte[] Content, string FileName)?> GetReportAsync(int establishmentId)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"api/legalization/report/{establishmentId}");
            if (response.IsSuccessStatusCode)
            {
                var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? $"Relatorio_{establishmentId}.pdf";
                var content = await response.Content.ReadAsByteArrayAsync();
                return (content, fileName);
            }
            return null;
        }

        public async Task<DocumentExpirationStatsDto> GetStatsAsync()
        {
            AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<DocumentExpirationStatsDto>("api/legalization/stats") ?? new DocumentExpirationStatsDto();
        }

        public async Task<LegalizationReportResultDto> GetReportDataAsync(LegalizationReportFilterDto filter)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/legalization/report", filter);
            return await response.Content.ReadFromJsonAsync<LegalizationReportResultDto>() ?? new LegalizationReportResultDto();
        }
    }
}