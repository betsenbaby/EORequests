using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{
    public interface IWorkflowPermissionReadService
    {
        Task<IReadOnlyList<PermissionMatrixRow>> GetMatrixAsync(Guid workflowTemplateId, CancellationToken ct = default);
    }

    public sealed record PermissionMatrixRow(
        Guid StepId,
        string StepCode,
        string StepName,
        IReadOnlyDictionary<string, bool> RoleCanAct // key: role name
    );
}
