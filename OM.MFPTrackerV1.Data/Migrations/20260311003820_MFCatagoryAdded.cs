using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class MFCatagoryAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TMFCategory",
                columns: table => new
                {
                    MFCatId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TMFCategory", x => x.MFCatId);
                });

            migrationBuilder.InsertData(
                table: "TMFCategory",
                columns: new[] { "MFCatId", "CategoryName" },
                values: new object[,]
                {
                    { 1, "Equity-Multi Cap " },
                    { 2, "Equity-Flexi Cap" },
                    { 3, "Equity-Large & MidCap" },
                    { 4, "Equity-Large Cap" },
                    { 5, "Equity-Mid Cap" },
                    { 6, "Equity-Small Cap" },
                    { 7, "Equity-ELSS" },
                    { 8, "Equity-Dividend Yield" },
                    { 9, "Equity-Contra" },
                    { 10, "Equity-Sectoral" },
                    { 11, "Equity-Value Oriented" },
                    { 12, "Debt-Liquid Fund" },
                    { 13, "Debt-Overnight Funds" },
                    { 14, "Debt-Money Market Funds" },
                    { 15, "Debt-Corporate Bond Funds" },
                    { 16, "Debt-Gilt Funds" },
                    { 17, "Hybrid Fund" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TMFCategory_CategoryName",
                table: "TMFCategory",
                column: "CategoryName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TMFCategory");
        }
    }
}
