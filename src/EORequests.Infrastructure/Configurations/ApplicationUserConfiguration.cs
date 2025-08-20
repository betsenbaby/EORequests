using EORequests.Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> e)
        {
            e.ToTable("application_user");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
             .HasDefaultValueSql("NEWSEQUENTIALID()")
             .ValueGeneratedOnAdd();

            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.Property(x => x.DisplayName).IsRequired().HasMaxLength(256);

            e.HasIndex(x => x.Email).IsUnique();
        }
    }
}
