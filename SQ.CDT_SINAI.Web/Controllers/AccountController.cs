using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Web.Models.ViewModels;
using SQ.CDT_SINAI.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Se já tiver o cookie, redireciona para a Home
            if (Request.Cookies["jwt_token"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authService.LoginAsync(model);

            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                // 1. Cria o Cookie com o Token JWT (para uso nos Services/API)
                Response.Cookies.Append("jwt_token", result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddHours(8),
                    SameSite = SameSiteMode.Strict
                });

                // 2. Cria a Identidade do Usuário no Site (Cookie Auth)
                var claims = ParseClaimsFromJwt(result.Token);
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(10),
                    AllowRefresh = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            _authService.Logout();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);
            
            var claims = new List<Claim>();
            if (keyValuePairs != null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    var value = kvp.Value.ToString();
                    var claimType = kvp.Key;

                    // Mapeamento manual de chaves JWT para ClaimTypes do .NET
                    switch (kvp.Key)
                    {
                        case "role":
                            claimType = ClaimTypes.Role;
                            break;
                        case "unique_name":
                            claimType = ClaimTypes.Name;
                            break;
                        case "email":
                            claimType = ClaimTypes.Email;
                            break;
                    }

                    claims.Add(new Claim(claimType, value));
                }
            }
            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}