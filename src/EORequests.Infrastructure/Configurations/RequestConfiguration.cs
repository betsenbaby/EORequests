using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class RequestConfiguration : IEntityTypeConfiguration<Request>
    {
        public void Configure(EntityTypeBuilder<Request> b)
        {
            b.ToTable("request");
            b.Property(x => x.Title).HasMaxLength(400).IsRequired();
            b.Property(x => x.ReferenceNumber).HasMaxLength(100);
            b.HasIndex(x => x.RequestTypeId);
            b.HasOne(x => x.RequestType)
             .WithMany(rt => rt.Requests)
             .HasForeignKey(x => x.RequestTypeId)
             .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.WorkflowInstance)
             .WithOne(wi => wi.Request)
             .HasForeignKey<WorkflowInstance>(wi => wi.RequestId)
             .OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.IsPreview)
              .HasDefaultValue(false);
            b.Property(x => x.PreviewCreatedOn);
            b.HasIndex(x => x.IsPreview);
        }
    }
}
