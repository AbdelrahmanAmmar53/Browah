using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Berwah.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInCategoryClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "SubCategories");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Categories",
                newName: "IconUrl");

            migrationBuilder.AddColumn<int>(
                name: "Condition",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Condition",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "IconUrl",
                table: "Categories",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SubCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
