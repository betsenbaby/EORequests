using EORequests.Domain.Audit;

namespace EORequests.Domain.Entities
{
    public class CommentReaction : AuditableEntity
    {
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
        public string Emoji { get; set; } = default!;   // e.g. 👍 ✅

        public Comment Comment { get; set; } = default!;
    }
}
