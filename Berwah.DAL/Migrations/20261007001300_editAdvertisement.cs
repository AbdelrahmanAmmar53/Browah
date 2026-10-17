using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Berwah.DAL.Migrations
{
    /// <inheritdoc />
    public partial class editAdvertisement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Advertisments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "AdvertisementReview",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdvertisementId = table.Column<int>(type: "int", nullable: false),
                    ReviewerId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsVerifiedPurchase = table.Column<bool>(type: "bit", nullable: false),
                    AdvertismentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertisementReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvertisementReview_Advertisments_AdvertisementId",
                        column: x => x.AdvertisementId,
                        principalTable: "Advertisments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdvertisementReview_Advertisments_AdvertismentId",
                        column: x => x.AdvertismentId,
                        principalTable: "Advertisments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdvertisementReview_AspNetUsers_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AdvertisementSpecifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdvertisementId = table.Column<int>(type: "int", nullable: false),
                    SpecificationKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SpecificationValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdvertismentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertisementSpecifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvertisementSpecifications_Advertisments_AdvertisementId",
                        column: x => x.AdvertisementId,
                        principalTable: "Advertisments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdvertisementSpecifications_Advertisments_AdvertismentId",
                        column: x => x.AdvertismentId,
                        principalTable: "Advertisments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementReview_AdvertisementId",
                table: "AdvertisementReview",
                column: "AdvertisementId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementReview_AdvertismentId",
                table: "AdvertisementReview",
                column: "AdvertismentId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementReview_ReviewerId",
                table: "AdvertisementReview",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementSpecifications_AdvertisementId",
                table: "AdvertisementSpecifications",
                column: "AdvertisementId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementSpecifications_AdvertismentId",
                table: "AdvertisementSpecifications",
                column: "AdvertismentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvertisementReview");

            migrationBuilder.DropTable(
                name: "AdvertisementSpecifications");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Advertisments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
