using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandAuditChangesJsonToClob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Oracle does not support changing NVARCHAR2 directly to CLOB.
            // Copy through a temporary column so existing audit history is retained.
            migrationBuilder.Sql("ALTER TABLE \"TASKFLOW\".\"AuditEntries\" ADD (\"ChangesJson_CLOB\" CLOB)");
            migrationBuilder.Sql("UPDATE \"TASKFLOW\".\"AuditEntries\" SET \"ChangesJson_CLOB\" = TO_CLOB(\"ChangesJson\")");
            migrationBuilder.Sql("ALTER TABLE \"TASKFLOW\".\"AuditEntries\" DROP COLUMN \"ChangesJson\"");
            migrationBuilder.Sql("ALTER TABLE \"TASKFLOW\".\"AuditEntries\" RENAME COLUMN \"ChangesJson_CLOB\" TO \"ChangesJson\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"TASKFLOW\".\"AuditEntries\" ADD (\"ChangesJson_TEXT\" NVARCHAR2(2000))");
            migrationBuilder.Sql("UPDATE \"TASKFLOW\".\"AuditEntries\" SET \"ChangesJson_TEXT\" = DBMS_LOB.SUBSTR(\"ChangesJson\", 2000, 1)");
            migrationBuilder.Sql("ALTER TABLE \"TASKFLOW\".\"AuditEntries\" DROP COLUMN \"ChangesJson\"");
            migrationBuilder.Sql("ALTER TABLE \"TASKFLOW\".\"AuditEntries\" RENAME COLUMN \"ChangesJson_TEXT\" TO \"ChangesJson\"");
        }
    }
}
