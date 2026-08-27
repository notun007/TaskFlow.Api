using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(TaskFlowDbContext))]
    [Migration("20260827100000_EnforceSingleAssigneeAndTester")]
    public partial class EnforceSingleAssigneeAndTester : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "TASKFLOW"."TaskAssignments" assignment
                SET assignment."IsDeleted" = 1,
                    assignment."IsPrimary" = 0
                WHERE assignment."Id" IN (
                    SELECT ranked."Id"
                    FROM (
                        SELECT candidate."Id",
                               ROW_NUMBER() OVER (
                                   PARTITION BY candidate."TaskItemId", candidate."Responsibility"
                                   ORDER BY CASE WHEN candidate."IsPrimary" = 1 THEN 0 ELSE 1 END,
                                            candidate."CreatedAt" DESC,
                                            candidate."Id") AS row_number
                        FROM "TASKFLOW"."TaskAssignments" candidate
                        WHERE candidate."IsDeleted" = 0
                          AND candidate."Responsibility" IN ('Assignee', 'Tester')) ranked
                    WHERE ranked.row_number > 1)
                """);

            migrationBuilder.Sql("""
                UPDATE "TASKFLOW"."TaskAssignments"
                SET "IsPrimary" = 0
                WHERE "IsDeleted" = 0
                  AND "Responsibility" IN ('Assignee', 'Tester')
                """);

            migrationBuilder.Sql("""
                UPDATE "TASKFLOW"."TransitionRolePermissions"
                SET "TaskScope" = 'TesterIsCurrentUser'
                WHERE "Role" = 'ReviewerTester'
                  AND "TaskScope" IN ('AssignedToCurrentUser', 'PrimaryAssignedToCurrentUser')
                """);

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX "TASKFLOW"."UX_TaskAssignments_SingleActiveAssigneeTester"
                ON "TASKFLOW"."TaskAssignments" (
                    CASE WHEN "IsDeleted" = 0 AND "Responsibility" IN ('Assignee', 'Tester') THEN "TaskItemId" END,
                    CASE WHEN "IsDeleted" = 0 AND "Responsibility" IN ('Assignee', 'Tester') THEN "Responsibility" END)
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX "TASKFLOW"."UX_TaskAssignments_SingleActiveAssigneeTester"
                """);

            migrationBuilder.Sql("""
                UPDATE "TASKFLOW"."TransitionRolePermissions"
                SET "TaskScope" = 'PrimaryAssignedToCurrentUser'
                WHERE "Role" = 'ReviewerTester'
                  AND "TaskScope" = 'TesterIsCurrentUser'
                """);
        }
    }
}
