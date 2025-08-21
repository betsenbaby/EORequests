using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class WorkflowStepTemplateConfiguration : IEntityTypeConfiguration<WorkflowStepTemplate>
    {
        public void Configure(EntityTypeBuilder<WorkflowStepTemplate> b)
        {
            b.ToTable("workflow_step_template");
            b.Property(x => x.Code).HasMaxLength(64).IsRequired();
            b.Property(x => x.Name).HasMaxLength(256).IsRequired();
            b.Property(x => x.AllowedRolesCsv).HasMaxLength(1000);
            b.Property(x => x.BranchRuleKey).HasMaxLength(128);
            b.Property(x => x.JsonSchema).HasColumnType("nvarchar(max)");

            b.HasIndex(x => new { x.WorkflowTemplateId, x.StepOrder }).IsUnique();

            b.HasOne(x => x.WorkflowTemplate)
             .WithMany(wt => wt.Steps)
             .HasForeignKey(x => x.WorkflowTemplateId)
             .OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.JsonSchemaVersion)
             .HasMaxLength(32)
             .HasDefaultValue("v1");
        }
    }
}
