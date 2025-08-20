using EORequests.Domain.Audit;
using EORequests.Domain.Enums;

namespace EORequests.Domain.Entities
{
    public class Attachment : AuditableEntity
    {
        public EntityType LinkedEntityType { get; set; }
        public Guid LinkedEntityId { get; set; }

        public Guid? VersionGroupId { get; set; }           // group files as versions
        public int? VersionNumber { get; set; }

        public string OriginalFileName { get; set; } = default!;
        public string StoredFileName { get; set; } = default!;    // path/key on storage
        public string ContentType { get; set; } = default!;
        public long SizeBytes { get; set; }

        public bool IsSoftDeleted { get; set; }
        public DateTime? SoftDeletedOn { get; set; }
    }
}
