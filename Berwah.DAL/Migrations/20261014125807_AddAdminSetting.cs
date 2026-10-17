using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Berwah.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementReview_Advertisments_AdvertisementId",
                table: "AdvertisementReview");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementReview_Advertisments_AdvertismentId",
                table: "AdvertisementReview");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementReview_AspNetUsers_ReviewerId",
                table: "AdvertisementReview");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertismentImage_Advertisments_AdvertismentId",
                table: "AdvertismentImage");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertismentImage_Images_ImageId",
                table: "AdvertismentImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdvertismentImage",
                table: "AdvertismentImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdvertisementReview",
                table: "AdvertisementReview");

            migrationBuilder.RenameTable(
                name: "AdvertismentImage",
                newName: "AdvertismentImages");

            migrationBuilder.RenameTable(
                name: "AdvertisementReview",
                newName: "AdvertisementReviews");

            migrationBuilder.RenameIndex(
                name: "IX_AdvertismentImage_ImageId",
                table: "AdvertismentImages",
                newName: "IX_AdvertismentImages_ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_AdvertisementReview_ReviewerId",
                table: "AdvertisementReviews",
                newName: "IX_AdvertisementReviews_ReviewerId");

            migrationBuilder.RenameIndex(
                name: "IX_AdvertisementReview_AdvertismentId",
                table: "AdvertisementReviews",
                newName: "IX_AdvertisementReviews_AdvertismentId");

            migrationBuilder.RenameIndex(
                name: "IX_AdvertisementReview_AdvertisementId",
                table: "AdvertisementReviews",
                newName: "IX_AdvertisementReviews_AdvertisementId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdvertismentImages",
                table: "AdvertismentImages",
                columns: new[] { "AdvertismentId", "ImageId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdvertisementReviews",
                table: "AdvertisementReviews",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WithdrawalFeeFixed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompanyCommissionPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinWithdrawalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxCashLimitForDelivery = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementReviews_Advertisments_AdvertisementId",
                table: "AdvertisementReviews",
                column: "AdvertisementId",
                principalTable: "Advertisments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementReviews_Advertisments_AdvertismentId",
                table: "AdvertisementReviews",
                column: "AdvertismentId",
                principalTable: "Advertisments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementReviews_AspNetUsers_ReviewerId",
                table: "AdvertisementReviews",
                column: "ReviewerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertismentImages_Advertisments_AdvertismentId",
                table: "AdvertismentImages",
                column: "AdvertismentId",
                principalTable: "Advertisments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertismentImages_Images_ImageId",
                table: "AdvertismentImages",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementReviews_Advertisments_AdvertisementId",
                table: "AdvertisementReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementReviews_Advertisments_AdvertismentId",
                table: "AdvertisementReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementReviews_AspNetUsers_ReviewerId",
                table: "AdvertisementReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertismentImages_Advertisments_AdvertismentId",
                table: "AdvertismentImages");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertismentImages_Images_ImageId",
                table: "AdvertismentImages");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdvertismentImages",
                table: "AdvertismentImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdvertisementReviews",
                table: "AdvertisementReviews");

            migrationBuilder.RenameTable(
                name: "AdvertismentImages",
                newName: "AdvertismentImage");

            migrationBuilder.RenameTable(
                name: "AdvertisementReviews",
                newName: "AdvertisementReview");

            migrationBuilder.RenameIndex(
                name: "IX_AdvertismentImages_ImageId",
                table: "AdvertismentImage",
                newName: "IX_AdvertismentImage_ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_AdvertisementReviews_ReviewerId",
                table: "AdvertisementReview",
                newName: "IX_AdvertisementReview_ReviewerId");

            migrationBuilder.RenameIndex(
                name: "IX_AdvertisementReviews_AdvertismentId",
                table: "AdvertisementReview",
                newName: "IX_AdvertisementReview_AdvertismentId");

            migrationBuilder.RenameIndex(
                name: "IX_AdvertisementReviews_AdvertisementId",
                table: "AdvertisementReview",
                newName: "IX_AdvertisementReview_AdvertisementId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdvertismentImage",
                table: "AdvertismentImage",
                columns: new[] { "AdvertismentId", "ImageId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdvertisementReview",
                table: "AdvertisementReview",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementReview_Advertisments_AdvertisementId",
                table: "AdvertisementReview",
                column: "AdvertisementId",
                principalTable: "Advertisments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementReview_Advertisments_AdvertismentId",
                table: "AdvertisementReview",
                column: "AdvertismentId",
                principalTable: "Advertisments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementReview_AspNetUsers_ReviewerId",
                table: "AdvertisementReview",
                column: "ReviewerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertismentImage_Advertisments_AdvertismentId",
                table: "AdvertismentImage",
                column: "AdvertismentId",
                principalTable: "Advertisments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertismentImage_Images_ImageId",
                table: "AdvertismentImage",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
