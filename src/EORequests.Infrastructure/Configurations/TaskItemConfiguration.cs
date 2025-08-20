using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> b)
        {
            b.ToTable("task_item");
            b.Property(x => x.Title).HasMaxLength(400).IsRequired();
            b.Property(x => x.Description).HasColumnType("nvarchar(max)");

            b.HasOne(x => x.WorkflowState)
             .WithMany(ws => ws.Tasks)
             .HasForeignKey(x => x.WorkflowStateId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ParentTask)
             .WithMany(p => p.SubTasks)
             .HasForeignKey(x => x.ParentTaskId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.WorkflowStateId, x.Status });
        }
    }
}
