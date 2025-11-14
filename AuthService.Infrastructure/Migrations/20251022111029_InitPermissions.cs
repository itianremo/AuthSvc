using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystemDefined",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: new Guid("9f0a1b2c-3d4e-5f6a-7b8c-9d0e1f2a3b4c"),
                column: "IsSystemDefined",
                value: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropColumn(
                name: "IsSystemDefined",
                table: "Permissions");
        }
    }
}
