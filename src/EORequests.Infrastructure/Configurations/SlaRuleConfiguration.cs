using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    // SlaRuleConfiguration.cs
    public sealed class SlaRuleConfiguration : IEntityTypeConfiguration<SlaRule>
    {
        public void Configure(EntityTypeBuilder<SlaRule> b)
        {
            b.ToTable("sla_rule");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
            b.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

            b.Property(x => x.DueDays).IsRequired();
            b.Property(x => x.ReminderOffsetsCsv).HasMaxLength(256);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            // >>> SINGLE, CLEAR RELATIONSHIP (1:1)
            b.HasOne(x => x.WorkflowStepTemplate)
             .WithOne(w => w.SlaRule)
             .HasForeignKey<SlaRule>(x => x.WorkflowStepTemplateId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.WorkflowStepTemplateId).IsUnique(); // enforce 1:1
        }
    }

}
