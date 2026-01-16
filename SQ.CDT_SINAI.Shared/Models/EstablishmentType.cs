using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SQ.CDT_SINAI.Shared.Models
{
    public enum ServiceLocationType
    {
        [Display(Name = "Unidade de Atendimento")] UnidadeAtendimento,
        [Display(Name = "NTH")] NTH,
        [Display(Name = "Outros")] Outros,
        [Display(Name = "Suporte")] Suporte,
        [Display(Name = "Célula de Apoio")] CelulaApoio,
        [Display(Name = "NTO")] NTO,
        [Display(Name = "Armazém")] Armazem,
        [Display(Name = "Unidade de Atendimento com TLR")] TLR,
        [Display(Name = "Coleta Consultório Próprio")] ColetaConsultorioProprio
    }

    public class EstablishmentType
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome do tipo é obrigatório.")]
        public string Name { get; set; } = string.Empty;
        
        public ServiceLocationType ServiceLocationType { get; set; }

        public List<DocumentType> NecessaryDocuments { get; set; } = new();
        public List<DocumentType> ClosingDocuments { get; set; } = new();

        [JsonIgnore]
        public List<Establishment> Establishments { get; set; } = new();
    }
}