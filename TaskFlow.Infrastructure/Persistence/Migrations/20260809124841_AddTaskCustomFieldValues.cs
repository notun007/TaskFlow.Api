using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskCustomFieldValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskCustomFieldValues",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TaskItemId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CustomFieldDefinitionId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Value = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCustomFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCustomFieldValues_CustomFieldDefinitions_CustomFieldDefinitionId",
                        column: x => x.CustomFieldDefinitionId,
                        principalSchema: "TASKFLOW",
                        principalTable: "CustomFieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskCustomFieldValues_TASKS_TaskItemId",
                        column: x => x.TaskItemId,
                        principalSchema: "TASKFLOW",
                        principalTable: "TASKS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskCustomFieldValues_CustomFieldDefinitionId",
                schema: "TASKFLOW",
                table: "TaskCustomFieldValues",
                column: "CustomFieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCustomFieldValues_TaskItemId_CustomFieldDefinitionId",
                schema: "TASKFLOW",
                table: "TaskCustomFieldValues",
                columns: new[] { "TaskItemId", "CustomFieldDefinitionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskCustomFieldValues",
                schema: "TASKFLOW");
        }
    }
}
