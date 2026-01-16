using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public interface IEstablishmentTypeService
    {
        Task<PaginatedResult<EstablishmentType>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null);
        Task<EstablishmentType?> GetByIdAsync(int id);
        Task<bool> CreateAsync(EstablishmentTypeDto type);
        Task<bool> UpdateAsync(EstablishmentTypeDto type);
    }
}