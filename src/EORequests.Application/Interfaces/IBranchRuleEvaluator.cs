using EORequests.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{

    public interface IBranchRuleEvaluator
    {
        /// <summary>
        /// Decide the next step order (1..N) given the current state and optional ruleKey.
        /// Return null to use the default "next order" (current+1).
        /// Return -1 to indicate the workflow should complete.
        /// </summary>
        Task<int?> EvaluateNextStepOrderAsync(WorkflowState current, string? ruleKey, CancellationToken ct = default);
    }
}
