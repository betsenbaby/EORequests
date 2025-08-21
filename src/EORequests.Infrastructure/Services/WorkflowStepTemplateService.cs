using EORequests.Application.Interfaces;
using EORequests.Domain.Entities;
using EORequests.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Infrastructure.Services
{
    public class WorkflowStepTemplateService : IWorkflowStepTemplateService
    {
        private readonly EoDbContext _db;
        private readonly ILogger<WorkflowStepTemplateService> _log;

        public WorkflowStepTemplateService(EoDbContext db, ILogger<WorkflowStepTemplateService> log)
        {
            _db = db; _log = log;
        }

        public async Task<IReadOnlyList<(WorkflowStepTemplate Step, string TemplateName)>> GetAllAsync(CancellationToken ct = default)
        {
            var rows = await _db.WorkflowStepTemplates
                .AsNoTracking()
                .Include(s => s.WorkflowTemplate)
                .OrderBy(s => s.WorkflowTemplate!.Name)
                .ThenBy(s => s.StepOrder)
                .Select(s => new { Step = s, TemplateName = s.WorkflowTemplate!.Name! })
                .ToListAsync(ct);

            return rows.Select(x => (x.Step, x.TemplateName)).ToList();
        }

        public async Task<string?> GetSchemaAsync(Guid stepTemplateId, CancellationToken ct = default)
        {
            return await _db.WorkflowStepTemplates
                .Where(s => s.Id == stepTemplateId)
                .Select(s => s.JsonSchema)
                .FirstOrDefaultAsync(ct);
        }

        public async Task UpdateSchemaAsync(Guid stepTemplateId, string? jsonSchema, CancellationToken ct = default)
        {
            var step = await _db.WorkflowStepTemplates.FirstOrDefaultAsync(s => s.Id == stepTemplateId, ct);
            if (step is null)
                throw new KeyNotFoundException($"WorkflowStepTemplate not found: {stepTemplateId}");

            step.JsonSchema = jsonSchema;
            await _db.SaveChangesAsync(ct);

            _log.LogInformation("Updated JsonSchema for step {StepId}", stepTemplateId);
        }
    }
}
