using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQ.CDT_SINAI.Shared.Models
{
    public enum IncidentType { External, Internal }
    public enum IncidentSeverity { Low, Medium, High } // Define os 3 tempos
    public enum IncidentStatus { Open, Responded, Closed }

    public enum ClientType
    {
        [Display(Name = "Autoridade Pública")] PublicAuthority,
        [Display(Name = "Célula de Apoio")] SupportCell,
        [Display(Name = "Colaborador")] Collaborator,
        [Display(Name = "Comunidade vizinha")] NeighboringCommunity,
        [Display(Name = "Consultor de negócios")] BusinessConsultant,
        [Display(Name = "Convênio")] HealthInsurance,
        [Display(Name = "Entidades Jurídicas")] LegalEntities,
        [Display(Name = "Fornecedor")] Supplier,
        [Display(Name = "Heart Team")] HeartTeam,
        [Display(Name = "Hospital")] Hospital,
        [Display(Name = "Imprensa")] Press,
        [Display(Name = "Laboratório apoiado")] SupportedLaboratory,
        [Display(Name = "Lumiax")] Lumiax,
        [Display(Name = "Médico")] Doctor,
        [Display(Name = "Médico - Consultório Dasa")] DoctorDasaOffice,
        [Display(Name = "Médico - GSC")] DoctorGSC,
        [Display(Name = "NAV")] NAV,
        [Display(Name = "Orgãos certificadores")] CertifyingBodies,
        [Display(Name = "Paciente")] Patient,
        [Display(Name = "Pesquisa Clinica")] ClinicalResearch,
        [Display(Name = "Profissionais da saúde")] HealthProfessionals,
        [Display(Name = "Projeto Churn")] ChurnProject
    }

    public enum ContactMethod
    {
        [Display(Name = "Caixa de sugestões")] SuggestionBox,
        [Display(Name = "Carta")] Letter,
        [Display(Name = "Célula Informacional")] InformationalCell,
        [Display(Name = "Chat")] Chat,
        [Display(Name = "Colaborador")] Collaborator,
        [Display(Name = "E-mail")] Email,
        [Display(Name = "Facebook")] Facebook,
        [Display(Name = "Fale Conosco")] ContactUs,
        [Display(Name = "Google Meu Negócio")] GoogleMyBusiness,
        [Display(Name = "Instagram")] Instagram,
        [Display(Name = "Midias Sociais")] SocialMedia,
        [Display(Name = "NAV")] NAV,
        [Display(Name = "Pesquisa - NPS")] NPSSurvey,
        [Display(Name = "Portal Livia - Canal Médico")] LiviaPortalDoctorChannel,
        [Display(Name = "Prog. de visitação médica")] MedicalVisitationProg,
        [Display(Name = "Reclame aqui")] ReclameAqui,
        [Display(Name = "Sistema Informatizado")] ComputerizedSystem,
        [Display(Name = "Telefone")] Phone,
        [Display(Name = "WhatsApp")] WhatsApp
    }

    public enum IncidentCategory
    {
        [Display(Name = "Elogio")] Compliment,
        [Display(Name = "Reclamação")] Complaint,
        [Display(Name = "Sugestão")] Suggestion,
        [Display(Name = "Solicitação")] Request, // Mantidos para Externas
        [Display(Name = "Incidente/Evento Adverso")] IncidentAdverseEvent,

        // Novos Tipos Internos
        [Display(Name = "Elogio Interno")] InternalCompliment,
        [Display(Name = "Reclamação Interna")] InternalComplaint,
        [Display(Name = "Sugestão Interna")] InternalSuggestion,
        [Display(Name = "Solicitação Interna")] InternalRequest
    }

    public class Incident
    {
        public int Id { get; set; }
        
        public IncidentType Type { get; set; }
        public IncidentSeverity Severity { get; set; }
        public IncidentStatus Status { get; set; } = IncidentStatus.Open;

        public ClientType ClientType { get; set; }
        public ContactMethod ContactMethod { get; set; }
        public IncidentCategory Category { get; set; }

        // Novos Campos para Ocorrência Interna
        public string? InvolvedArea { get; set; }
        public string? LocationOrUnit { get; set; }
        public string? TargetArea { get; set; } // Área Destinatária (Texto)
        public string? InvolvedRegional { get; set; }
        public string? InvolvedBrand { get; set; }

        // Novos Campos (Interna e Externa)
        public string? ClientName { get; set; }
        public string? ProtocolNumber { get; set; }
        public string? ClientCpf { get; set; }

        // Novos Campos Específicos (Externa/Interna Opcionais)
        public string? Cip { get; set; }
        public string? HealthInsurance { get; set; } // Convênio
        public string? ClientPhone { get; set; }
        public string? ClientEmail { get; set; }
        public string? ClientAddress { get; set; }
        public string? SecondaryContactName { get; set; }
        public DateTime? ClientBirthDate { get; set; }
        public string? DoctorCrm { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorEmail { get; set; }
        public string? DoctorPhone1 { get; set; }
        public string? DoctorPhone2 { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        // Dados do Relator (Quem abriu)
        public string? ReporterName { get; set; } // Para externos (Anônimo ou Nome digitado)
        public string? ReporterContact { get; set; } // Email/Tel para externos
        public int? ReporterId { get; set; } // Para internos (FK)
        [ForeignKey("ReporterId")]
        public Collaborator? Reporter { get; set; }

        // Dados do Alvo (Quem recebe/Gestor/Médico)
        public string? TargetAreaOrProfessional { get; set; } // Texto livre para externos
        public int? TargetId { get; set; } // Para internos (FK)
        [ForeignKey("TargetId")]
        public Collaborator? Target { get; set; }

        // Prazos e Respostas
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime Deadline { get; set; } // Prazo calculado
        
        public string? Response { get; set; }
        public DateTime? ResponseDate { get; set; }
    }
}