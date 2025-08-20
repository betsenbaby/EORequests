using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class CommentReactionConfiguration : IEntityTypeConfiguration<CommentReaction>
    {
        public void Configure(EntityTypeBuilder<CommentReaction> b)
        {
            b.ToTable("comment_reaction");
            b.Property(x => x.Emoji).HasMaxLength(16).IsRequired();

            b.HasOne(x => x.Comment)
             .WithMany(c => c.Reactions)
             .HasForeignKey(x => x.CommentId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.CommentId, x.UserId, x.Emoji }).IsUnique();
        }
    }
}
