using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedsAdminAndSuperAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"));

            migrationBuilder.UpdateData(
                table: "Apps",
                keyColumn: "AppId",
                keyValue: new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"),
                column: "AppName",
                value: "GlobalAdminApp");

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "PermissionName" },
                values: new object[] { new Guid("9f0a1b2c-3d4e-5f6a-7b8c-9d0e1f2a3b4c"), "SuperAccess" });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "RolePermissionId" },
                values: new object[] { new Guid("9f0a1b2c-3d4e-5f6a-7b8c-9d0e1f2a3b4c"), new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"), new Guid("0a1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("9f0a1b2c-3d4e-5f6a-7b8c-9d0e1f2a3b4c"), new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: new Guid("9f0a1b2c-3d4e-5f6a-7b8c-9d0e1f2a3b4c"));

            migrationBuilder.UpdateData(
                table: "Apps",
                keyColumn: "AppId",
                keyValue: new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"),
                column: "AppName",
                value: "Global Admin Dashboard");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "AppId", "RoleName" },
                values: new object[] { new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"), new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), "Editor" });
        }
    }
}
