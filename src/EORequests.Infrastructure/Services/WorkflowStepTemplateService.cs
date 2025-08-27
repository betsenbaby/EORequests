using EORequests.Application.Interfaces;
using EORequests.Domain.Entities;
using EORequests.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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


        public async Task<IReadOnlyList<StepListItem>> ListForTemplateAsync(Guid workflowTemplateId, CancellationToken ct = default)
        => await _db.WorkflowStepTemplates
            .AsNoTracking()
            .Where(s => s.WorkflowTemplateId == workflowTemplateId)
            .OrderBy(s => s.StepOrder)
            .Select(s => new StepListItem(
                s.Id, s.StepOrder, s.Code!, s.Name!, s.AssignmentMode.ToString(),
                s.AllowCreatorOrPreparer, !string.IsNullOrWhiteSpace(s.JsonSchema)))
            .ToListAsync(ct);

        public async Task<StepEditDto?> GetAsync(Guid stepId, CancellationToken ct = default)
            => await _db.WorkflowStepTemplates
                .AsNoTracking()
                .Where(s => s.Id == stepId)
                .Select(s => new StepEditDto(s.Id, s.WorkflowTemplateId, s.StepOrder, s.Code!, s.Name!,
                                             s.AssignmentMode.ToString(), s.AllowedRolesCsv, s.AllowCreatorOrPreparer))
                .FirstOrDefaultAsync(ct);

        public async Task<Guid> CreateAsync(StepEditDto dto, CancellationToken ct = default)
        {
            var e = new EORequests.Domain.Entities.WorkflowStepTemplate
            {
                WorkflowTemplateId = dto.WorkflowTemplateId,
                StepOrder = dto.StepOrder,
                Code = dto.Code,
                Name = dto.Name,
                AssignmentMode = Enum.Parse<EORequests.Domain.Enums.AssignmentMode>(dto.AssignmentMode, ignoreCase: true),
                AllowedRolesCsv = dto.AllowedRolesCsv,
                AllowCreatorOrPreparer = dto.AllowCreatorOrPreparer
            };
            _db.WorkflowStepTemplates.Add(e);
            await _db.SaveChangesAsync(ct);
            return e.Id;
        }

        public async Task UpdateAsync(StepEditDto dto, CancellationToken ct = default)
        {
            var s = await _db.WorkflowStepTemplates.FirstAsync(x => x.Id == dto.Id, ct);
            s.StepOrder = dto.StepOrder;
            s.Code = dto.Code;
            s.Name = dto.Name;
            s.AssignmentMode = Enum.Parse<EORequests.Domain.Enums.AssignmentMode>(dto.AssignmentMode, true);
            s.AllowedRolesCsv = dto.AllowedRolesCsv;
            s.AllowCreatorOrPreparer = dto.AllowCreatorOrPreparer;
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid stepId, CancellationToken ct = default)
        {
            var s = await _db.WorkflowStepTemplates.FirstAsync(x => x.Id == stepId, ct);
            _db.WorkflowStepTemplates.Remove(s);
            await _db.SaveChangesAsync(ct);
        }

        public async Task RenumberAsync(Guid workflowTemplateId, CancellationToken ct = default)
        {
            var steps = await _db.WorkflowStepTemplates
                .Where(s => s.WorkflowTemplateId == workflowTemplateId)
                .OrderBy(s => s.StepOrder)
                .ToListAsync(ct);

            int i = 1;
            foreach (var s in steps) s.StepOrder = i++;
            await _db.SaveChangesAsync(ct);
        }

    }
}
