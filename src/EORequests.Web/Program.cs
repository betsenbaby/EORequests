using EORequests.Domain.Security;
using EORequests.Infrastructure.Data;
using EORequests.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// --- DbContext ---
var conn = builder.Configuration.GetConnectionString("EORequests");
builder.Services.AddDbContext<EoDbContext>(opt => opt.UseSqlServer(conn));

// --- Serilog bootstrap ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

// --- MVC / Razor / Blazor ---
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// --- HealthChecks ---
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddDbContextCheck<EoDbContext>(name: "db", tags: new[] { "ready" });

// ==============================
// Authentication & Authorization
// ==============================
var provider = builder.Configuration["Authentication:Provider"] ?? "OpenIddict";
var section = provider == "AzureAd"
    ? builder.Configuration.GetSection("Authentication:AzureAd")
    : builder.Configuration.GetSection("Authentication:OIDC");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = ".EORequests.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = section["Authority"];
        options.ClientId = section["ClientId"]!;
        options.ClientSecret = section["ClientSecret"];
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        options.RequireHttpsMetadata = true;

        options.CallbackPath = section["CallbackPath"] ?? "/signin-oidc";

        // Scopes
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        if (provider.Equals("OpenIddict", StringComparison.OrdinalIgnoreCase))
            options.Scope.Add("roles");

        // Claim mapping (resilient for OpenIddict + Azure)
        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Email, "email");
        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Email, "emailaddress"); // OpenIddict
        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Email, "name");         // fallback if only "name" present
        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Upn, "upn");            // Azure
        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Email, "preferred_username"); // Azure
        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Role, "role");
        options.ClaimActions.MapJsonKey("roles", "roles");

        options.GetClaimsFromUserInfoEndpoint = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = section["Authority"],
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };

        // End-session / single logout
        options.Events.OnRedirectToIdentityProviderForSignOut = context =>
        {
            var postLogout = section["PostLogoutRedirectUri"] ?? "/";
            string? endSession;

            if (provider.Equals("AzureAd", StringComparison.OrdinalIgnoreCase))
            {
                endSession = $"{section["Authority"]?.TrimEnd('/')}/logout";
            }
            else
            {
                endSession = section["EndSessionEndpoint"];
            }

            if (!string.IsNullOrWhiteSpace(endSession))
            {
                var uri = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(
                    endSession!, new Dictionary<string, string?>
                    {
                        ["post_logout_redirect_uri"] = postLogout
                    });

                context.Response.Redirect(uri);
                context.HandleResponse();
            }
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IClaimsTransformation, DbRoleClaimsTransformer>();

builder.Services.AddHttpContextAccessor();

// ==============================

var app = builder.Build();


// --- Middleware pipeline ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<EoDbContext>();
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);
    }

}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// --- Endpoints ---
app.MapControllers();
app.MapBlazorHub();
app.MapRazorPages();

static Task WriteHealthJson(HttpContext ctx, HealthReport report)
{
    ctx.Response.ContentType = "application/json";
    var payload = new
    {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration,
        entries = report.Entries.ToDictionary(
            e => e.Key,
            e => new
            {
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration,
                error = e.Value.Exception?.Message,
                data = e.Value.Data
            })
    };
    return ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live"),
    ResponseWriter = WriteHealthJson
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    ResponseWriter = WriteHealthJson
});

app.MapFallbackToPage("/_Host");

app.Run();
