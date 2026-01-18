using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SQ.CDT_SINAI.Web.Services
{
    public interface ICollaboratorService
    {
        Task<PaginatedResult<Collaborator>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null);
        Task<Collaborator?> GetByIdAsync(int id);
        Task<string?> CreateAsync(RegisterCollaboratorDto dto);
        Task<string?> UpdateAsync(int id, UpdateCollaboratorDto dto);
        Task<string?> UpdatePhotoAsync(int id, IFormFile file);
        Task<string?> ChangePasswordAsync(ChangePasswordDto dto);
        Task<(byte[] Content, string ContentType)?> GetPhotoAsync(int id);
    }
}