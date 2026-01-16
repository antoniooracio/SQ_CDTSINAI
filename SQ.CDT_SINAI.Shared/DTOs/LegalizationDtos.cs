using SQ.CDT_SINAI.Shared.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SQ.CDT_SINAI.Shared.DTOs
{
    public class EstablishmentDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Name { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Regional { get; set; } = string.Empty;

        // Listas de IDs para seleção no formulário
        public List<int> BrandIds { get; set; } = new();
        public List<int> EstablishmentTypeIds { get; set; } = new();
    }

    public class EstablishmentTypeDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome do tipo é obrigatório.")]
        public string Name { get; set; } = string.Empty;
        public ServiceLocationType ServiceLocationType { get; set; }
        public List<int> NecessaryDocumentIds { get; set; } = new();
        public List<int> ClosingDocumentIds { get; set; } = new();
    }

    public class DocumentExpirationStatsDto
    {
        public int Expired { get; set; }
        public int ExpiresIn5Days { get; set; }
        public int ExpiresIn15Days { get; set; }
        public int ExpiresIn30Days { get; set; }
    }
}