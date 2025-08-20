using EORequests.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{

    public interface ISlaService
    {
        Task<DateTime?> ComputeAndSetDueDateAsync(WorkflowState state, CancellationToken ct = default);
        Task ScheduleReminderAndEscalationJobsAsync(WorkflowState state, CancellationToken ct = default);
        Task CancelJobsForStateAsync(Guid workflowStateId); // in case a state is completed/cancelled early
    }
}
