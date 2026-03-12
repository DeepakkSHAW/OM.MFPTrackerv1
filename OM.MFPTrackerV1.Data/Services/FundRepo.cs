using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OM.MFPTrackerV1.Data.Services
{

		public interface IFundRepo
		{
			//Task<(IReadOnlyList<Fund> Items, int TotalCount)> GetAsync(
			//	string? nameContains = null,
			//	string? amcContains = null,
			//	string? codeContains = null,     // SchemeCode contains
			//	int? mfcCatId = null,            // exact category id
			//	bool? onlyTxnAllowed = null,     // true = only IsTransactionAllowed==true
			//	bool? onlyNavAllowed = null,     // true = only IsNavAllowed==true
			//	int skip = 0,
			//	int take = 25,
			//	string sortBy = "FundName",
			//	bool desc = false,
			//	CancellationToken ct = default);
		Task<(IReadOnlyList<Fund> Items, int TotalCount)> GetAsync(
				string? nameContains = null,
				string? amcContains = null,
				string? codeContains = null,     // SchemeCode contains
				int? mfcCatId = null,            // exact category id
				bool? onlyTxnAllowed = null,     // true => only IsTransactionAllowed == true
				bool? onlyNavAllowed = null,     // true => only IsNavAllowed == true
				int skip = 0,
				int take = 25,
				string sortBy = "FundName",
				bool desc = false,
				string? isinContains = null,     // NEW (optional)
				CancellationToken ct = default);

		Task<Fund?> GetByIdAsync(int id, CancellationToken ct = default);

			Task<int> AddAsync(Fund entity, CancellationToken ct = default);

			Task UpdateAsync(Fund entity, CancellationToken ct = default);

			Task DeleteAsync(int id, CancellationToken ct = default);
	}
	
		public class FundRepo : IFundRepo
		{
			private readonly MFPTrackerDbContext _db;
			public FundRepo(MFPTrackerDbContext db) => _db = db;

			//public async Task<(IReadOnlyList<Fund> Items, int TotalCount)> GetAsync(
			//	string? nameContains = null,
			//	string? amcContains = null,
			//	string? codeContains = null,
			//	int? mfcCatId = null,
			//	bool? onlyTxnAllowed = null,
			//	bool? onlyNavAllowed = null,
			//	int skip = 0,
			//	int take = 25,
			//	string sortBy = "FundName",
			//	bool desc = false,
			//	CancellationToken ct = default)
			//{
			//	var q = _db.Set<Fund>()
			//			   .AsNoTracking()
			//			   .Include(f => f.Category)
			//			   .AsQueryable();

			//	// ---- Case-insensitive filters (SQLite NOCASE) ----
			//	if (!string.IsNullOrWhiteSpace(nameContains))
			//	{
			//		var pattern = $"%{nameContains}%";
			//		q = q.Where(f => f.FundName != null &&
			//						 EF.Functions.Like(EF.Functions.Collate(f.FundName, "NOCASE"), pattern));
			//	}
			//	if (!string.IsNullOrWhiteSpace(amcContains))
			//	{
			//		var pattern = $"%{amcContains}%";
			//		q = q.Where(f => f.AMCName != null &&
			//						 EF.Functions.Like(EF.Functions.Collate(f.AMCName, "NOCASE"), pattern));
			//	}
			//	if (!string.IsNullOrWhiteSpace(codeContains))
			//	{
			//		var pattern = $"%{codeContains}%";
			//		q = q.Where(f => f.SchemeCode != null &&
			//						 EF.Functions.Like(EF.Functions.Collate(f.SchemeCode, "NOCASE"), pattern));
			//	}
			//	if (mfcCatId is int catId && catId > 0)
			//	{
			//		q = q.Where(f => f.MFCatId == catId);
			//	}
			//	if (onlyTxnAllowed is true) q = q.Where(f => f.IsTransactionAllowed);
			//	if (onlyNavAllowed is true) q = q.Where(f => f.IsNavAllowed);

			//	var total = await q.CountAsync(ct);

			//	// ---- Sorting (typed + stable) ----
			//	q = (sortBy, desc) switch
			//	{
			//		("FundName", true) => q.OrderByDescending(f => EF.Functions.Collate(f.FundName!, "NOCASE")).ThenBy(f => f.FundId),
			//		("FundName", false) => q.OrderBy(f => EF.Functions.Collate(f.FundName!, "NOCASE")).ThenBy(f => f.FundId),

			//		("AMCName", true) => q.OrderByDescending(f => EF.Functions.Collate(f.AMCName!, "NOCASE")).ThenBy(f => f.FundId),
			//		("AMCName", false) => q.OrderBy(f => EF.Functions.Collate(f.AMCName!, "NOCASE")).ThenBy(f => f.FundId),

			//		("SchemeCode", true) => q.OrderByDescending(f => EF.Functions.Collate(f.SchemeCode!, "NOCASE")).ThenBy(f => f.FundId),
			//		("SchemeCode", false) => q.OrderBy(f => EF.Functions.Collate(f.SchemeCode!, "NOCASE")).ThenBy(f => f.FundId),

			//		("ISIN", true) => q.OrderByDescending(f => EF.Functions.Collate(f.ISIN!, "NOCASE")).ThenBy(f => f.FundId),
			//		("ISIN", false) => q.OrderBy(f => EF.Functions.Collate(f.ISIN!, "NOCASE")).ThenBy(f => f.FundId),

			//		("Category", true) => q.OrderByDescending(f => EF.Functions.Collate(f.Category!.CategoryName!, "NOCASE")).ThenBy(f => f.FundId),
			//		("Category", false) => q.OrderBy(f => EF.Functions.Collate(f.Category!.CategoryName!, "NOCASE")).ThenBy(f => f.FundId),

			//		// default by FundName asc
			//		_ => q.OrderBy(f => EF.Functions.Collate(f.FundName!, "NOCASE")).ThenBy(f => f.FundId),
			//	};

			//	var items = await q.Skip(Math.Max(0, skip))
			//					   .Take(Math.Max(1, take))
			//					   .ToListAsync(ct);

			//	return (items, total);
			//}

		public async Task<(IReadOnlyList<Fund> Items, int TotalCount)> GetAsync(
			string? nameContains = null,
			string? amcContains = null,
			string? codeContains = null,
			int? mfcCatId = null,
			bool? onlyTxnAllowed = null,
			bool? onlyNavAllowed = null,
			int skip = 0,
			int take = 25,
			string sortBy = "FundName",
			bool desc = false,
			string? isinContains = null,
			CancellationToken ct = default)
		{
			var q = _db.Set<Fund>()
					   .AsNoTracking()
					   .Include(f => f.Category)
					   .AsQueryable();

			// ------------- Case-insensitive filters (SQLite NOCASE) -------------
			if (!string.IsNullOrWhiteSpace(nameContains))
			{
				var pattern = $"%{nameContains}%";
				q = q.Where(f => f.FundName != null &&
								 EF.Functions.Like(EF.Functions.Collate(f.FundName, "NOCASE"), pattern));
			}

			if (!string.IsNullOrWhiteSpace(amcContains))
			{
				var pattern = $"%{amcContains}%";
				q = q.Where(f => f.AMCName != null &&
								 EF.Functions.Like(EF.Functions.Collate(f.AMCName, "NOCASE"), pattern));
			}

			if (!string.IsNullOrWhiteSpace(codeContains))
			{
				var pattern = $"%{codeContains}%";
				q = q.Where(f => f.SchemeCode != null &&
								 EF.Functions.Like(EF.Functions.Collate(f.SchemeCode, "NOCASE"), pattern));
			}

			if (!string.IsNullOrWhiteSpace(isinContains))
			{
				var pattern = $"%{isinContains}%";
				q = q.Where(f => f.ISIN != null &&
								 EF.Functions.Like(EF.Functions.Collate(f.ISIN, "NOCASE"), pattern));
			}

			if (mfcCatId is int catId && catId > 0)
				q = q.Where(f => f.MFCatId == catId);

			if (onlyTxnAllowed is true)
				q = q.Where(f => f.IsTransactionAllowed);

			if (onlyNavAllowed is true)
				q = q.Where(f => f.IsNavAllowed);

			var total = await q.CountAsync(ct);

			// ------------------- Typed, stable sorting (NOCASE) -------------------
			q = (sortBy, desc) switch
			{
				("FundName", true) => q.OrderByDescending(f => EF.Functions.Collate(f.FundName!, "NOCASE")).ThenBy(f => f.FundId),
				("FundName", false) => q.OrderBy(f => EF.Functions.Collate(f.FundName!, "NOCASE")).ThenBy(f => f.FundId),

				("AMCName", true) => q.OrderByDescending(f => EF.Functions.Collate(f.AMCName!, "NOCASE")).ThenBy(f => f.FundId),
				("AMCName", false) => q.OrderBy(f => EF.Functions.Collate(f.AMCName!, "NOCASE")).ThenBy(f => f.FundId),

				("SchemeCode", true) => q.OrderByDescending(f => EF.Functions.Collate(f.SchemeCode!, "NOCASE")).ThenBy(f => f.FundId),
				("SchemeCode", false) => q.OrderBy(f => EF.Functions.Collate(f.SchemeCode!, "NOCASE")).ThenBy(f => f.FundId),

				("ISIN", true) => q.OrderByDescending(f => EF.Functions.Collate(f.ISIN!, "NOCASE")).ThenBy(f => f.FundId),
				("ISIN", false) => q.OrderBy(f => EF.Functions.Collate(f.ISIN!, "NOCASE")).ThenBy(f => f.FundId),

				// Category sort by Category.CategoryName (case-insensitive)
				("Category", true) => q.OrderByDescending(f => EF.Functions.Collate(f.Category!.CategoryName!, "NOCASE")).ThenBy(f => f.FundId),
				("Category", false) => q.OrderBy(f => EF.Functions.Collate(f.Category!.CategoryName!, "NOCASE")).ThenBy(f => f.FundId),

				// Optional: sort by audit timestamps (if you expose them in UI)
				("InDate", true) => q.OrderByDescending(f => f.InDate).ThenBy(f => f.FundId),
				("InDate", false) => q.OrderBy(f => f.InDate).ThenBy(f => f.FundId),
				("UpdateDate", true) => q.OrderByDescending(f => f.UpdateDate).ThenBy(f => f.FundId),
				("UpdateDate", false) => q.OrderBy(f => f.UpdateDate).ThenBy(f => f.FundId),

				// Default: FundName ASC
				_ => q.OrderBy(f => EF.Functions.Collate(f.FundName!, "NOCASE")).ThenBy(f => f.FundId),
			};

			// -------------------------- Paging --------------------------
			var items = await q.Skip(Math.Max(0, skip))
							   .Take(Math.Max(1, take))
							   .ToListAsync(ct);

			return (items, total);
		}
		
		public Task<Fund?> GetByIdAsync(int id, CancellationToken ct = default)
				=> _db.Set<Fund>().AsNoTracking().Include(f => f.Category).FirstOrDefaultAsync(f => f.FundId == id, ct);

			public async Task<int> AddAsync(Fund entity, CancellationToken ct = default)
			{
				Normalize(entity);
				ValidateRequired(entity);

				// ---- Uniqueness (NOCASE) ----
				await EnsureUniqueAsync(entity, isUpdate: false, ct);

				// Audit
				var now = DateTime.UtcNow;
				entity.InDate = now;
				entity.UpdateDate = now;

				_db.Set<Fund>().Add(entity);
				await _db.SaveChangesAsync(ct);
				return entity.FundId;
			}

			public async Task UpdateAsync(Fund entity, CancellationToken ct = default)
			{
				var existing = await _db.Set<Fund>().FirstOrDefaultAsync(f => f.FundId == entity.FundId, ct);
				if (existing is null) return;

				Normalize(entity);
				ValidateRequired(entity);
				await EnsureUniqueAsync(entity, isUpdate: true, ct);

				// Map
				existing.FundName = entity.FundName;
				existing.SchemeCode = entity.SchemeCode;
				existing.ISIN = entity.ISIN;
				existing.AMCName = entity.AMCName;
				existing.IsTransactionAllowed = entity.IsTransactionAllowed;
				existing.IsNavAllowed = entity.IsNavAllowed;
				existing.MFCatId = entity.MFCatId;

				// Audit
				existing.UpdateDate = DateTime.UtcNow;

				await _db.SaveChangesAsync(ct);
			}

			public async Task DeleteAsync(int id, CancellationToken ct = default)
			{
				var row = await _db.Set<Fund>().FindAsync(new object?[] { id }, ct);
				if (row is null) return;
				_db.Set<Fund>().Remove(row);
				await _db.SaveChangesAsync(ct);
			}

			// ---- Helpers ----
			private static void Normalize(Fund e)
			{
				e.FundName = e.FundName?.Trim() ?? string.Empty;
				e.SchemeCode = e.SchemeCode?.Trim() ?? string.Empty;
				e.ISIN = e.ISIN?.Trim() ?? string.Empty;
				e.AMCName = e.AMCName?.Trim() ?? string.Empty;
			}

			private static void ValidateRequired(Fund e)
			{
				if (string.IsNullOrWhiteSpace(e.FundName)) throw new ArgumentException("Fund Name is required.", nameof(e.FundName));
				if (string.IsNullOrWhiteSpace(e.SchemeCode)) throw new ArgumentException("Scheme Code is required.", nameof(e.SchemeCode));
				if (string.IsNullOrWhiteSpace(e.ISIN)) throw new ArgumentException("ISIN is required.", nameof(e.ISIN));
				if (string.IsNullOrWhiteSpace(e.AMCName)) throw new ArgumentException("AMC Name is required.", nameof(e.AMCName));
				if (e.MFCatId <= 0) throw new ArgumentException("Category is required.", nameof(e.MFCatId));
			}

			private async Task EnsureUniqueAsync(Fund e, bool isUpdate, CancellationToken ct)
			{
				// Exclude self on update
				var q = _db.Set<Fund>().AsNoTracking().Where(x => !isUpdate || x.FundId != e.FundId);

				var dupName = await q.AnyAsync(x => EF.Functions.Collate(x.FundName, "NOCASE") == e.FundName, ct);
				if (dupName) throw new InvalidOperationException("Fund Name already exists. Please choose a unique name.");

				var dupScheme = await q.AnyAsync(x => EF.Functions.Collate(x.SchemeCode, "NOCASE") == e.SchemeCode, ct);
				if (dupScheme) throw new InvalidOperationException("Scheme Code already exists. Please choose a unique code.");

				var dupIsin = await q.AnyAsync(x => EF.Functions.Collate(x.ISIN, "NOCASE") == e.ISIN, ct);
				if (dupIsin) throw new InvalidOperationException("ISIN already exists. Please choose a unique code.");
			}
		}
	
}
