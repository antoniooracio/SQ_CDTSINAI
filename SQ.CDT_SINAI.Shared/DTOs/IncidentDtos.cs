using System.ComponentModel.DataAnnotations;
using SQ.CDT_SINAI.Shared.Models;

namespace SQ.CDT_SINAI.Shared.DTOs
{
    public class CreateExternalIncidentDto
    {
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Informe o nome do profissional ou área.")]
        public string TargetAreaOrProfessional { get; set; }

        public string? ReporterName { get; set; }
        public string? ReporterContact { get; set; }
        
        [Required]
        public IncidentSeverity Severity { get; set; }

        [Required(ErrorMessage = "O Tipo de Cliente é obrigatório.")]
        public ClientType ClientType { get; set; }

        [Required(ErrorMessage = "O Meio de Contato é obrigatório.")]
        public ContactMethod ContactMethod { get; set; }

        [Required(ErrorMessage = "O Tipo de Ocorrência é obrigatório.")]
        public IncidentCategory Category { get; set; }

        // Novos Campos Opcionais
        public string? ProtocolNumber { get; set; }
        public string? ClientCpf { get; set; }
        public string? Cip { get; set; }
        public string? HealthInsurance { get; set; }
        public string? ClientName { get; set; }
        public string? ClientPhone { get; set; }
        public string? ClientEmail { get; set; }
        public string? ClientAddress { get; set; }
        public string? SecondaryContactName { get; set; }
        public DateTime? ClientBirthDate { get; set; }
        public string? DoctorCrm { get; set; }
        public string? InvolvedRegional { get; set; } // Já existe no interno, adicionando aqui se necessário ou reusando
        public string? DoctorName { get; set; }
        public string? DoctorEmail { get; set; }
        public string? DoctorPhone1 { get; set; }
        public string? DoctorPhone2 { get; set; }
    }

    public class CreateInternalIncidentDto
    {
        [Required]
        public string Description { get; set; }
        [Required]
        public int TargetId { get; set; } // ID do colaborador alvo
        [Required]
        public IncidentSeverity Severity { get; set; }

        [Required(ErrorMessage = "O Tipo de Ocorrência é obrigatório.")]
        public IncidentCategory Category { get; set; }

        // Novos Campos
        public string? InvolvedArea { get; set; }
        
        public string? LocationOrUnit { get; set; }

        [Required(ErrorMessage = "A Área Destinatária é obrigatória.")]
        public string TargetArea { get; set; }

        [Required(ErrorMessage = "A Regional envolvida é obrigatória.")]
        public string InvolvedRegional { get; set; }

        [Required(ErrorMessage = "A Marca envolvida é obrigatória.")]
        public string InvolvedBrand { get; set; }

        // Novos Campos Solicitados
        public string? ClientName { get; set; }
        public string? ProtocolNumber { get; set; }
        public string? ClientCpf { get; set; }
    }
}