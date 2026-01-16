using SQ.CDT_SINAI.Web.Models.DTOs;
using SQ.CDT_SINAI.Web.Models.ViewModels;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginViewModel model);
    }
}