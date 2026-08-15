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
    public DbSet<Epic> Epics => Set<Epic>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<ProjectRoleAssignment> ProjectRoleAssignments => Set<ProjectRoleAssignment>();
    public DbSet<TransitionRolePermission> TransitionRolePermissions => Set<TransitionRolePermission>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<ProjectRelease> ProjectReleases => Set<ProjectRelease>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();
    public DbSet<TaskStatusHistory> TaskStatusHistory => Set<TaskStatusHistory>();
    public DbSet<TaskCustomFieldValue> TaskCustomFieldValues => Set<TaskCustomFieldValue>();
    public DbSet<TaskLink> TaskLinks => Set<TaskLink>();
    public DbSet<TaskAttachment> TaskAttachments => Set<TaskAttachment>();
    public DbSet<WorkItemType> WorkItemTypes => Set<WorkItemType>();
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();
    public DbSet<CustomFieldOption> CustomFieldOptions => Set<CustomFieldOption>();
    public DbSet<CustomFieldContext> CustomFieldContexts => Set<CustomFieldContext>();
    public DbSet<WorkflowScheme> WorkflowSchemes => Set<WorkflowScheme>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<ProjectBoard> ProjectBoards => Set<ProjectBoard>();
    public DbSet<ProjectBoardColumn> ProjectBoardColumns => Set<ProjectBoardColumn>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("TASKFLOW");
        builder.Entity<TaskItem>().ToTable("TASKS");
        builder.Entity<TaskItem>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<TaskItem>().Property(x => x.Priority).HasConversion<string>();
        builder.Entity<TaskItem>().Property(x => x.Severity).HasConversion<string>();
        builder.Entity<TaskItem>().HasOne(x => x.Epic).WithMany(x => x.Tasks).HasForeignKey(x => x.EpicId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<TaskItem>().HasOne(x => x.Feature).WithMany(x => x.Tasks).HasForeignKey(x => x.FeatureId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<TaskItem>().HasOne(x => x.ParentTask).WithMany(x => x.Subtasks).HasForeignKey(x => x.ParentTaskId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<TaskItem>().HasIndex(x => x.ParentTaskId);
        builder.Entity<Epic>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<Epic>().HasIndex(x => new { x.ProjectId, x.Name }).IsUnique();
        builder.Entity<Epic>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Feature>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<Feature>().HasIndex(x => new { x.EpicId, x.Name }).IsUnique();
        builder.Entity<Feature>().HasIndex(x => x.ProjectId);
        builder.Entity<Feature>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<TaskAssignment>().Property(x => x.Responsibility).HasConversion<string>();
        builder.Entity<ProjectRoleAssignment>().Property(x => x.Role).HasConversion<string>();
        builder.Entity<ProjectRoleAssignment>().HasIndex(x => new { x.ProjectId, x.UserId, x.Role }).IsUnique();
        builder.Entity<ProjectRoleAssignment>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<TransitionRolePermission>().Property(x => x.FromStatus).HasConversion<string>();
        builder.Entity<TransitionRolePermission>().Property(x => x.ToStatus).HasConversion<string>();
        builder.Entity<TransitionRolePermission>().Property(x => x.Role).HasConversion<string>();
        builder.Entity<TransitionRolePermission>().HasIndex(x => new { x.FromStatus, x.ToStatus, x.Role }).IsUnique();
        builder.Entity<TransitionRolePermission>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<TransitionRolePermission>().HasData(UniversalTransitionRolePolicy.Permissions.Select(x => new { x.Id, FromStatus = x.From, ToStatus = x.To, x.Role, CreatedAt = UniversalTransitionRolePolicy.SeededAt, UpdatedAt = (DateTimeOffset?)null, IsDeleted = false }));
        builder.Entity<TaskStatusHistory>().Property(x => x.FromStatus).HasConversion<string>();
        builder.Entity<TaskStatusHistory>().Property(x => x.ToStatus).HasConversion<string>();
        builder.Entity<TaskStatusHistory>().HasIndex(x => new { x.TaskItemId, x.CreatedAt });
        builder.Entity<TaskStatusHistory>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<TaskCustomFieldValue>().HasIndex(x => new { x.TaskItemId, x.CustomFieldDefinitionId }).IsUnique();
        builder.Entity<TaskCustomFieldValue>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<TaskLink>().Property(x => x.Type).HasConversion<string>();
        builder.Entity<TaskLink>().HasIndex(x => new { x.SourceTaskId, x.TargetTaskId, x.Type }).IsUnique();
        builder.Entity<TaskLink>().HasOne(x => x.SourceTask).WithMany(x => x.OutgoingLinks).HasForeignKey(x => x.SourceTaskId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<TaskLink>().HasOne(x => x.TargetTask).WithMany(x => x.IncomingLinks).HasForeignKey(x => x.TargetTaskId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<TaskLink>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<TaskAttachment>().Property(x => x.Content).HasColumnType("BLOB");
        builder.Entity<TaskAttachment>().HasIndex(x => new { x.TaskItemId, x.CreatedAt });
        builder.Entity<TaskAttachment>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<TaskItem>().HasIndex(x => x.TaskNumber).IsUnique();
        builder.Entity<TaskItem>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Sprint>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<Sprint>().HasIndex(x => new { x.ProjectId, x.Name }).IsUnique();
        builder.Entity<Sprint>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<ProjectRelease>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<ProjectRelease>().HasIndex(x => new { x.ProjectId, x.Name }).IsUnique();
        builder.Entity<ProjectRelease>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<TaskComment>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<AuditEntry>().Property(x => x.ChangesJson).HasColumnType("CLOB");
        builder.Entity<AuditEntry>().HasIndex(x => new { x.EntityName, x.EntityId });
        builder.Entity<WorkItemType>().HasIndex(x => x.Key).IsUnique();
        builder.Entity<WorkItemType>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<WorkItemType>().HasData(BuiltInWorkItemTypes.All.Select(item => new
        {
            item.Id,
            item.Key,
            item.Name,
            item.Description,
            IsActive = true,
            IsSystem = true,
            item.SortOrder,
            CreatedAt = BuiltInWorkItemTypes.SeededAt,
            UpdatedAt = (DateTimeOffset?)null,
            IsDeleted = false
        }));
        builder.Entity<CustomFieldDefinition>().Property(x => x.Type).HasConversion<string>();
        builder.Entity<CustomFieldDefinition>().HasIndex(x => x.Key).IsUnique();
        builder.Entity<CustomFieldDefinition>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<CustomFieldDefinition>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<CustomFieldOption>().HasIndex(x => new { x.CustomFieldDefinitionId, x.Value }).IsUnique();
        builder.Entity<CustomFieldOption>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<CustomFieldContext>().HasIndex(x => new { x.CustomFieldDefinitionId, x.WorkItemTypeId }).IsUnique();
        builder.Entity<CustomFieldContext>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<WorkflowScheme>().HasIndex(x => x.WorkItemTypeId).IsUnique();
        builder.Entity<WorkflowScheme>().HasIndex(x => x.ProjectId).IsUnique();
        builder.Entity<WorkflowScheme>().HasOne(x => x.Project).WithOne(x => x.WorkflowScheme).HasForeignKey<WorkflowScheme>(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<WorkflowScheme>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<WorkflowTransition>().Property(x => x.FromStatus).HasConversion<string>();
        builder.Entity<WorkflowTransition>().Property(x => x.ToStatus).HasConversion<string>();
        builder.Entity<WorkflowTransition>().HasIndex(x => new { x.WorkflowSchemeId, x.FromStatus, x.ToStatus }).IsUnique();
        builder.Entity<WorkflowTransition>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<ProjectBoard>().HasIndex(x => x.ProjectId).IsUnique();
        builder.Entity<ProjectBoard>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<ProjectBoard>().HasOne(x => x.Project).WithOne(x => x.Board).HasForeignKey<ProjectBoard>(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ProjectBoardColumn>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<ProjectBoardColumn>().HasIndex(x => new { x.ProjectBoardId, x.Status }).IsUnique();
        builder.Entity<ProjectBoardColumn>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<WorkflowScheme>().HasData(new { Id = BuiltInWorkflow.SchemeId, Name = "Default workflow", WorkItemTypeId = (Guid?)null, IsDefault = true, CreatedAt = BuiltInWorkflow.SeededAt, UpdatedAt = (DateTimeOffset?)null, IsDeleted = false });
        builder.Entity<WorkflowTransition>().HasData(BuiltInWorkflow.Transitions.Select(x => new { x.Id, WorkflowSchemeId = BuiltInWorkflow.SchemeId, FromStatus = x.From, ToStatus = x.To, x.SortOrder, CreatedAt = BuiltInWorkflow.SeededAt, UpdatedAt = (DateTimeOffset?)null, IsDeleted = false }));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries().Where(x => x.Entity is TaskFlow.Domain.Common.Entity && x.State == EntityState.Modified))
            ((TaskFlow.Domain.Common.Entity)entry.Entity).UpdatedAt = DateTimeOffset.UtcNow;
        return await base.SaveChangesAsync(cancellationToken);
    }
}
