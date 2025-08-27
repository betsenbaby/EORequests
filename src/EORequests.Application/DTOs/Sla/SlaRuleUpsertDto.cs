using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.DTOs.Sla
{
    public sealed class SlaRuleUpsertDto
    {
        [Required] public Guid WorkflowStepTemplateId { get; set; }
        [Range(1, 3650)] public int DueDays { get; set; } = 5;

        // Comma separated positive integers; each must be < DueDays
        public string ReminderOffsetsCsv { get; set; } = "";

        // Positive integer. Optional. Represents “days after due” for escalation.
        [Range(1, 3650)]
        public int? EscalationOffsetDays { get; set; }

        public bool IsActive { get; set; } = true;

        // for optimistic concurrency
        public byte[]? RowVersion { get; set; }
    }
}
