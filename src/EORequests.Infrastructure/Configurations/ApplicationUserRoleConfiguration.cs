using EORequests.Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserRole> e)
        {
            e.ToTable("application_user_role");
            e.HasKey(x => new { x.UserId, x.RoleId });

            e.HasOne(x => x.User)
             .WithMany(u => u.ApplicationUserRoles)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Role)
             .WithMany(r => r.ApplicationUserRoles)
             .HasForeignKey(x => x.RoleId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
