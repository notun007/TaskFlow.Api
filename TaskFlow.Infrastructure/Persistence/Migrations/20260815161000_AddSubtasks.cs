using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TaskFlow.Infrastructure.Persistence;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations;

[DbContext(typeof(TaskFlowDbContext))]
[Migration("20260815161000_AddSubtasks")]
public partial class AddSubtasks : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(name: "ParentTaskId", schema: "TASKFLOW", table: "TASKS", type: "RAW(16)", nullable: true);
        migrationBuilder.CreateIndex(name: "IX_TASKS_ParentTaskId", schema: "TASKFLOW", table: "TASKS", column: "ParentTaskId");
        migrationBuilder.AddForeignKey(name: "FK_TASKS_TASKS_ParentTaskId", schema: "TASKFLOW", table: "TASKS", column: "ParentTaskId", principalSchema: "TASKFLOW", principalTable: "TASKS", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_TASKS_TASKS_ParentTaskId", schema: "TASKFLOW", table: "TASKS");
        migrationBuilder.DropIndex(name: "IX_TASKS_ParentTaskId", schema: "TASKFLOW", table: "TASKS");
        migrationBuilder.DropColumn(name: "ParentTaskId", schema: "TASKFLOW", table: "TASKS");
    }
}
