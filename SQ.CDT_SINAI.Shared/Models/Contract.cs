using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQ.CDT_SINAI.Shared.Models
{
    public enum ContractStatus
    {
        Active = 1,     // Vigente
        Expired = 2,    // Vencido
        Canceled = 3,   // Rescindido/Cancelado
        Draft = 0       // Rascunho
    }

    public enum ContractType
    {
        [Display(Name = "Serviço")]
        Service = 1,
        [Display(Name = "Locação")]
        Lease = 2,
        [Display(Name = "Manutenção")]
        Maintenance = 3,
        [Display(Name = "Outros")]
        Other = 99
    }

    public enum PaymentFrequency
    {
        [Display(Name = "Mensal (Recorrente)")]
        Monthly = 1,
        [Display(Name = "Pagamento Único")]
        Single = 2,
        [Display(Name = "Parcelado")]
        Installments = 3
    }

    public enum AmendmentType
    {
        TermExtension = 1,  // Prorrogação de Prazo
        ValueAdjustment = 2,// Reajuste de Valor
        ScopeChange = 3,    // Alteração de Escopo
        Other = 99          // Outros
    }

    public class Contract
    {
        public int Id { get; set; }

        public int EstablishmentId { get; set; }
        [ForeignKey("EstablishmentId")]
        public Establishment? Establishment { get; set; }

        [Required]
        public string ContractNumber { get; set; } = string.Empty; // Ex: 123/2024

        public ContractType Type { get; set; } = ContractType.Service;

        [Required]
        public string VendorName { get; set; } = string.Empty; // Fornecedor

        public string ObjectDescription { get; set; } = string.Empty; // O que é o contrato?

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } // Data Fim Atual (pode ser atualizada por aditivos)

        public decimal MonthlyValue { get; set; } // Valor da Parcela ou Valor Recorrente
        
        public PaymentFrequency PaymentFrequency { get; set; } = PaymentFrequency.Monthly;
        public int? InstallmentCount { get; set; } // Quantidade de Parcelas (se aplicável)
        public decimal? TotalContractValue { get; set; } // Valor Total do Contrato (se aplicável)

        public bool AutomaticRenewal { get; set; }
        public int RenewalMonths { get; set; }

        public ContractStatus Status { get; set; } = ContractStatus.Active;

        public string? FilePath { get; set; } // Arquivo do Contrato Original
        public string? FileName { get; set; }

        public List<ContractAmendment> Amendments { get; set; } = new();
    }

    public class ContractAmendment
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        [ForeignKey("ContractId")]
        public Contract? Contract { get; set; }

        public string AmendmentNumber { get; set; } = string.Empty; // Ex: 1º Termo Aditivo
        public AmendmentType Type { get; set; }
        public DateTime SignatureDate { get; set; }

        // Campos que podem ser alterados (opcionais, pois nem todo aditivo muda tudo)
        public DateTime? NewEndDate { get; set; }
        public decimal? NewMonthlyValue { get; set; }

        public string Description { get; set; } = string.Empty; // Justificativa
        
        public string? FilePath { get; set; } // PDF do Aditivo
        public string? FileName { get; set; }
    }
}