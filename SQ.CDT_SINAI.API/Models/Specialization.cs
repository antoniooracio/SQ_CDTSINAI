using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SQ.CDT_SINAI.API.Models
{
    public class Specialization
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome da especialização é obrigatório.")]
        public string Name { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        
        [JsonIgnore] // Evita ciclo infinito no JSON
        public List<Collaborator> Collaborators { get; set; } = new();
    }
}