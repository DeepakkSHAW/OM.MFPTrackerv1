using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class datafiledchanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FH_FirstName_Len",
                table: "TFolioHolder");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FH_LastName_Len",
                table: "TFolioHolder");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FH_FirstName_Len",
                table: "TFolioHolder",
                sql: "length(FirstName) BETWEEN 1 AND 50");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FH_LastName_Len",
                table: "TFolioHolder",
                sql: "length(LastName) BETWEEN 1 AND 50");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FH_FirstName_Len",
                table: "TFolioHolder");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FH_LastName_Len",
                table: "TFolioHolder");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FH_FirstName_Len",
                table: "TFolioHolder",
                sql: "length(FirstName) BETWEEN 1 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FH_LastName_Len",
                table: "TFolioHolder",
                sql: "length(LastName) BETWEEN 1 AND 100");
        }
    }
}
