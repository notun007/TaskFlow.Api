using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectBoardSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectBoards",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ProjectId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectBoards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectBoards_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "TASKFLOW",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectBoardColumns",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ProjectBoardId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    SortOrder = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    WipLimit = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Status = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    IsDefaultDestination = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectBoardColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectBoardColumns_ProjectBoards_ProjectBoardId",
                        column: x => x.ProjectBoardId,
                        principalSchema: "TASKFLOW",
                        principalTable: "ProjectBoards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBoardColumns_ProjectBoardId_Status",
                schema: "TASKFLOW",
                table: "ProjectBoardColumns",
                columns: new[] { "ProjectBoardId", "Status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBoards_ProjectId",
                schema: "TASKFLOW",
                table: "ProjectBoards",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectBoardColumns",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "ProjectBoards",
                schema: "TASKFLOW");
        }
    }
}
