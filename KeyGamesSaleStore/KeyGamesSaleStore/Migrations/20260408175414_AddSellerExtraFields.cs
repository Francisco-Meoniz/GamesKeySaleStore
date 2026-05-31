using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeyGamesSaleStore.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "SellerProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DNI",
                table: "SellerProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "SellerProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "SellerProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MercadoPagoAccount",
                table: "SellerProfiles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "DNI",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "SellerProfiles");

            migrationBuilder.DropColumn(
                name: "MercadoPagoAccount",
                table: "SellerProfiles");
        }
    }
}
