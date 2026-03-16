using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class addedFolioOwner02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "lastName",
                table: "TFolioOwner",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                collation: "NOCASE");

            migrationBuilder.UpdateData(
                table: "TFolioOwner",
                keyColumn: "FolioOwnerId",
                keyValue: 1,
                column: "lastName",
                value: "Sachin");

            migrationBuilder.UpdateData(
                table: "TFolioOwner",
                keyColumn: "FolioOwnerId",
                keyValue: 2,
                column: "lastName",
                value: "Ganguly");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lastName",
                table: "TFolioOwner");
        }
    }
}
