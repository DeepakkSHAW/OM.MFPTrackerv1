//using Microsoft.EntityFrameworkCore;
//using OM.MFPTrackerV1.Data.Models;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace OM.MFPTrackerV1.Data.Services
//{
//	public interface IMFCategoryRepo
//	{
//		Task<(IReadOnlyList<MFCategory> Items, int TotalCount)> GetAsync(string? nameContains = null, int skip = 0, int take = 20, string sortBy = "CategoryName",   // only column we have in the grid
//			bool desc = false, CancellationToken ct = default);

//		Task<MFCategory?> GetByIdAsync(int id, CancellationToken ct = default);

//		Task<int> AddAsync(MFCategory entity, CancellationToken ct = default);

//		Task UpdateAsync(MFCategory entity, CancellationToken ct = default);

//		Task DeleteAsync(int id, CancellationToken ct = default);

//		//ONLY for UI to display counts (without DTOs)
//		Task<Dictionary<int, int>> GetFundCountsAsync(CancellationToken ct = default);
//	}
//	public class MFCategoryRepo : IMFCategoryRepo
//	{
//		private readonly MFPTrackerDbContext _db;
//		public MFCategoryRepo(MFPTrackerDbContext db) => _db = db;
//		public async Task<(IReadOnlyList<MFCategory> Items, int TotalCount)> GetAsync(string? nameContains = null, int skip = 0, int take = 20, string sortBy = "CategoryName", bool desc = false, CancellationToken ct = default)
//		{
//			var q = _db.Set<MFCategory>().AsNoTracking();

//			if (!string.IsNullOrWhiteSpace(nameContains))
//			{
//				var pattern = $"%{nameContains}%";
//				q = q.Where(c => c.CategoryName != null &&
//								 EF.Functions.Like(EF.Functions.Collate(c.CategoryName, "NOCASE"), pattern));
//			}

//			var total = await q.CountAsync(ct);

//			// Only one sortable column here, but keep the switch for future columns.
//			q = (sortBy, desc) switch
//			{
//				("CategoryName", true) => q.OrderByDescending(c => EF.Functions.Collate(c.CategoryName!, "NOCASE"))
//											.ThenBy(c => c.MFCatId),
//				_ => q.OrderBy(c => EF.Functions.Collate(c.CategoryName!, "NOCASE"))
//											.ThenBy(c => c.MFCatId),
//			};

//			var items = await q.Skip(Math.Max(0, skip))
//							   .Take(Math.Max(1, take))
//							   .ToListAsync(ct);

//			return (items, total);
//		}

//		public Task<MFCategory?> GetByIdAsync(int id, CancellationToken ct = default)
//			=> _db.Set<MFCategory>().AsNoTracking().FirstOrDefaultAsync(x => x.MFCatId == id, ct);

//		public async Task<int> AddAsync(MFCategory entity, CancellationToken ct = default)
//		{
//			Validate(entity);

//			// Friendly duplicate check (DB unique index still the source of truth)
//			var dup = await _db.Set<MFCategory>().AsNoTracking()
//				.AnyAsync(x => EF.Functions.Collate(x.CategoryName, "NOCASE") == entity.CategoryName, ct);
//			if (dup)
//				throw new InvalidOperationException("Category already exists. Please choose a unique name.");

//			_db.Set<MFCategory>().Add(entity);
//			await _db.SaveChangesAsync(ct);
//			return entity.MFCatId;
//		}

//		public async Task UpdateAsync(MFCategory entity, CancellationToken ct = default)
//		{
//			var existing = await _db.Set<MFCategory>().FirstOrDefaultAsync(x => x.MFCatId == entity.MFCatId, ct);
//			if (existing is null) return;

//			Validate(entity);

//			var dup = await _db.Set<MFCategory>().AsNoTracking()
//				.AnyAsync(x => x.MFCatId != entity.MFCatId &&
//							   EF.Functions.Collate(x.CategoryName, "NOCASE") == entity.CategoryName, ct);
//			if (dup)
//				throw new InvalidOperationException("Category already exists. Please choose a unique name.");

