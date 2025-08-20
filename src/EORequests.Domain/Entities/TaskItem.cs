using EORequests.Domain.Audit;
using EORequests.Domain.Enums;
using System.Net.Mail;

namespace EORequests.Domain.Entities
{
    public class TaskItem : AuditableEntity
    {
        public Guid WorkflowStateId { get; set; }
        public Guid? ParentTaskId { get; set; }    // for subtasks

        public string Title { get; set; } = default!;
        public string? Description { get; set; }

        public Guid? AssignedToUserId { get; set; }
        public DateTime? DueOn { get; set; }
        public TaskProgressStatus Status { get; set; } = TaskProgressStatus.NotStarted;
        public bool IsGating { get; set; }         // blocks step completion when open

        public WorkflowState WorkflowState { get; set; } = default!;
        public TaskItem? ParentTask { get; set; }
        public ICollection<TaskItem> SubTasks { get; set; } = new List<TaskItem>();

        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
