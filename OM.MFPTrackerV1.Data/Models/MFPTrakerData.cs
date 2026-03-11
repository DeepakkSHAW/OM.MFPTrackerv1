using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OM.MFPTrackerV1.Data.Models
{
	public class FolioHolder
	{
		[Key]
		public int FolioHolderId { get; set; }          // Primary Key

		[Required(ErrorMessage = "First Name is Required")]
		[MaxLength(100)]
		public string FirstName { get; set; } = null!;

		[Required(ErrorMessage = "last Name is Required")]
		[StringLength(100, MinimumLength = 4, ErrorMessage = "Last name should be between 100 to 3 characters long")]
		public string LastName { get; set; } = null!;

		[DataType(DataType.Date)]
		[DateInPastAttribute(ErrorMessage = "Date of Birth must be in the past")]
		public DateTime DateOfBirth { get; set; }

		[Required(ErrorMessage = "Signature is Required")]
		[StringLength(5, MinimumLength = 2, ErrorMessage = "Signature should be between 5 to 2 characters long")]
		public string Signature { get; set; } = null!;
		public DateTime InDate { get; set; }//only for audit purposes
		public DateTime UpdateDate { get; set; }//only for audit purposes

		// Navigation
		//public ICollection<Folio> Folios { get; set; } = new List<Folio>();
	}
	public class MFCategory
	{
		[Key]
		public int MFCatId { get; set; }
		[Required(ErrorMessage = "Category Name is Required")]
		[StringLength(50, MinimumLength = 8, ErrorMessage = "Category Name should be between 50 to 8 characters long")]
		public string CategoryName { get; set; } = null!;

		// Navigation
		//public ICollection<MutualFund> MutualFunds { get; set; } = new List<MutualFund>();
	}
	public class Fund
	{
		[Key]
		public int FundId { get; set; }
		
		[Required(ErrorMessage = "Fund Name is Required")]
		[StringLength(100, MinimumLength = 5, ErrorMessage = "Fund Name should be between 100 to 5 characters long")]
		public string FundName { get; set; } = null!;

		[Required(ErrorMessage = "Scheme Code is Required")]
		[MaxLength(20)]
		public string SchemeCode { get; set; } = null!;

		[Required(ErrorMessage = "ISIN Code is Required")]
		[MaxLength(20)]
		public string ISIN { get; set; } = null!;

		[Required(ErrorMessage = "AMC Name is Required")]
		[MaxLength(100)]
		public string AMCName { get; set; } = null!;
		[Required]
		public bool IsTransactionAllowed { get; set; } = true;
		[Required]
		public bool IsNavAllowed { get; set; } = true;

		// ==========================
		// 🔹 Foreign Keys
		// ==========================
		public int MFCatId { get; set; } // Foreign Key
		public MFCategory? Category { get; set; }// Navigation
		// ==========================
		// 🔹 audit purposes
		// ==========================
		public DateTime InDate { get; set; }
		public DateTime UpdateDate { get; set; }
	}
}
