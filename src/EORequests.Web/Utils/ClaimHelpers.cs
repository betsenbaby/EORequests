using System.Security.Claims;

namespace EORequests.Web.Utils
{
    public static class ClaimHelpers
    {
        public static string? GetUserEmail(ClaimsPrincipal p)
        {
            var email =
                p.FindFirst(ClaimTypes.Email)?.Value
                ?? p.FindFirst("preferred_username")?.Value   // Azure
                ?? p.FindFirst("upn")?.Value                  // sometimes Azure
                ?? p.Identity?.Name;

            return IsEmailLike(email) ? email : null;
        }

        private static bool IsEmailLike(string? value)
            => !string.IsNullOrWhiteSpace(value) && value.Contains('@') && value.Contains('.');
    }
}
