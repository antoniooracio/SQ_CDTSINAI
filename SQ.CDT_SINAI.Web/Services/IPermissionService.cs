using SQ.CDT_SINAI.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string role, string module, string action);
        Task<List<PermissionMatrixDto>> GetMatrixAsync();
        Task<bool> UpdatePermissionsAsync(UpdateRolePermissionsDto dto);
        Task<bool> CopyPermissionsAsync(int sourceRoleId, int targetRoleId);
    }
}
