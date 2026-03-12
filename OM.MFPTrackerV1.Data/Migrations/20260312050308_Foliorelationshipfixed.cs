using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class Foliorelationshipfixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FolioHolderId1",
                table: "TFolio",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FundId1",
                table: "TFolio",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 1,
                columns: new[] { "FolioHolderId1", "FundId1" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 2,
                columns: new[] { "FolioHolderId1", "FundId1" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "TFolio",
                keyColumn: "FolioId",
                keyValue: 3,
                columns: new[] { "FolioHolderId1", "FundId1" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_TFolio_FolioHolderId1",
                table: "TFolio",
                column: "FolioHolderId1");

            migrationBuilder.CreateIndex(
                name: "IX_TFolio_FundId1",
                table: "TFolio",
                column: "FundId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TFolio_TFolioHolder_FolioHolderId1",
                table: "TFolio",
                column: "FolioHolderId1",
                principalTable: "TFolioHolder",
                principalColumn: "FolioHolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_TFolio_TFund_FundId1",
                table: "TFolio",
                column: "FundId1",
                principalTable: "TFund",
                principalColumn: "FundId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TFolio_TFolioHolder_FolioHolderId1",
                table: "TFolio");

            migrationBuilder.DropForeignKey(
                name: "FK_TFolio_TFund_FundId1",
                table: "TFolio");

            migrationBuilder.DropIndex(
                name: "IX_TFolio_FolioHolderId1",
                table: "TFolio");

            migrationBuilder.DropIndex(
                name: "IX_TFolio_FundId1",
                table: "TFolio");

            migrationBuilder.DropColumn(
                name: "FolioHolderId1",
                table: "TFolio");

            migrationBuilder.DropColumn(
                name: "FundId1",
                table: "TFolio");
        }
    }
}
