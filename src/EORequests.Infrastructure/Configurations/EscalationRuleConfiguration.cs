using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class EscalationRuleConfiguration : IEntityTypeConfiguration<EscalationRule>
    {
        public void Configure(EntityTypeBuilder<EscalationRule> b)
        {
            b.ToTable("escalation_rule");
            b.Property(x => x.EscalateAfterDays).IsRequired();
            b.Property(x => x.EscalateToRolesCsv).HasMaxLength(1000);

            b.HasOne(x => x.StepTemplate)
             .WithMany(st => st.EscalationRules)
             .HasForeignKey(x => x.WorkflowStepTemplateId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.WorkflowStepTemplateId);
        }
    }
}
