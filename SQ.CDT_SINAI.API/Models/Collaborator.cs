using System.Collections.Generic;

namespace SQ.CDT_SINAI.API.Models
{
    public class Collaborator
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Em produção, usar Hash!

        // Dados Pessoais
        public string Cpf { get; set; } = string.Empty;
        public string Rg { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;

        // Endereço
        public string AddressStreet { get; set; } = string.Empty;
        public string AddressNumber { get; set; } = string.Empty;
        public string AddressNeighborhood { get; set; } = string.Empty;
        public string AddressCity { get; set; } = string.Empty;
        public string AddressState { get; set; } = string.Empty;
        public string AddressZipCode { get; set; } = string.Empty;

        // Dados Profissionais
        public string ProfessionalLicense { get; set; } = string.Empty; // Ex: CRM, COREN
        public string JobTitle { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }

        public bool Active { get; set; } = true;

        public List<Specialization> Specializations { get; set; } = new();
    }
}