using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class MentionConfiguration : IEntityTypeConfiguration<Mention>
    {
        public void Configure(EntityTypeBuilder<Mention> b)
        {
            b.ToTable("mention");
            b.HasOne(x => x.Comment)
             .WithMany(c => c.Mentions)
             .HasForeignKey(x => x.CommentId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.CommentId, x.MentionedUserId }).IsUnique();
        }
    }
}
