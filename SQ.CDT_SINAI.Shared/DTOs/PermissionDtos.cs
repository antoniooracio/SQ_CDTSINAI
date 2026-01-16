using System.Collections.Generic;

namespace SQ.CDT_SINAI.Shared.DTOs
{
    public class PermissionMatrixDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new(); // Formato: "Module.Action"
    }

    public class UpdateRolePermissionsDto
    {
        public int RoleId { get; set; }
        public List<string> Permissions { get; set; } = new();
    }
}