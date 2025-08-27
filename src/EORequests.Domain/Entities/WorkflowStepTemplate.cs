using EORequests.Domain.Audit;
using EORequests.Domain.Enums;

namespace EORequests.Domain.Entities
{
    public class WorkflowStepTemplate : AuditableEntity
    {
        public Guid WorkflowTemplateId { get; set; }

        public int StepOrder { get; set; }                   // 1..N
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public AssignmentMode AssignmentMode { get; set; }

        public string? AllowedRolesCsv { get; set; }         // for RoleBased
        public bool AllowCreatorOrPreparer { get; set; }     // flag to allow creator/preparer act

        public string? BranchRuleKey { get; set; }           // optional branch/skip rule key
        public string? JsonSchema { get; set; }              // dynamic form schema for this step
        public string JsonSchemaVersion { get; set; } = "v1"; //

        public WorkflowTemplate WorkflowTemplate { get; set; } = default!;
        public SlaRule? SlaRule { get; set; }
        public ICollection<EscalationRule> EscalationRules { get; set; } = new List<EscalationRule>();
    }
}
