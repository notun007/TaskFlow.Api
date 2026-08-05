using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.Infrastructure.Persistence;

public sealed class TaskFlowDbContext(DbContextOptions<TaskFlowDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<SoftwareApplication> SoftwareApplications => Set<SoftwareApplication>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("TASKFLOW");
        builder.Entity<TaskItem>().ToTable("TASKS");
        builder.Entity<TaskItem>().Property(x => x.Type).HasConversion<string>();
        builder.Entity<TaskItem>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<TaskItem>().Property(x => x.Priority).HasConversion<string>();
        builder.Entity<TaskItem>().Property(x => x.Severity).HasConversion<string>();
        builder.Entity<TaskAssignment>().Property(x => x.Responsibility).HasConversion<string>();
        builder.Entity<TaskItem>().HasIndex(x => x.TaskNumber).IsUnique();
        builder.Entity<TaskItem>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<TaskComment>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<AuditEntry>().HasIndex(x => new { x.EntityName, x.EntityId });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries().Where(x => x.Entity is TaskFlow.Domain.Common.Entity && x.State == EntityState.Modified))
            ((TaskFlow.Domain.Common.Entity)entry.Entity).UpdatedAt = DateTimeOffset.UtcNow;
        return await base.SaveChangesAsync(cancellationToken);
    }
}
