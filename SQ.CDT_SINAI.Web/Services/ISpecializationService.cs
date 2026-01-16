using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public interface ISpecializationService
    {
        Task<PaginatedResult<Specialization>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null);
        Task<bool> CreateAsync(Specialization specialization);
        Task<Specialization?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(Specialization specialization);
    }
}