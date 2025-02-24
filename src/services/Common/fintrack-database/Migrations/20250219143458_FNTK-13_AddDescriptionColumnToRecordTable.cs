using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fintrack_database.Migrations
{
    /// <inheritdoc />
    public partial class FNTK13_AddDescriptionColumnToRecordTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "record",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_hungarian_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "record");
        }
    }
}
