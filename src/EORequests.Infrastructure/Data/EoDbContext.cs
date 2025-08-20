using EORequests.Domain.Audit;
using EORequests.Domain.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EORequests.Infrastructure.Data;

public class EoDbContext : DbContext
{
    private readonly IHttpContextAccessor _http;

    public EoDbContext(DbContextOptions<EoDbContext> options, IHttpContextAccessor http) : base(options)
    {
        _http = http;
    }

    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
    public DbSet<ApplicationUserRole> ApplicationUserRoles => Set<ApplicationUserRole>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Global config for all AuditableEntity descendants
        foreach (var et in b.Model.GetEntityTypes()
                     .Where(t => typeof(AuditableEntity).IsAssignableFrom(t.ClrType)))
        {
            b.Entity(et.ClrType).Property<byte[]>("RowVersion").IsRowVersion();
            b.Entity(et.ClrType).Property<string?>("CreatedBy").HasMaxLength(256);
            b.Entity(et.ClrType).Property<string?>("ModifiedBy").HasMaxLength(256);
        }

        // ApplicationUser
        b.Entity<ApplicationUser>(e =>
        {
            e.ToTable("application_user");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()")     // DB generates IDs
                .ValueGeneratedOnAdd();

            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.Property(x => x.DisplayName).IsRequired().HasMaxLength(256);
            e.HasIndex(x => x.Email).IsUnique();
        });

        // ApplicationRole
        b.Entity<ApplicationRole>(e =>
        {
            e.ToTable("application_role");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()")
                .ValueGeneratedOnAdd();

            e.Property(x => x.Name).IsRequired().HasMaxLength(128);
            e.HasIndex(x => x.Name).IsUnique();
        });

        // Join
        b.Entity<ApplicationUserRole>(e =>
        {
            e.ToTable("application_user_role");
            e.HasKey(x => new { x.UserId, x.RoleId });
            e.HasOne(x => x.User).WithMany(u => u.ApplicationUserRoles).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Role).WithMany(r => r.ApplicationUserRoles).HasForeignKey(x => x.RoleId);
        });


        // ActivityLog (if you use it)
        b.Entity<ActivityLog>(e =>
        {
            e.ToTable("activity_log");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()")
                .ValueGeneratedOnAdd();

            e.Property(x => x.Actor).HasMaxLength(256);
            e.Property(x => x.Action).HasMaxLength(256);
            e.Property(x => x.EntityType).HasMaxLength(128);
            e.Property(x => x.EntityId).HasMaxLength(64);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var user = _http.HttpContext?.User;
        var email = user?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                 ?? user?.Identity?.Name;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOn = now;
                entry.Entity.CreatedBy = email;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedOn = now;
                entry.Entity.ModifiedBy = email;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
