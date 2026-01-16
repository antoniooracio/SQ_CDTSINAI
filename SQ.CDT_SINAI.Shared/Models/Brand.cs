using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SQ.CDT_SINAI.Shared.Models
{
    public class Brand
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome da marca é obrigatório.")]
        public string Name { get; set; } = string.Empty;
        
        [JsonIgnore]
        public List<Establishment> Establishments { get; set; } = new();
    }
}