using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SQ.CDT_SINAI.Shared.Models
{
    public class Role
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty; // Ex: Admin, Coordenador, Operacional

        [JsonIgnore]
        public List<Collaborator> Collaborators { get; set; } = new();
    }
}
