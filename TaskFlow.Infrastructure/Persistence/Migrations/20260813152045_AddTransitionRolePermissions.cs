using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransitionRolePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransitionRolePermissions",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FromStatus = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    ToStatus = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Role = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransitionRolePermissions", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "TASKFLOW",
                table: "TransitionRolePermissions",
                columns: new[] { "Id", "CreatedAt", "FromStatus", "IsDeleted", "Role", "ToStatus", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0001-000000000001"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Draft", false, "Requester", "Submitted", null },
                    { new Guid("30000000-0000-0000-0001-000000000002"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Draft", false, "ProjectAdmin", "Submitted", null },
                    { new Guid("30000000-0000-0000-0001-000000000003"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Draft", false, "Requester", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-000000000004"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Draft", false, "ProjectAdmin", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-000000000005"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Submitted", false, "ProductOwner", "Triaged", null },
                    { new Guid("30000000-0000-0000-0001-000000000006"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Submitted", false, "TeamLead", "Triaged", null },
                    { new Guid("30000000-0000-0000-0001-000000000007"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Submitted", false, "ProjectAdmin", "Triaged", null },
                    { new Guid("30000000-0000-0000-0001-000000000008"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Submitted", false, "ProductOwner", "Rejected", null },
                    { new Guid("30000000-0000-0000-0001-000000000009"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Submitted", false, "ProjectAdmin", "Rejected", null },
                    { new Guid("30000000-0000-0000-0001-00000000000a"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Submitted", false, "ProductOwner", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-00000000000b"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Submitted", false, "ProjectAdmin", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-00000000000c"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Triaged", false, "ProductOwner", "Approved", null },
                    { new Guid("30000000-0000-0000-0001-00000000000d"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Triaged", false, "ProjectAdmin", "Approved", null },
                    { new Guid("30000000-0000-0000-0001-00000000000e"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Triaged", false, "ProductOwner", "PendingInformation", null },
                    { new Guid("30000000-0000-0000-0001-00000000000f"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Triaged", false, "ProjectAdmin", "PendingInformation", null },
                    { new Guid("30000000-0000-0000-0001-000000000010"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Triaged", false, "ProductOwner", "Rejected", null },
                    { new Guid("30000000-0000-0000-0001-000000000011"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Triaged", false, "ProjectAdmin", "Rejected", null },
                    { new Guid("30000000-0000-0000-0001-000000000012"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Triaged", false, "ProductOwner", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-000000000013"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Triaged", false, "ProjectAdmin", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-000000000014"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Approved", false, "TeamLead", "Assigned", null },
                    { new Guid("30000000-0000-0000-0001-000000000015"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Approved", false, "ProjectAdmin", "Assigned", null },
                    { new Guid("30000000-0000-0000-0001-000000000016"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Approved", false, "TeamLead", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-000000000017"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Approved", false, "ProjectAdmin", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-000000000018"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Assigned", false, "TeamMember", "InProgress", null },
                    { new Guid("30000000-0000-0000-0001-000000000019"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Assigned", false, "ProjectAdmin", "InProgress", null },
                    { new Guid("30000000-0000-0000-0001-00000000001a"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Assigned", false, "TeamMember", "PendingInformation", null },
                    { new Guid("30000000-0000-0000-0001-00000000001b"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Assigned", false, "ProjectAdmin", "PendingInformation", null },
                    { new Guid("30000000-0000-0000-0001-00000000001c"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Assigned", false, "TeamMember", "PendingVendor", null },
                    { new Guid("30000000-0000-0000-0001-00000000001d"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Assigned", false, "ProjectAdmin", "PendingVendor", null },
                    { new Guid("30000000-0000-0000-0001-00000000001e"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Assigned", false, "TeamMember", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-00000000001f"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Assigned", false, "ProjectAdmin", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-000000000020"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, "TeamMember", "PendingInformation", null },
                    { new Guid("30000000-0000-0000-0001-000000000021"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, "ProjectAdmin", "PendingInformation", null },
                    { new Guid("30000000-0000-0000-0001-000000000022"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, "TeamMember", "PendingVendor", null },
                    { new Guid("30000000-0000-0000-0001-000000000023"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, "ProjectAdmin", "PendingVendor", null },
                    { new Guid("30000000-0000-0000-0001-000000000024"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, "TeamMember", "ReadyForTesting", null },
                    { new Guid("30000000-0000-0000-0001-000000000025"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, "ProjectAdmin", "ReadyForTesting", null },
                    { new Guid("30000000-0000-0000-0001-000000000026"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, "TeamMember", "Resolved", null },
                    { new Guid("30000000-0000-0000-0001-000000000027"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, "ProjectAdmin", "Resolved", null },
                    { new Guid("30000000-0000-0000-0001-000000000028"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, "TeamMember", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-000000000029"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", false, "ProjectAdmin", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-00000000002a"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingInformation", false, "TeamMember", "Triaged", null },
                    { new Guid("30000000-0000-0000-0001-00000000002b"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingInformation", false, "ProjectAdmin", "Triaged", null },
                    { new Guid("30000000-0000-0000-0001-00000000002c"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingInformation", false, "TeamMember", "Assigned", null },
                    { new Guid("30000000-0000-0000-0001-00000000002d"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingInformation", false, "ProjectAdmin", "Assigned", null },
                    { new Guid("30000000-0000-0000-0001-00000000002e"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingInformation", false, "TeamMember", "InProgress", null },
                    { new Guid("30000000-0000-0000-0001-00000000002f"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingInformation", false, "ProjectAdmin", "InProgress", null },
                    { new Guid("30000000-0000-0000-0001-000000000030"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingInformation", false, "TeamMember", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-000000000031"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingInformation", false, "ProjectAdmin", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-000000000032"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingVendor", false, "TeamMember", "Assigned", null },
                    { new Guid("30000000-0000-0000-0001-000000000033"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingVendor", false, "ProjectAdmin", "Assigned", null },
                    { new Guid("30000000-0000-0000-0001-000000000034"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingVendor", false, "TeamMember", "InProgress", null },
                    { new Guid("30000000-0000-0000-0001-000000000035"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingVendor", false, "ProjectAdmin", "InProgress", null },
                    { new Guid("30000000-0000-0000-0001-000000000036"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingVendor", false, "TeamMember", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-000000000037"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PendingVendor", false, "ProjectAdmin", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-000000000038"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "ReadyForTesting", false, "ReviewerTester", "InProgress", null },
                    { new Guid("30000000-0000-0000-0001-000000000039"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "ReadyForTesting", false, "ProjectAdmin", "InProgress", null },
                    { new Guid("30000000-0000-0000-0001-00000000003a"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "ReadyForTesting", false, "ReviewerTester", "Uat", null },
                    { new Guid("30000000-0000-0000-0001-00000000003b"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "ReadyForTesting", false, "ProjectAdmin", "Uat", null },
                    { new Guid("30000000-0000-0000-0001-00000000003c"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "ReadyForTesting", false, "ReviewerTester", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-00000000003d"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "ReadyForTesting", false, "ProjectAdmin", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-00000000003e"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Uat", false, "ReviewerTester", "InProgress", null },
                    { new Guid("30000000-0000-0000-0001-00000000003f"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Uat", false, "ProjectAdmin", "InProgress", null },
                    { new Guid("30000000-0000-0000-0001-000000000040"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Uat", false, "ReviewerTester", "Resolved", null },
                    { new Guid("30000000-0000-0000-0001-000000000041"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Uat", false, "ProjectAdmin", "Resolved", null },
                    { new Guid("30000000-0000-0000-0001-000000000042"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Uat", false, "ReviewerTester", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-000000000043"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Uat", false, "ProjectAdmin", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-000000000044"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Resolved", false, "Requester", "Closed", null },
                    { new Guid("30000000-0000-0000-0001-000000000045"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Resolved", false, "ProductOwner", "Closed", null },
                    { new Guid("30000000-0000-0000-0001-000000000046"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Resolved", false, "ProjectAdmin", "Closed", null },
                    { new Guid("30000000-0000-0000-0001-000000000047"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Resolved", false, "Requester", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-000000000048"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Resolved", false, "ProductOwner", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-000000000049"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Resolved", false, "ProjectAdmin", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-00000000004a"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Closed", false, "Requester", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-00000000004b"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Closed", false, "ProductOwner", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-00000000004c"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Closed", false, "ProjectAdmin", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-00000000004d"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Rejected", false, "Requester", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-00000000004e"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Rejected", false, "ProductOwner", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-00000000004f"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Rejected", false, "ProjectAdmin", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-000000000050"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Cancelled", false, "Requester", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-000000000051"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Cancelled", false, "ProductOwner", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-000000000052"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Cancelled", false, "ProjectAdmin", "Reopened", null },
                    { new Guid("30000000-0000-0000-0001-000000000053"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Reopened", false, "TeamMember", "Triaged", null },
                    { new Guid("30000000-0000-0000-0001-000000000054"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Reopened", false, "ProjectAdmin", "Triaged", null },
                    { new Guid("30000000-0000-0000-0001-000000000055"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Reopened", false, "TeamMember", "Assigned", null },
                    { new Guid("30000000-0000-0000-0001-000000000056"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Reopened", false, "ProjectAdmin", "Assigned", null },
                    { new Guid("30000000-0000-0000-0001-000000000057"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Reopened", false, "TeamMember", "InProgress", null },
                    { new Guid("30000000-0000-0000-0001-000000000058"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Reopened", false, "ProjectAdmin", "InProgress", null },
                    { new Guid("30000000-0000-0000-0001-000000000059"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Reopened", false, "TeamMember", "Cancelled", null },
                    { new Guid("30000000-0000-0000-0001-00000000005a"), new DateTimeOffset(new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Reopened", false, "ProjectAdmin", "Cancelled", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransitionRolePermissions_FromStatus_ToStatus_Role",
                schema: "TASKFLOW",
                table: "TransitionRolePermissions",
                columns: new[] { "FromStatus", "ToStatus", "Role" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransitionRolePermissions",
                schema: "TASKFLOW");
        }
    }
}
