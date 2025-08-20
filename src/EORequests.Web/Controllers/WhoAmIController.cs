using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EORequests.Web.Controllers
{
    [Route("whoami")]
    public class WhoAmIController : Controller
    {
        [HttpGet("json")]
        [Authorize] // require login
        public IActionResult GetJson()
        {
            var identity = User.Identity?.IsAuthenticated == true ? "authenticated" : "anonymous";
            var name = User.Identity?.Name ?? "(no name)";
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "(no email)";
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

            return Json(new { identity, name, email, roles });
        }

        [HttpGet("")]
        [Authorize]
        public IActionResult Index()
        {
            var model = new WhoAmIViewModel
            {
                Identity = User.Identity?.IsAuthenticated == true ? "authenticated" : "anonymous",
                Name = User.Identity?.Name ?? "(no name)",
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "(no email)",
                Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray(),
                Claims = User.Claims.Select(c => new ClaimDto(c.Type, c.Value)).ToList()
            };

            return View(model);
        }
    }

    public record ClaimDto(string Type, string Value);

    public class WhoAmIViewModel
    {
        public string Identity { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string[] Roles { get; set; } = Array.Empty<string>();
        public List<ClaimDto> Claims { get; set; } = new();
    }
}
