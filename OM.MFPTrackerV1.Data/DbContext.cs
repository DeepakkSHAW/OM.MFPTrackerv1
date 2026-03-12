using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data.Models;

namespace OM.MFPTrackerV1.Data
{
	public class MFPTrackerDbContext : DbContext
	{
		public MFPTrackerDbContext(DbContextOptions<MFPTrackerDbContext> options) : base(options) { }
		public DbSet<FolioHolder> FolioHolders => Set<FolioHolder>();
		public DbSet<MFCategory> MFCategories => Set<MFCategory>();
		public DbSet<Fund> Funds => Set<Fund>();
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<FolioHolder>(entity =>
			{
				entity.ToTable("TFolioHolder");
				entity.HasKey(p => p.FolioHolderId);                 // Set key for entity

				entity.Property(p => p.FirstName).IsRequired().UseCollation("NOCASE").HasMaxLength(100);
				entity.Property(p => p.LastName).IsRequired().UseCollation("NOCASE").HasMaxLength(100);
				entity.Property(p => p.Signature).IsRequired().UseCollation("NOCASE").HasMaxLength(5);
				entity.Property(p => p.InDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
				entity.Property(p => p.UpdateDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();
				// Recommended indexes & Unique constraints
				entity.HasIndex(e => e.FolioHolderId);
				entity.HasIndex(e => e.FirstName).IsUnique();
				entity.HasIndex(x => x.Signature).IsUnique();
				entity.HasIndex(x => new { x.FirstName, x.LastName, x.DateOfBirth }).IsUnique(); // Same person unique: First+Last+DOB (names case-insensitive via column collation)

				// Relationship with AMC
				//entity.HasOne(m => m.Amc).WithMany(a => a.MutualFunds).HasForeignKey(m => m.AmcId).OnDelete(DeleteBehavior.Restrict);
				// Data seeding
				entity.HasData(
				new FolioHolder() { FolioHolderId = 1, FirstName = "Rupam", LastName = "Shaw", DateOfBirth = DateTime.Parse("1/1/2002"), Signature = "RS" },
				new FolioHolder() { FolioHolderId = 2, FirstName = "Deepak", LastName = "Shaw", DateOfBirth = DateTime.Parse("10/12/1981"), Signature = "DK" },
				new FolioHolder() { FolioHolderId = 3, FirstName = "Jagruti", LastName = "Shaw", DateOfBirth = DateTime.Parse("21/04/1974"), Signature = "JS" },
				new FolioHolder() { FolioHolderId = 4, FirstName = "Divyam", LastName = "Shaw", DateOfBirth = DateTime.Parse("11/11/2001"), Signature = "DS" },
				new FolioHolder() { FolioHolderId = 5, FirstName = "Durga Prasad", LastName = "Shaw", Signature = "DP" },
				new FolioHolder() { FolioHolderId = 6, FirstName = "Radha", LastName = "Shaw", Signature = "RD" }
				);
			});
			modelBuilder.Entity<MFCategory>(entity =>
			{
				entity.ToTable("TMFCategory");
				entity.HasKey(p => p.MFCatId);                 // Set key for entity
				entity.Property(p => p.CategoryName).IsRequired().UseCollation("NOCASE").HasMaxLength(50);
				// Recommended indexes & Unique constraints
				entity.HasIndex(c => c.CategoryName).IsUnique();
				// (Optional explicit mapping)
				entity.HasMany(c => c.Funds).WithOne(f => f.Category).HasForeignKey(f => f.MFCatId).OnDelete(DeleteBehavior.Restrict);
				entity.HasData(
				   new MFCategory() { MFCatId = 1, CategoryName = "Equity-Multi Cap " },
				   new MFCategory() { MFCatId = 2, CategoryName = "Equity-Flexi Cap" },
				   new MFCategory() { MFCatId = 3, CategoryName = "Equity-Large & MidCap" },
				   new MFCategory() { MFCatId = 4, CategoryName = "Equity-Large Cap" },
				   new MFCategory() { MFCatId = 5, CategoryName = "Equity-Mid Cap" },
				   new MFCategory() { MFCatId = 6, CategoryName = "Equity-Small Cap" },
				   new MFCategory() { MFCatId = 7, CategoryName = "Equity-ELSS" },
				   new MFCategory() { MFCatId = 8, CategoryName = "Equity-Dividend Yield" },
				   new MFCategory() { MFCatId = 9, CategoryName = "Equity-Contra" },
				   new MFCategory() { MFCatId = 10, CategoryName = "Equity-Sectoral" },
				   new MFCategory() { MFCatId = 11, CategoryName = "Equity-Value Oriented" },
				   new MFCategory() { MFCatId = 12, CategoryName = "Debt-Liquid Fund" },
				   new MFCategory() { MFCatId = 13, CategoryName = "Debt-Overnight Funds" },
				   new MFCategory() { MFCatId = 14, CategoryName = "Debt-Money Market Funds" },
				   new MFCategory() { MFCatId = 15, CategoryName = "Debt-Corporate Bond Funds" },
				   new MFCategory() { MFCatId = 16, CategoryName = "Debt-Gilt Funds" },
				   new MFCategory() { MFCatId = 17, CategoryName = "Hybrid Fund" }
					);
			});
			modelBuilder.Entity<Fund>(entity =>
			{
				entity.ToTable("TFund");
				entity.HasKey(p => p.FundId);                   // Set key for entity

				entity.Property(p => p.FundName).IsRequired().UseCollation("NOCASE").HasMaxLength(100);
				entity.Property(p => p.SchemeCode).IsRequired().UseCollation("NOCASE").HasMaxLength(20);
				entity.Property(p => p.ISIN).IsRequired().UseCollation("NOCASE").HasMaxLength(20);
				entity.Property(p => p.AMCName).IsRequired().UseCollation("NOCASE").HasMaxLength(100);
				entity.Property(p => p.IsTransactionAllowed).IsRequired().HasDefaultValue(true);
				entity.Property(p => p.IsNavAllowed).IsRequired().HasDefaultValue(true);
				entity.Property(p => p.InDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
				entity.Property(p => p.UpdateDate).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();
				// Recommended indexes & Unique constraints
				entity.HasIndex(c => c.FundName).IsUnique();
				entity.HasIndex(c => c.SchemeCode).IsUnique();
				entity.HasIndex(c => c.ISIN).IsUnique();

				// Helpful indexes optional
				entity.HasIndex(c => c.AMCName);
				entity.HasIndex(c => c.MFCatId);

				// Relationship with Mutual Fund Category 
				entity.HasOne(f => f.Category).WithMany().HasForeignKey(f => f.MFCatId).OnDelete(DeleteBehavior.Restrict);
				//Data Seeding
				entity.HasData(
					new Fund { FundId = 1, ISIN = "INF846K01K35", SchemeCode = "125354", AMCName = "AXIS MF", FundName = "AXIS SMALL CAP Fund - DIRECT PLAN - GROWTH", IsTransactionAllowed = true, IsNavAllowed = true, MFCatId = 1 },
					new Fund { FundId = 2, ISIN = "INF194KB1AJ8", SchemeCode = "147944", AMCName = "BANDHAN MF", FundName = "BANDHAN SMALL CAP FUND - REGULAR PLAN GROWTH", IsTransactionAllowed = true, IsNavAllowed = true, MFCatId = 1 }
					);
			});
		}
	}
}