//			existing.CategoryName = entity.CategoryName.Trim();

//			await _db.SaveChangesAsync(ct);
//		}

//		public async Task DeleteAsyncv0(int id, CancellationToken ct = default)
//		{
//			var row = await _db.Set<MFCategory>().FindAsync(new object?[] { id }, ct);
//			if (row is null) return;

//			_db.Set<MFCategory>().Remove(row);
//			await _db.SaveChangesAsync(ct);
//		}
//		public async Task DeleteAsync(int id, CancellationToken ct = default)
//		{
//			// prevent deleting categories with funds (and provide a friendly message)
//			var hasFunds = await _db.Set<Fund>().AsNoTracking().AnyAsync(f => f.MFCatId == id, ct);
//			if (hasFunds)
//				throw new InvalidOperationException("Cannot delete this category because it has one or more linked funds.");

//			var row = await _db.Set<MFCategory>().FindAsync(new object?[] { id }, ct);
//			if (row is null) return;

//			_db.Set<MFCategory>().Remove(row);
//			await _db.SaveChangesAsync(ct);
//		}
//		public async Task<Dictionary<int, int>> GetFundCountsAsync(CancellationToken ct = default)
//		{
//			return await _db.Set<Fund>()
//				.AsNoTracking()
//				.GroupBy(f => f.MFCatId)
//				.Select(g => new { g.Key, Cnt = g.Count() })
//				.ToDictionaryAsync(x => x.Key, x => x.Cnt, ct);
//		}
//		private static void Validate(MFCategory e)
//		{
//			if (string.IsNullOrWhiteSpace(e.CategoryName))
//				throw new ArgumentException("Category Name is required.", nameof(e.CategoryName));
//			e.CategoryName = e.CategoryName.Trim();
//		}

//	}
//}

//namespace OM.MFPTrackerV1.Data.Services
//{
//	public interface IMFCategoryRepo
//	{

using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data.Models;

namespace OM.MFPTrackerV1.Data.Services
{
	public interface IMFCategoryRepo
	{
		Task<(IReadOnlyList<MFCategory> Items, int TotalCount)> GetAsync(
			string? nameContains = null,
			int skip = 0,
			int take = 25,
			string sortBy = "CategoryName",
			bool desc = false,
			CancellationToken ct = default);
		Task<IReadOnlyList<MFCategory>> GetAllAsync();
		Task<MFCategory?> GetByIdAsync(int id, CancellationToken ct = default);
		Task<int> AddAsync(MFCategory entity, CancellationToken ct = default);
		Task UpdateAsync(MFCategory entity, CancellationToken ct = default);
		Task DeleteAsync(int id, CancellationToken ct = default);
		Task<Dictionary<int, int>> GetFundCountsAsync(CancellationToken ct = default);
		Task<Dictionary<int, decimal>> GetTotalInvestmentByCategoryAsync(CancellationToken ct = default);
	}

	public class MFCategoryRepo : IMFCategoryRepo
	{
		private readonly MFPTrackerDbContext _db;
		public MFCategoryRepo(MFPTrackerDbContext db) => _db = db;

		public async Task<(IReadOnlyList<MFCategory> Items, int TotalCount)> GetAsync(
			string? nameContains = null,
			int skip = 0,
			int take = 25,
			string sortBy = "CategoryName",
			bool desc = false,
			CancellationToken ct = default)
		{
			var q = _db.Set<MFCategory>().AsNoTracking();

			if (!string.IsNullOrWhiteSpace(nameContains))
			{
				var pattern = $"%{nameContains}%";
				q = q.Where(x => EF.Functions.Like(EF.Functions.Collate(x.CategoryName, "NOCASE"), pattern));
			}

			var total = await q.CountAsync(ct);

			q = (sortBy, desc) switch
			{
				("CategoryName", true) => q.OrderByDescending(x => EF.Functions.Collate(x.CategoryName, "NOCASE")).ThenBy(x => x.MFCatId),
				_ => q.OrderBy(x => EF.Functions.Collate(x.CategoryName, "NOCASE")).ThenBy(x => x.MFCatId),
			};

			var items = await q.Skip(Math.Max(0, skip)).Take(Math.Max(1, take)).ToListAsync(ct);
			return (items, total);
		}

