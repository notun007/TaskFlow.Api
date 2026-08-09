using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomFieldSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomFieldDefinitions",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Key = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Type = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsActive = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldContexts",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CustomFieldDefinitionId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    WorkItemTypeId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    IsRequired = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    DefaultValue = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldContexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomFieldContexts_CustomFieldDefinitions_CustomFieldDefinitionId",
                        column: x => x.CustomFieldDefinitionId,
                        principalSchema: "TASKFLOW",
                        principalTable: "CustomFieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomFieldContexts_WorkItemTypes_WorkItemTypeId",
                        column: x => x.WorkItemTypeId,
                        principalSchema: "TASKFLOW",
                        principalTable: "WorkItemTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldOptions",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CustomFieldDefinitionId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Value = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Label = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    SortOrder = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IsActive = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomFieldOptions_CustomFieldDefinitions_CustomFieldDefinitionId",
                        column: x => x.CustomFieldDefinitionId,
                        principalSchema: "TASKFLOW",
                        principalTable: "CustomFieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldContexts_CustomFieldDefinitionId_WorkItemTypeId",
                schema: "TASKFLOW",
                table: "CustomFieldContexts",
                columns: new[] { "CustomFieldDefinitionId", "WorkItemTypeId" },
                unique: true,
                filter: "\"WorkItemTypeId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldContexts_WorkItemTypeId",
                schema: "TASKFLOW",
                table: "CustomFieldContexts",
                column: "WorkItemTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldDefinitions_Key",
                schema: "TASKFLOW",
                table: "CustomFieldDefinitions",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldDefinitions_Name",
                schema: "TASKFLOW",
                table: "CustomFieldDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldOptions_CustomFieldDefinitionId_Value",
                schema: "TASKFLOW",
                table: "CustomFieldOptions",
                columns: new[] { "CustomFieldDefinitionId", "Value" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomFieldContexts",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "CustomFieldOptions",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "CustomFieldDefinitions",
                schema: "TASKFLOW");
        }
    }
}
