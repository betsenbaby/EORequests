using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{
    public interface IWorkflowReadService
    {
        Task<Guid> GetInstanceIdByStateIdAsync(Guid stateId, CancellationToken ct = default);

        // NEW: resolve RequestId from a WorkflowStateId
        Task<Guid> GetRequestIdByStateIdAsync(Guid stateId, CancellationToken ct = default);
    }
}
