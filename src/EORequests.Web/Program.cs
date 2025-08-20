using EORequests.Application.Interfaces;
using EORequests.Domain.Enums;
using EORequests.Domain.Security;
using EORequests.Infrastructure.Data;
using EORequests.Infrastructure.External;
using EORequests.Infrastructure.Services;
using EORequests.Web.Security;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// Logging (Serilog)
// ---------------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

// ---------------------------
// Data
// ---------------------------
var conn = builder.Configuration.GetConnectionString("EORequests");
builder.Services.AddDbContext<EoDbContext>(opt => opt.UseSqlServer(conn));

// ---------------------------
// Hangfire (SQL Server storage)
// ---------------------------
builder.Services.AddHangfire(cfg =>
{
    cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings()
       .UseSqlServerStorage(conn, new SqlServerStorageOptions
       {
           PrepareSchemaIfNecessary = true,
           QueuePollInterval = TimeSpan.FromSeconds(15)
       });
});
builder.Services.AddHangfireServer();

// ---------------------------
// MVC / Razor / Blazor
// ---------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// ---------------------------
// HealthChecks
// ---------------------------
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddDbContextCheck<EoDbContext>(name: "db", tags: new[] { "ready" });

// ---------------------------
// Authentication & Authorization
// ---------------------------
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

        // Claim mapping
        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Email, "email");
        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Email, "emailaddress");
        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Email, "name");
        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Upn, "upn");
        options.ClaimActions.MapUniqueJsonKey(ClaimTypes.Email, "preferred_username");
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

        // Single logout / end-session
        options.Events.OnRedirectToIdentityProviderForSignOut = context =>
        {
            var postLogout = section["PostLogoutRedirectUri"] ?? "/";
            string? endSession = provider.Equals("AzureAd", StringComparison.OrdinalIgnoreCase)
                ? $"{section["Authority"]?.TrimEnd('/')}/logout"
                : section["EndSessionEndpoint"];

            if (!string.IsNullOrWhiteSpace(endSession))
            {
                var uri = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(
                    endSession!, new Dictionary<string, string?> { ["post_logout_redirect_uri"] = postLogout });
                context.Response.Redirect(uri);
                context.HandleResponse();
            }
            return Task.CompletedTask;
        };

        options.Events.OnTokenValidated = async context =>
        {
            // resolve services
            var sp = context.HttpContext.RequestServices;
            var db = sp.GetRequiredService<EoDbContext>();

            var principal = context.Principal!;
            var id = (ClaimsIdentity)principal.Identity!;

            // Extract email + names from claims (works with OpenIddict + Azure AD)
            string? email =
                principal.FindFirstValue(ClaimTypes.Email)
                ?? principal.FindFirstValue("preferred_username")
                ?? principal.FindFirstValue(ClaimTypes.Upn);

            string? givenName =
                principal.FindFirstValue(ClaimTypes.GivenName)
                ?? principal.FindFirstValue("given_name");
            string? familyName =
                principal.FindFirstValue(ClaimTypes.Surname)
                ?? principal.FindFirstValue("family_name");

            if (string.IsNullOrWhiteSpace(email))
            {
                // If email is missing, you can decide to block sign-in or proceed without app_user_id.
                // Here we just return; downstream code should handle missing app_user_id defensively.
                return;
            }

            email = email.Trim();

            // Find or create the ApplicationUser row
            var user = await db.ApplicationUsers
                .AsTracking()
                .SingleOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    Email = email,
                    FirstName = givenName ?? "",
                    LastName = familyName ?? "",
                    DisplayName = $"{givenName} {familyName}".Trim(),
                    IsActive = true
                };
                db.ApplicationUsers.Add(user);
                await db.SaveChangesAsync();
            }
            else
            {
                // Optional: keep names/display in sync with IdP
                var changed = false;
                var display = $"{givenName} {familyName}".Trim();

                if (!string.IsNullOrWhiteSpace(givenName) && user.FirstName != givenName) { user.FirstName = givenName; changed = true; }
                if (!string.IsNullOrWhiteSpace(familyName) && user.LastName != familyName) { user.LastName = familyName; changed = true; }
                if (!string.IsNullOrWhiteSpace(display) && user.DisplayName != display) { user.DisplayName = display; changed = true; }

                if (changed) await db.SaveChangesAsync();
            }

            // Stamp the app user id into the auth cookie
            if (!id.HasClaim(c => c.Type == "app_user_id"))
            {
                id.AddClaim(new Claim("app_user_id", user.Id.ToString()));
            }
        };
    });



builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IClaimsTransformation, DbRoleClaimsTransformer>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddScoped<IWorkflowEngine, WorkflowEngine>();
builder.Services.AddSingleton<IBranchRuleEvaluator, BranchRuleEvaluator>();
builder.Services.AddSingleton<IDomainEventDispatcher, LoggingEventDispatcher>();
builder.Services.AddScoped<ISlaService, SlaService>();
builder.Services.AddScoped<ISlaJobRunner, SlaJobRunner>();

// ---------------------------


// Access control service
builder.Services.AddScoped<IAccessControlService, AccessControlService>();

// Authorization policies for common actions
builder.Services.AddSingleton<IAuthorizationHandler, StepActionAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("Step_View", p => p.AddRequirements(new StepActionRequirement(StepAction.View)));
    options.AddPolicy("Step_Act", p => p.AddRequirements(new StepActionRequirement(StepAction.Act)));
    options.AddPolicy("Step_Upload", p => p.AddRequirements(new StepActionRequirement(StepAction.UploadAttachment)));
    options.AddPolicy("Step_Comment", p => p.AddRequirements(new StepActionRequirement(StepAction.Comment)));
    options.AddPolicy("Step_CreateTask", p => p.AddRequirements(new StepActionRequirement(StepAction.CreateTask)));
    options.AddPolicy("Step_Assign", p => p.AddRequirements(new StepActionRequirement(StepAction.AssignStep)));
});

// ---------------------------
// External APIs (Personnel)
// ---------------------------
builder.Services.Configure<PersonnelApiOptions>(
    builder.Configuration.GetSection(PersonnelApiOptions.SectionName));

builder.Services.AddHttpClient<IExternalUserSyncService, ExternalUserSyncService>((sp, http) =>
{
    var opts = sp.GetRequiredService<IOptions<PersonnelApiOptions>>().Value;
    http.BaseAddress = new Uri(opts.BaseUrl, UriKind.Absolute); // e.g. https://eoportal.unece.org/Personnel/
    http.DefaultRequestHeaders.Add("X-Api-Key", opts.ApiKey);
    http.Timeout = TimeSpan.FromSeconds(60);
});

// ===========================
var app = builder.Build();
// ===========================

// ---------------------------
// DEV bootstrap (migrate + seed + one-off sync)
// ---------------------------
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EoDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);

    var sync = scope.ServiceProvider.GetRequiredService<IExternalUserSyncService>();
    await sync.SyncUsersAsync();
}

// ---------------------------
// Middleware pipeline
// ---------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Controllers / Blazor / Razor
app.MapControllers();
app.MapBlazorHub();
app.MapRazorPages();

// Health checks
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


// ---------------------------
// Hangfire dashboard (Admin only)
// ---------------------------
// Hangfire dashboard (requires Hangfire.AspNetCore 1.8+)
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthorizationPolicy("AdminOnly") }
});


// ---------------------------
// Recurring jobs (idempotent)
// ---------------------------
// Run the external personnel sync every day at 04:00 (server time)
RecurringJob.AddOrUpdate<IExternalUserSyncService>(
    "UpdateSyncPersonnelJob",
    s => s.SyncUsersAsync(System.Threading.CancellationToken.None),
    "0 4 * * *");


app.MapFallbackToPage("/_Host");
app.Run();

// ===========================
// Helpers
// ===========================
static async Task WriteHealthJson(HttpContext ctx, HealthReport report)
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
    await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
}



// Hangfire dashboard policy adapter
public sealed class HangfireDashboardAuthorizationPolicy : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    private readonly string _policyName;
    public HangfireDashboardAuthorizationPolicy(string policyName) => _policyName = policyName;

    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        var http = context.GetHttpContext();
        var authService = http.RequestServices.GetRequiredService<IAuthorizationService>();
        var result = authService.AuthorizeAsync(http.User, _policyName).GetAwaiter().GetResult();
        return result.Succeeded;
    }
}
