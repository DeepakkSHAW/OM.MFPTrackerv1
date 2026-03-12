using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixFundCatRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MFCategoryMFCatId",
                table: "TFund",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "TFund",
                keyColumn: "FundId",
                keyValue: 1,
                column: "MFCategoryMFCatId",
                value: null);

            migrationBuilder.UpdateData(
                table: "TFund",
                keyColumn: "FundId",
                keyValue: 2,
                column: "MFCategoryMFCatId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_TFund_MFCategoryMFCatId",
                table: "TFund",
                column: "MFCategoryMFCatId");

            migrationBuilder.AddForeignKey(
                name: "FK_TFund_TMFCategory_MFCategoryMFCatId",
                table: "TFund",
                column: "MFCategoryMFCatId",
                principalTable: "TMFCategory",
                principalColumn: "MFCatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TFund_TMFCategory_MFCategoryMFCatId",
                table: "TFund");

            migrationBuilder.DropIndex(
                name: "IX_TFund_MFCategoryMFCatId",
                table: "TFund");

            migrationBuilder.DropColumn(
                name: "MFCategoryMFCatId",
                table: "TFund");
        }
    }
}
