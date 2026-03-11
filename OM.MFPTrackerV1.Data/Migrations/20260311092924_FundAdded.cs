using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class FundAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TFund",
                columns: table => new
                {
                    FundId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    SchemeCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, collation: "NOCASE"),
                    ISIN = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, collation: "NOCASE"),
                    AMCName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    IsTransactionAllowed = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    IsNavAllowed = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    MFCatId = table.Column<int>(type: "INTEGER", nullable: false),
                    InDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TFund", x => x.FundId);
                    table.ForeignKey(
                        name: "FK_TFund_TMFCategory_MFCatId",
                        column: x => x.MFCatId,
                        principalTable: "TMFCategory",
                        principalColumn: "MFCatId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TFund",
                columns: new[] { "FundId", "AMCName", "FundName", "ISIN", "IsNavAllowed", "IsTransactionAllowed", "MFCatId", "SchemeCode" },
                values: new object[,]
                {
                    { 1, "AXIS MF", "AXIS SMALL CAP Fund - DIRECT PLAN - GROWTH", "INF846K01K35", true, true, 1, "125354" },
                    { 2, "BANDHAN MF", "BANDHAN SMALL CAP FUND - REGULAR PLAN GROWTH", "INF194KB1AJ8", true, true, 1, "147944" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TFund_AMCName",
                table: "TFund",
                column: "AMCName");

            migrationBuilder.CreateIndex(
                name: "IX_TFund_FundName",
                table: "TFund",
                column: "FundName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TFund_ISIN",
                table: "TFund",
                column: "ISIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TFund_MFCatId",
                table: "TFund",
                column: "MFCatId");

            migrationBuilder.CreateIndex(
                name: "IX_TFund_SchemeCode",
                table: "TFund",
                column: "SchemeCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TFund");
        }
    }
}
