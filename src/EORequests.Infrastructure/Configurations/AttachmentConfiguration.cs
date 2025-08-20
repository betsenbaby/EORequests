using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> b)
        {
            b.ToTable("attachment");
            b.Property(x => x.OriginalFileName).HasMaxLength(500).IsRequired();
            b.Property(x => x.StoredFileName).HasMaxLength(500).IsRequired();
            b.Property(x => x.ContentType).HasMaxLength(256).IsRequired();

            b.HasIndex(x => new { x.LinkedEntityType, x.LinkedEntityId });
            b.HasIndex(x => x.VersionGroupId);
        }
    }
}
