using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SQ.CDT_SINAI.Web.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Filters
{
    public class PermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string _module;
        private readonly string _action;
        private readonly IPermissionService _permissionService;

        public PermissionFilter(string module, string action, IPermissionService permissionService)
        {
            _module = module;
            _action = action;
            _permissionService = permissionService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // 1. Verifica se o usuário está autenticado
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new ChallengeResult();
                return;
            }

            // 2. Obtém o Role do usuário (do Cookie/Claims)
            var role = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(role))
            {
                context.Result = new ForbidResult();
                return;
            }

            // 3. Verifica a permissão no serviço
            if (!await _permissionService.HasPermissionAsync(role, _module, _action))
            {
                // Se for uma requisição AJAX/API, retorna 403. Se for tela, redireciona para "Acesso Negado".
                if (IsAjaxRequest(context.HttpContext.Request))
                {
                    context.Result = new ForbidResult();
                }
                else
                {
                    // Você pode criar uma View "AccessDenied" no AccountController
                    context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                }
            }
            
            await Task.CompletedTask;
        }

        private bool IsAjaxRequest(Microsoft.AspNetCore.Http.HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}
