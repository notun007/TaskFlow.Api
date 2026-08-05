using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "TASKFLOW");

            migrationBuilder.Sql(@"
BEGIN
    EXECUTE IMMEDIATE 'CREATE TABLE ""TASKFLOW"".""AspNetRoles"" (
        ""Id"" RAW(16) NOT NULL,
        ""Name"" NVARCHAR2(256),
        ""NormalizedName"" NVARCHAR2(256),
        ""ConcurrencyStamp"" NVARCHAR2(2000),
        CONSTRAINT ""PK_AspNetRoles"" PRIMARY KEY (""Id"")
    )';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -955 THEN
            RAISE;
        END IF;
END;");

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DisplayName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Department = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    JobTitle = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsActive = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    UserName = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditEntries",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EntityName = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    EntityId = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Action = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ActorReference = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ChangesJson = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IpAddress = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    SupportEmail = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SupportPhone = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ContractReference = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SlaDetails = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    RoleId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ClaimType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ClaimValue = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "TASKFLOW",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ClaimType = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ClaimValue = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "TASKFLOW",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "TASKFLOW",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "TASKFLOW",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "TASKFLOW",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    RoleId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "TASKFLOW",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "TASKFLOW",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "TASKFLOW",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    LoginProvider = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Value = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "TASKFLOW",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "TASKFLOW",
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoftwareApplications",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IsThirdParty = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    VendorId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    BusinessOwner = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TechnicalOwner = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SupportTeam = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Criticality = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Technology = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CurrentVersion = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IsProduction = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwareApplications_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalSchema: "TASKFLOW",
                        principalTable: "Vendors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    ProjectKey = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Objectives = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    StartDate = table.Column<string>(type: "NVARCHAR2(10)", nullable: true),
                    TargetDate = table.Column<string>(type: "NVARCHAR2(10)", nullable: true),
                    ProjectManager = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Sponsor = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SoftwareApplicationId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_SoftwareApplications_SoftwareApplicationId",
                        column: x => x.SoftwareApplicationId,
                        principalSchema: "TASKFLOW",
                        principalTable: "SoftwareApplications",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TASKS",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TaskNumber = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    Title = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Type = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Priority = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Severity = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ProjectId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SoftwareApplicationId = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    Environment = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DueDate = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    SlaDueDate = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    Resolution = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Impact = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ReproductionSteps = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ExpectedResult = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    ActualResult = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    RootCause = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Workaround = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Source = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    SourceReference = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TASKS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TASKS_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "TASKFLOW",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TASKS_SoftwareApplications_SoftwareApplicationId",
                        column: x => x.SoftwareApplicationId,
                        principalSchema: "TASKFLOW",
                        principalTable: "SoftwareApplications",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaskAssignments",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TaskItemId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Responsibility = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PartyReference = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DisplayName = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskAssignments_TASKS_TaskItemId",
                        column: x => x.TaskItemId,
                        principalSchema: "TASKFLOW",
                        principalTable: "TASKS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskComments",
                schema: "TASKFLOW",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TaskItemId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    AuthorReference = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Body = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    IsDeleted = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskComments_TASKS_TaskItemId",
                        column: x => x.TaskItemId,
                        principalSchema: "TASKFLOW",
                        principalTable: "TASKS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "TASKFLOW",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "TASKFLOW",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "\"NormalizedName\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "TASKFLOW",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "TASKFLOW",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "TASKFLOW",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "TASKFLOW",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "TASKFLOW",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "\"NormalizedUserName\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_EntityName_EntityId",
                schema: "TASKFLOW",
                table: "AuditEntries",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SoftwareApplicationId",
                schema: "TASKFLOW",
                table: "Projects",
                column: "SoftwareApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareApplications_VendorId",
                schema: "TASKFLOW",
                table: "SoftwareApplications",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_TaskItemId",
                schema: "TASKFLOW",
                table: "TaskAssignments",
                column: "TaskItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_TaskItemId",
                schema: "TASKFLOW",
                table: "TaskComments",
                column: "TaskItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TASKS_ProjectId",
                schema: "TASKFLOW",
                table: "TASKS",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TASKS_SoftwareApplicationId",
                schema: "TASKFLOW",
                table: "TASKS",
                column: "SoftwareApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_TASKS_TaskNumber",
                schema: "TASKFLOW",
                table: "TASKS",
                column: "TaskNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_DepartmentId",
                schema: "TASKFLOW",
                table: "Teams",
                column: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "AuditEntries",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "TaskAssignments",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "TaskComments",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "Teams",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "TASKS",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "Departments",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "Projects",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "SoftwareApplications",
                schema: "TASKFLOW");

            migrationBuilder.DropTable(
                name: "Vendors",
                schema: "TASKFLOW");
        }
    }
}
