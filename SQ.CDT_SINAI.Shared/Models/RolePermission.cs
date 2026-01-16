using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQ.CDT_SINAI.Shared.Models
{
    public class RolePermission
    {
        public int Id { get; set; }
        
        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public Role? Role { get; set; }

        [Required]
        public string Module { get; set; } = string.Empty; // Ex: Collaborator
        
        [Required]
        public string Action { get; set; } = string.Empty; // Ex: Create
    }
}