using EORequests.Domain.Audit;
using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(EntityTypeBuilder<ActivityLog> e)
        {
            e.ToTable("activity_log");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
             .HasDefaultValueSql("NEWSEQUENTIALID()")
             .ValueGeneratedOnAdd();

            e.Property(x => x.Actor).HasMaxLength(256);
            e.Property(x => x.Action).HasMaxLength(256);
            e.Property(x => x.EntityType).HasMaxLength(128);
            e.Property(x => x.EntityId).HasMaxLength(64); // keep string unless you switch to Guid
        }
    }
}
