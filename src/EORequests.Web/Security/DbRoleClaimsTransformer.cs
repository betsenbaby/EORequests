using System.Security.Claims;
using EORequests.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EORequests.Web.Security;

public class DbRoleClaimsTransformer : IClaimsTransformation
{
    private readonly EoDbContext _db;
    private readonly IMemoryCache _cache;

    public DbRoleClaimsTransformer(EoDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true) return principal;
        if (principal.Identity is not ClaimsIdentity identity) return principal;

        // Robust email extraction
        var email =
            principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("preferred_username")?.Value   // Azure
            ?? principal.FindFirst("upn")?.Value                  // sometimes Azure
            ?? principal.Identity.Name;

        if (string.IsNullOrWhiteSpace(email)) return principal;

        var cacheKey = $"roles:{email}";
        if (!_cache.TryGetValue(cacheKey, out string[]? roles))
        {
            roles = await _db.ApplicationUsers
                .Where(u => u.Email == email)
                .SelectMany(u => u.ApplicationUserRoles.Select(ur => ur.Role.Name))
                .Distinct()
                .ToArrayAsync();

            _cache.Set(cacheKey, roles, TimeSpan.FromMinutes(15));
        }

        roles ??= Array.Empty<string>();

        foreach (var role in roles)
        {
            if (!string.IsNullOrWhiteSpace(role) && !identity.HasClaim(ClaimTypes.Role, role))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }

        return principal;
    }
}