		public Task<MFCategory?> GetByIdAsync(int id, CancellationToken ct = default) =>
			_db.Set<MFCategory>().AsNoTracking().FirstOrDefaultAsync(x => x.MFCatId == id, ct);

		public async Task<IReadOnlyList<MFCategory>> GetAllAsync()
		{
			return await _db.Set<MFCategory>()
				.AsNoTracking()
				.OrderBy(x => x.CategoryName)
				.ToListAsync();
		}

		public async Task<int> AddAsync(MFCategory entity, CancellationToken ct = default)
		{
			entity.CategoryName = (entity.CategoryName ?? "").Trim();
			if (string.IsNullOrWhiteSpace(entity.CategoryName))
				throw new ArgumentException("Category Name is required.", nameof(entity.CategoryName));

			var dup = await _db.Set<MFCategory>().AsNoTracking()
				.AnyAsync(x => EF.Functions.Collate(x.CategoryName, "NOCASE") == entity.CategoryName, ct);
			if (dup) throw new InvalidOperationException("Category already exists.");

			_db.Set<MFCategory>().Add(entity);
			await _db.SaveChangesAsync(ct);
			return entity.MFCatId;
		}

		public async Task UpdateAsync(MFCategory entity, CancellationToken ct = default)
		{
			var existing = await _db.Set<MFCategory>().FirstOrDefaultAsync(x => x.MFCatId == entity.MFCatId, ct);
			if (existing is null) return;

			var name = (entity.CategoryName ?? "").Trim();
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Category Name is required.", nameof(entity.CategoryName));

			var dup = await _db.Set<MFCategory>().AsNoTracking()
				.AnyAsync(x => x.MFCatId != entity.MFCatId && EF.Functions.Collate(x.CategoryName, "NOCASE") == name, ct);
			if (dup) throw new InvalidOperationException("Category already exists.");

			existing.CategoryName = name;
			await _db.SaveChangesAsync(ct);
		}

		public async Task DeleteAsync(int id, CancellationToken ct = default)
		{
			var hasFunds = await _db.Set<Fund>().AsNoTracking().AnyAsync(f => f.MFCatId == id, ct);
			if (hasFunds) throw new InvalidOperationException("Cannot delete category: one or more funds are linked.");

			var row = await _db.Set<MFCategory>().FindAsync(new object?[] { id }, ct);
			if (row is null) return;

			_db.Set<MFCategory>().Remove(row);
			await _db.SaveChangesAsync(ct);
		}

		public async Task<Dictionary<int, int>> GetFundCountsAsync(CancellationToken ct = default)
		{
			return await _db.Set<Fund>().AsNoTracking()
				.GroupBy(f => f.MFCatId)
				.Select(g => new { g.Key, Cnt = g.Count() })
				.ToDictionaryAsync(x => x.Key, x => x.Cnt, ct);
		}
		public async Task<Dictionary<int, decimal>> GetTotalInvestmentByCategoryAsync(
			CancellationToken ct = default)
		{
			var result =
				await (from tx in _db.MutualFundTransactions
					   join fund in _db.Funds
						   on tx.FundId equals fund.FundId
					   join cat in _db.MFCategories
						   on fund.MFCatId equals cat.MFCatId
					   where
						   tx.TxnType == TransactionType.BUY ||
						   tx.TxnType == TransactionType.SIP ||
						   tx.TxnType == TransactionType.SWITCH_IN ||
						   tx.TxnType == TransactionType.DIV_REINVEST
					   group tx by cat.MFCatId into g
					   select new
					   {
						   CategoryId = g.Key,
						   Total = g.Sum(x => x.AmountPaid)
					   })
				.ToDictionaryAsync(
					x => x.CategoryId,
					x => x.Total,
					ct);

			return result;
		}
	}
}