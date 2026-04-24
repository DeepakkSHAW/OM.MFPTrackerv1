using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initialdataseeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 2,
                column: "FolioNumber",
                value: "4.99183E+11");

            migrationBuilder.InsertData(
                table: "TFolio",
                columns: new[] { "FolioId", "AMCId", "AttachedBank", "FolioHolderId", "FolioNumber", "FolioPurpose", "IsActive" },
                values: new object[,]
                {
                    { 7, 10, "DK HDFC NRO", 2, "12938622", "Retirement Savings", true },
                    { 8, 8, "DK HDFC NRO", 2, "18968155", "Diversification", true },
                    { 9, 3, "DK HDFC NRO", 2, "19915197402", "Big Investment", true },
                    { 10, 10, "DK HDFC NRO", 2, "4183002", "Index Tracking", true },
                    { 11, 4, "DK HDFC NRO", 2, "4751368", "Diversification", true },
                    { 12, 5, "DK KOTAK NRE", 2, "79931281531", "Diversification", true },
                    { 13, 5, "DK KOTAK NRE", 2, "79931510915", "SIP", true },
                    { 14, 5, "DK KOTAK NRE", 2, "79933657726", "SIP for JAG", true },
                    { 15, 5, "DK KOTAK NRE", 2, "79944037504", "One more SIP on MIR", true },
                    { 16, 5, "DK HDFC NRO", 2, "79954053351", "Tax saving purpose", true },
                    { 17, 1, "DK HDFC NRO", 2, "9.10146E+11", "Diversification", true },
                    { 18, 1, "DK HDFC NRO", 2, "9.1016E+11", "INV for JAG (LIC Money)", true },
                    { 19, 1, "DK KOTAK NRE", 2, "91093138511", "Parking in small cap", true },
                    { 20, 1, "DK KOTAK NRE", 2, "91093138814", "JAG long term SIP", true },
                    { 21, 1, "DK KOTAK NRE", 2, "91093138965", "DIV long term SIP", true },
                    { 22, 8, "RS SBI", 1, "18917658", "RUPs INV", true },
                    { 23, 7, "RS SBI", 6, "10231846", "Rups Inv", true },
                    { 24, 5, "SBI", 1, "79936204371", "SIP", true },
                    { 25, 1, "SBI", 1, "Axis Focused 25 Fund", "Diversification", true },
                    { 26, 1, "SBI", 1, "9.10147E+11", "RS INV", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 26);

            migrationBuilder.UpdateData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 2,
                column: "FolioNumber",
                value: "499183354147");
        }
    }
}
