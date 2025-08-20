using EORequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EORequests.Infrastructure.Configurations
{
    public class RequestTypeConfiguration : IEntityTypeConfiguration<RequestType>
    {
        public void Configure(EntityTypeBuilder<RequestType> b)
        {
            b.ToTable("request_type");
            b.Property(x => x.Code).HasMaxLength(64).IsRequired();
            b.Property(x => x.Name).HasMaxLength(256).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);
            b.HasIndex(x => x.Code).IsUnique();
        }
    }
}
