using EORequests.Domain.Audit;
using EORequests.Domain.Entities;
using EORequests.Domain.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;              // <-- needed for .Where(...)
using System.Security.Claims;

namespace EORequests.Infrastructure.Data;

public class EoDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContext;

    public EoDbContext(DbContextOptions<EoDbContext> options) : base(options)
    {
    }

    public EoDbContext(DbContextOptions<EoDbContext> options, IHttpContextAccessor httpContext)
        : base(options)
    {
        _httpContext = httpContext;
    }

    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
    public DbSet<ApplicationUserRole> ApplicationUserRoles => Set<ApplicationUserRole>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    // Step 1 entities
    public DbSet<RequestType> RequestTypes => Set<RequestType>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<WorkflowTemplate> WorkflowTemplates => Set<WorkflowTemplate>();
    public DbSet<WorkflowStepTemplate> WorkflowStepTemplates => Set<WorkflowStepTemplate>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowState> WorkflowStates => Set<WorkflowState>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<SlaRule> SlaRules => Set<SlaRule>();
    public DbSet<EscalationRule> EscalationRules => Set<EscalationRule>();
    public DbSet<CommentThread> CommentThreads => Set<CommentThread>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentReaction> CommentReactions => Set<CommentReaction>();
    public DbSet<Mention> Mentions => Set<Mention>();
    public DbSet<FormResponse> FormResponses => Set<FormResponse>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.ApplyConfigurationsFromAssembly(typeof(EoDbContext).Assembly);

        // Global NEWSEQUENTIALID for Guid PKs named "Id"
        foreach (var entity in b.Model.GetEntityTypes())
        {
            var id = entity.FindProperty("Id");
            if (id != null && id.ClrType == typeof(Guid))
            {
                id.SetDefaultValueSql("NEWSEQUENTIALID()");
                id.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
            }
        }

        // Map real RowVersion on all AuditableEntity types
        foreach (var et in b.Model.GetEntityTypes()
                 .Where(t => typeof(AuditableEntity).IsAssignableFrom(t.ClrType)))
        {
            b.Entity(et.ClrType)
             .Property<byte[]>(nameof(AuditableEntity.RowVersion))
             .IsRowVersion()
             .HasColumnName("row_version");
        }
    }


    public override int SaveChanges()
    {
        ApplyAuditStamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditStamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditStamps()
    {
        var now = DateTime.UtcNow;

        var email = _httpContext?.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                 ?? _httpContext?.HttpContext?.User?.Identity?.Name;

        foreach (var e in ChangeTracker.Entries<AuditableEntity>())
        {
            if (e.State == EntityState.Added)
            {
                e.Entity.CreatedOn = now;
                e.Entity.CreatedBy = email;
            }
            else if (e.State == EntityState.Modified)
            {
                e.Entity.ModifiedOn = now;
                e.Entity.ModifiedBy = email;
            }
        }
    }
}
