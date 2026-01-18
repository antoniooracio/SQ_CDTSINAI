using System;
using System.ComponentModel.DataAnnotations;
using SQ.CDT_SINAI.Shared.Models;

namespace SQ.CDT_SINAI.Shared.DTOs
{
    public class ContractDto
    {
        public int Id { get; set; }
        public int EstablishmentId { get; set; }
        
        [Required(ErrorMessage = "O número do contrato é obrigatório")]
        public string ContractNumber { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O fornecedor é obrigatório")]
        public string VendorName { get; set; } = string.Empty;
        
        public string ObjectDescription { get; set; } = string.Empty;
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyValue { get; set; }
        public bool AutomaticRenewal { get; set; }
        public int RenewalMonths { get; set; }
        public ContractStatus Status { get; set; }
    }

    public class ContractAmendmentDto
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        
        [Required]
        public string AmendmentNumber { get; set; } = string.Empty;
        public AmendmentType Type { get; set; }
        public DateTime SignatureDate { get; set; }
        public DateTime? NewEndDate { get; set; }
        public decimal? NewMonthlyValue { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class ContractExpirationStatsDto
    {
        public int Expired { get; set; }
        public int ExpiresIn5Days { get; set; }
        public int ExpiresIn15Days { get; set; }
        public int ExpiresIn30Days { get; set; }
    }
}