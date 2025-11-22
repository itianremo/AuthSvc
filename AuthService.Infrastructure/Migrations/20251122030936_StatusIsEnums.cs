using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StatusIsEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserAppStatuses",
                keyColumn: "UserAppId",
                keyValue: new Guid("7d8e9f0a-1b2c-3d4e-5f6a-7b8c9d0e1f2a"),
                column: "Status",
                value: "Active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserAppStatuses",
                keyColumn: "UserAppId",
                keyValue: new Guid("7d8e9f0a-1b2c-3d4e-5f6a-7b8c9d0e1f2a"),
                column: "Status",
                value: "active");
        }
    }
}
