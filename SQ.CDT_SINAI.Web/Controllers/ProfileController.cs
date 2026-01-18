using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Web.Models.ViewModels;
using SQ.CDT_SINAI.Web.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ICollaboratorService _collaboratorService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(ICollaboratorService collaboratorService, ILogger<ProfileController> logger)
        {
            _collaboratorService = collaboratorService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) 
                {
                    _logger.LogWarning("Tentativa de acesso ao perfil sem Claim de ID.");
                    return RedirectToAction("Login", "Account");
                }
                
                var userId = int.Parse(userIdClaim.Value);
                _logger.LogInformation($"Buscando dados do usuário ID: {userId}");

                var user = await _collaboratorService.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogError($"Usuário ID {userId} retornou NULL da API.");
                    return NotFound("Usuário não encontrado no sistema.");
                }

                var model = new UserProfileViewModel
                {
                    Name = user?.Name ?? User.Identity?.Name ?? "Usuário",
                    Email = user?.Email ?? User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                    Role = user?.Role?.Name ?? User.FindFirst(ClaimTypes.Role)?.Value ?? "",
                    CurrentPhotoUrl = user?.ProfilePictureUrl
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar perfil do usuário.");
                // Retorna o erro na tela para visualizarmos o que está acontecendo
                return Content($"ERRO NO PERFIL: {ex.Message}\n\nSTACK TRACE:\n{ex.StackTrace}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePhoto(UserProfileViewModel model)
        {
            if (model.PhotoUpload == null)
            {
                TempData["Error"] = "Selecione uma imagem para enviar.";
                return RedirectToAction("Index");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var error = await _collaboratorService.UpdatePhotoAsync(userId, model.PhotoUpload);

            if (error != null)
                TempData["Error"] = error;
            else
                TempData["Success"] = "Foto atualizada com sucesso!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var dto = new ChangePasswordDto
            {
                CurrentPassword = model.CurrentPassword,
                NewPassword = model.NewPassword
            };

            var error = await _collaboratorService.ChangePasswordAsync(dto);

            if (error != null)
            {
                if (error.Contains("Senha atual"))
                {
                    ModelState.AddModelError("CurrentPassword", "A senha atual está incorreta.");
                }
                else
                {
                    TempData["Error"] = error;
                }
                return View("Index", model);
            }

            TempData["Success"] = "Senha alterada com sucesso!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetAvatar()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    var userId = int.Parse(userIdClaim.Value);
                    var result = await _collaboratorService.GetPhotoAsync(userId);
                    
                    if (result != null)
                    {
                        return File(result.Value.Content, result.Value.ContentType);
                    }
                }
            }
            catch
            {
                // Em caso de erro na API, retorna imagem padrão
            }
            // Retorna imagem padrão se não tiver foto personalizada ou erro
            return Redirect("/img/default-profile.png"); 
        }
    }
}