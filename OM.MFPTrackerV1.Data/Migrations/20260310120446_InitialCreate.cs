using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TFolioHolder",
                columns: table => new
                {
                    FolioHolderId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Signature = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false, collation: "NOCASE"),
                    InDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TFolioHolder", x => x.FolioHolderId);
                });

            migrationBuilder.InsertData(
                table: "TFolioHolder",
                columns: new[] { "FolioHolderId", "DateOfBirth", "FirstName", "LastName", "Signature" },
                values: new object[,]
                {
                    { 1, new DateTime(2002, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rupam", "Shaw", "RS" },
                    { 2, new DateTime(1981, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Deepak", "Shaw", "DK" },
                    { 3, new DateTime(1974, 4, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jagruti", "Shaw", "JS" },
                    { 4, new DateTime(2001, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Divyam", "Shaw", "DS" },
                    { 5, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Durga Prasad", "Shaw", "DP" },
                    { 6, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Radha", "Shaw", "RD" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TFolioHolder_FirstName",
                table: "TFolioHolder",
                column: "FirstName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TFolioHolder_FolioHolderId",
                table: "TFolioHolder",
                column: "FolioHolderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TFolioHolder");
        }
    }
}
