using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskAssignmentUserReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedUserId",
                schema: "TASKFLOW",
                table: "TaskAssignments",
                type: "RAW(16)",
                nullable: true);

            // Preserve existing email-based user assignments where possible. Older GUID-text
            // references remain supported by the application until they are edited.
            migrationBuilder.Sql("""
                UPDATE "TASKFLOW"."TaskAssignments" assignment
                SET "AssignedUserId" = (
                    SELECT appUser."Id"
                    FROM "TASKFLOW"."AspNetUsers" appUser
                    WHERE UPPER(appUser."Email") = UPPER(assignment."PartyReference")
                      AND ROWNUM = 1)
                WHERE assignment."AssignedUserId" IS NULL
                  AND assignment."Responsibility" IN ('Assignee', 'Tester', 'UatOwner', 'Approver')
                  AND EXISTS (
                    SELECT 1
                    FROM "TASKFLOW"."AspNetUsers" appUser
                    WHERE UPPER(appUser."Email") = UPPER(assignment."PartyReference"))
                """);

            // Only migrate unchanged legacy Team Member defaults; explicitly configured
            // scopes such as AllProjectTasks or AssignedToCurrentUser are left untouched.
            migrationBuilder.Sql("""
                UPDATE "TASKFLOW"."TransitionRolePermissions"
                SET "TaskScope" = 'AssigneeIsCurrentUser'
                WHERE "Role" = 'TeamMember'
                  AND "TaskScope" = 'OwnedByCurrentUser'
                """);

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_AssignedUserId",
                schema: "TASKFLOW",
                table: "TaskAssignments",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_AspNetUsers_AssignedUserId",
                schema: "TASKFLOW",
                table: "TaskAssignments",
                column: "AssignedUserId",
                principalSchema: "TASKFLOW",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_AspNetUsers_AssignedUserId",
                schema: "TASKFLOW",
                table: "TaskAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_AssignedUserId",
                schema: "TASKFLOW",
                table: "TaskAssignments");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                schema: "TASKFLOW",
                table: "TaskAssignments");

            migrationBuilder.Sql("""
                UPDATE "TASKFLOW"."TransitionRolePermissions"
                SET "TaskScope" = 'OwnedByCurrentUser'
                WHERE "Role" = 'TeamMember'
                  AND "TaskScope" = 'AssigneeIsCurrentUser'
                """);
        }
    }
}
