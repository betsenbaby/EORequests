using EORequests.Application.DTOs.Sla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{

    public interface ISlaAdminService
    {
        Task<SlaRuleDto?> GetForStepAsync(Guid workflowStepTemplateId, CancellationToken ct = default);

        /// <summary>
        /// Creates or updates the SLA rule for a step. Validates:
        /// - All reminder offsets are positive and strictly less than DueDays
        /// - EscalationOffsetDays, if provided, is positive
        /// </summary>
        Task<SlaRuleDto> UpsertForStepAsync(SlaRuleUpsertDto dto, CancellationToken ct = default);
    }
}
