using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data.Models;

namespace OM.MFPTrackerV1.Data
{
	public class MFPTrackerDbContext : DbContext
	{
		public MFPTrackerDbContext(DbContextOptions<MFPTrackerDbContext> options) : base(options) { }
		public DbSet<FolioHolder> FolioHolders => Set<FolioHolder>();
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
		}
	}
}