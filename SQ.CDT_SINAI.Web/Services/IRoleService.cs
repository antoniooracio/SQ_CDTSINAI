using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public interface IRoleService
    {
        Task<PaginatedResult<Role>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null);
        Task<Role?> GetByIdAsync(int id);
        Task<bool> CreateAsync(Role role);
        Task<bool> UpdateAsync(Role role);
        Task<string?> DeleteAsync(int id);
    }
}