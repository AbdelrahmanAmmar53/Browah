using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Berwah.DAL.Migrations
{
    /// <inheritdoc />
    public partial class addticketstoadvertisement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdvertisementId",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AdvertisementId",
                table: "Tickets",
                column: "AdvertisementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Advertisments_AdvertisementId",
                table: "Tickets",
                column: "AdvertisementId",
                principalTable: "Advertisments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Advertisments_AdvertisementId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_AdvertisementId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AdvertisementId",
                table: "Tickets");
        }
    }
}
