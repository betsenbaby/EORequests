using EORequests.Domain.Audit;
using EORequests.Domain.Enums;
using static System.Collections.Specialized.BitVector32;

namespace EORequests.Domain.Entities
{
    public class Comment : AuditableEntity
    {
        public Guid ThreadId { get; set; }
        public Guid? ParentCommentId { get; set; }     // limit depth in UI/service
        public Guid AuthorUserId { get; set; }

        public string Body { get; set; } = default!;
        public CommentVisibility Visibility { get; set; } = CommentVisibility.Internal;

        public CommentThread Thread { get; set; } = default!;
        public Comment? Parent { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();

        public ICollection<CommentReaction> Reactions { get; set; } = new List<CommentReaction>();
        public ICollection<Mention> Mentions { get; set; } = new List<Mention>();
    }
}
