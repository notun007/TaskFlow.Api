using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TaskFlow.Infrastructure.Persistence;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations;

[DbContext(typeof(TaskFlowDbContext))]
[Migration("20260817093000_AddTransitionTaskScopes")]
public partial class AddTransitionTaskScopes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TaskScope",
            schema: "TASKFLOW",
            table: "TransitionRolePermissions",
            type: "NVARCHAR2(64)",
            nullable: false,
            defaultValue: "AllProjectTasks");

        migrationBuilder.Sql("UPDATE \"TASKFLOW\".\"TransitionRolePermissions\" SET \"TaskScope\" = 'ReportedByCurrentUser' WHERE \"Role\" = 'Requester'");
        migrationBuilder.Sql("UPDATE \"TASKFLOW\".\"TransitionRolePermissions\" SET \"TaskScope\" = 'OwnedByCurrentUser' WHERE \"Role\" = 'TeamMember'");
        migrationBuilder.Sql("UPDATE \"TASKFLOW\".\"TransitionRolePermissions\" SET \"TaskScope\" = 'AssignedToCurrentUser' WHERE \"Role\" = 'ReviewerTester'");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(name: "TaskScope", schema: "TASKFLOW", table: "TransitionRolePermissions");
}
