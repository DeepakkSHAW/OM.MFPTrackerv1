using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class specialEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TSpecialEvent",
                columns: table => new
                {
                    SpecialEventId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true, collation: "NOCASE"),
                    EventDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FundId = table.Column<int>(type: "INTEGER", nullable: true),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, collation: "NOCASE"),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Info", collation: "NOCASE"),
                    InDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TSpecialEvent", x => x.SpecialEventId);
                    table.ForeignKey(
                        name: "FK_TSpecialEvent_TFund_FundId",
                        column: x => x.FundId,
                        principalTable: "TFund",
                        principalColumn: "FundId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TSpecialEvent",
                columns: new[] { "SpecialEventId", "Description", "EventDate", "EventType", "FundId", "InDate", "Severity", "Title" },
                values: new object[,]
                {
                    { 1, "Global equity markets corrected sharply due to COVID-19 pandemic fears.", new DateTime(2020, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Market", null, new DateTime(2020, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Critical", "COVID-19 Market Crash" },
                    { 2, "Government reintroduced 10% LTCG tax on equity gains exceeding ₹1L.", new DateTime(2018, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Regulatory", null, new DateTime(2018, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Warning", "Reintroduction of LTCG Tax on Equity" },
                    { 3, "RBI increased repo rate to curb inflation, impacting debt fund NAVs.", new DateTime(2022, 5, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Macro", null, new DateTime(2022, 5, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Info", "RBI Begins Rate Hike Cycle" },
                    { 4, "Annual Union Budget presented with implications for equity and debt markets.", new DateTime(2023, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Market", null, new DateTime(2023, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Info", "Union Budget Announcement" },
                    { 5, "Expense ratio revised by fund house.", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fund", 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Info", "Expense Ratio Revision" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TSpecialEvent_EventDate",
                table: "TSpecialEvent",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_TSpecialEvent_EventType",
                table: "TSpecialEvent",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_TSpecialEvent_FundId",
                table: "TSpecialEvent",
                column: "FundId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TSpecialEvent");
        }
    }
}
