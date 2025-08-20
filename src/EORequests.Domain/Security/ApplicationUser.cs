using EORequests.Domain.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Domain.Security
{
    public class ApplicationUser : AuditableEntity
    {
        public string Email { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? IndexNumber { get; set; }
        public bool IsActive { get; set; }

        public ICollection<ApplicationUserRole> ApplicationUserRoles { get; set; } = new List<ApplicationUserRole>();
    }
}
