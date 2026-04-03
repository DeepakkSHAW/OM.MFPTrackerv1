using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 1,
                columns: new[] { "AMCId", "AttachedBank", "FolioHolderId", "FolioNumber", "FolioPurpose" },
                values: new object[] { 9, "JAG IDFC NRO", 3, "37959966", "Long term Portfolio" });

            migrationBuilder.UpdateData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 2,
                columns: new[] { "AMCId", "AttachedBank", "FolioHolderId", "FolioNumber", "FolioPurpose" },
                values: new object[] { 6, "DK HDFC NRO", 2, "499183354147", "Small can investment - experiment" });

            migrationBuilder.InsertData(
                table: "TFund",
                columns: new[] { "FundId", "AMCId", "FundName", "ISIN", "IsNavAllowed", "IsTransactionAllowed", "MFCatId", "SchemeCode" },
                values: new object[,]
                {
                    { 7, 4, "Kotak Large & Midcap Fund - Direct- Growth", "INF174K01LF9", true, true, 3, "120158" },
                    { 8, 5, "Mirae Asset Large & Midcap Fund - Direct Plan - Growth", "INF769K01BI1", true, true, 3, "118834" },
                    { 9, 5, "Mirae Asset ELSS Tax Saver Fund - Direct Plan - Growth", "INF769K01DM9", true, true, 7, "135781" },
                    { 10, 6, "Nippon India Small Cap Fund - Direct Plan - Growth", "INF204K01K15", true, true, 6, "118778" },
                    { 11, 8, "SBI Large Cap Fund - Direct Plan - Growth", "INF200K01QX4", true, true, 4, "119598" },
                    { 12, 8, "SBI Small Cap Fund - Direct Plan - Growth", "INF200K01T51", true, true, 6, "125497" },
                    { 13, 10, "Tata Retirement Savings Fund- Progressive Plan - Direct Plan - Growth", "INF277K01QO1", true, true, 2, "119251" },
                    { 14, 10, "Tata S&P BSE Sensex Index Fund - Direct Plan", "INF277K01PK1", true, true, 4, "119287" },
                    { 15, 2, "BANDHAN Small Cap Fund - Regular Plan - Growth", "INF194KB1AJ8", true, true, 6, "147944" },
                    { 16, 9, "HDFC Small Cap Fund - Growth Option - Direct Plan", "INF179KA1RW5", true, true, 6, "130503" },
                    { 17, 9, "HDFC Gold ETF Fund of Fund - Direct Plan", "INF179K01VX0", true, true, 19, "119132" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TFund",
                keyColumn: "FundId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TFund",
                keyColumn: "FundId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TFund",
                keyColumn: "FundId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TFund",
                keyColumn: "FundId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TFund",
                keyColumn: "FundId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "TFund",
                keyColumn: "FundId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "TFund",
                keyColumn: "FundId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "TFund",
                keyColumn: "FundId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "TFund",
                keyColumn: "FundId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "TFund",
                keyColumn: "FundId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "TFund",
                keyColumn: "FundId",
                keyValue: 17);

            migrationBuilder.UpdateData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 1,
                columns: new[] { "AMCId", "AttachedBank", "FolioHolderId", "FolioNumber", "FolioPurpose" },
                values: new object[] { 1, null, 2, "1234567", "Investment Portfolio" });

            migrationBuilder.UpdateData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 2,
                columns: new[] { "AMCId", "AttachedBank", "FolioHolderId", "FolioNumber", "FolioPurpose" },
                values: new object[] { 2, null, 1, "2233445", "Family Holdings" });
        }
    }
}
