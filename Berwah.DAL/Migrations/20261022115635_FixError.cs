using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Berwah.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_AspNetUsers_UserId1",
                table: "ServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_ServiceCategories_ServiceCategoryId",
                table: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "ServiceCategories");

            migrationBuilder.RenameColumn(
                name: "ServiceCategoryId",
                table: "ServiceRequests",
                newName: "SubCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceRequests_ServiceCategoryId",
                table: "ServiceRequests",
                newName: "IX_ServiceRequests_SubCategoryId");

            migrationBuilder.AlterColumn<int>(
                name: "UserId1",
                table: "ServiceRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "ServiceRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_UserId",
                table: "ServiceRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_AspNetUsers_UserId",
                table: "ServiceRequests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_AspNetUsers_UserId1",
                table: "ServiceRequests",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_SubCategories_SubCategoryId",
                table: "ServiceRequests",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_AspNetUsers_UserId",
                table: "ServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_AspNetUsers_UserId1",
                table: "ServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_SubCategories_SubCategoryId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_UserId",
                table: "ServiceRequests");

            migrationBuilder.RenameColumn(
                name: "SubCategoryId",
                table: "ServiceRequests",
                newName: "ServiceCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceRequests_SubCategoryId",
                table: "ServiceRequests",
                newName: "IX_ServiceRequests_ServiceCategoryId");

            migrationBuilder.AlterColumn<int>(
                name: "UserId1",
                table: "ServiceRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ServiceRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "ServiceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IconUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategories", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_AspNetUsers_UserId1",
                table: "ServiceRequests",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_ServiceCategories_ServiceCategoryId",
                table: "ServiceRequests",
                column: "ServiceCategoryId",
                principalTable: "ServiceCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
