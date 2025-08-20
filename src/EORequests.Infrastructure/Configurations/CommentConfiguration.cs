using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> b)
        {
            b.ToTable("comment");
            b.Property(x => x.Body).HasColumnType("nvarchar(max)").IsRequired();

            b.HasOne(x => x.Thread)
             .WithMany(t => t.Comments)
             .HasForeignKey(x => x.ThreadId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Parent)
             .WithMany(p => p.Replies)
             .HasForeignKey(x => x.ParentCommentId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.ThreadId);
        }
    }
}
