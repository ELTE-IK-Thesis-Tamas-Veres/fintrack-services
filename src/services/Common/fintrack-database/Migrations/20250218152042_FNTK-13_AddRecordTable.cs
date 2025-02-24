using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fintrack_database.Migrations
{
    /// <inheritdoc />
    public partial class FNTK13_AddRecordTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "record",
                columns: table => new
                {
                    id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    amount = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    categoryId = table.Column<uint>(type: "int unsigned", nullable: true),
                    userId = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "FK_Record_Category",
                        column: x => x.categoryId,
                        principalTable: "category",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Record_User",
                        column: x => x.userId,
                        principalTable: "user",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_hungarian_ci");

            migrationBuilder.CreateIndex(
                name: "IX_record_categoryId",
                table: "record",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "IX_record_userId",
                table: "record",
                column: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "record");
        }
    }
}
