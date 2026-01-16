using System;
using System.ComponentModel.DataAnnotations;
using SQ.CDT_SINAI.Shared.Models;

namespace SQ.CDT_SINAI.Shared.DTOs
{
    public class UpdateDocumentStatusDto
    {
        public int EstablishmentId { get; set; }
        public int DocumentTypeId { get; set; }
        
        [Required]
        public DocumentStatus Status { get; set; }
        
        public DateTime? EmissionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? ProtocolNumber { get; set; }
        public decimal? Cost { get; set; }
        public string? Justification { get; set; }

        public bool AutomaticRenewal { get; set; }
        public int RenewalMonths { get; set; }
        
        // Nota: O arquivo (IFormFile) será tratado separadamente na API ou via MultipartFormDataContent
        // pois DTOs compartilhados geralmente não devem depender de bibliotecas HTTP/Web.
    }
}
