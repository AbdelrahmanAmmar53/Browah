using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Berwah.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSubCategoryToAD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubCategoryId",
                table: "Advertisments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Advertisments_SubCategoryId",
                table: "Advertisments",
                column: "SubCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisments_SubCategories_SubCategoryId",
                table: "Advertisments",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertisments_SubCategories_SubCategoryId",
                table: "Advertisments");

            migrationBuilder.DropIndex(
                name: "IX_Advertisments_SubCategoryId",
                table: "Advertisments");

            migrationBuilder.DropColumn(
                name: "SubCategoryId",
                table: "Advertisments");
        }
    }
}
