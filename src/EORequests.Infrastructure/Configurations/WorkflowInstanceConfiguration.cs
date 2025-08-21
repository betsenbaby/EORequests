using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
    {
        public void Configure(EntityTypeBuilder<WorkflowInstance> b)
        {
            b.ToTable("workflow_instance");
            b.HasIndex(x => x.RequestId).IsUnique();
            b.Property(x => x.IsComplete).HasDefaultValue(false);

            b.HasOne(x => x.Request)
            .WithOne(r => r.WorkflowInstance)
            .HasForeignKey<WorkflowInstance>(x => x.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.CurrentStep)
            .WithMany()
            .HasForeignKey(x => x.CurrentStepId)
            .OnDelete(DeleteBehavior.Restrict); // or NoAction
        }
    }
}
