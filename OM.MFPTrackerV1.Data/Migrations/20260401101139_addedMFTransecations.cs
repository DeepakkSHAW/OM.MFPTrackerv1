using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OM.MFPTrackerV1.Data.Migrations
{
    /// <inheritdoc />
    public partial class addedMFTransecations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TMFTransaction_TFolio_FolioId",
                table: "TMFTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_TMFTransaction_TFund_FundId",
                table: "TMFTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TMFTransaction",
                table: "TMFTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TMFTransaction_Date",
                table: "TMFTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TMFTransaction_FolioId",
                table: "TMFTransaction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Txn_Note_Len",
                table: "TMFTransaction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Txn_Source_Len",
                table: "TMFTransaction");

            migrationBuilder.RenameTable(
                name: "TMFTransaction",
                newName: "TMutualFundTransaction");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "TMutualFundTransaction",
                newName: "TransactionDate");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TMutualFundTransaction",
                newName: "TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_TMFTransaction_FundId",
                table: "TMutualFundTransaction",
                newName: "IX_TMutualFundTransaction_FundId");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "TMutualFundTransaction",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true,
                oldCollation: "NOCASE");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "TMutualFundTransaction",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true,
                oldCollation: "NOCASE");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNo",
                table: "TMutualFundTransaction",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TMutualFundTransaction",
                table: "TMutualFundTransaction",
                column: "TransactionId");

            migrationBuilder.UpdateData(
                table: "TMutualFundTransaction",
                keyColumn: "TransactionId",
                keyValue: 1,
                columns: new[] { "AmountPaid", "InDate", "NAV", "Note", "ReferenceNo", "Source", "TransactionDate", "Units", "UpdateDate" },
                values: new object[] { 4256.18m, new DateTime(2023, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 42.3500m, "Lumpsum investment", "TXN1001", "Initial Purchase", new DateTime(2023, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 100.500000m, new DateTime(2023, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "TMutualFundTransaction",
                columns: new[] { "TransactionId", "AmountPaid", "FolioId", "FundId", "InDate", "NAV", "Note", "ReferenceNo", "Source", "TransactionDate", "TxnType", "Units", "UpdateDate" },
                values: new object[,]
                {
                    { 2, 2210.00m, 1, 1, new DateTime(2023, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 44.2000m, "Monthly SIP", "TXN1002", "SIP", new DateTime(2023, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 50.000000m, new DateTime(2023, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 4899.78m, 1, 2, new DateTime(2023, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 65.1000m, "Diversification", "TXN1003", "Lumpsum", new DateTime(2023, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 75.250000m, new DateTime(2023, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TMutualFundTransaction_FolioId_TransactionDate",
                table: "TMutualFundTransaction",
                columns: new[] { "FolioId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TMutualFundTransaction_TxnType",
                table: "TMutualFundTransaction",
                column: "TxnType");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MFTransaction_AmountPaid_Positive",
                table: "TMutualFundTransaction",
                sql: "AmountPaid >= 0.0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MFTransaction_NAV_Positive",
                table: "TMutualFundTransaction",
                sql: "NAV >= 0.0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MFTransaction_Units_Positive",
                table: "TMutualFundTransaction",
                sql: "Units >= 0.0");

            migrationBuilder.AddForeignKey(
                name: "FK_TMutualFundTransaction_TFolio_FolioId",
                table: "TMutualFundTransaction",
                column: "FolioId",
                principalTable: "TFolio",
                principalColumn: "FolioId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TMutualFundTransaction_TFund_FundId",
                table: "TMutualFundTransaction",
                column: "FundId",
                principalTable: "TFund",
                principalColumn: "FundId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TMutualFundTransaction_TFolio_FolioId",
                table: "TMutualFundTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_TMutualFundTransaction_TFund_FundId",
                table: "TMutualFundTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TMutualFundTransaction",
                table: "TMutualFundTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TMutualFundTransaction_FolioId_TransactionDate",
                table: "TMutualFundTransaction");

            migrationBuilder.DropIndex(
                name: "IX_TMutualFundTransaction_TxnType",
                table: "TMutualFundTransaction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MFTransaction_AmountPaid_Positive",
                table: "TMutualFundTransaction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MFTransaction_NAV_Positive",
                table: "TMutualFundTransaction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MFTransaction_Units_Positive",
                table: "TMutualFundTransaction");

            migrationBuilder.DeleteData(
                table: "TMutualFundTransaction",
                keyColumn: "TransactionId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TMutualFundTransaction",
                keyColumn: "TransactionId",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "ReferenceNo",
                table: "TMutualFundTransaction");

            migrationBuilder.RenameTable(
                name: "TMutualFundTransaction",
                newName: "TMFTransaction");

            migrationBuilder.RenameColumn(
                name: "TransactionDate",
                table: "TMFTransaction",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "TMFTransaction",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_TMutualFundTransaction_FundId",
                table: "TMFTransaction",
                newName: "IX_TMFTransaction_FundId");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "TMFTransaction",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "TMFTransaction",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TMFTransaction",
                table: "TMFTransaction",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "TMFTransaction",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AmountPaid", "Date", "InDate", "NAV", "Note", "Source", "Units", "UpdateDate" },
                values: new object[] { 480m, new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 12m, null, "Kotak Bank NRO", 40m, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_TMFTransaction_Date",
                table: "TMFTransaction",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_TMFTransaction_FolioId",
                table: "TMFTransaction",
                column: "FolioId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Txn_Note_Len",
                table: "TMFTransaction",
                sql: "Note IS NULL OR length(Note) <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Txn_Source_Len",
                table: "TMFTransaction",
                sql: "Source IS NULL OR length(Source) <= 50");

            migrationBuilder.AddForeignKey(
                name: "FK_TMFTransaction_TFolio_FolioId",
                table: "TMFTransaction",
                column: "FolioId",
                principalTable: "TFolio",
                principalColumn: "FolioId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TMFTransaction_TFund_FundId",
                table: "TMFTransaction",
                column: "FundId",
                principalTable: "TFund",
                principalColumn: "FundId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
