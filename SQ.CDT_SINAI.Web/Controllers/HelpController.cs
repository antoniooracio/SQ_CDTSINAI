using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Markdig;
using System.IO;

namespace SQ.CDT_SINAI.Web.Controllers
{
    public class HelpController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public HelpController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            var path = Path.Combine(_env.WebRootPath, "docs", "MANUAL_USUARIO.md");
            if (!System.IO.File.Exists(path))
            {
                return Content("Manual não encontrado. Verifique se o arquivo existe em wwwroot/docs/MANUAL_USUARIO.md");
            }
            
            var markdown = System.IO.File.ReadAllText(path);
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var html = Markdown.ToHtml(markdown, pipeline);
            
            return View((object)html);
        }
    }
}