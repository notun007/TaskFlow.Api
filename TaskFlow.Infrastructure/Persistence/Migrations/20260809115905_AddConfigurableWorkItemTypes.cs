using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurableWorkItemTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkItemTypes",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Key = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsActive = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    IsSystem = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItemTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "TASKFLOW",
                table: "WorkItemTypes",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "IsDeleted", "IsSystem", "Key", "Name", "SortOrder", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "A defect or unexpected system behavior.", true, false, true, "Bug", "Bug", 10, null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "A business or functional requirement.", true, false, true, "Requirement", "Requirement", 20, null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "A controlled request to change an existing service.", true, false, true, "ChangeRequest", "Change request", 30, null },
                    { new Guid("10000000-0000-0000-0000-000000000004"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "An improvement to an existing capability.", true, false, true, "Enhancement", "Enhancement", 40, null },
                    { new Guid("10000000-0000-0000-0000-000000000005"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "An operational incident requiring restoration.", true, false, true, "Incident", "Incident", 50, null },
                    { new Guid("10000000-0000-0000-0000-000000000006"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "An action captured from a meeting or committee.", true, false, true, "MeetingAction", "Meeting action", 60, null },
                    { new Guid("10000000-0000-0000-0000-000000000007"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Testing or quality-assurance work.", true, false, true, "Testing", "Testing", 70, null },
                    { new Guid("10000000-0000-0000-0000-000000000008"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "A deployment or release activity.", true, false, true, "Deployment", "Deployment", 80, null },
                    { new Guid("10000000-0000-0000-0000-000000000009"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Planned system or service maintenance.", true, false, true, "Maintenance", "Maintenance", 90, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemTypes_Key",
                schema: "TASKFLOW",
                table: "WorkItemTypes",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemTypes_Name",
                schema: "TASKFLOW",
                table: "WorkItemTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkItemTypes",
                schema: "TASKFLOW");
        }
    }
}
