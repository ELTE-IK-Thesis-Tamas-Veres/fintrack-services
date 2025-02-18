using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fintrack_database.Migrations
{
    /// <inheritdoc />
    public partial class FNTK12_AddParentCategoryColumnToCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "category",
                newName: "userId");

            migrationBuilder.RenameIndex(
                name: "IX_category_user_id",
                table: "category",
                newName: "IX_category_userId");

            migrationBuilder.AddColumn<uint>(
                name: "parentCategoryId",
                table: "category",
                type: "int unsigned",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_category_parentCategoryId",
                table: "category",
                column: "parentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Category_Category",
                table: "category",
                column: "parentCategoryId",
                principalTable: "category",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Category_Category",
                table: "category");

            migrationBuilder.DropIndex(
                name: "IX_category_parentCategoryId",
                table: "category");

            migrationBuilder.DropColumn(
                name: "parentCategoryId",
                table: "category");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "category",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_category_userId",
                table: "category",
                newName: "IX_category_user_id");
        }
    }
}
