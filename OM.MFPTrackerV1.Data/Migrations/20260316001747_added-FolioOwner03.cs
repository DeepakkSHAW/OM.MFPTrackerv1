using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class addedFolioOwner03 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "lastName",
                table: "TFolioOwner",
                newName: "LastName");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "TFolioOwner",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "TFolioOwner",
                keyColumn: "FolioOwnerId",
                keyValue: 1,
                column: "DateOfBirth",
                value: new DateTime(2002, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "TFolioOwner",
                keyColumn: "FolioOwnerId",
                keyValue: 2,
                column: "DateOfBirth",
                value: new DateTime(2010, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "TFolioOwner");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "TFolioOwner",
                newName: "lastName");
        }
    }
}
