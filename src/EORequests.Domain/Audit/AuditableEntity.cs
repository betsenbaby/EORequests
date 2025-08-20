using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Domain.Audit
{
    public abstract class AuditableEntity
    {
        public Guid Id { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        // Optimistic concurrency
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
