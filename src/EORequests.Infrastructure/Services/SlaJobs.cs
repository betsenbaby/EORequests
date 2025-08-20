using EORequests.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace EORequests.Infrastructure.Services
{
    public static class SlaJobs
    {
        // DI via method signature is supported by Hangfire (it will resolve scoped services)
        public static async Task SendReminder(Guid workflowStateId, int daysBeforeDue, EoDbContext db, ILoggerFactory loggerFactory)
        {
            var log = loggerFactory.CreateLogger("SLA.Reminder");
            var s = await db.WorkflowStates
                .Include(x => x.WorkflowInstance).ThenInclude(i => i.Request)
                .Include(x => x.StepTemplate)
                .FirstOrDefaultAsync(x => x.Id == workflowStateId);

            if (s == null || s.IsComplete) { log.LogInformation("Skip reminder: state not found or complete {StateId}", workflowStateId); return; }

            // TODO: plug NotificationService (SignalR + Email) in Step 9
            log.LogInformation("Reminder T-{Days} for state {StateId} ({Step}) of request {Req}", daysBeforeDue, s.Id, s.StepTemplate.Code, s.WorkflowInstance.Request.Title);
        }

        public static async Task Escalate(Guid workflowStateId, EoDbContext db, ILoggerFactory loggerFactory)
        {
            var log = loggerFactory.CreateLogger("SLA.Escalation");
            var s = await db.WorkflowStates
                .Include(x => x.WorkflowInstance).ThenInclude(i => i.Request)
                .Include(x => x.StepTemplate)
                .FirstOrDefaultAsync(x => x.Id == workflowStateId);

            if (s == null || s.IsComplete) { log.LogInformation("Skip escalation: state not found or complete {StateId}", workflowStateId); return; }

            // TODO: Step 9 will notify; Step 12 Admin UI could set escalation target (role/email)
            log.LogWarning("Escalation T+1 for state {StateId} ({Step}) of request {Req}", s.Id, s.StepTemplate.Code, s.WorkflowInstance.Request.Title);
        }
    }
}
