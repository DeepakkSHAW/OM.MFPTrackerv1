using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedFolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TFolio",
                columns: table => new
                {
                    FolioId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FolioNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, collation: "NOCASE"),
                    FolioPurpose = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true, collation: "NOCASE"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    AttachedBank = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, collation: "NOCASE"),
                    FolioHolderId = table.Column<int>(type: "INTEGER", nullable: false),
                    FundId = table.Column<int>(type: "INTEGER", nullable: false),
                    InDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TFolio", x => x.FolioId);
                    table.ForeignKey(
                        name: "FK_TFolio_TFolioHolder_FolioHolderId",
                        column: x => x.FolioHolderId,
                        principalTable: "TFolioHolder",
                        principalColumn: "FolioHolderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TFolio_TFund_FundId",
                        column: x => x.FundId,
                        principalTable: "TFund",
                        principalColumn: "FundId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TFolio",
                columns: new[] { "FolioId", "AttachedBank", "FolioHolderId", "FolioNumber", "FolioPurpose", "FundId", "IsActive" },
                values: new object[,]
                {
                    { 1, null, 1, "FOLIO123", "Investment in Axis Small Cap Fund", 1, true },
                    { 2, null, 2, "FOLIO456", "Investment in Axis Small Cap Fund", 1, true },
                    { 3, null, 3, "FOLIO789", "Investment in Bandhan Small Cap Fund", 2, true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TFolio_FolioHolderId_FundId_FolioNumber",
                table: "TFolio",
                columns: new[] { "FolioHolderId", "FundId", "FolioNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TFolio_FundId",
                table: "TFolio",
                column: "FundId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TFolio");
        }
    }
}
