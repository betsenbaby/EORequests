using EORequests.Domain.Audit;

namespace EORequests.Domain.Entities
{
    public class EscalationRule : AuditableEntity
    {
        public Guid WorkflowStepTemplateId { get; set; }
        public int EscalateAfterDays { get; set; }   // after DueOn + N days

        public string? EscalateToRolesCsv { get; set; }
        public WorkflowStepTemplate StepTemplate { get; set; } = default!;
    }
}
