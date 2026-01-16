using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQ.CDT_SINAI.Shared.Models
{
    public class DocumentRenewalLog
    {
        public int Id { get; set; }
        
        public int EstablishmentDocumentId { get; set; }
        [ForeignKey("EstablishmentDocumentId")]
        public EstablishmentDocument? EstablishmentDocument { get; set; }

        public DateTime RenewalDate { get; set; } = DateTime.Now;
        public DateTime OldExpirationDate { get; set; }
        public DateTime NewExpirationDate { get; set; }
        
        public string? Details { get; set; }
    }
}