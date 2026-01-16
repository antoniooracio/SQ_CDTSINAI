using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public interface IBrandService
    {
        Task<PaginatedResult<Brand>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null);
        Task<Brand?> GetByIdAsync(int id);
        Task<bool> CreateAsync(Brand brand);
        Task<bool> UpdateAsync(Brand brand);
    }
}