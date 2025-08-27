using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.DTOs.Sla
{ 
public sealed class SlaRuleDto
{
    public Guid Id { get; init; }
    public Guid WorkflowStepTemplateId { get; init; }
    public int DueDays { get; init; }                      // e.g., 5
    public string ReminderOffsetsCsv { get; init; } = "";  // e.g., "3,1" (T-3, T-1)
    public int? EscalationOffsetDays { get; init; }        // e.g., 1 (T+1)
    public bool IsActive { get; init; }
    public byte[]? RowVersion { get; init; }
}
}
