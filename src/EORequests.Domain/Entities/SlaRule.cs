using EORequests.Domain.Audit;

namespace EORequests.Domain.Entities
{
    public sealed class SlaRule : AuditableEntity
    {
        public Guid WorkflowStepTemplateId { get; set; }
        public int DueDays { get; set; }
        public string ReminderOffsetsCsv { get; set; } = "";
        public int? EscalationOffsetDays { get; set; }
        public bool IsActive { get; set; } = true;

        public WorkflowStepTemplate? WorkflowStepTemplate { get; set; }
    }
}
