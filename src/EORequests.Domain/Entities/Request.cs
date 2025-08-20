using EORequests.Domain.Audit;
using System.Net.Mail;

namespace EORequests.Domain.Entities
{
    public class Request : AuditableEntity
    {
        public Guid RequestTypeId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public Guid? PreparedByUserId { get; set; }

        public string Title { get; set; } = default!;
        public string? ReferenceNumber { get; set; }   // e.g., human-friendly code

        public bool IsClosed { get; set; }

        public RequestType RequestType { get; set; } = default!;
        public WorkflowInstance? WorkflowInstance { get; set; }

        public ICollection<CommentThread> CommentThreads { get; set; } = new List<CommentThread>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
