using EORequests.Domain.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Domain.Audit
{
    public class ActivityLog : AuditableEntity
    {
        public string? Actor { get; set; }        // user email or id
        public string? Action { get; set; }       // "Created project", "Deleted request"
        public string? EntityType { get; set; }   // e.g. "Request", "Project"
        public string? EntityId { get; set; }     // string since PK types may vary
        public string? Details { get; set; }      // JSON or text
    }
}
