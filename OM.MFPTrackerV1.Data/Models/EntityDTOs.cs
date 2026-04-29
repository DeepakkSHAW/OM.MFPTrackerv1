using System;
using System.Collections.Generic;
using System.Text;

namespace OM.MFPTrackerV1.Data.Models
{
	public sealed class TransactionImportSummary
	{
		public int Inserted { get; set; }
		public int SkippedAsDuplicate { get; set; }
		public int InvalidRows { get; set; }

		public override string ToString()
		{
			return $"{Inserted} inserted, " +
				   $"{SkippedAsDuplicate} skipped (duplicate), " +
				   $"{InvalidRows} skipped (invalid rows)";
		}
	}

	public sealed class FolioTransactionPreviewRow
	{
		// Raw identity fields
		public string FolioNumber { get; set; } = string.Empty;
		public string FundCode { get; set; } = string.Empty;

		// Transaction details
		public DateTime TransactionDate { get; set; }
		public decimal Units { get; set; }
		public decimal Nav { get; set; }

		// Derived field (NAV × Units)
		public decimal Amount { get; set; }

		// Preview classification
		public PreviewRowStatus Status { get; set; }

		// optional metadata
		public string? ReferenceNo { get; set; }
		public string? Source { get; set; }
		public string? Note { get; set; }

	}
	public enum PreviewRowStatus
	{
		Valid,
		Duplicate,
		Invalid
	}
	public sealed class NavSyncSummary
	{
		public int Inserted { get; init; }
		public int SkippedAsDuplicate { get; init; }
		public int FundNotFound { get; init; }
		public int InvalidRows { get; init; }

		public int TotalProcessed =>
			Inserted + SkippedAsDuplicate + FundNotFound + InvalidRows;

		public override string ToString()
		{
			return $"{Inserted} inserted, " +
				   $"{SkippedAsDuplicate} skipped (duplicate), " +
				   $"{FundNotFound} skipped (fund not found), " +
				   $"{InvalidRows} skipped (invalid rows)";
		}
	}

	public sealed record FundLookupDto(int Id, string Name);
	public sealed record FundNavPoint(
		DateTime NavDate,
		decimal NavValue
	);
}
