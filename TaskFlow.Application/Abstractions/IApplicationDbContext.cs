using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Department> Departments { get; }
    DbSet<Team> Teams { get; }
    DbSet<Vendor> Vendors { get; }
    DbSet<SoftwareApplication> SoftwareApplications { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectRoleAssignment> ProjectRoleAssignments { get; }
    DbSet<Sprint> Sprints { get; }
    DbSet<ProjectRelease> ProjectReleases { get; }
    DbSet<TaskItem> Tasks { get; }
    DbSet<TaskAssignment> TaskAssignments { get; }
    DbSet<TaskComment> TaskComments { get; }
    DbSet<TaskStatusHistory> TaskStatusHistory { get; }
    DbSet<TaskCustomFieldValue> TaskCustomFieldValues { get; }
    DbSet<TaskLink> TaskLinks { get; }
    DbSet<TaskAttachment> TaskAttachments { get; }
    DbSet<WorkItemType> WorkItemTypes { get; }
    DbSet<CustomFieldDefinition> CustomFieldDefinitions { get; }
    DbSet<CustomFieldOption> CustomFieldOptions { get; }
    DbSet<CustomFieldContext> CustomFieldContexts { get; }
    DbSet<WorkflowScheme> WorkflowSchemes { get; }
    DbSet<WorkflowTransition> WorkflowTransitions { get; }
    DbSet<ProjectBoard> ProjectBoards { get; }
    DbSet<ProjectBoardColumn> ProjectBoardColumns { get; }
    DbSet<AuditEntry> AuditEntries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
