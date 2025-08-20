using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class SlaRuleConfiguration : IEntityTypeConfiguration<SlaRule>
    {
        public void Configure(EntityTypeBuilder<SlaRule> b)
        {
            b.ToTable("sla_rule");
            b.Property(x => x.DueDays).IsRequired();

            b.HasOne(x => x.StepTemplate)
             .WithMany(st => st.SlaRules)
             .HasForeignKey(x => x.WorkflowStepTemplateId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.WorkflowStepTemplateId);
        }
    }
}
