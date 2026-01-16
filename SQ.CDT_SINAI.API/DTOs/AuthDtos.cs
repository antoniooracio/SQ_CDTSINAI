using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SQ.CDT_SINAI.API.Validations;

namespace SQ.CDT_SINAI.API.DTOs
{
    public record LoginDto(string Email, string Password);

    public class RegisterCollaboratorDto
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Digite um e-mail válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [CpfValidation(ErrorMessage = "CPF inválido.")]
        public string Cpf { get; set; }
        public string Rg { get; set; }
        public DateTime BirthDate { get; set; }
        public string PhoneNumber { get; set; }

        public string AddressStreet { get; set; }
        public string AddressNumber { get; set; }
        public string AddressNeighborhood { get; set; }
        public string AddressCity { get; set; }
        public string AddressState { get; set; }
        public string AddressZipCode { get; set; }

        public string ProfessionalLicense { get; set; }
        public string JobTitle { get; set; }
        public DateTime AdmissionDate { get; set; }

        // Lista de IDs das especializações
        public List<int> SpecializationIds { get; set; } = new();
    }

    public class UpdateCollaboratorDto
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Digite um e-mail válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [CpfValidation(ErrorMessage = "CPF inválido.")]
        public string Cpf { get; set; }
        public string Rg { get; set; }
        public DateTime BirthDate { get; set; }
        public string PhoneNumber { get; set; }

        public string AddressStreet { get; set; }
        public string AddressNumber { get; set; }
        public string AddressNeighborhood { get; set; }
        public string AddressCity { get; set; }
        public string AddressState { get; set; }
        public string AddressZipCode { get; set; }

        public string ProfessionalLicense { get; set; }
        public string JobTitle { get; set; }
        public DateTime AdmissionDate { get; set; }
        public bool Active { get; set; }

        public List<int> SpecializationIds { get; set; } = new();
    }
}