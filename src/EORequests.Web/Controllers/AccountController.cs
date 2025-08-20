using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace EORequests.Web.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        [HttpGet("signin")]
        public IActionResult SignIn(string? returnUrl = "/")
            => Challenge(new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
                         OpenIdConnectDefaults.AuthenticationScheme);

        [HttpGet("signout")]
        public IActionResult SignOutApp()
            => SignOut(new AuthenticationProperties { RedirectUri = "/" },
                       CookieAuthenticationDefaults.AuthenticationScheme,
                       OpenIdConnectDefaults.AuthenticationScheme);
    }
}
