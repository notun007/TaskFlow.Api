using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations;

public partial class AddPrimaryTaskAssignments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsPrimary",
            schema: "TASKFLOW",
            table: "TaskAssignments",
            type: "NUMBER(1)",
            nullable: false,
            defaultValue: false);

        // Keep existing assignments usable: the oldest Tester/UAT Owner becomes primary.
        migrationBuilder.Sql("""
            MERGE INTO "TASKFLOW"."TaskAssignments" target
            USING (
                SELECT "Id"
                FROM (
                    SELECT "Id", ROW_NUMBER() OVER (
                        PARTITION BY "TaskItemId", "Responsibility"
                        ORDER BY "CreatedAt", "Id") AS rn
                    FROM "TASKFLOW"."TaskAssignments"
                    WHERE "IsDeleted" = 0 AND "Responsibility" IN ('Tester', 'UatOwner')
                )
                WHERE rn = 1
            ) source
            ON (target."Id" = source."Id")
            WHEN MATCHED THEN UPDATE SET target."IsPrimary" = 1
            """);

        migrationBuilder.Sql("""
            UPDATE "TASKFLOW"."TransitionRolePermissions"
            SET "TaskScope" = 'PrimaryAssignedToCurrentUser'
            WHERE "Role" = 'ReviewerTester'
              AND "TaskScope" = 'AssignedToCurrentUser'
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE "TASKFLOW"."TransitionRolePermissions"
            SET "TaskScope" = 'AssignedToCurrentUser'
            WHERE "Role" = 'ReviewerTester'
              AND "TaskScope" = 'PrimaryAssignedToCurrentUser'
            """);

        migrationBuilder.DropColumn(
            name: "IsPrimary",
            schema: "TASKFLOW",
            table: "TaskAssignments");
    }
}
