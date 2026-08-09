using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskAttachments",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TaskItemId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FileName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ContentType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Size = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: false),
                    UploadedBy = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskAttachments_TASKS_TaskItemId",
                        column: x => x.TaskItemId,
                        principalSchema: "TASKFLOW",
                        principalTable: "TASKS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskAttachments_TaskItemId_CreatedAt",
                schema: "TASKFLOW",
                table: "TaskAttachments",
                columns: new[] { "TaskItemId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskAttachments",
                schema: "TASKFLOW");
        }
    }
}
