using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public interface IEstablishmentService
    {
        Task<PaginatedResult<Establishment>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null);
        Task<Establishment?> GetByIdAsync(int id);
        Task<string?> CreateAsync(EstablishmentDto dto);
        Task<string?> UpdateAsync(int id, EstablishmentDto dto);
    }
}