using System.Net.Http.Json;
using EORequests.Application.Interfaces;
using EORequests.Domain.Security;
using EORequests.Infrastructure.Data;
using EORequests.Infrastructure.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EORequests.Infrastructure.Services;

public class ExternalUserSyncService : IExternalUserSyncService
{
    private readonly HttpClient _http;
    private readonly EoDbContext _db;
    private readonly ILogger<ExternalUserSyncService> _log;

    // With Option A, BaseUrl is "https://eoportal.unece.org/Personnel/"
    private const string EndpointPath = "api/DataApi/GetPortalUsers";

    public ExternalUserSyncService(HttpClient http, EoDbContext db, ILogger<ExternalUserSyncService> log)
    {
        _http = http;
        _db = db;
        _log = log;
    }

    public async Task<int> SyncUsersAsync(CancellationToken ct = default)
    {
        _log.LogInformation("User sync: calling Personnel API at {Base}{Path}", _http.BaseAddress, EndpointPath);

        using var resp = await _http.GetAsync(EndpointPath, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
        {
            _log.LogError("Personnel API failed: {Status}. Payload: {Payload}", resp.StatusCode, raw);
            throw new HttpRequestException($"Personnel API failed: {resp.StatusCode}");
        }

        List<ExternalUserModel>? externalUsers;
        try
        {
            externalUsers = System.Text.Json.JsonSerializer.Deserialize<List<ExternalUserModel>>(raw,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to parse Personnel API JSON. Raw: {Payload}", raw);
            throw;
        }

        externalUsers ??= new List<ExternalUserModel>();
        _log.LogInformation("User sync: received {Count} external users.", externalUsers.Count);

        // Normalize and pre-index
        foreach (var u in externalUsers)
        {
            u.Email = (u.Email ?? string.Empty).Trim();
            u.FirstName = (u.FirstName ?? string.Empty).Trim();
            u.LastName = (u.LastName ?? string.Empty).Trim();
            u.IndexNumber = u.IndexNumber?.Trim();
        }

        var externalIds = externalUsers.Select(u => u.Id).ToHashSet();

        // Load existing users tracked
        var existing = await _db.ApplicationUsers.AsTracking().ToDictionaryAsync(u => u.Id, ct);

        int added = 0, updated = 0, deactivated = 0, reactivated = 0;

        // Upsert pass
        foreach (var ext in externalUsers)
        {
            if (!existing.TryGetValue(ext.Id, out var user))
            {
                // New local record using external GUID (overrides NEWSEQUENTIALID default — OK)
                user = new ApplicationUser
                {
                    Id = ext.Id,
                    Email = ext.Email,
                    FirstName = ext.FirstName,
                    LastName = ext.LastName,
                    DisplayName = $"{ext.FirstName} {ext.LastName}".Trim(),
                    IndexNumber = ext.IndexNumber,
                    IsActive = ext.Active
                };
                _db.ApplicationUsers.Add(user);
                added++;
            }
            else
            {
                var changed = false;

                if (!string.Equals(user.Email, ext.Email, StringComparison.OrdinalIgnoreCase))
                { user.Email = ext.Email; changed = true; }

                if (!string.Equals(user.FirstName, ext.FirstName, StringComparison.Ordinal))
                { user.FirstName = ext.FirstName; changed = true; }

                if (!string.Equals(user.LastName, ext.LastName, StringComparison.Ordinal))
                { user.LastName = ext.LastName; changed = true; }

                var displayName = $"{ext.FirstName} {ext.LastName}".Trim();
                if (!string.Equals(user.DisplayName, displayName, StringComparison.Ordinal))
                { user.DisplayName = displayName; changed = true; }

                if (!string.Equals(user.IndexNumber, ext.IndexNumber, StringComparison.Ordinal))
                { user.IndexNumber = ext.IndexNumber; changed = true; }

                if (user.IsActive != ext.Active)
                {
                    user.IsActive = ext.Active;
                    changed = true;
                    if (ext.Active) reactivated++;
                }

                if (changed) updated++;
            }
        }

        // Deactivate pass — users missing from API
        foreach (var u in existing.Values)
        {
            if (!externalIds.Contains(u.Id) && u.IsActive)
            {
                u.IsActive = false;
                deactivated++;
            }
        }

        await _db.SaveChangesAsync(ct);
        _log.LogInformation("User sync done: {Added} added, {Updated} updated, {Reactivated} reactivated, {Deactivated} deactivated.",
            added, updated, reactivated, deactivated);

        return added + updated + reactivated + deactivated;
    }
}
