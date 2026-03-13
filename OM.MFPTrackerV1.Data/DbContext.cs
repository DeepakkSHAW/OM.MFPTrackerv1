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
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			//modelBuilder.Entity<FolioHolder>(entity =>
			//{
			//	entity.ToTable("TFolioHolder");
			//	entity.HasKey(p => p.FolioHolderId);                 // Set key for entity

			//	entity.Property(p => p.FirstName).IsRequired().UseCollation("NOCASE").HasMaxLength(100);
			//	entity.Property(p => p.LastName).IsRequired().UseCollation("NOCASE").HasMaxLength(100);
			//	entity.Property(p => p.Signature).IsRequired().UseCollation("NOCASE").HasMaxLength(5);
			//	entity.Property(p => p.InDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
			//	entity.Property(p => p.UpdateDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();
			//	// Recommended indexes & Unique constraints
			//	entity.HasIndex(e => e.FolioHolderId);
			//	entity.HasIndex(e => e.FirstName).IsUnique();
			//	entity.HasIndex(x => x.Signature).IsUnique();
			//	entity.HasIndex(x => new { x.FirstName, x.LastName, x.DateOfBirth }).IsUnique(); // Same person unique: First+Last+DOB (names case-insensitive via column collation)

			//	// Relationship with AMC
			//	//entity.HasOne(m => m.Amc).WithMany(a => a.MutualFunds).HasForeignKey(m => m.AmcId).OnDelete(DeleteBehavior.Restrict);
			//	// 1 Holder -> many Folios (Restrict delete to protect Folios)
			//	entity.HasMany(h => h.Folios).WithOne(x => x.Holder).HasForeignKey(x => x.FolioHolderId).OnDelete(DeleteBehavior.Restrict);
			//	// Data seeding
			//	entity.HasData(
			//	new FolioHolder() { FolioHolderId = 1, FirstName = "Rupam", LastName = "Shaw", DateOfBirth = DateTime.Parse("1/1/2002"), Signature = "RS" },
			//	new FolioHolder() { FolioHolderId = 2, FirstName = "Deepak", LastName = "Shaw", DateOfBirth = DateTime.Parse("10/12/1981"), Signature = "DK" },
			//	new FolioHolder() { FolioHolderId = 3, FirstName = "Jagruti", LastName = "Shaw", DateOfBirth = DateTime.Parse("21/04/1974"), Signature = "JS" },
			//	new FolioHolder() { FolioHolderId = 4, FirstName = "Divyam", LastName = "Shaw", DateOfBirth = DateTime.Parse("11/11/2001"), Signature = "DS" },
			//	new FolioHolder() { FolioHolderId = 5, FirstName = "Durga Prasad", LastName = "Shaw", Signature = "DP" },
			//	new FolioHolder() { FolioHolderId = 6, FirstName = "Radha", LastName = "Shaw", Signature = "RD" }
			//	);
			//});
			//modelBuilder.Entity<MFCategory>(entity =>
			//{
			//	entity.ToTable("TMFCategory");
			//	entity.HasKey(p => p.MFCatId);                 // Set key for entity
			//	entity.Property(p => p.CategoryName).IsRequired().UseCollation("NOCASE").HasMaxLength(50);
			//	// Recommended indexes & Unique constraints
			//	entity.HasIndex(c => c.CategoryName).IsUnique();
			//	// 1 Category -> many Funds (Restrict delete to protect Funds)
			//	entity.HasMany(c => c.Funds).WithOne(f => f.Category).HasForeignKey(f => f.MFCatId).OnDelete(DeleteBehavior.Restrict);
			//	entity.HasData(
			//	   new MFCategory() { MFCatId = 1, CategoryName = "Equity-Multi Cap " },
			//	   new MFCategory() { MFCatId = 2, CategoryName = "Equity-Flexi Cap" },
			//	   new MFCategory() { MFCatId = 3, CategoryName = "Equity-Large & MidCap" },
			//	   new MFCategory() { MFCatId = 4, CategoryName = "Equity-Large Cap" },
			//	   new MFCategory() { MFCatId = 5, CategoryName = "Equity-Mid Cap" },
			//	   new MFCategory() { MFCatId = 6, CategoryName = "Equity-Small Cap" },
			//	   new MFCategory() { MFCatId = 7, CategoryName = "Equity-ELSS" },
			//	   new MFCategory() { MFCatId = 8, CategoryName = "Equity-Dividend Yield" },
			//	   new MFCategory() { MFCatId = 9, CategoryName = "Equity-Contra" },
			//	   new MFCategory() { MFCatId = 10, CategoryName = "Equity-Sectoral" },
			//	   new MFCategory() { MFCatId = 11, CategoryName = "Equity-Value Oriented" },
			//	   new MFCategory() { MFCatId = 12, CategoryName = "Debt-Liquid Fund" },
			//	   new MFCategory() { MFCatId = 13, CategoryName = "Debt-Overnight Funds" },
			//	   new MFCategory() { MFCatId = 14, CategoryName = "Debt-Money Market Funds" },
			//	   new MFCategory() { MFCatId = 15, CategoryName = "Debt-Corporate Bond Funds" },
			//	   new MFCategory() { MFCatId = 16, CategoryName = "Debt-Gilt Funds" },
			//	   new MFCategory() { MFCatId = 17, CategoryName = "Hybrid Fund" }
			//		);
			//});
			//modelBuilder.Entity<Fund>(entity =>
			//{
			//	entity.ToTable("TFund");
			//	entity.HasKey(p => p.FundId);                   // Set key for entity

			//	entity.Property(p => p.FundName).IsRequired().UseCollation("NOCASE").HasMaxLength(100);
			//	entity.Property(p => p.SchemeCode).IsRequired().UseCollation("NOCASE").HasMaxLength(20);
			//	entity.Property(p => p.ISIN).IsRequired().UseCollation("NOCASE").HasMaxLength(20);
			//	entity.Property(p => p.AMCName).IsRequired().UseCollation("NOCASE").HasMaxLength(100);
			//	entity.Property(p => p.IsTransactionAllowed).IsRequired().HasDefaultValue(true);
			//	entity.Property(p => p.IsNavAllowed).IsRequired().HasDefaultValue(true);
			//	entity.Property(p => p.InDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
			//	entity.Property(p => p.UpdateDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();
			//	// Recommended indexes & Unique constraints
			//	entity.HasIndex(c => c.FundName).IsUnique();
			//	entity.HasIndex(c => c.SchemeCode).IsUnique();
			//	entity.HasIndex(c => c.ISIN).IsUnique();

			//	// Helpful indexes optional
			//	entity.HasIndex(c => c.AMCName);
			//	entity.HasIndex(c => c.MFCatId);

			//	// Relationship with Mutual Fund Category 
			//	entity.HasOne(f => f.Category).WithMany().HasForeignKey(f => f.MFCatId).OnDelete(DeleteBehavior.Restrict);

			//	// 1 Fund → many Folios (Restrict deletion)
			//	entity.HasMany(f => f.Folios).WithOne(x => x.Fund).HasForeignKey(x => x.FundId).OnDelete(DeleteBehavior.Restrict);

			//	//Data Seeding
			//	entity.HasData(
			//		new Fund { FundId = 1, ISIN = "INF846K01K35", SchemeCode = "125354", AMCName = "AXIS MF", FundName = "AXIS SMALL CAP Fund - DIRECT PLAN - GROWTH", IsTransactionAllowed = true, IsNavAllowed = true, MFCatId = 1 },
			//		new Fund { FundId = 2, ISIN = "INF194KB1AJ8", SchemeCode = "147944", AMCName = "BANDHAN MF", FundName = "BANDHAN SMALL CAP FUND - REGULAR PLAN GROWTH", IsTransactionAllowed = true, IsNavAllowed = true, MFCatId = 1 }
			//		);
			//});
			//modelBuilder.Entity<Folio>(entity =>
			//{
			//	entity.ToTable("TFolio");
			//	entity.HasKey(x => x.FolioId);

			//	entity.Property(x => x.FolioNumber).IsRequired().UseCollation("NOCASE").HasMaxLength(50);
			//	entity.Property(x => x.FolioPurpose).UseCollation("NOCASE").HasMaxLength(100);
			//	entity.Property(x => x.AttachedBank).UseCollation("NOCASE").HasMaxLength(50);
			//	entity.Property(p => p.IsActive).HasDefaultValue(true);
			//	// Audit
			//	entity.Property(x => x.InDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
			//	entity.Property(x => x.UpdateDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();               // SQLite won’t auto-update on UPDATE without trigger—set in repo

			//	// Relationships (Restrict delete)
			//	// or .WithMany(h => h.Folios) if you add nav on FolioHolder
			//	entity.HasOne(f => f.Holder).WithMany().HasForeignKey(f => f.FolioHolderId).OnDelete(DeleteBehavior.Restrict);
			//	// or .WithMany(f => f.Folios) if you add nav on Fund
			//	entity.HasOne(f => f.Fund).WithMany().HasForeignKey(f => f.FundId).OnDelete(DeleteBehavior.Restrict);

			//	// Case-insensitive unique composite (prevents duplicates)
			//	entity.HasIndex(x => new { x.FolioHolderId, x.FundId, x.FolioNumber }).IsUnique();
			//	//Data Seeding
			//	entity.HasData(
			//		new Folio { FolioId = 1, FolioHolderId = 1, FundId = 1, FolioNumber = "FOLIO123", FolioPurpose = "Investment in Axis Small Cap Fund" },
			//		new Folio { FolioId = 2, FolioHolderId = 2, FundId = 1, FolioNumber = "FOLIO456", FolioPurpose = "Investment in Axis Small Cap Fund" },
			//		new Folio { FolioId = 3, FolioHolderId = 3, FundId = 2, FolioNumber = "FOLIO789", FolioPurpose = "Investment in Bandhan Small Cap Fund" }
			//		);
			//});

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
					new AMC { AMCId = 3, AMCName = "HDFC Mutual Fund" }
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
					new MFCategory { MFCatId = 1, CategoryName = "Equity - Small Cap" },
					new MFCategory { MFCatId = 2, CategoryName = "Equity - Multi Cap" },
					new MFCategory { MFCatId = 3, CategoryName = "Debt - Liquid" },
					new MFCategory { MFCatId = 4, CategoryName = "Hybrid - Aggressive" }
						);
			});

			// -------------------- Fund --------------------
			modelBuilder.Entity<Fund>(e =>
			{
				//e.ToTable("TFund");
				e.HasKey(p => p.FundId);

				e.Property(p => p.FundName).IsRequired().UseCollation("NOCASE").HasMaxLength(100);
				e.Property(p => p.SchemeCode).IsRequired().UseCollation("NOCASE").HasMaxLength(20);
				e.Property(p => p.ISIN).IsRequired().UseCollation("NOCASE").HasMaxLength(20);
				e.Property(p => p.AMCName).IsRequired().UseCollation("NOCASE").HasMaxLength(100);

				e.Property(p => p.IsTransactionAllowed).HasDefaultValue(true);
				e.Property(p => p.IsNavAllowed).HasDefaultValue(true);

				e.Property(p => p.InDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
				e.Property(p => p.UpdateDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();

				e.HasIndex(c => c.FundName).IsUnique();
				e.HasIndex(c => c.SchemeCode).IsUnique();
				e.HasIndex(c => c.ISIN).IsUnique();

				e.HasIndex(c => c.AMCId);
				e.HasIndex(c => c.MFCatId);

				e.ToTable("TFund", t =>
				{
					t.HasCheckConstraint("CK_Fund_FundName_Len", "length(FundName) BETWEEN 5 AND 100");
					t.HasCheckConstraint("CK_Fund_SchemeCode_Len", "length(SchemeCode) BETWEEN 1 AND 20");
					t.HasCheckConstraint("CK_Fund_ISIN_Len", "length(ISIN) BETWEEN 1 AND 20");
					t.HasCheckConstraint("CK_Fund_AMCName_Len", "length(AMCName) BETWEEN 1 AND 100");
				});

				e.HasData(
					new Fund
					{
						FundId = 1,
						FundName = "Axis Small Cap Fund - Direct Plan - Growth",
						SchemeCode = "125354",
						ISIN = "INF846K01K35",
						AMCName = "Axis MF",
						AMCId = 1,
						MFCatId = 1,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					},
					new Fund
					{
						FundId = 2,
						FundName = "Bandhan Small Cap Fund - Regular Plan - Growth",
						SchemeCode = "147944",
						ISIN = "INF194KB1AJ8",
						AMCName = "Bandhan MF",
						AMCId = 2,
						MFCatId = 1,
						IsTransactionAllowed = true,
						IsNavAllowed = true
					}
				);
			});

			// -------------------- FolioHolder --------------------
			modelBuilder.Entity<FolioHolder>(e =>
			{
				//e.ToTable("TFolioHolder");
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

				e.ToTable("TFolioHolder", t =>
				{
					t.HasCheckConstraint("CK_FH_FirstName_Len", "length(FirstName) BETWEEN 3 AND 50");
					t.HasCheckConstraint("CK_FH_LastName_Len", "length(LastName) BETWEEN 3 AND 50");
					t.HasCheckConstraint("CK_FH_Signature_Len", "length(Signature) BETWEEN 2 AND 5");
				});

				e.HasData(
					new FolioHolder { FolioHolderId = 1, FirstName = "Rupam", LastName = "Shaw", DateOfBirth = new DateTime(2002, 1, 1), Signature = "RS" },
					new FolioHolder { FolioHolderId = 2, FirstName = "Deepak", LastName = "Shaw", DateOfBirth = new DateTime(1981, 12, 10), Signature = "DK" }
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
						FolioNumber = "1234567",
						AMCId = 1,
						FolioHolderId = 2,
						FolioPurpose = "Investment Portfolio"
					},
					new Folio
					{
						FolioId = 2,
						FolioNumber = "2233445",
						AMCId = 2,
						FolioHolderId = 1,
						FolioPurpose = "Family Holdings"
					}
				);
			});

			// -------------------- MutualFundTransaction --------------------
			modelBuilder.Entity<MutualFundTransaction>(e =>
			{
				//e.ToTable("TMFTransaction");
				e.HasKey(x => x.Id);

				//e.Property(x => x.TxnType).IsRequired().UseCollation("NOCASE").HasMaxLength(20);
				e.Property(x => x.TxnType).HasConversion<int>().IsRequired();
				e.Property(x => x.Source).UseCollation("NOCASE").HasMaxLength(50);
				e.Property(x => x.Note).UseCollation("NOCASE").HasMaxLength(100);

				e.Property(x => x.InDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
				e.Property(x => x.UpdateDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();

				// Helpful indexes
				e.HasIndex(x => x.Date);
				e.HasIndex(x => x.FolioId);
				e.HasIndex(x => x.FundId);

				e.ToTable("TMFTransaction", t =>
				{
					// Source/Note are optional; enforce reasonable caps
					t.HasCheckConstraint("CK_Txn_Source_Len", "Source IS NULL OR length(Source) <= 50");
					t.HasCheckConstraint("CK_Txn_Note_Len", "Note IS NULL OR length(Note) <= 100");
					// Units/NAV > 0 checks are better enforced in repo/business logic,
					// but you could also add: t.HasCheckConstraint("CK_Txn_Positive", "Units > 0 AND NAV > 0");
				});

				e.HasData(
					new MutualFundTransaction
					{
						Id = 1,
						Date = new DateTime(2024, 10, 1),
						FolioId = 1,
						FundId = 1,
						TxnType = TransactionType.BUY,
						Units = 40,
						NAV = 12,
						AmountPaid = 480,
						Source = "Kotak Bank NRO"
					}
				);
			});
		}
	}
}