using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Berwah.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EditAdLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DeliveryLatitude",
                table: "ServiceRequests",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DeliveryLongitude",
                table: "ServiceRequests",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PickupLatitude",
                table: "ServiceRequests",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PickupLongitude",
                table: "ServiceRequests",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "ServiceOffers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "ServiceOffers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MerchantStatus",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickedUpAt",
                table: "OrderItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Advertisments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Advertisments",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryLatitude",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "DeliveryLongitude",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "PickupLatitude",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "PickupLongitude",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "ServiceOffers");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "ServiceOffers");

            migrationBuilder.DropColumn(
                name: "MerchantStatus",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PickedUpAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Advertisments");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Advertisments");
        }
    }
}
