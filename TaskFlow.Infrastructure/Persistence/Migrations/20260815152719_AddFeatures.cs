using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FeatureId",
                schema: "TASKFLOW",
                table: "TASKS",
                type: "RAW(16)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Features",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ProjectId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EpicId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TargetDate = table.Column<string>(type: "NVARCHAR2(10)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Features_Epics_EpicId",
                        column: x => x.EpicId,
                        principalSchema: "TASKFLOW",
                        principalTable: "Epics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Features_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "TASKFLOW",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TASKS_FeatureId",
                schema: "TASKFLOW",
                table: "TASKS",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Features_EpicId_Name",
                schema: "TASKFLOW",
                table: "Features",
                columns: new[] { "EpicId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Features_ProjectId",
                schema: "TASKFLOW",
                table: "Features",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_TASKS_Features_FeatureId",
                schema: "TASKFLOW",
                table: "TASKS",
                column: "FeatureId",
                principalSchema: "TASKFLOW",
                principalTable: "Features",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TASKS_Features_FeatureId",
                schema: "TASKFLOW",
                table: "TASKS");

            migrationBuilder.DropTable(
                name: "Features",
                schema: "TASKFLOW");

            migrationBuilder.DropIndex(
                name: "IX_TASKS_FeatureId",
                schema: "TASKFLOW",
                table: "TASKS");

            migrationBuilder.DropColumn(
                name: "FeatureId",
                schema: "TASKFLOW",
                table: "TASKS");
        }
    }
}
