using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSprintPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SprintId",
                schema: "TASKFLOW",
                table: "TASKS",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Sprints",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Goal = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ProjectId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    StartDate = table.Column<string>(type: "NVARCHAR2(10)", nullable: true),
                    EndDate = table.Column<string>(type: "NVARCHAR2(10)", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sprints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sprints_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "TASKFLOW",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TASKS_SprintId",
                schema: "TASKFLOW",
                table: "TASKS",
                column: "SprintId");

            migrationBuilder.CreateIndex(
                name: "IX_Sprints_ProjectId_Name",
                schema: "TASKFLOW",
                table: "Sprints",
                columns: new[] { "ProjectId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TASKS_Sprints_SprintId",
                schema: "TASKFLOW",
                table: "TASKS",
                column: "SprintId",
                principalSchema: "TASKFLOW",
                principalTable: "Sprints",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TASKS_Sprints_SprintId",
                schema: "TASKFLOW",
                table: "TASKS");

            migrationBuilder.DropTable(
                name: "Sprints",
                schema: "TASKFLOW");

            migrationBuilder.DropIndex(
                name: "IX_TASKS_SprintId",
                schema: "TASKFLOW",
                table: "TASKS");

            migrationBuilder.DropColumn(
                name: "SprintId",
                schema: "TASKFLOW",
                table: "TASKS");
        }
    }
}
