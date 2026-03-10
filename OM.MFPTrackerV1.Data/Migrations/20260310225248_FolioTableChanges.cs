using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class FolioTableChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TFolioHolder_FirstName_LastName_DateOfBirth",
                table: "TFolioHolder",
                columns: new[] { "FirstName", "LastName", "DateOfBirth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TFolioHolder_Signature",
                table: "TFolioHolder",
                column: "Signature",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TFolioHolder_FirstName_LastName_DateOfBirth",
                table: "TFolioHolder");

            migrationBuilder.DropIndex(
                name: "IX_TFolioHolder_Signature",
                table: "TFolioHolder");
        }
    }
}
