using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class CommentThreadConfiguration : IEntityTypeConfiguration<CommentThread>
    {
        public void Configure(EntityTypeBuilder<CommentThread> b)
        {
            b.ToTable("comment_thread");
            b.HasIndex(x => new { x.LinkedEntityType, x.LinkedEntityId });
        }
    }
}
