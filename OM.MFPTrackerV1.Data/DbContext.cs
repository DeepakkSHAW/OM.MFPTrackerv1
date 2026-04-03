using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data.Models;

namespace OM.MFPTrackerV1.Data
{
	public class MFPTrackerDbContext : DbContext
	{
		public MFPTrackerDbContext(DbContextOptions<MFPTrackerDbContext> options) : base(options) { }
		public DbSet<AMC> AMCs => Set<AMC>();
		public DbSet<MFCategory> MFCategories => Set<MFCategory>();
		public DbSet<Fund> Funds => Set<Fund>();
		public DbSet<FolioHolder> FolioHolders => Set<FolioHolder>();
		public DbSet<Folio> Folios => Set<Folio>();
		public DbSet<MutualFundTransaction> MutualFundTransactions => Set<MutualFundTransaction>();
		public DbSet<FolioOwner> folioOwners => Set<FolioOwner>();
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{

			// -------------------- AMC --------------------
			modelBuilder.Entity<AMC>(e =>
			{
				e.HasKey(x => x.AMCId);
				e.Property(x => x.AMCName).IsRequired().UseCollation("NOCASE").HasMaxLength(100);
				e.HasIndex(x => x.AMCName).IsUnique(); // one AMC per name (NOCASE)

				e.HasMany(x => x.Funds).WithOne(f => f.AMC).HasForeignKey(f => f.AMCId).OnDelete(DeleteBehavior.Restrict);
				e.HasMany(x => x.Folios).WithOne(f => f.AMC).HasForeignKey(f => f.AMCId).OnDelete(DeleteBehavior.Restrict);

				e.ToTable("TAMC", t =>
				{
					t.HasCheckConstraint("CK_AMC_AMCName_Len", "length(AMCName) BETWEEN 2 AND 100");
				});
				// Data seeding
				e.HasData(
					new AMC { AMCId = 1, AMCName = "Axis Mutual Fund" },
					new AMC { AMCId = 2, AMCName = "Bandhan Mutual Fund" },
					new AMC { AMCId = 3, AMCName = "Canara Robeco Mutual Fund" },
					new AMC { AMCId = 4, AMCName = "Kotak Mutual Fund" },
					new AMC { AMCId = 5, AMCName = "Mirae Asset Mutual Fund" },
					new AMC { AMCId = 6, AMCName = "Nippon India Mutual Fund" },
					new AMC { AMCId = 7, AMCName = "Parag Parikh Mutual Fund" },
					new AMC { AMCId = 8, AMCName = "SBI Mutual Fund" },
					new AMC { AMCId = 9, AMCName = "HDFC Mutual Fund" },
					new AMC { AMCId = 10, AMCName = "TATA Mutual Fund" },
					new AMC { AMCId = 11, AMCName = "ICICI Mutual Fund" },
					new AMC { AMCId = 12, AMCName = "UTI Mutual Fund" },
					new AMC { AMCId = 13, AMCName = "Aditya Birla Sun Life Mutual Fund" },
					new AMC { AMCId = 14, AMCName = "Jio BlackRock Mutual Fund" }
						);
			});

			// -------------------- MFCategory --------------------
			modelBuilder.Entity<MFCategory>(e =>
			{
				e.HasKey(p => p.MFCatId);
				e.Property(p => p.CategoryName).IsRequired().UseCollation("NOCASE").HasMaxLength(50);
				e.HasIndex(c => c.CategoryName).IsUnique();

				e.HasMany(c => c.Funds).WithOne(f => f.Category).HasForeignKey(f => f.MFCatId).OnDelete(DeleteBehavior.Restrict);

				e.ToTable("TMFCategory", t =>
				{
					t.HasCheckConstraint("CK_MFCat_Name_Len", "length(CategoryName) BETWEEN 3 AND 50");
				});

				e.HasData(
					new MFCategory { MFCatId = 1, CategoryName = "Equity - Multi Cap" },
					new MFCategory { MFCatId = 2, CategoryName = "Equity - Flexi Cap" },
					new MFCategory { MFCatId = 3, CategoryName = "Equity - Large Mid Cap" },
					new MFCategory { MFCatId = 4, CategoryName = "Equity - Large Cap" },
					new MFCategory { MFCatId = 5, CategoryName = "Equity - Mid Cap" },
					new MFCategory { MFCatId = 6, CategoryName = "Equity - Small Cap" },
					new MFCategory { MFCatId = 7, CategoryName = "Equity - ELSS Cap" },
					new MFCategory { MFCatId = 8, CategoryName = "Equity - Dividend Yield MF" },
					new MFCategory { MFCatId = 9, CategoryName = "Equity - Value Oriented MF" },
					new MFCategory { MFCatId = 10, CategoryName = "Equity - Sectoral MF" },
					new MFCategory { MFCatId = 11, CategoryName = "Debt - Liquid" },
					new MFCategory { MFCatId = 12, CategoryName = "Debt - Corporate Bonds MF" },
					new MFCategory { MFCatId = 13, CategoryName = "Debt - Dynamic Bond Debt MF" },
					new MFCategory { MFCatId = 14, CategoryName = "Debt - Ultra Short Term MF" },
					new MFCategory { MFCatId = 15, CategoryName = "Hybrid - Aggressive" },
					new MFCategory { MFCatId = 16, CategoryName = "Hybrid - Balanced" },
					new MFCategory { MFCatId = 17, CategoryName = "Hybrid - Equity Savings" },
					new MFCategory { MFCatId = 18, CategoryName = "Hybrid - Dynamic Asset allocation" },
					new MFCategory { MFCatId = 19, CategoryName = "Commodities - Gold MF (ETF)" },
					new MFCategory { MFCatId = 20, CategoryName = "Commodities - Silver MF (ETF)" }
						);
			});

			// -------------------- Fund --------------------
			modelBuilder.Entity<Fund>(e =>
			{
				e.ToTable("TFund");
				e.HasKey(p => p.FundId);

				e.Property(p => p.FundName).IsRequired().UseCollation("NOCASE").HasMaxLength(100);
				e.Property(p => p.SchemeCode).IsRequired().UseCollation("NOCASE").HasMaxLength(20);
				e.Property(p => p.ISIN).IsRequired().UseCollation("NOCASE").HasMaxLength(20);

				e.Property(p => p.IsTransactionAllowed).HasDefaultValue(true);
				e.Property(p => p.IsNavAllowed).HasDefaultValue(true);

				e.Property(p => p.InDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
				e.Property(p => p.UpdateDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();


				// ---- Indexes ----
				e.HasIndex(c => c.FundName); // NOT unique
				e.HasIndex(c => new { c.AMCId, c.SchemeCode }).IsUnique();
				e.HasIndex(c => c.ISIN).IsUnique();

				e.HasIndex(c => c.AMCId);
				e.HasIndex(c => c.MFCatId);

				// ---- Constraints ----
				e.HasCheckConstraint("CK_Fund_FundName_Len", "length(FundName) BETWEEN 5 AND 100");
				e.HasCheckConstraint("CK_Fund_SchemeCode_Len", "length(SchemeCode) BETWEEN 1 AND 20");
				e.HasCheckConstraint("CK_Fund_ISIN_Len", "length(ISIN) BETWEEN 1 AND 20");

				e.HasData(
					new Fund
					{
						FundId = 1,
						FundName = "Parag Parikh Flexi Cap Fund - Direct Plan - Growth",
						SchemeCode = "122639",
						ISIN = "INF879O01027",
						AMCId = 7,
						MFCatId = 2,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 2,
						FundName = "Parag Parikh Dynamic Asset Allocation Fund - Direct Plan Growth",
						SchemeCode = "152468",
						ISIN = "INF879O01266",
						AMCId = 7,
						MFCatId = 1,//18
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 3,
						FundName = "Axis Small Cap Fund - Direct Plan - Growth",
						SchemeCode = "125354",
						ISIN = "INF846K01K35",
						AMCId = 1,
						MFCatId = 6,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 4,
						FundName = "Canara Robeco SMALL CAP Fund - Direct Plan - Growth",
						SchemeCode = "146130",
						ISIN = "INF760K01JC6",
						AMCId = 3,
						MFCatId = 17,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 5,
						FundName = "Canara Robeco LARGE AND MID CAP Fund - Direct Plan - Growth",
						SchemeCode = "118278",
						ISIN = "INF760K01EI4",
						AMCId = 3,
						MFCatId = 3,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 6,
						FundName = "Canara Robeco Value Fund - Direct Plan - Growth",
						SchemeCode = "149085",
						ISIN = "INF760K01JW4",
						AMCId = 3,
						MFCatId = 9,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 7,
						FundName = "Kotak Large & Midcap Fund - Direct- Growth",
						SchemeCode = "120158",
						ISIN = "INF174K01LF9",
						AMCId = 4,
						MFCatId = 3,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 8,
						FundName = "Mirae Asset Large & Midcap Fund - Direct Plan - Growth",
						SchemeCode = "118834",
						ISIN = "INF769K01BI1",
						AMCId = 5,
						MFCatId = 3,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 9,
						FundName = "Mirae Asset ELSS Tax Saver Fund - Direct Plan - Growth",
						SchemeCode = "135781",
						ISIN = "INF769K01DM9",
						AMCId = 5,
						MFCatId = 7,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 10,
						FundName = "Nippon India Small Cap Fund - Direct Plan - Growth",
						SchemeCode = "118778",
						ISIN = "INF204K01K15",
						AMCId = 6,
						MFCatId = 6,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 11,
						FundName = "SBI Large Cap Fund - Direct Plan - Growth",
						SchemeCode = "119598",
						ISIN = "INF200K01QX4",
						AMCId = 8,
						MFCatId = 4,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 12,
						FundName = "SBI Small Cap Fund - Direct Plan - Growth",
						SchemeCode = "125497",
						ISIN = "INF200K01T51",
						AMCId = 8,
						MFCatId = 6,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 13,
						FundName = "Tata Retirement Savings Fund- Progressive Plan - Direct Plan - Growth",
						SchemeCode = "119251",
						ISIN = "INF277K01QO1",
						AMCId = 10,
						MFCatId = 2,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 14,
						FundName = "Tata S&P BSE Sensex Index Fund - Direct Plan",
						SchemeCode = "119287",
						ISIN = "INF277K01PK1",
						AMCId = 10,
						MFCatId = 4,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 15,
						FundName = "BANDHAN Small Cap Fund - Regular Plan - Growth",
						SchemeCode = "147944",
						ISIN = "INF194KB1AJ8",
						AMCId = 2,
						MFCatId = 6,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 16,
						FundName = "HDFC Small Cap Fund - Growth Option - Direct Plan",
						SchemeCode = "130503",
						ISIN = "INF179KA1RW5",
						AMCId = 9,
						MFCatId = 6,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 17,
						FundName = "HDFC Gold ETF Fund of Fund - Direct Plan",
						SchemeCode = "119132",
						ISIN = "INF179K01VX0",
						AMCId = 9,
						MFCatId = 19,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					}
				);
			});
			// -------------------- FolioOwner Only for Bug fixing --------------------
			modelBuilder.Entity<FolioOwner>(e =>
			{
				e.ToTable("TFolioOwner");
				e.HasKey(x => x.FolioOwnerId);

				e.Property(x => x.FirstName).IsRequired().UseCollation("NOCASE").HasMaxLength(50);
				e.Property(x => x.LastName).IsRequired().UseCollation("NOCASE").HasMaxLength(50);
				e.Property(x => x.Signature).IsRequired().UseCollation("NOCASE").HasMaxLength(5);

				e.HasIndex(x => x.Signature).IsUnique();
				e.HasIndex(x => new { x.FirstName, x.LastName, x.DateOfBirth }).IsUnique();

				e.HasData(
					new FolioOwner { FolioOwnerId = 1, FirstName = "AAA", LastName = "Delete me", DateOfBirth = new DateTime(2002, 5, 15), Signature = "DK" },
					new FolioOwner { FolioOwnerId = 2, FirstName = "BBB", LastName = "Delete me", DateOfBirth = new DateTime(2010, 12, 1), Signature = "RS" }
				);
			});

			// -------------------- FolioHolder --------------------
			modelBuilder.Entity<FolioHolder>(e =>
			{
				e.ToTable("TFolioHolder");
				e.HasKey(x => x.FolioHolderId);

				e.Property(x => x.FirstName).IsRequired().UseCollation("NOCASE").HasMaxLength(50);
				e.Property(x => x.LastName).IsRequired().UseCollation("NOCASE").HasMaxLength(50);
				e.Property(x => x.Signature).IsRequired().UseCollation("NOCASE").HasMaxLength(5);

				e.Property(x => x.InDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
				e.Property(x => x.UpdateDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();

				// Uniqueness: per your rule (names NOCASE via column collation)
				e.HasIndex(x => x.Signature).IsUnique();
				e.HasIndex(x => new { x.FirstName, x.LastName, x.DateOfBirth }).IsUnique();

				e.HasMany(h => h.Folios).WithOne(f => f.Holder).HasForeignKey(f => f.FolioHolderId).OnDelete(DeleteBehavior.Restrict);

				e.HasData(
					new FolioHolder { FolioHolderId = 1, FirstName = "Rupam", LastName = "Shaw", DateOfBirth = new DateTime(1977, 1, 1), Signature = "RS" },
					new FolioHolder { FolioHolderId = 2, FirstName = "Deepak", LastName = "Shaw", DateOfBirth = new DateTime(1973, 12, 31), Signature = "DK" },
					new FolioHolder { FolioHolderId = 3, FirstName = "Jagruti", LastName = "Shaw", DateOfBirth = new DateTime(2002, 1, 1), Signature = "JS" },
					new FolioHolder { FolioHolderId = 4, FirstName = "Divyam", LastName = "Shaw", DateOfBirth = new DateTime(2010, 1, 10), Signature = "DIVS" },
					new FolioHolder { FolioHolderId = 5, FirstName = "DP", LastName = "Shaw", DateOfBirth = new DateTime(1951, 1, 1), Signature = "PAA" },
					new FolioHolder { FolioHolderId = 6, FirstName = "Radha", LastName = "Devi", DateOfBirth = new DateTime(1961, 12, 31), Signature = "MAA" }
					);
			});

			// -------------------- Folio --------------------
			modelBuilder.Entity<Folio>(e =>
			{
				//e.ToTable("TFolio");
				e.HasKey(x => x.FolioId);

				e.Property(x => x.FolioNumber).IsRequired().UseCollation("NOCASE").HasMaxLength(50);
				e.Property(x => x.FolioPurpose).UseCollation("NOCASE").HasMaxLength(100);
				e.Property(x => x.AttachedBank).UseCollation("NOCASE").HasMaxLength(50);
				e.Property(x => x.IsActive).HasDefaultValue(true);

				e.Property(x => x.InDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
				e.Property(x => x.UpdateDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();

				// A folio is unique per AMC + number (investors can have same folio number across AMCs)
				e.HasIndex(x => new { x.AMCId, x.FolioNumber }).IsUnique();

				e.HasIndex(x => x.FolioHolderId);
				e.HasIndex(x => x.AMCId);

				e.ToTable("TFolio", t =>
				{
					t.HasCheckConstraint("CK_Folio_Number_Len", "length(FolioNumber) BETWEEN 5 AND 50");
					t.HasCheckConstraint("CK_Folio_Purpose_Len", "FolioPurpose IS NULL OR length(FolioPurpose) <= 100");
					t.HasCheckConstraint("CK_Folio_Bank_Len", "AttachedBank IS NULL OR length(AttachedBank) <= 50");
				});


				e.HasData(
					new Folio
					{
						FolioId = 1,
						FolioNumber = "37959966", // JAG HDFC Small cap and Gold ETF
						AMCId = 9,
						FolioHolderId = 3,
						FolioPurpose = "Long term Portfolio",
						AttachedBank = "JAG IDFC NRO",
					},
					new Folio
					{
						FolioId = 2,
						FolioNumber = "499183354147", // Nippon India Small Cap Fund - Direct Plan - Growth (DK)
						AMCId = 6,
						FolioHolderId = 2,
						FolioPurpose = "Small can investment - experiment",
						AttachedBank = "DK HDFC NRO",
					}
				);
			});

			// -------------------- MutualFundTransaction --------------------
			modelBuilder.Entity<MutualFundTransaction>(e =>
			{
				e.ToTable("TMutualFundTransaction");
				e.HasKey(x => x.TransactionId); // Key


				// Core Properties
				e.Property(x => x.TransactionDate).IsRequired();

				e.Property(x => x.TxnType).IsRequired().HasConversion<int>();
				// Stored as INT (enum) for performance & safety

				// --------------------
				// Financial Precision (VERY IMPORTANT)
				// --------------------
				e.Property(x => x.Units)
					.IsRequired()
					.HasPrecision(18, 6);   // MF units need high precision

				e.Property(x => x.NAV)
					.IsRequired()
					.HasPrecision(18, 4);   // NAV accuracy (₹ paise + decimals)

				e.Property(x => x.AmountPaid)
					.IsRequired()
					.HasPrecision(18, 2);   // Money = 2 decimal places

				// Optional Metadata
				e.Property(x => x.ReferenceNo).HasMaxLength(50);
				e.Property(x => x.Source).HasMaxLength(50);
				e.Property(x => x.Note).HasMaxLength(100);

				// --------------------
				// Audit Fields
				// --------------------
				e.Property(x => x.InDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
				e.Property(x => x.UpdateDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();

				// --------------------
				// Relationships
				// --------------------
				e.HasOne(x => x.Folio)
					.WithMany()    // Later you may expose ICollection<MutualFundTransaction>
					.HasForeignKey(x => x.FolioId)
					.OnDelete(DeleteBehavior.Restrict);

				e.HasOne(x => x.Fund)
					.WithMany()    // Same here; keeps entity clean
					.HasForeignKey(x => x.FundId)
					.OnDelete(DeleteBehavior.Restrict);

				// --------------------
				// Indexes
				// --------------------
				e.HasIndex(x => new { x.FolioId, x.TransactionDate });
				e.HasIndex(x => x.FundId);
				e.HasIndex(x => x.TxnType);

				// --------------------
				// Safety Check Constraints (SQLite)
				// --------------------
				e.HasCheckConstraint("CK_MFTransaction_Units_Positive", "Units >= 0.0");
				e.HasCheckConstraint("CK_MFTransaction_NAV_Positive", "NAV >= 0.0");
				e.HasCheckConstraint("CK_MFTransaction_AmountPaid_Positive", "AmountPaid >= 0.0");

				// -------------------- Seed Data --------------------
				e.HasData(
					// BUY transaction
					new MutualFundTransaction
					{
						TransactionId = 1,
						TransactionDate = new DateTime(2023, 01, 10),
						FolioId = 1,
						FundId = 1,
						TxnType = TransactionType.BUY,
						Units = 100.500000m,
						NAV = 42.3500m,
						AmountPaid = 4256.18m, // 100.5 * 42.35 = 4256.175 → rounded
						ReferenceNo = "TXN1001",
						Source = "Initial Purchase",
						Note = "Lumpsum investment",
						InDate = new DateTime(2023, 01, 10),
						UpdateDate = new DateTime(2023, 01, 10)
					},

					// SIP / BUY transaction (same folio, same fund)
					new MutualFundTransaction
					{
						TransactionId = 2,
						TransactionDate = new DateTime(2023, 02, 10),
						FolioId = 1,
						FundId = 1,
						TxnType = TransactionType.BUY,
						Units = 50.000000m,
						NAV = 44.2000m,
						AmountPaid = 2210.00m,
						ReferenceNo = "TXN1002",
						Source = "SIP",
						Note = "Monthly SIP",
						InDate = new DateTime(2023, 02, 10),
						UpdateDate = new DateTime(2023, 02, 10)
					},

					// BUY into another fund under same folio (important test case)
					new MutualFundTransaction
					{
						TransactionId = 3,
						TransactionDate = new DateTime(2023, 03, 05),
						FolioId = 1,
						FundId = 2,
						TxnType = TransactionType.BUY,
						Units = 75.250000m,
						NAV = 65.1000m,
						AmountPaid = 4899.78m,
						ReferenceNo = "TXN1003",
						Source = "Lumpsum",
						Note = "Diversification",
						InDate = new DateTime(2023, 03, 05),
						UpdateDate = new DateTime(2023, 03, 05)
					}
				);
			});
		}
	}
}