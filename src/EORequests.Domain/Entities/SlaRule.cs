using EORequests.Domain.Audit;

namespace EORequests.Domain.Entities
{
    public class SlaRule : AuditableEntity
    {
        public Guid WorkflowStepTemplateId { get; set; }
        public int DueDays { get; set; }             // simplistic SLA: N calendar days

        public WorkflowStepTemplate StepTemplate { get; set; } = default!;
    }
}
