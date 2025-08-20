using EORequests.Application.Interfaces;
using EORequests.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EORequests.Infrastructure.Services
{
    public class BranchRuleEvaluator : IBranchRuleEvaluator
    {
        public Task<int?> EvaluateNextStepOrderAsync(WorkflowState current, string? ruleKey, CancellationToken ct = default)
        {
            // v1: if ruleKey like "goto:3" jump to step order 3; "end" completes; otherwise null => next
            if (string.IsNullOrWhiteSpace(ruleKey)) return Task.FromResult<int?>(null);

            if (string.Equals(ruleKey, "end", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult<int?>(-1);

            var m = Regex.Match(ruleKey, @"^goto:(\d+)$", RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[1].Value, out var order))
                return Task.FromResult<int?>(order);

            return Task.FromResult<int?>(null);
        }
    }
}
