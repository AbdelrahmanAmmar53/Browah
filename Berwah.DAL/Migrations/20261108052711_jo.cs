using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Berwah.DAL.Migrations
{
    /// <inheritdoc />
    public partial class jo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ProviderLatitude",
                table: "ServiceRequests",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProviderLocationAt",
                table: "ServiceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ProviderLongitude",
                table: "ServiceRequests",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMain",
                table: "AdvertismentImages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProviderLatitude",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ProviderLocationAt",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ProviderLongitude",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "IsMain",
                table: "AdvertismentImages");
        }
    }
}
