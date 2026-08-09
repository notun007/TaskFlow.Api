using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomFieldScreenPlacement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SectionName",
                schema: "TASKFLOW",
                table: "CustomFieldContexts",
                type: "NVARCHAR2(2000)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ShowOnCreate",
                schema: "TASKFLOW",
                table: "CustomFieldContexts",
                type: "NUMBER(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowOnDetails",
                schema: "TASKFLOW",
                table: "CustomFieldContexts",
                type: "NUMBER(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowOnEdit",
                schema: "TASKFLOW",
                table: "CustomFieldContexts",
                type: "NUMBER(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SectionName",
                schema: "TASKFLOW",
                table: "CustomFieldContexts");

            migrationBuilder.DropColumn(
                name: "ShowOnCreate",
                schema: "TASKFLOW",
                table: "CustomFieldContexts");

            migrationBuilder.DropColumn(
                name: "ShowOnDetails",
                schema: "TASKFLOW",
                table: "CustomFieldContexts");

            migrationBuilder.DropColumn(
                name: "ShowOnEdit",
                schema: "TASKFLOW",
                table: "CustomFieldContexts");
        }
    }
}
