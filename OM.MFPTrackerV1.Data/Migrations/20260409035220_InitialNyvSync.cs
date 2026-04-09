using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialNyvSync : Migration
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
                name: "TSystemState",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    Value = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true, collation: "NOCASE"),
                    InDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TSystemState", x => x.Key);
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
                name: "TFundNav",
                columns: table => new
                {
                    FundNavId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundId = table.Column<int>(type: "INTEGER", nullable: false),
                    NavDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NavValue = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, collation: "NOCASE"),
                    FetchedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    InDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TFundNav", x => x.FundNavId);
                    table.CheckConstraint("CK_FundNav_NavValue_Positive", "NavValue > 0.0");
                    table.ForeignKey(
                        name: "FK_TFundNav_TFund_FundId",
                        column: x => x.FundId,
                        principalTable: "TFund",
                        principalColumn: "FundId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TMutualFundTransaction",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FolioId = table.Column<int>(type: "INTEGER", nullable: false),
                    FundId = table.Column<int>(type: "INTEGER", nullable: false),
                    TxnType = table.Column<int>(type: "INTEGER", nullable: false),
                    Units = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    NAV = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ReferenceNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    InDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TMutualFundTransaction", x => x.TransactionId);
                    table.CheckConstraint("CK_MFTransaction_AmountPaid_Positive", "AmountPaid >= 0.0");
                    table.CheckConstraint("CK_MFTransaction_NAV_Positive", "NAV >= 0.0");
                    table.CheckConstraint("CK_MFTransaction_Units_Positive", "Units >= 0.0");
                    table.ForeignKey(
                        name: "FK_TMutualFundTransaction_TFolio_FolioId",
                        column: x => x.FolioId,
                        principalTable: "TFolio",
                        principalColumn: "FolioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TMutualFundTransaction_TFund_FundId",
                        column: x => x.FundId,
                        principalTable: "TFund",
                        principalColumn: "FundId",
                        onDelete: ReferentialAction.Restrict);
                });

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
                table: "TAMC",
                columns: new[] { "AMCId", "AMCName" },
                values: new object[,]
                {
                    { 1, "Axis Mutual Fund" },
                    { 2, "Bandhan Mutual Fund" },
                    { 3, "Canara Robeco Mutual Fund" },
                    { 4, "Kotak Mutual Fund" },
                    { 5, "Mirae Asset Mutual Fund" },
                    { 6, "Nippon India Mutual Fund" },
                    { 7, "Parag Parikh Mutual Fund" },
                    { 8, "SBI Mutual Fund" },
                    { 9, "HDFC Mutual Fund" },
                    { 10, "TATA Mutual Fund" },
                    { 11, "ICICI Mutual Fund" },
                    { 12, "UTI Mutual Fund" },
                    { 13, "Aditya Birla Sun Life Mutual Fund" },
                    { 14, "Jio BlackRock Mutual Fund" }
                });

            migrationBuilder.InsertData(
                table: "TFolioHolder",
                columns: new[] { "FolioHolderId", "DateOfBirth", "FirstName", "LastName", "Signature" },
                values: new object[,]
                {
                    { 1, new DateTime(1977, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rupam", "Shaw", "RS" },
                    { 2, new DateTime(1973, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Deepak", "Shaw", "DK" },
                    { 3, new DateTime(2002, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jagruti", "Shaw", "JS" },
                    { 4, new DateTime(2010, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Divyam", "Shaw", "DIVS" },
                    { 5, new DateTime(1951, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "DP", "Shaw", "PAA" },
                    { 6, new DateTime(1961, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Radha", "Devi", "MAA" }
                });

            migrationBuilder.InsertData(
                table: "TFolioOwner",
                columns: new[] { "FolioOwnerId", "DateOfBirth", "FirstName", "LastName", "Signature" },
                values: new object[,]
                {
                    { 1, new DateTime(2002, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "AAA", "Delete me", "DK" },
                    { 2, new DateTime(2010, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BBB", "Delete me", "RS" }
                });

            migrationBuilder.InsertData(
                table: "TMFCategory",
                columns: new[] { "MFCatId", "CategoryName" },
                values: new object[,]
                {
                    { 1, "Equity - Multi Cap" },
                    { 2, "Equity - Flexi Cap" },
                    { 3, "Equity - Large Mid Cap" },
                    { 4, "Equity - Large Cap" },
                    { 5, "Equity - Mid Cap" },
                    { 6, "Equity - Small Cap" },
                    { 7, "Equity - ELSS Cap" },
                    { 8, "Equity - Dividend Yield MF" },
                    { 9, "Equity - Value Oriented MF" },
                    { 10, "Equity - Sectoral MF" },
                    { 11, "Debt - Liquid" },
                    { 12, "Debt - Corporate Bonds MF" },
                    { 13, "Debt - Dynamic Bond Debt MF" },
                    { 14, "Debt - Ultra Short Term MF" },
                    { 15, "Hybrid - Aggressive" },
                    { 16, "Hybrid - Balanced" },
                    { 17, "Hybrid - Equity Savings" },
                    { 18, "Hybrid - Dynamic Asset allocation" },
                    { 19, "Commodities - Gold MF (ETF)" },
                    { 20, "Commodities - Silver MF (ETF)" }
                });

            migrationBuilder.InsertData(
                table: "TSpecialEvent",
                columns: new[] { "SpecialEventId", "Description", "EventDate", "EventType", "FundId", "InDate", "Severity", "Title" },
                values: new object[,]
                {
                    { 1, "Global equity markets corrected sharply due to COVID-19 pandemic fears.", new DateTime(2020, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Market", null, new DateTime(2020, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Critical", "COVID-19 Market Crash" },
                    { 2, "Government reintroduced 10% LTCG tax on equity gains exceeding ₹1L.", new DateTime(2018, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Regulatory", null, new DateTime(2018, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Warning", "Reintroduction of LTCG Tax on Equity" },
                    { 3, "RBI increased repo rate to curb inflation, impacting debt fund NAVs.", new DateTime(2022, 5, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Macro", null, new DateTime(2022, 5, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Info", "RBI Begins Rate Hike Cycle" },
                    { 4, "Annual Union Budget presented with implications for equity and debt markets.", new DateTime(2023, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Market", null, new DateTime(2023, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Info", "Union Budget Announcement" }
                });

            migrationBuilder.InsertData(
                table: "TSystemState",
                columns: new[] { "Key", "Value" },
                values: new object[,]
                {
                    { "AppName", "MFT" },
                    { "AppVersion", "1.0.0" },
                    { "LastNavFetch", null },
                    { "LastTransactionSync", null }
                });

            migrationBuilder.InsertData(
                table: "TFolio",
                columns: new[] { "FolioId", "AMCId", "AttachedBank", "FolioHolderId", "FolioNumber", "FolioPurpose", "IsActive" },
                values: new object[,]
                {
                    { 1, 9, "JAG IDFC NRO", 3, "37959966", "Long term Portfolio", true },
                    { 2, 6, "DK HDFC NRO", 2, "499183354147", "Small can investment - experiment", true },
                    { 3, 7, "DK HDFC NRO", 2, "10121489", "Long term Investment", true },
                    { 4, 7, "DK KOTAK NRE", 2, "10510544", "Long term Repatriate Inv", true },
                    { 5, 7, "DK IDFC NRO", 2, "17412588", "Long term Repatriate Inv", true },
                    { 6, 7, "DK IDFC NRO", 3, "17713086", "Long term Repatriate Inv", true }
                });

            migrationBuilder.InsertData(
                table: "TFund",
                columns: new[] { "FundId", "AMCId", "FundName", "ISIN", "IsNavAllowed", "IsTransactionAllowed", "MFCatId", "SchemeCode" },
                values: new object[,]
                {
                    { 1, 7, "Parag Parikh Flexi Cap Fund - Direct Plan - Growth", "INF879O01027", true, true, 2, "122639" },
                    { 2, 7, "Parag Parikh Dynamic Asset Allocation Fund - Direct Plan Growth", "INF879O01266", true, true, 1, "152468" },
                    { 3, 1, "Axis Small Cap Fund - Direct Plan - Growth", "INF846K01K35", true, true, 6, "125354" },
                    { 4, 3, "Canara Robeco SMALL CAP Fund - Direct Plan - Growth", "INF760K01JC6", true, true, 17, "146130" },
                    { 5, 3, "Canara Robeco LARGE AND MID CAP Fund - Direct Plan - Growth", "INF760K01EI4", true, true, 3, "118278" },
                    { 6, 3, "Canara Robeco Value Fund - Direct Plan - Growth", "INF760K01JW4", true, true, 9, "149085" },
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

            migrationBuilder.InsertData(
                table: "TMutualFundTransaction",
                columns: new[] { "TransactionId", "AmountPaid", "FolioId", "FundId", "InDate", "NAV", "Note", "ReferenceNo", "Source", "TransactionDate", "TxnType", "Units", "UpdateDate" },
                values: new object[,]
                {
                    { 1, 4256.18m, 1, 1, new DateTime(2023, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 42.3500m, "Lumpsum investment", "TXN1001", "Initial Purchase", new DateTime(2023, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 100.500000m, new DateTime(2023, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 2210.00m, 1, 1, new DateTime(2023, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 44.2000m, "Monthly SIP", "TXN1002", "SIP", new DateTime(2023, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 50.000000m, new DateTime(2023, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 4899.78m, 1, 2, new DateTime(2023, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 65.1000m, "Diversification", "TXN1003", "Lumpsum", new DateTime(2023, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 75.250000m, new DateTime(2023, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "TSpecialEvent",
                columns: new[] { "SpecialEventId", "Description", "EventDate", "EventType", "FundId", "InDate", "Severity", "Title" },
                values: new object[] { 5, "Expense ratio revised by fund house.", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fund", 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Info", "Expense Ratio Revision" });

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
                name: "IX_TFolio_FolioHolderId",
                table: "TFolio",
                column: "FolioHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_TFolio_FolioNumber",
                table: "TFolio",
                column: "FolioNumber",
                unique: true);

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
                name: "IX_TFundNav_FundId_NavDate",
                table: "TFundNav",
                columns: new[] { "FundId", "NavDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TMFCategory_CategoryName",
                table: "TMFCategory",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TMutualFundTransaction_FolioId_TransactionDate",
                table: "TMutualFundTransaction",
                columns: new[] { "FolioId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TMutualFundTransaction_FundId",
                table: "TMutualFundTransaction",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_TMutualFundTransaction_TxnType",
                table: "TMutualFundTransaction",
                column: "TxnType");

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

            migrationBuilder.CreateIndex(
                name: "IX_SystemState_Key",
                table: "TSystemState",
                column: "Key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TFolioOwner");

            migrationBuilder.DropTable(
                name: "TFundNav");

            migrationBuilder.DropTable(
                name: "TMutualFundTransaction");

            migrationBuilder.DropTable(
                name: "TSpecialEvent");

            migrationBuilder.DropTable(
                name: "TSystemState");

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
