using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class seedRolesAndPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RolePermissionId",
                table: "RolePermissions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "PermissionName" },
                values: new object[,]
                {
                    { new Guid("11111111-aaaa-bbbb-cccc-111111111111"), "Create" },
                    { new Guid("22222222-aaaa-bbbb-cccc-222222222222"), "Edit" },
                    { new Guid("33333333-aaaa-bbbb-cccc-333333333333"), "Delete" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "AppId", "RoleName" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-5678-90ab-cdef-112233445566"), new Guid("28db6987-8066-438e-bb3c-53c6b6515c6c"), "Editor" },
                    { new Guid("d3f4a1b2-9c8e-4a6f-8f2a-123456789abc"), new Guid("28db6987-8066-438e-bb3c-53c6b6515c6c"), "Admin" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "RolePermissionId" },
                values: new object[,]
                {
                    { new Guid("11111111-aaaa-bbbb-cccc-111111111111"), new Guid("a1b2c3d4-5678-90ab-cdef-112233445566"), new Guid("b4d4e1f5-6a7b-7c9d-0e1f-4a5b6c7d8e9f") },
                    { new Guid("22222222-aaaa-bbbb-cccc-222222222222"), new Guid("a1b2c3d4-5678-90ab-cdef-112233445566"), new Guid("c5e5f2a6-7b8c-8d0e-1f2a-5b6c7d8e9f0a") },
                    { new Guid("11111111-aaaa-bbbb-cccc-111111111111"), new Guid("d3f4a1b2-9c8e-4a6f-8f2a-123456789abc"), new Guid("e1a1f8d2-3b4c-4f6a-9a7b-1c2d3e4f5a6b") },
                    { new Guid("22222222-aaaa-bbbb-cccc-222222222222"), new Guid("d3f4a1b2-9c8e-4a6f-8f2a-123456789abc"), new Guid("f2b2e9c3-4d5e-5a7b-8c9d-2e3f4a5b6c7d") },
                    { new Guid("33333333-aaaa-bbbb-cccc-333333333333"), new Guid("d3f4a1b2-9c8e-4a6f-8f2a-123456789abc"), new Guid("a3c3d0e4-5f6a-6b8c-9d0e-3f4a5b6c7d8e") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("11111111-aaaa-bbbb-cccc-111111111111"), new Guid("a1b2c3d4-5678-90ab-cdef-112233445566") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("22222222-aaaa-bbbb-cccc-222222222222"), new Guid("a1b2c3d4-5678-90ab-cdef-112233445566") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("11111111-aaaa-bbbb-cccc-111111111111"), new Guid("d3f4a1b2-9c8e-4a6f-8f2a-123456789abc") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("22222222-aaaa-bbbb-cccc-222222222222"), new Guid("d3f4a1b2-9c8e-4a6f-8f2a-123456789abc") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("33333333-aaaa-bbbb-cccc-333333333333"), new Guid("d3f4a1b2-9c8e-4a6f-8f2a-123456789abc") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: new Guid("11111111-aaaa-bbbb-cccc-111111111111"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: new Guid("22222222-aaaa-bbbb-cccc-222222222222"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: new Guid("33333333-aaaa-bbbb-cccc-333333333333"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("a1b2c3d4-5678-90ab-cdef-112233445566"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("d3f4a1b2-9c8e-4a6f-8f2a-123456789abc"));

            migrationBuilder.DropColumn(
                name: "RolePermissionId",
                table: "RolePermissions");
        }
    }
}
