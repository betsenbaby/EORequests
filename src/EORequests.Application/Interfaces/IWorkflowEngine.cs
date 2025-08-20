using EORequests.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{
    public interface IWorkflowEngine
    {
        Task<WorkflowInstance> StartInstanceAsync(Guid requestId, Guid startedByUserId, CancellationToken ct = default);
        Task<(bool canAdvance, string? reason)> CanAdvanceAsync(Guid instanceId, CancellationToken ct = default);
        Task<WorkflowState> AdvanceAsync(Guid instanceId, Guid byUserId, CancellationToken ct = default);
        Task<WorkflowState> SkipOrBranchAsync(Guid instanceId, string ruleKey, Guid byUserId, CancellationToken ct = default);
    }
}
