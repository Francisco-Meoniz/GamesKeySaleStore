using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeyGamesSaleStore.Migrations
{
    /// <inheritdoc />
    public partial class AddLastVerificationEmailSent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastVerificationEmailSent",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastVerificationEmailSent",
                table: "Users");
        }
    }
}
