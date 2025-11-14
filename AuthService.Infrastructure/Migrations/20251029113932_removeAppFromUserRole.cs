using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removeAppFromUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Apps_AppId",
                table: "UserRoles");

            migrationBuilder.AlterColumn<Guid>(
                name: "AppId",
                table: "UserRoles",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyValue: new Guid("8e9f0a1b-2c3d-4e5f-6a7b-8c9d0e1f2a3b"),
                column: "AppId",
                value: null);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Apps_AppId",
                table: "UserRoles",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "AppId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Apps_AppId",
                table: "UserRoles");

            migrationBuilder.AlterColumn<Guid>(
                name: "AppId",
                table: "UserRoles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyValue: new Guid("8e9f0a1b-2c3d-4e5f-6a7b-8c9d0e1f2a3b"),
                column: "AppId",
                value: new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"));

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Apps_AppId",
                table: "UserRoles",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "AppId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
