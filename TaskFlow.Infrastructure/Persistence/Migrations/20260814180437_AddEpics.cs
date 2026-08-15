using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEpics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EpicId",
                schema: "TASKFLOW",
                table: "TASKS",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Epics",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ProjectId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TargetDate = table.Column<string>(type: "NVARCHAR2(10)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Epics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Epics_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "TASKFLOW",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TASKS_EpicId",
                schema: "TASKFLOW",
                table: "TASKS",
                column: "EpicId");

            migrationBuilder.CreateIndex(
                name: "IX_Epics_ProjectId_Name",
                schema: "TASKFLOW",
                table: "Epics",
                columns: new[] { "ProjectId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TASKS_Epics_EpicId",
                schema: "TASKFLOW",
                table: "TASKS",
                column: "EpicId",
                principalSchema: "TASKFLOW",
                principalTable: "Epics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TASKS_Epics_EpicId",
                schema: "TASKFLOW",
                table: "TASKS");

            migrationBuilder.DropTable(
                name: "Epics",
                schema: "TASKFLOW");

            migrationBuilder.DropIndex(
                name: "IX_TASKS_EpicId",
                schema: "TASKFLOW",
                table: "TASKS");

            migrationBuilder.DropColumn(
                name: "EpicId",
                schema: "TASKFLOW",
                table: "TASKS");
        }
    }
}
