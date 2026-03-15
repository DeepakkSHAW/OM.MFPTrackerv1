using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class addedFolioOwner01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TFolioOwner",
                columns: table => new
                {
                    FolioOwnerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TFolioOwner", x => x.FolioOwnerId);
                });

            migrationBuilder.InsertData(
                table: "TFolioOwner",
                columns: new[] { "FolioOwnerId", "FirstName" },
                values: new object[,]
                {
                    { 1, "Rupam" },
                    { 2, "Deepak" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TFolioOwner");
        }
    }
}
