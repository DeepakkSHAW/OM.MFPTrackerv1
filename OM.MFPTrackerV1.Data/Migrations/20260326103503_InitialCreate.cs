using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TAMC",
                columns: table => new
                {
                    AMCId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AMCName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAMC", x => x.AMCId);
                    table.CheckConstraint("CK_AMC_AMCName_Len", "length(AMCName) BETWEEN 2 AND 100");
                });

            migrationBuilder.CreateTable(
                name: "TFolioHolder",
                columns: table => new
                {
                    FolioHolderId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, collation: "NOCASE"),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, collation: "NOCASE"),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Signature = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false, collation: "NOCASE"),
                    InDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TFolioHolder", x => x.FolioHolderId);
                });

            migrationBuilder.CreateTable(
                name: "TFolioOwner",
                columns: table => new
                {
                    FolioOwnerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, collation: "NOCASE"),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, collation: "NOCASE"),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Signature = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TFolioOwner", x => x.FolioOwnerId);
                });

            migrationBuilder.CreateTable(
                name: "TMFCategory",
                columns: table => new
                {
                    MFCatId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TMFCategory", x => x.MFCatId);
                    table.CheckConstraint("CK_MFCat_Name_Len", "length(CategoryName) BETWEEN 3 AND 50");
                });

            migrationBuilder.CreateTable(
                name: "TFolio",
                columns: table => new
                {
                    FolioId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FolioNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, collation: "NOCASE"),
                    FolioPurpose = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true, collation: "NOCASE"),
                    AttachedBank = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, collation: "NOCASE"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    AMCId = table.Column<int>(type: "INTEGER", nullable: false),
                    FolioHolderId = table.Column<int>(type: "INTEGER", nullable: false),
                    InDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TFolio", x => x.FolioId);
                    table.CheckConstraint("CK_Folio_Bank_Len", "AttachedBank IS NULL OR length(AttachedBank) <= 50");
                    table.CheckConstraint("CK_Folio_Number_Len", "length(FolioNumber) BETWEEN 5 AND 50");
                    table.CheckConstraint("CK_Folio_Purpose_Len", "FolioPurpose IS NULL OR length(FolioPurpose) <= 100");
                    table.ForeignKey(
                        name: "FK_TFolio_TAMC_AMCId",
                        column: x => x.AMCId,
                        principalTable: "TAMC",
                        principalColumn: "AMCId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TFolio_TFolioHolder_FolioHolderId",
                        column: x => x.FolioHolderId,
                        principalTable: "TFolioHolder",
                        principalColumn: "FolioHolderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TFund",
                columns: table => new
                {
                    FundId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    SchemeCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, collation: "NOCASE"),
                    ISIN = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, collation: "NOCASE"),
                    IsTransactionAllowed = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    IsNavAllowed = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    AMCId = table.Column<int>(type: "INTEGER", nullable: false),
                    MFCatId = table.Column<int>(type: "INTEGER", nullable: false),
                    InDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TFund", x => x.FundId);
                    table.CheckConstraint("CK_Fund_FundName_Len", "length(FundName) BETWEEN 5 AND 100");
                    table.CheckConstraint("CK_Fund_ISIN_Len", "length(ISIN) BETWEEN 1 AND 20");
                    table.CheckConstraint("CK_Fund_SchemeCode_Len", "length(SchemeCode) BETWEEN 1 AND 20");
                    table.ForeignKey(
                        name: "FK_TFund_TAMC_AMCId",
                        column: x => x.AMCId,
                        principalTable: "TAMC",
                        principalColumn: "AMCId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TFund_TMFCategory_MFCatId",
                        column: x => x.MFCatId,
                        principalTable: "TMFCategory",
                        principalColumn: "MFCatId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TMFTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FolioId = table.Column<int>(type: "INTEGER", nullable: false),
                    FundId = table.Column<int>(type: "INTEGER", nullable: false),
                    TxnType = table.Column<int>(type: "INTEGER", nullable: false),
                    Units = table.Column<decimal>(type: "TEXT", nullable: false),
                    NAV = table.Column<decimal>(type: "TEXT", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, collation: "NOCASE"),
                    Note = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true, collation: "NOCASE"),
                    InDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TMFTransaction", x => x.Id);
                    table.CheckConstraint("CK_Txn_Note_Len", "Note IS NULL OR length(Note) <= 100");
                    table.CheckConstraint("CK_Txn_Source_Len", "Source IS NULL OR length(Source) <= 50");
                    table.ForeignKey(
                        name: "FK_TMFTransaction_TFolio_FolioId",
                        column: x => x.FolioId,
                        principalTable: "TFolio",
                        principalColumn: "FolioId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TMFTransaction_TFund_FundId",
                        column: x => x.FundId,
                        principalTable: "TFund",
                        principalColumn: "FundId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TAMC",
                columns: new[] { "AMCId", "AMCName" },
                values: new object[,]
                {
                    { 1, "Axis Mutual Fund" },
                    { 2, "Bandhan Mutual Fund" },
                    { 3, "HDFC Mutual Fund" }
                });

            migrationBuilder.InsertData(
                table: "TFolioHolder",
                columns: new[] { "FolioHolderId", "DateOfBirth", "FirstName", "LastName", "Signature" },
                values: new object[,]
                {
                    { 1, new DateTime(2012, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rupam", "Shaw", "RS" },
                    { 2, new DateTime(1981, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Deepak", "Shaw", "DK" }
                });

            migrationBuilder.InsertData(
                table: "TFolioOwner",
                columns: new[] { "FolioOwnerId", "DateOfBirth", "FirstName", "LastName", "Signature" },
                values: new object[,]
                {
                    { 1, new DateTime(2002, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rupam", "Sachin", "DK" },
                    { 2, new DateTime(2010, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Deepak", "Ganguly", "RS" }
                });

            migrationBuilder.InsertData(
                table: "TMFCategory",
                columns: new[] { "MFCatId", "CategoryName" },
                values: new object[,]
                {
                    { 1, "Equity - Small Cap" },
                    { 2, "Equity - Multi Cap" },
                    { 3, "Debt - Liquid" },
                    { 4, "Hybrid - Aggressive" }
                });

            migrationBuilder.InsertData(
                table: "TFolio",
                columns: new[] { "FolioId", "AMCId", "AttachedBank", "FolioHolderId", "FolioNumber", "FolioPurpose", "IsActive" },
                values: new object[,]
                {
                    { 1, 1, null, 2, "1234567", "Investment Portfolio", true },
                    { 2, 2, null, 1, "2233445", "Family Holdings", true }
                });

            migrationBuilder.InsertData(
                table: "TFund",
                columns: new[] { "FundId", "AMCId", "FundName", "ISIN", "IsNavAllowed", "IsTransactionAllowed", "MFCatId", "SchemeCode" },
                values: new object[,]
                {
                    { 1, 1, "Axis Small Cap Fund - Direct Plan - Growth", "INF846K01K35", true, true, 1, "125354" },
                    { 2, 2, "Bandhan Small Cap Fund - Regular Plan - Growth", "INF194KB1AJ8", true, true, 1, "147944" }
                });

            migrationBuilder.InsertData(
                table: "TMFTransaction",
                columns: new[] { "Id", "AmountPaid", "Date", "FolioId", "FundId", "NAV", "Note", "Source", "TxnType", "Units" },
                values: new object[] { 1, 480m, new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, 12m, null, "Kotak Bank NRO", 1, 40m });

            migrationBuilder.CreateIndex(
                name: "IX_TAMC_AMCName",
                table: "TAMC",
                column: "AMCName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TFolio_AMCId",
                table: "TFolio",
                column: "AMCId");

            migrationBuilder.CreateIndex(
                name: "IX_TFolio_AMCId_FolioNumber",
                table: "TFolio",
                columns: new[] { "AMCId", "FolioNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TFolio_FolioHolderId",
                table: "TFolio",
                column: "FolioHolderId");

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

            migrationBuilder.CreateIndex(
                name: "IX_TFolioOwner_FirstName_LastName_DateOfBirth",
                table: "TFolioOwner",
                columns: new[] { "FirstName", "LastName", "DateOfBirth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TFolioOwner_Signature",
                table: "TFolioOwner",
                column: "Signature",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TFund_AMCId",
                table: "TFund",
                column: "AMCId");

            migrationBuilder.CreateIndex(
                name: "IX_TFund_AMCId_SchemeCode",
                table: "TFund",
                columns: new[] { "AMCId", "SchemeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TFund_FundName",
                table: "TFund",
                column: "FundName");

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
                name: "IX_TMFCategory_CategoryName",
                table: "TMFCategory",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TMFTransaction_Date",
                table: "TMFTransaction",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_TMFTransaction_FolioId",
                table: "TMFTransaction",
                column: "FolioId");

            migrationBuilder.CreateIndex(
                name: "IX_TMFTransaction_FundId",
                table: "TMFTransaction",
                column: "FundId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TFolioOwner");

            migrationBuilder.DropTable(
                name: "TMFTransaction");

            migrationBuilder.DropTable(
                name: "TFolio");

            migrationBuilder.DropTable(
                name: "TFund");

            migrationBuilder.DropTable(
                name: "TFolioHolder");

            migrationBuilder.DropTable(
                name: "TAMC");

            migrationBuilder.DropTable(
                name: "TMFCategory");
        }
    }
}
