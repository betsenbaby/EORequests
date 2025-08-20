using EORequests.Application.Interfaces;
using EORequests.Domain.Entities;
using EORequests.Infrastructure.Data;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Infrastructure.Services
{
    public class SlaService : ISlaService
    {
        private readonly EoDbContext _db;
        private readonly ILogger<SlaService> _log;

        public SlaService(EoDbContext db, ILogger<SlaService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task<DateTime?> ComputeAndSetDueDateAsync(WorkflowState state, CancellationToken ct = default)
        {
            var rule = await _db.SlaRules
                .AsNoTracking()
                .Where(r => r.WorkflowStepTemplateId == state.StepTemplateId)
                .OrderByDescending(r => r.DueDays)
                .FirstOrDefaultAsync(ct);

            if (rule == null)
            {
                _log.LogInformation("No SLA rule for stepTemplate {StepTemplateId}", state.StepTemplateId);
                return null;
            }

            var due = DateTime.UtcNow.Date.AddDays(rule.DueDays).AddHours(17); // due at 17:00 UTC by default
            state.DueOn = due;
            await _db.SaveChangesAsync(ct);
            return due;
        }

        public Task ScheduleReminderAndEscalationJobsAsync(WorkflowState state, CancellationToken ct = default)
        {
            if (state.DueOn is null) return Task.CompletedTask;

            var due = state.DueOn.Value;
            var id = state.Id.ToString("N");

            // Idempotent delete
            BackgroundJob.Delete($"sla:reminder:T-3:{id}");
            BackgroundJob.Delete($"sla:reminder:T-1:{id}");
            BackgroundJob.Delete($"sla:escalation:T+1:{id}");

            if (DateTime.UtcNow < due.AddDays(-3))
                BackgroundJob.Schedule<ISlaJobRunner>(
                    r => r.SendReminder(state.Id, 3),
                    due.AddDays(-3));

            if (DateTime.UtcNow < due.AddDays(-1))
                BackgroundJob.Schedule<ISlaJobRunner>(
                    r => r.SendReminder(state.Id, 1),
                    due.AddDays(-1));

            if (DateTime.UtcNow < due.AddDays(+1))
                BackgroundJob.Schedule<ISlaJobRunner>(
                    r => r.Escalate(state.Id),
                    due.AddDays(+1));

            _log.LogInformation("SLA jobs scheduled for state {StateId} (due {Due})", state.Id, due);
            return Task.CompletedTask;
        }

        public Task CancelJobsForStateAsync(Guid workflowStateId)
        {
            var id = workflowStateId.ToString("N");
            BackgroundJob.Delete($"sla:reminder:T-3:{id}");
            BackgroundJob.Delete($"sla:reminder:T-1:{id}");
            BackgroundJob.Delete($"sla:escalation:T+1:{id}");
            return Task.CompletedTask;
        }
    }
}
