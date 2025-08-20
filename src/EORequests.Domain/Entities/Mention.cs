using EORequests.Domain.Audit;

namespace EORequests.Domain.Entities
{
    public class Mention : AuditableEntity
    {
        public Guid CommentId { get; set; }
        public Guid MentionedUserId { get; set; }

        public Comment Comment { get; set; } = default!;
    }
}
