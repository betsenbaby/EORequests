using EORequests.Application.Interfaces;
using EORequests.Domain.Entities;
using EORequests.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EORequests.Infrastructure.Services
{
    public class FormService : IFormService
    {
        private readonly EoDbContext _db;
        private readonly ILogger<FormService> _log;

        public FormService(EoDbContext db, ILogger<FormService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task<string?> GetSchemaAsync(Guid workflowStateId, CancellationToken ct = default)
        {
            try
            {
                return await _db.WorkflowStates
                    .Where(s => s.Id == workflowStateId)
                    .Select(s => s.StepTemplate.JsonSchema)
                    .FirstOrDefaultAsync(ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to load schema for state {StateId}", workflowStateId);
                throw;
            }
        }

        public async Task<FormResponse?> GetResponseAsync(Guid workflowStateId, CancellationToken ct = default)
        {
            try
            {
                return await _db.FormResponses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.WorkflowStateId == workflowStateId, ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to load FormResponse for state {StateId}", workflowStateId);
                throw;
            }
        }

        public async Task<FormResponse> UpsertResponseAsync(
            Guid workflowStateId,
            string jsonData,
            string? summary = null,
            CancellationToken ct = default)
        {
            try
            {
                // Track existing row (1–1 with WorkflowState)
                var resp = await _db.FormResponses
                    .SingleOrDefaultAsync(r => r.WorkflowStateId == workflowStateId, ct);

                if (resp is null)
                {
                    // Capture the schema version active for this step now (freeze on first save)
                    var schemaVersion = await _db.WorkflowStates
                        .Where(s => s.Id == workflowStateId)
                        .Select(s => s.StepTemplate.JsonSchemaVersion)
                        .FirstOrDefaultAsync(ct) ?? "v1";

                    resp = new FormResponse
                    {
                        WorkflowStateId = workflowStateId,
                        JsonData = jsonData,
                        Summary = summary,
                        SchemaVersionCaptured = schemaVersion
                    };

                    _db.FormResponses.Add(resp);
                }
                else
                {
                    // Update payload; keep the originally captured version for compatibility
                    resp.JsonData = jsonData;
                    if (summary is not null)
                        resp.Summary = summary;

                    // If you want to refresh to current schema version instead, uncomment:
                    // resp.SchemaVersionCaptured = await _db.WorkflowStates
                    //     .Where(s => s.Id == workflowStateId)
                    //     .Select(s => s.StepTemplate.JsonSchemaVersion)
                    //     .FirstOrDefaultAsync(ct) ?? resp.SchemaVersionCaptured;
                }

                await _db.SaveChangesAsync(ct);

                _log.LogInformation(
                    "FormResponse saved for state {StateId} (schemaVersion={SchemaVersion})",
                    workflowStateId, resp.SchemaVersionCaptured ?? "n/a");

                return resp;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to save FormResponse for state {StateId}", workflowStateId);
                throw;
            }
        }
    }
}
