using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class WorkflowStateConfiguration : IEntityTypeConfiguration<WorkflowState>
    {
        public void Configure(EntityTypeBuilder<WorkflowState> b)
        {
            b.ToTable("workflow_state");
            b.HasIndex(x => new { x.WorkflowInstanceId, x.StepTemplateId }).IsUnique();
            b.Property(x => x.IsComplete).HasDefaultValue(false);

            b.HasOne(x => x.WorkflowInstance)
             .WithMany(wi => wi.States)
             .HasForeignKey(x => x.WorkflowInstanceId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.StepTemplate)
             .WithMany()
             .HasForeignKey(x => x.StepTemplateId)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
