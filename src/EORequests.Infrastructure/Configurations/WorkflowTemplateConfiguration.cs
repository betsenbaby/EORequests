using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class WorkflowTemplateConfiguration : IEntityTypeConfiguration<WorkflowTemplate>
    {
        public void Configure(EntityTypeBuilder<WorkflowTemplate> b)
        {
            b.ToTable("workflow_template");
            b.Property(x => x.Code).HasMaxLength(64).IsRequired();
            b.Property(x => x.Name).HasMaxLength(256).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);
            b.HasIndex(x => new { x.RequestTypeId, x.Code }).IsUnique();

            b.HasOne(x => x.RequestType)
             .WithMany(rt => rt.WorkflowTemplates)
             .HasForeignKey(x => x.RequestTypeId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
