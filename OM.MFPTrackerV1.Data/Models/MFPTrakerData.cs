using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OM.MFPTrackerV1.Data.Models
{
	public enum TransactionType
	{
		BUY = 1,
		SELL = 2,
		SIP = 3,
		SWITCH_IN = 4,
		SWITCH_OUT = 5,
		DIV_REINVEST = 6,
		DIV_PAYOUT = 7
	}
	public class AMC
	{
		[Key] public int AMCId { get; set; }

		//[Required, StringLength(100, MinimumLength = 2, ErrorMessage = "AMC Name should be between 2 to 100 Characters long.")]
		[Required]
		[StringLength(100, MinimumLength = 2, ErrorMessage = "AMC Name should be between 2 and 100 characters.")]
		[RegularExpression(@"^[A-Za-z0-9 _-]{2,100}$", ErrorMessage = "AMC Name must be alphanumeric (letters, numbers, spaces only) and 2–100 characters.")]
		public string AMCName { get; set; } = null!;

		public ICollection<Fund> Funds { get; set; } = new List<Fund>();
		public ICollection<Folio> Folios { get; set; } = new List<Folio>();
	}

	public class MFCategory
	{
		[Key] public int MFCatId { get; set; }

		//[Required, StringLength(50, MinimumLength = 3)]
		[Required]
		[StringLength(50, MinimumLength = 3, ErrorMessage = "Category should be between 3 and 50 characters.")]
		[RegularExpression(@"^[A-Za-z0-9 _-]{3,50}$", ErrorMessage = "Category Name must be alphanumeric (letters, numbers, spaces only) and 3–50 characters.")]
		public string CategoryName { get; set; } = null!;

		public ICollection<Fund> Funds { get; set; } = new List<Fund>();
	}

	public class Fund
	{
		[Key] public int FundId { get; set; }

		[Required]
		[StringLength(100, MinimumLength = 5, ErrorMessage = "Fund Name should be between 5 and 100 characters.")]
		//[RegularExpression(@"^[A-Za-z0-9 _-]{5,100}$", ErrorMessage = "Fund Name must be alphanumeric (letters, numbers, spaces only) and 5–100 characters.")]
		//[Required, StringLength(100, MinimumLength = 5)]
		public string FundName { get; set; } = null!;

		[Required, MaxLength(20, ErrorMessage = "SchemeCode: Max allowed character in 20")] public string SchemeCode { get; set; } = null!;
		[Required, MaxLength(20, ErrorMessage = "ISIN: Max allowed character in 20")] public string ISIN { get; set; } = null!;

		[Required] public bool IsTransactionAllowed { get; set; } = true;
		[Required] public bool IsNavAllowed { get; set; } = true;


		// ---------- Foreign Keys ----------
		[Required]
		public int AMCId { get; set; }
		public AMC? AMC { get; set; }

		[Required]
		public int MFCatId { get; set; }
		public MFCategory? Category { get; set; }

		// ---------- Audit ----------
		public DateTime InDate { get; set; }
		public DateTime UpdateDate { get; set; }

	}

	public class FolioOwner
	{
		[Key] public int FolioOwnerId { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 2, ErrorMessage = "First Name should be between 2 and 50 characters.")]
		public string FirstName { get; set; } = null!;
		[Required]
		[StringLength(50, MinimumLength = 2, ErrorMessage = "last Name should be between 2 and 50 characters.")]
		public string LastName { get; set; } = null!;
		public DateTime DateOfBirth { get; set; }

		[Required]
		[StringLength(5, MinimumLength = 2, ErrorMessage = "Signature should be between 2 and 5 characters.")]
		public string Signature { get; set; } = null!;
	}
	public class FolioHolder
	{
		[Key] public int FolioHolderId { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 3, ErrorMessage = "First Name should be between 3 and 50 characters.")]
		public string FirstName { get; set; } = null!;

		[Required]
		[StringLength(50, MinimumLength = 3, ErrorMessage = "Last Name should be between 3 and 50 characters.")]
		public string LastName { get; set; } = null!;

		public DateTime DateOfBirth { get; set; }

		[Required]
		[StringLength(5, MinimumLength = 2, ErrorMessage = "Signature should be between 2 and 5 characters.")]
		public string Signature { get; set; } = null!;

		public DateTime InDate { get; set; }
		public DateTime UpdateDate { get; set; }

		public ICollection<Folio> Folios { get; set; } = new List<Folio>();
	}

	public class FolioHolder_v1
	{
		[Key] public int FolioHolderId { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 3, ErrorMessage = "First Name should be between 3 and 50 characters.")]
		[RegularExpression(@"^[A-Za-z0-9 \-\_]{3,50}$", ErrorMessage = "Name must be alphanumeric (letters, numbers, spaces only) and 3–50 characters.")]
		public string FirstName { get; set; } = null!;
		[Required]
		[StringLength(50, MinimumLength = 3, ErrorMessage = "Last Name should be between 3 and 50 characters.")]
		[RegularExpression(@"^[A-Za-z0-9 \-\_]{3,50}$", ErrorMessage = "Last Name must be alphanumeric (letters, numbers, spaces only) and 3–50 characters.")]
		public string LastName { get; set; } = null!;

		//[DataType(DataType.Date)]
		//[DateInPastAttribute(ErrorMessage = "Date of Birth must be in the past")]
		public DateTime DateOfBirth { get; set; }

		[Required]
		[StringLength(5, MinimumLength = 2, ErrorMessage = "Signature should be between 2 and 5 characters.")]
		[RegularExpression(@"^[A-Za-z0-9 ]{2,5}$", ErrorMessage = "Only 2-5 characters allowed")]
		public string Signature { get; set; } = null!;

		public DateTime InDate { get; set; }
		public DateTime UpdateDate { get; set; }

		public ICollection<Folio> Folios { get; set; } = new List<Folio>();
	}

	public class Folio
	{
		[Key] public int FolioId { get; set; }
		[Required]
		[StringLength(50, MinimumLength = 5, ErrorMessage = "Folio Number should be between 5 and 50 characters.")]
		//[RegularExpression(@"^[A-Za-z0-9 ]{5,50}$", ErrorMessage = "Folio Number Name must be alphanumeric (letters, numbers, spaces only) and 5–50 characters.")]
		public string FolioNumber { get; set; } = null!;

		[MaxLength(100)] public string? FolioPurpose { get; set; }
		[MaxLength(50)] public string? AttachedBank { get; set; }
		public bool IsActive { get; set; } = true;

		// FKs
		public int AMCId { get; set; }             // Folio is per AMC
		public AMC? AMC { get; set; }

		public int FolioHolderId { get; set; }     // Folio belongs to one holder
		public FolioHolder? Holder { get; set; }

		// Audit
		public DateTime InDate { get; set; }
		public DateTime UpdateDate { get; set; }
	}

	public class MutualFundTransaction
	{
		[Key]
		public int TransactionId { get; set; }

		[Required]
		public DateTime TransactionDate { get; set; }

		[Required]
		public int FolioId { get; set; }
		public Folio Folio { get; set; } = null!;

		[Required]
		public int FundId { get; set; }
		public Fund Fund { get; set; } = null!;

		public TransactionType TxnType { get; set; } = TransactionType.BUY;

		[Range(0.000001, double.MaxValue)]
		public decimal Units { get; set; }

		[Range(0.000001, double.MaxValue)]
		public decimal NAV { get; set; }

		// Computed: Units * NAV (rounded)
		public decimal AmountPaid { get; set; }

		//Reference / meta fields
		[MaxLength(50)] public string? ReferenceNo { get; set; }
		[MaxLength(50)] public string? Source { get; set; }
		[MaxLength(100)] public string? Note { get; set; }

		// Audit fields
		public DateTime InDate { get; set; }
		public DateTime UpdateDate { get; set; }
	}

	public class FundNav
	{
		[Key]
		public int FundNavId { get; set; }

		// -------- Identity --------
		[Required]
		public int FundId { get; set; }
		public Fund Fund { get; set; } = null!;

		// -------- NAV Data --------
		[Required]
		public DateTime NavDate { get; set; }   // NAV as on this date

		[Required]
		public decimal NavValue { get; set; }   // NAV value

		// -------- Metadata --------
		[Required, MaxLength(20)]
		public string Source { get; set; } = "AMFI";

		public DateTime FetchedAt { get; set; }

		// -------- Audit --------
		public DateTime InDate { get; set; }
	}

	public class SpecialEvent
	{
		[Key]
		public int SpecialEventId { get; set; }

		// -------- Event Core --------
		[Required, MaxLength(100)]
		public string Title { get; set; } = null!;

		[MaxLength(500)]
		public string? Description { get; set; }

		[Required]
		public DateTime EventDate { get; set; }

		// -------- Scope --------
		// NULL = Market-wide event
		public int? FundId { get; set; }
		public Fund? Fund { get; set; }

		// -------- Classification --------
		[Required, MaxLength(30)]
		public string EventType { get; set; } = null!;
		// Examples: Market, Regulatory, Fund, Corporate

		[MaxLength(20)]
		public string Severity { get; set; } = "Info";
		// Examples: Info, Warning, Critical

		// -------- Audit --------
		public DateTime InDate { get; set; }
	}

	public sealed class SystemState
	{
		[Key]
		[MaxLength(100)]
		public string Key { get; set; } = string.Empty;

		[MaxLength(500)]
		public string? Value { get; set; }

		// -------- Audit --------
		public DateTime InDate { get; set; }
		public DateTime UpdateDate { get; set; }
	}
}
