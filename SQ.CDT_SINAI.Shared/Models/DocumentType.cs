using System.ComponentModel.DataAnnotations;

namespace SQ.CDT_SINAI.Shared.Models
{
    public class DocumentType
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O nome do documento é obrigatório.")]
        public string Name { get; set; } = string.Empty; // Ex: Alvará de Funcionamento
        
        public string ResponsibleArea { get; set; } = string.Empty; // Ex: Jurídico, Engenharia
        
        public bool Active { get; set; } = true; // Identifica se o documento está ativo
        public bool IsCompliance { get; set; } // Identifica se é um documento de Compliance Obrigatório

    }
}
