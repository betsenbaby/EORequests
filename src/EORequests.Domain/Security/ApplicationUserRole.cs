using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Domain.Security
{
    public class ApplicationUserRole
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = default!;
        public Guid RoleId { get; set; }
        public ApplicationRole Role { get; set; } = default!;
    }
}
