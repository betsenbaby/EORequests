using EORequests.Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> e)
        {
            e.ToTable("application_role");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
             .HasDefaultValueSql("NEWSEQUENTIALID()")
             .ValueGeneratedOnAdd();

            e.Property(x => x.Name).IsRequired().HasMaxLength(128);
            e.HasIndex(x => x.Name).IsUnique();
        }
    }
}
