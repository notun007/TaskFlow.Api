using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TaskFlow.Infrastructure.Persistence;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations;

[DbContext(typeof(TaskFlowDbContext))]
[Migration("20260816143000_AddTaskOwnershipAndEffort")]
public partial class AddTaskOwnershipAndEffort : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(name: "OwnerUserId", schema: "TASKFLOW", table: "TASKS", type: "RAW(16)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "OwnerDisplayName", schema: "TASKFLOW", table: "TASKS", type: "NVARCHAR2(2000)", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "ReporterUserId", schema: "TASKFLOW", table: "TASKS", type: "RAW(16)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ReporterDisplayName", schema: "TASKFLOW", table: "TASKS", type: "NVARCHAR2(2000)", nullable: true);
        migrationBuilder.AddColumn<int>(name: "EstimatedEffortMinutes", schema: "TASKFLOW", table: "TASKS", type: "NUMBER(10)", nullable: true);
        migrationBuilder.CreateIndex(name: "IX_TASKS_OwnerUserId", schema: "TASKFLOW", table: "TASKS", column: "OwnerUserId");
        migrationBuilder.CreateIndex(name: "IX_TASKS_ReporterUserId", schema: "TASKFLOW", table: "TASKS", column: "ReporterUserId");
        migrationBuilder.AddForeignKey(name: "FK_TASKS_AspNetUsers_OwnerUserId", schema: "TASKFLOW", table: "TASKS", column: "OwnerUserId", principalSchema: "TASKFLOW", principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
        migrationBuilder.AddForeignKey(name: "FK_TASKS_AspNetUsers_ReporterUserId", schema: "TASKFLOW", table: "TASKS", column: "ReporterUserId", principalSchema: "TASKFLOW", principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_TASKS_AspNetUsers_OwnerUserId", schema: "TASKFLOW", table: "TASKS");
        migrationBuilder.DropForeignKey(name: "FK_TASKS_AspNetUsers_ReporterUserId", schema: "TASKFLOW", table: "TASKS");
        migrationBuilder.DropIndex(name: "IX_TASKS_OwnerUserId", schema: "TASKFLOW", table: "TASKS");
        migrationBuilder.DropIndex(name: "IX_TASKS_ReporterUserId", schema: "TASKFLOW", table: "TASKS");
        migrationBuilder.DropColumn(name: "OwnerUserId", schema: "TASKFLOW", table: "TASKS");
        migrationBuilder.DropColumn(name: "OwnerDisplayName", schema: "TASKFLOW", table: "TASKS");
        migrationBuilder.DropColumn(name: "ReporterUserId", schema: "TASKFLOW", table: "TASKS");
        migrationBuilder.DropColumn(name: "ReporterDisplayName", schema: "TASKFLOW", table: "TASKS");
        migrationBuilder.DropColumn(name: "EstimatedEffortMinutes", schema: "TASKFLOW", table: "TASKS");
    }
}
