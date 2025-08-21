using EORequests.Domain.Audit;

namespace EORequests.Domain.Entities
{
    public class WorkflowInstance : AuditableEntity
    {
        public Guid RequestId { get; set; }
        public Guid? CurrentStepId { get; set; }
        public bool IsComplete { get; set; }

        public Request Request { get; set; } = default!;
        public WorkflowState? CurrentStep { get; set; } = default!;

        public ICollection<WorkflowState> States { get; set; } = new List<WorkflowState>();
    }
}
