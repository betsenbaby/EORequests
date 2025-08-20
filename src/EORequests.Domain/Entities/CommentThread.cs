using EORequests.Domain.Audit;
using EORequests.Domain.Enums;
using System.Xml.Linq;

namespace EORequests.Domain.Entities
{
    public class CommentThread : AuditableEntity
    {
        public EntityType LinkedEntityType { get; set; }
        public Guid LinkedEntityId { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
