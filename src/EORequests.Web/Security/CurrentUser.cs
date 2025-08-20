using System.Security.Claims;

namespace EORequests.Web.Security
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly ClaimsPrincipal _user;
        public CurrentUser(IHttpContextAccessor http)
            => _user = http.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

        public Guid? TryGetId()
            => Guid.TryParse(_user.FindFirstValue("app_user_id"), out var g) ? g : null;

        public Guid GetIdOrThrow()
            => TryGetId() ?? throw new InvalidOperationException("Missing app_user_id claim. Is the user signed in?");

        public string? Email()
            => _user.FindFirstValue(ClaimTypes.Email) ?? _user.FindFirstValue("preferred_username") ?? _user.FindFirstValue(ClaimTypes.Upn);

        public IReadOnlyCollection<string> Roles()
            => _user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
    }
}
