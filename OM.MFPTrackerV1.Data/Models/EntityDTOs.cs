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
	public class BubblePointDto
	{
		public DateTime Date { get; set; }
		public decimal Nav { get; set; }
		public decimal TotalInvestment { get; set; }
		public decimal Units { get; set; }

		public int FundId { get; set; }
		public int HolderId { get; set; }

		public string FundName { get; set; } = "";
		public string HolderName { get; set; } = "";
	}
	public sealed class TransactionFilter
	{
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

		public int? CategoryId { get; set; }
		public int? AMCId { get; set; }
		public int? FundId { get; set; }
		public int? FolioId { get; set; }
		public int? HolderId { get; set; }
	}
	public class TransactionExplorerFilter
	{
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

		public int? CategoryId { get; set; }
		public int? AmcId { get; set; }
		public int? FundId { get; set; }
		public int? FolioId { get; set; }
		public int? HolderId { get; set; }
		public TransactionType? TxnType { get; set; }  
		public string? FreeText { get; set; } 
	}

	public class PortfolioCashFlowDto
	{
		public int? TransactionId { get; set; }   // null for NAV row
		public DateTime Date { get; set; }
		public decimal Amount { get; set; }       // signed cash flow
		public string TxnType { get; set; } = "";
	}
	public class PortfolioReturnResultDto
	{
		// Existing
		public decimal? Xirr { get; set; }
		public decimal? AnnualizedReturn { get; set; }

		// ✅ NEW
		public decimal InvestedAmount { get; set; }
		public decimal CurrentValue { get; set; }
		public decimal AbsoluteReturn { get; set; }      // %
		public decimal AbsoluteReturnAmount { get; set; }

		public List<PortfolioCashFlowDto> CashFlows { get; set; }
			= new();
	}
}
