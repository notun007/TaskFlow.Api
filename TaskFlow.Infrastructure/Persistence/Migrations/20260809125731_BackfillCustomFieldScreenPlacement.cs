using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillCustomFieldScreenPlacement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"TASKFLOW\".\"CustomFieldContexts\" SET \"SectionName\" = 'Additional information', \"ShowOnCreate\" = 1, \"ShowOnEdit\" = 1, \"ShowOnDetails\" = 1 WHERE \"SectionName\" = ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
