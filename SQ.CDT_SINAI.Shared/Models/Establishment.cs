using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SQ.CDT_SINAI.Shared.Models
{
    public class Establishment
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Name { get; set; } = string.Empty;
        
        public string Cnpj { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        
        public string Address { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty; // Município
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Regional { get; set; } = string.Empty;

        // Relacionamentos (Muitos-para-Muitos)
        public List<Brand> Brands { get; set; } = new();
        public List<EstablishmentType> EstablishmentTypes { get; set; } = new();
        public List<EstablishmentDocument> Documents { get; set; } = new();
        
        // Colaboradores que têm acesso a este estabelecimento
        [JsonIgnore]
        public List<Collaborator> Collaborators { get; set; } = new();
    }
}