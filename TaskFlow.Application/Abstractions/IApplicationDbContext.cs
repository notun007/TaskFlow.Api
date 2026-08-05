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
    DbSet<TaskItem> Tasks { get; }
    DbSet<TaskAssignment> TaskAssignments { get; }
    DbSet<TaskComment> TaskComments { get; }
    DbSet<AuditEntry> AuditEntries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
