using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskLinks",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SourceTaskId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TargetTaskId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Type = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskLinks_TASKS_SourceTaskId",
                        column: x => x.SourceTaskId,
                        principalSchema: "TASKFLOW",
                        principalTable: "TASKS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskLinks_TASKS_TargetTaskId",
                        column: x => x.TargetTaskId,
                        principalSchema: "TASKFLOW",
                        principalTable: "TASKS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskLinks_SourceTaskId_TargetTaskId_Type",
                schema: "TASKFLOW",
                table: "TaskLinks",
                columns: new[] { "SourceTaskId", "TargetTaskId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskLinks_TargetTaskId",
                schema: "TASKFLOW",
                table: "TaskLinks",
                column: "TargetTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskLinks",
                schema: "TASKFLOW");
        }
    }
}
