using EORequests.Domain.Audit;

namespace EORequests.Domain.Entities
{
    public class WorkflowTemplate : AuditableEntity
    {
        public Guid RequestTypeId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        public RequestType RequestType { get; set; } = default!;
        public ICollection<WorkflowStepTemplate> Steps { get; set; } = new List<WorkflowStepTemplate>();
        public ICollection<SlaRule> SlaRules { get; set; } = new List<SlaRule>();
        public ICollection<EscalationRule> EscalationRules { get; set; } = new List<EscalationRule>();
    }
}
