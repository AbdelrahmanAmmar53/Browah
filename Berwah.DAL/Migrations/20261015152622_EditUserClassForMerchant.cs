using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Berwah.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EditUserClassForMerchant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AcceptOrdersAutomatically",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BusinessDescription",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessType",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryTimeSpan",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumOrder",
                table: "AspNetUsers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkHours",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptOrdersAutomatically",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BusinessDescription",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BusinessName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BusinessType",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DeliveryTimeSpan",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MinimumOrder",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WorkHours",
                table: "AspNetUsers");
        }
    }
}
