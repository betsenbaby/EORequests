using EORequests.Application.Interfaces;
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
    public class SlaJobRunner : ISlaJobRunner
    {
        private readonly EoDbContext _db;
        private readonly ILogger<SlaJobRunner> _log;

        public SlaJobRunner(EoDbContext db, ILogger<SlaJobRunner> log)
        {
            _db = db;
            _log = log;
        }

        public async Task SendReminder(Guid workflowStateId, int daysBeforeDue)
        {
            var s = await _db.WorkflowStates
                .Include(x => x.WorkflowInstance).ThenInclude(i => i.Request)
                .Include(x => x.StepTemplate)
                .FirstOrDefaultAsync(x => x.Id == workflowStateId);

            if (s == null || s.IsComplete)
            {
                _log.LogInformation("Skip reminder: state not found or complete {StateId}", workflowStateId);
                return;
            }

            // TODO (Step 9): hook NotificationService (SignalR/Email)
            _log.LogInformation("Reminder T-{Days} for state {StateId} ({Step}) of request {Req}",
                daysBeforeDue, s.Id, s.StepTemplate.Code, s.WorkflowInstance.Request.Title);
        }

        public async Task Escalate(Guid workflowStateId)
        {
            var s = await _db.WorkflowStates
                .Include(x => x.WorkflowInstance).ThenInclude(i => i.Request)
                .Include(x => x.StepTemplate)
                .FirstOrDefaultAsync(x => x.Id == workflowStateId);

            if (s == null || s.IsComplete)
            {
                _log.LogInformation("Skip escalation: state not found or complete {StateId}", workflowStateId);
                return;
            }

            // TODO (Step 9): notify escalation target
            _log.LogWarning("Escalation T+1 for state {StateId} ({Step}) of request {Req}",
                s.Id, s.StepTemplate.Code, s.WorkflowInstance.Request.Title);
        }
    }
}
