using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RobustRelationsOnDomains2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Apps_AppId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserApps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_AppId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserAppStatuses_UserId",
                table: "UserAppStatuses");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: new Guid("0f1e2d3c-4b5a-6978-8e9d-abcdef123456"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: new Guid("1c2d3e4f-5a6b-7c8d-9e0f-1a2b3c4d5e6f"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: new Guid("9a7f6e5d-4c3b-2a1f-8e9d-0f1e2d3c4b5a"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-1234567890ef"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: new Guid("f3b2c1d4-8e9a-4f2b-9c1d-7a6e5b4c3d2a"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyColumnType: "uniqueidentifier",
                keyValue: new Guid("8e9f0a1b-2c3d-4e5f-6a7b-8c9d0e1f2a3b"));

            migrationBuilder.DropColumn(
                name: "GlobalAccountStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserRoleId",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "AppId",
                table: "UserRoles");

            migrationBuilder.RenameColumn(
                name: "UserAppStatusId",
                table: "UserAppStatuses",
                newName: "UserAppId");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLogin",
                table: "UserAppStatuses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "UserAppStatuses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiry",
                table: "UserAppStatuses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PermissionName",
                table: "Permissions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AppName",
                table: "Apps",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.CreateTable(
                name: "RolesPermissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesPermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolesPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolesPermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Apps",
                keyColumn: "AppId",
                keyValue: new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"),
                column: "AppName",
                value: "Global Admin App");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"),
                column: "RoleName",
                value: "Root Admin");

            migrationBuilder.InsertData(
                table: "RolesPermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { new Guid("9f0a1b2c-3d4e-5f6a-7b8c-9d0e1f2a3b4c"), new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7") });

            migrationBuilder.InsertData(
                table: "UserAppStatuses",
                columns: new[] { "UserAppId", "AppId", "LastLogin", "RefreshToken", "RefreshTokenExpiry", "Status", "UserId" },
                values: new object[] { new Guid("7d8e9f0a-1b2c-3d4e-5f6a-7b8c9d0e1f2a"), new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), null, "", null, "active", new Guid("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f") });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"), new Guid("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f"),
                column: "UpdatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_UserAppStatuses_AppId",
                table: "UserAppStatuses",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAppStatuses_UserId_AppId",
                table: "UserAppStatuses",
                columns: new[] { "UserId", "AppId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionName",
                table: "Permissions",
                column: "PermissionName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Apps_AppName",
                table: "Apps",
                column: "AppName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermissions_PermissionId",
                table: "RolesPermissions",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAppStatuses_Apps_AppId",
                table: "UserAppStatuses",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "AppId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAppStatuses_Apps_AppId",
                table: "UserAppStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropTable(
                name: "RolesPermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserAppStatuses_AppId",
                table: "UserAppStatuses");

            migrationBuilder.DropIndex(
                name: "IX_UserAppStatuses_UserId_AppId",
                table: "UserAppStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_PermissionName",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Apps_AppName",
                table: "Apps");

            migrationBuilder.DeleteData(
                table: "UserAppStatuses",
                keyColumn: "UserAppId",
                keyValue: new Guid("7d8e9f0a-1b2c-3d4e-5f6a-7b8c9d0e1f2a"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"), new Guid("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f") });

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLogin",
                table: "UserAppStatuses");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "UserAppStatuses");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiry",
                table: "UserAppStatuses");

            migrationBuilder.RenameColumn(
                name: "UserAppId",
                table: "UserAppStatuses",
                newName: "UserAppStatusId");

            migrationBuilder.AddColumn<string>(
                name: "GlobalAccountStatus",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserRoleId",
                table: "UserRoles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AppId",
                table: "UserRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PermissionName",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "AppName",
                table: "Apps",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                column: "UserRoleId");

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RolePermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.RolePermissionId);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserApps",
                columns: table => new
                {
                    UserAppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApps", x => x.UserAppId);
                    table.ForeignKey(
                        name: "FK_UserApps_Apps_AppId",
                        column: x => x.AppId,
                        principalTable: "Apps",
                        principalColumn: "AppId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserApps_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Apps",
                keyColumn: "AppId",
                keyValue: new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"),
                column: "AppName",
                value: "GlobalAdminApp");

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "IsSystemDefined", "PermissionName" },
                values: new object[,]
                {
                    { new Guid("0f1e2d3c-4b5a-6978-8e9d-abcdef123456"), true, "ManageAssigns" },
                    { new Guid("1c2d3e4f-5a6b-7c8d-9e0f-1a2b3c4d5e6f"), true, "ManageUsers" },
                    { new Guid("9a7f6e5d-4c3b-2a1f-8e9d-0f1e2d3c4b5a"), true, "ManageRoles" },
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-1234567890ef"), true, "ManagePermissions" },
                    { new Guid("f3b2c1d4-8e9a-4f2b-9c1d-7a6e5b4c3d2a"), true, "ManageApps" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "RolePermissionId", "PermissionId", "RoleId" },
                values: new object[] { new Guid("0a1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"), new Guid("9f0a1b2c-3d4e-5f6a-7b8c-9d0e1f2a3b4c"), new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7") });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"),
                column: "RoleName",
                value: "RootAdmin");

            migrationBuilder.InsertData(
                table: "UserApps",
                columns: new[] { "UserAppId", "AccountStatus", "AppId", "CreatedAt", "LastLogin", "RefreshToken", "RefreshTokenExpiry", "UserId" },
                values: new object[] { new Guid("7d8e9f0a-1b2c-3d4e-5f6a-7b8c9d0e1f2a"), "active", new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "", null, new Guid("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f") });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "UserRoleId", "AppId", "RoleId", "UserId" },
                values: new object[] { new Guid("8e9f0a1b-2c3d-4e5f-6a7b-8c9d0e1f2a3b"), null, new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"), new Guid("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f"),
                column: "GlobalAccountStatus",
                value: "active");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_AppId",
                table: "UserRoles",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAppStatuses_UserId",
                table: "UserAppStatuses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApps_AppId",
                table: "UserApps",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApps_UserId",
                table: "UserApps",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Apps_AppId",
                table: "UserRoles",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "AppId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
