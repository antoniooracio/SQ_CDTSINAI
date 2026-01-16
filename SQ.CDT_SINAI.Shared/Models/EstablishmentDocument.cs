using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQ.CDT_SINAI.Shared.Models
{
    public enum DocumentStatus
    {
        Pending = 0,    // Pendente
        Active = 1,     // Emitido/Ativo
        Protocol = 2,   // Protocolo
        Provisional = 3,// Provisório
        Exempt = 4,     // Isento
        Expired = 5     // Vencido (Geralmente calculado, mas pode ser um status forçado)
    }

    public class EstablishmentDocument
    {
        public int Id { get; set; }

        public int EstablishmentId { get; set; }
        [ForeignKey("EstablishmentId")]
        public Establishment? Establishment { get; set; }

        public int DocumentTypeId { get; set; }
        [ForeignKey("DocumentTypeId")]
        public DocumentType? DocumentType { get; set; }

        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

        public DateTime? EmissionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        
        public string? ProtocolNumber { get; set; }
        public decimal? Cost { get; set; }
        
        public string? FilePath { get; set; } // Caminho do arquivo salvo no servidor
        public string? FileName { get; set; } // Nome original do arquivo

        public string? Justification { get; set; } // Para casos de Isento

        public bool AutomaticRenewal { get; set; }
        public int RenewalMonths { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
