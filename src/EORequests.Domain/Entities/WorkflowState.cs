using EORequests.Domain.Audit;
using EORequests.Domain.Enums;
using System.Net.Mail;

namespace EORequests.Domain.Entities
{
    public class WorkflowState : AuditableEntity
    {
        public Guid WorkflowInstanceId { get; set; }
        public Guid StepTemplateId { get; set; }

        public WorkflowStateCode StateCode { get; set; }     // PendingAction, Completed, etc.
        public Guid? AssigneeUserId { get; set; }            // SelectedByPreviousStep/AutoAssign
        public DateTime? DueOn { get; set; }
        public bool IsComplete { get; set; }

        public WorkflowInstance WorkflowInstance { get; set; } = default!;
        public WorkflowStepTemplate StepTemplate { get; set; } = default!;

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
