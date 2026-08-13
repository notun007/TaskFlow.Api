using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectWorkflows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                schema: "TASKFLOW",
                table: "WorkflowSchemes",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "TASKFLOW",
                table: "WorkflowSchemes",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "ProjectId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSchemes_ProjectId",
                schema: "TASKFLOW",
                table: "WorkflowSchemes",
                column: "ProjectId",
                unique: true,
                filter: "\"ProjectId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSchemes_Projects_ProjectId",
                schema: "TASKFLOW",
                table: "WorkflowSchemes",
                column: "ProjectId",
                principalSchema: "TASKFLOW",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSchemes_Projects_ProjectId",
                schema: "TASKFLOW",
                table: "WorkflowSchemes");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowSchemes_ProjectId",
                schema: "TASKFLOW",
                table: "WorkflowSchemes");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "TASKFLOW",
                table: "WorkflowSchemes");
        }
    }
}
