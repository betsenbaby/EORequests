using EORequests.Application.Interfaces;
using EORequests.Domain.Entities;
using EORequests.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace EORequests.Infrastructure.Services
{
    public class WorkflowPreviewService : IWorkflowPreviewService
    {
        private readonly EoDbContext _db;
        private readonly IWorkflowEngine _engine;
        private readonly ILogger<WorkflowPreviewService> _log;

        public WorkflowPreviewService(EoDbContext db, IWorkflowEngine engine, ILogger<WorkflowPreviewService> log)
        {
            _db = db; _engine = engine; _log = log;
        }

        public async Task<Guid?> CreatePreviewAtStepAsync(Guid stepTemplateId, Guid byUserId, CancellationToken ct = default)
        {
            // Load step + template + request type
            var step = await _db.WorkflowStepTemplates
                .Include(s => s.WorkflowTemplate)
                .FirstOrDefaultAsync(s => s.Id == stepTemplateId, ct)
                ?? throw new KeyNotFoundException($"Step not found: {stepTemplateId}");

            var reqTypeId = step.WorkflowTemplate.RequestTypeId;

            // Create a throwaway Request (flag it for cleanup if you want)
            var req = new Request
            {
                RequestTypeId = reqTypeId,
                Title = $"Preview for {step.Code}",
                CreatedByUserId = byUserId,
                PreparedByUserId = byUserId,
                IsPreview = true,
                PreviewCreatedOn = DateTime.UtcNow
            };
            _db.Requests.Add(req);
            await _db.SaveChangesAsync(ct);

            // Start instance
            var inst = await _engine.StartInstanceAsync(req.Id, byUserId, ct);

            // If the desired step is not the first, advance until we reach it
            var stepsOrdered = await _db.WorkflowStepTemplates
                .Where(s => s.WorkflowTemplateId == step.WorkflowTemplateId)
                .OrderBy(s => s.StepOrder)
                .Select(s => new { s.Id, s.StepOrder })
                .ToListAsync(ct);

            var targetOrder = stepsOrdered.First(x => x.Id == stepTemplateId).StepOrder;

            // Get the current state id
            var currentStateId = inst.CurrentStepId;

            // If current step is behind target, advance
            while (true)
            {
                var curr = await _db.WorkflowStates
                    .Include(s => s.StepTemplate)
                    .FirstAsync(s => s.Id == currentStateId, ct);

                if (curr.StepTemplate.StepOrder >= targetOrder) break;

                var next = await _engine.AdvanceAsync(inst.Id, byUserId, ct);
                currentStateId = next.Id;
            }

            _log.LogInformation("Preview created for step {StepId} -> state {StateId}", stepTemplateId, currentStateId);
            return currentStateId;
        }
    }
}
