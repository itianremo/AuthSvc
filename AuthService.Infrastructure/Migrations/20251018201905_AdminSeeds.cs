using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdminSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Apps",
                columns: new[] { "AppId", "AppName", "RedirectUrls", "Scopes" },
                values: new object[] { new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), "Global Admin Dashboard", "http://localhost:3000/auth/callback", "openid profile email admin" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "GlobalAccountStatus", "HashedPassword", "IsEmailVerified", "IsPhoneVerified", "PhoneNumber" },
                values: new object[] { new Guid("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@authsvc.local", "active", "AQAAAAEAACcQAAAAEAdminHashedPasswordHere", true, true, "+201144664441" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "AppId", "RoleName" },
                values: new object[,]
                {
                    { new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"), new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), "Editor" },
                    { new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"), new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), "Admin" }
                });

            migrationBuilder.InsertData(
                table: "UserApps",
                columns: new[] { "UserAppId", "AccountStatus", "AppId", "CreatedAt", "LastLogin", "RefreshToken", "UserId" },
                values: new object[] { new Guid("7d8e9f0a-1b2c-3d4e-5f6a-7b8c9d0e1f2a"), "active", new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "", new Guid("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f") });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "UserRoleId", "AppId", "RoleId", "UserId" },
                values: new object[] { new Guid("8e9f0a1b-2c3d-4e5f-6a7b-8c9d0e1f2a3b"), new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"), new Guid("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"));

            migrationBuilder.DeleteData(
                table: "UserApps",
                keyColumn: "UserAppId",
                keyValue: new Guid("7d8e9f0a-1b2c-3d4e-5f6a-7b8c9d0e1f2a"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyValue: new Guid("8e9f0a1b-2c3d-4e5f-6a7b-8c9d0e1f2a3b"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("7c9e6679-7425-40de-944b-e07fc1f90ae7"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f"));

            migrationBuilder.DeleteData(
                table: "Apps",
                keyColumn: "AppId",
                keyValue: new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"));

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
    }
}
