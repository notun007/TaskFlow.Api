using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurableWorkflows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkflowSchemes",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    WorkItemTypeId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    IsDefault = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowSchemes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowSchemes_WorkItemTypes_WorkItemTypeId",
                        column: x => x.WorkItemTypeId,
                        principalSchema: "TASKFLOW",
                        principalTable: "WorkItemTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitions",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    WorkflowSchemeId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FromStatus = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    ToStatus = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    SortOrder = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_WorkflowSchemes_WorkflowSchemeId",
                        column: x => x.WorkflowSchemeId,
                        principalSchema: "TASKFLOW",
                        principalTable: "WorkflowSchemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "TASKFLOW",
                table: "WorkflowSchemes",
                columns: new[] { "Id", "CreatedAt", "IsDefault", "IsDeleted", "Name", "UpdatedAt", "WorkItemTypeId" },
                values: new object[] { new Guid("20000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, false, "Default workflow", null, null });

            migrationBuilder.InsertData(
                schema: "TASKFLOW",
                table: "WorkflowTransitions",
                columns: new[] { "Id", "CreatedAt", "FromStatus", "IsDeleted", "SortOrder", "ToStatus", "UpdatedAt", "WorkflowSchemeId" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0001-000000000001"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Draft", false, 10, "Submitted", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000002"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Draft", false, 20, "Cancelled", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000003"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Submitted", false, 30, "Triaged", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000004"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Submitted", false, 40, "Rejected", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000005"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Submitted", false, 50, "Cancelled", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000006"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Triaged", false, 60, "Approved", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000007"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Triaged", false, 70, "PendingInformation", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000008"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Triaged", false, 80, "Rejected", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000009"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Triaged", false, 90, "Cancelled", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-00000000000a"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Approved", false, 100, "Assigned", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-00000000000b"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Approved", false, 110, "Cancelled", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-00000000000c"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Assigned", false, 120, "InProgress", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-00000000000d"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Assigned", false, 130, "PendingInformation", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-00000000000e"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Assigned", false, 140, "PendingVendor", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-00000000000f"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Assigned", false, 150, "Cancelled", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000010"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, 160, "PendingInformation", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000011"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, 170, "PendingVendor", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000012"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, 180, "ReadyForTesting", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000013"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, 190, "Resolved", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000014"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, 200, "Cancelled", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000015"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingInformation", false, 210, "Triaged", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000016"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingInformation", false, 220, "Assigned", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000017"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingInformation", false, 230, "InProgress", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000018"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingInformation", false, 240, "Cancelled", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000019"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingVendor", false, 250, "Assigned", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-00000000001a"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingVendor", false, 260, "InProgress", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-00000000001b"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingVendor", false, 270, "Cancelled", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-00000000001c"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "ReadyForTesting", false, 280, "Uat", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-00000000001d"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "ReadyForTesting", false, 290, "InProgress", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-00000000001e"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "ReadyForTesting", false, 300, "Reopened", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-00000000001f"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Uat", false, 310, "Resolved", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000020"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Uat", false, 320, "InProgress", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000021"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Uat", false, 330, "Reopened", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000022"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Resolved", false, 340, "Closed", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000023"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Resolved", false, 350, "Reopened", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000024"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Closed", false, 360, "Reopened", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000025"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Rejected", false, 370, "Reopened", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000026"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Cancelled", false, 380, "Reopened", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000027"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Reopened", false, 390, "Triaged", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000028"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Reopened", false, 400, "Assigned", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-000000000029"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Reopened", false, 410, "InProgress", null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0001-00000000002a"), new DateTimeOffset(new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Reopened", false, 420, "Cancelled", null, new Guid("20000000-0000-0000-0000-000000000001") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSchemes_WorkItemTypeId",
                schema: "TASKFLOW",
                table: "WorkflowSchemes",
                column: "WorkItemTypeId",
                unique: true,
                filter: "\"WorkItemTypeId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_WorkflowSchemeId_FromStatus_ToStatus",
                schema: "TASKFLOW",
                table: "WorkflowTransitions",
                columns: new[] { "WorkflowSchemeId", "FromStatus", "ToStatus" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowTransitions",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "WorkflowSchemes",
                schema: "TASKFLOW");
        }
    }
}
