using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OM.MFPTrackerV1.Data.Models
{
	//public class FolioHolder
	//{
	//	[Key]
	//	public int FolioHolderId { get; set; }          // Primary Key

	//	[Required(ErrorMessage = "First Name is Required")]
	//	[MaxLength(100)]
	//	public string FirstName { get; set; } = null!;

	//	[Required(ErrorMessage = "last Name is Required")]
	//	[StringLength(100, MinimumLength = 4, ErrorMessage = "Last name should be between 100 to 3 characters long")]
	//	public string LastName { get; set; } = null!;

	//	[DataType(DataType.Date)]
	//	[DateInPastAttribute(ErrorMessage = "Date of Birth must be in the past")]
	//	public DateTime DateOfBirth { get; set; }

	//	[Required(ErrorMessage = "Signature is Required")]
	//	[StringLength(5, MinimumLength = 2, ErrorMessage = "Signature should be between 5 to 2 characters long")]
	//	public string Signature { get; set; } = null!;
	//	public DateTime InDate { get; set; }//only for audit purposes
	//	public DateTime UpdateDate { get; set; }//only for audit purposes

	//	// Navigation
	//	public ICollection<Folio> Folios { get; set; } = new List<Folio>();
	//}
	//public class MFCategory
	//{
	//	[Key]
	//	public int MFCatId { get; set; }
	//	[Required(ErrorMessage = "Category Name is Required")]
	//	[StringLength(50, MinimumLength = 8, ErrorMessage = "Category Name should be between 50 to 8 characters long")]
	//	public string CategoryName { get; set; } = null!;

	//	//Navigation
	//	public ICollection<Fund> Funds { get; set; } = new List<Fund>();

	//	// (Optional) convenience for UI; set at runtime
	//	[NotMapped] public int FundCount { get; set; }
	//}
	//public class Fund
	//{
	//	[Key]
	//	public int FundId { get; set; }

	//	[Required(ErrorMessage = "Fund Name is Required")]
	//	[StringLength(100, MinimumLength = 5, ErrorMessage = "Fund Name should be between 100 to 5 characters long")]
	//	public string FundName { get; set; } = null!;

	//	[Required(ErrorMessage = "Scheme Code is Required")]
	//	[MaxLength(20)]
	//	public string SchemeCode { get; set; } = null!;

	//	[Required(ErrorMessage = "ISIN Code is Required")]
	//	[MaxLength(20)]
	//	public string ISIN { get; set; } = null!;

	//	[Required(ErrorMessage = "AMC Name is Required")]
	//	[MaxLength(100)]
	//	public string AMCName { get; set; } = null!;
	//	[Required]
	//	public bool IsTransactionAllowed { get; set; } = true;
	//	[Required]
	//	public bool IsNavAllowed { get; set; } = true;

	//	// ==========================
	//	// 🔹 Foreign Keys
	//	// ==========================
	//	public int MFCatId { get; set; } // Foreign Key
	//	public MFCategory? Category { get; set; }// Navigation

	//	// 🔹 Inverse navigation (1 Fund → many Folios)
	//	public ICollection<Folio> Folios { get; set; } = new List<Folio>();

	//	// ==========================
	//	// 🔹 audit purposes
	//	// ==========================
	//	public DateTime InDate { get; set; }
	//	public DateTime UpdateDate { get; set; }
	//}
	//public class Folio
	//{
	//	[Key]
	//	public int FolioId { get; set; }
	//	[Required(ErrorMessage = "Folio Number is Required")]
	//	[StringLength(50, MinimumLength = 5, ErrorMessage = "Folio Number should be between 50 to 5 characters long")]
	//	public string FolioNumber { get; set; } = null!;

	//	[MaxLength(100, ErrorMessage = "Folio Purpose shouldn't be more than 100 characters long")]
	//	public string? FolioPurpose { get; set; }

	//	public bool IsActive { get; set; } = true;

	//	[MaxLength(50, ErrorMessage = "Attached Bank to this Folio shouldn't be more than 50 characters long")]
	//	public string? AttachedBank { get; set; }
	//	// ==========================
	//	// 🔹 Foreign Keys
	//	// ==========================
	//	public int FolioHolderId { get; set; } // Foreign Key
	//	public FolioHolder? Holder { get; set; } // Navigation
	//	public int FundId { get; set; } // Foreign Key
	//	public Fund? Fund { get; set; } // Navigation
	//	public DateTime InDate { get; set; }
	//	public DateTime UpdateDate { get; set; }
	//}

	public enum TransactionType
	{
		BUY = 1,
		SELL = 2,
		SIP = 3,
		SWITCH_IN = 4,
		SWITCH_OUT = 5,
		DIVIDEND_REINVEST = 6
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
		[RegularExpression(@"^[A-Za-z0-9 ]{5,50}$", ErrorMessage = "Folio Number Name must be alphanumeric (letters, numbers, spaces only) and 5–50 characters.")]
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
		[Key] public int Id { get; set; }

		[Required] public DateTime Date { get; set; }

		[Required] public int FolioId { get; set; }
		public Folio Folio { get; set; } = null!;

		[Required] public int FundId { get; set; }
		public Fund Fund { get; set; } = null!;

		// Domain: Buy/Sell/DividendReinvest etc.
		//[Required, MaxLength(20)]
		//public string TxnType { get; set; } = "BUY";
		public TransactionType TxnType { get; set; } = TransactionType.BUY;

		[Range(0.000001, double.MaxValue)]
		public decimal Units { get; set; }

		[Range(0.000001, double.MaxValue)]
		public decimal NAV { get; set; }

		public decimal AmountPaid { get; set; }     // computed Units * NAV (rounded bank-style)
		[MaxLength(50)] public string? Source { get; set; }
		[MaxLength(100)] public string? Note { get; set; }

		// Audit
		public DateTime InDate { get; set; }
		public DateTime UpdateDate { get; set; }
	}
}
