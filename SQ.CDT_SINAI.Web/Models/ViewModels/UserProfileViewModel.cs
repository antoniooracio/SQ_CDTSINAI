using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SQ.CDT_SINAI.Web.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? CurrentPhotoUrl { get; set; }

        [Display(Name = "Foto de Perfil")]
        public IFormFile? PhotoUpload { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "A senha atual é obrigatória.")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
        [Compare("NewPassword", ErrorMessage = "As senhas não conferem.")]
        public string? ConfirmNewPassword { get; set; }
    }
}