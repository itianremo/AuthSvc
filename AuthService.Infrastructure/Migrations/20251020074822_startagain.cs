using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class startagain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f"),
                column: "HashedPassword",
                value: "AQAAAAIAAYagAAAAEOs3vT6YZ5lIVhQRS7xFq17KvA/u2g7qv+9s0MH2FXyXdvsHLg0rMqwL9KeGKXOvdw==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("6c7d8e9f-0a1b-2c3d-4e5f-6a7b8c9d0e1f"),
                column: "HashedPassword",
                value: "AQAAAAEAACcQAAAAEAdminHashedPasswordHere");
        }
    }
}
