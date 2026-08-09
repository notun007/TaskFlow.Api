using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskStatusHistory",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TaskItemId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FromStatus = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ToStatus = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ActorReference = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Comment = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskStatusHistory_TASKS_TaskItemId",
                        column: x => x.TaskItemId,
                        principalSchema: "TASKFLOW",
                        principalTable: "TASKS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskStatusHistory_TaskItemId_CreatedAt",
                schema: "TASKFLOW",
                table: "TaskStatusHistory",
                columns: new[] { "TaskItemId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskStatusHistory",
                schema: "TASKFLOW");
        }
    }
}
