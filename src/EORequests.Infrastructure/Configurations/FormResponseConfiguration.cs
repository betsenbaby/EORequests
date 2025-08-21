using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;



namespace EORequests.Infrastructure.Configurations
{
    public class FormResponseConfiguration : IEntityTypeConfiguration<FormResponse>
    {
        public void Configure(EntityTypeBuilder<FormResponse> b)
        {
            b.ToTable("form_response");
            b.HasKey(x => x.Id);

            b.Property(x => x.JsonData).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.Summary).HasMaxLength(512);

            b.HasOne(x => x.WorkflowState)
             .WithOne()
             .HasForeignKey<FormResponse>(x => x.WorkflowStateId)
             .OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.SchemaVersionCaptured).HasMaxLength(32);
        }
    }
}
