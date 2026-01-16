using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public interface IDocumentTypeService
    {
        Task<PaginatedResult<DocumentType>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null);
        Task<DocumentType?> GetByIdAsync(int id);
        Task<bool> CreateAsync(DocumentType documentType);
        Task<bool> UpdateAsync(DocumentType documentType);
        Task<string?> DeleteAsync(int id);
    }
}