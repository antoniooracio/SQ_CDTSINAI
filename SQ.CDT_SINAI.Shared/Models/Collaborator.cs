using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQ.CDT_SINAI.Shared.Models
{
    public class Collaborator
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

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
        public string ProfessionalLicense { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }

        public bool Active { get; set; } = true;

        // Controle de Acesso
        public int? RoleId { get; set; }
        [ForeignKey("RoleId")]
        public Role? Role { get; set; }

        public List<Specialization> Specializations { get; set; } = new();
        
        // Escopo de Dados: Quais estabelecimentos ele pode ver
        public List<Establishment> Establishments { get; set; } = new();
    }
}