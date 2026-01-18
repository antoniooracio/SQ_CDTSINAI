using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using SQ.CDT_SINAI.Shared.Models;

namespace SQ.CDT_SINAI.Shared.DTOs
{
    public class ContractDto
    {
        public int Id { get; set; }
        public int EstablishmentId { get; set; }
        
        [Required(ErrorMessage = "O número do contrato é obrigatório.")]
        public string ContractNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo de contrato é obrigatório.")]
        public ContractType? Type { get; set; }
        
        [Required(ErrorMessage = "O fornecedor é obrigatório.")]
        public string VendorName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "A descrição do objeto é obrigatória.")]
        public string ObjectDescription { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "A data de início é obrigatória.")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "A data de fim é obrigatória.")]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "O valor mensal é obrigatório.")]
        public decimal? MonthlyValue { get; set; }
        
        public PaymentFrequency PaymentFrequency { get; set; }
        public int? InstallmentCount { get; set; }
        public decimal? TotalContractValue { get; set; }

        public bool AutomaticRenewal { get; set; }
        public int? RenewalMonths { get; set; }
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

    public class ContractReportFilterDto
    {
        public List<int>? EstablishmentIds { get; set; }
        public List<int>? BrandIds { get; set; }
        public string? Regional { get; set; }
        public List<ContractType>? Types { get; set; }
        public List<ContractStatus>? Statuses { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ContractReportResultDto
    {
        public int TotalContractsFound { get; set; }
        public decimal TotalMonthlyValue { get; set; }
        public decimal TotalRecurringValue { get; set; } // Valor Mensal Fixo
        public decimal TotalNonRecurringValue { get; set; } // Valor Total de Contratos Pontuais/Parcelados
        public ContractChartData ContractsByType { get; set; } = new();
        public ContractChartData ContractsByStatus { get; set; } = new();
        public ContractChartData ContractsByRegional { get; set; } = new();
    }

    public class ContractChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<ContractChartDataset> Datasets { get; set; } = new();
    }

    public class ContractChartDataset
    {
        public string Label { get; set; } = string.Empty;
        public List<int> Data { get; set; } = new();
        public string BackgroundColor { get; set; } = string.Empty;
    }
}