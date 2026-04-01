//using Microsoft.EntityFrameworkCore;
//using OM.MFPTrackerV1.Data.Models;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace OM.MFPTrackerV1.Data.Services
//{
//	public interface IFolioRepo
//	{
//		Task<(IReadOnlyList<Folio> Items, int TotalCount)> GetAsync(
//			string? folioContains = null,
//			string? holderNameContains = null,
//			string? fundNameContains = null,
//			bool? onlyActive = null,
//			int? holderId = null,          // optional direct filter by holder
//			int? fundId = null,            // optional direct filter by fund
//			int skip = 0,
//			int take = 25,
//			string sortBy = "FolioNumber", // FolioNumber | Holder | Fund | Bank | Active
//			bool desc = false,
//			CancellationToken ct = default);

//		Task<Folio?> GetByIdAsync(int id, CancellationToken ct = default);
//		Task<int> AddAsync(Folio entity, CancellationToken ct = default);
//		Task UpdateAsync(Folio entity, CancellationToken ct = default);
//		Task DeleteAsync(int id, CancellationToken ct = default);
//	}
//	public class FolioRepo : IFolioRepo
//	{
//		private readonly MFPTrackerDbContext _db;
//		public FolioRepo(MFPTrackerDbContext db) => _db = db;
//		public async Task<(IReadOnlyList<Folio> Items, int TotalCount)> GetAsync(
//			string? folioContains = null,
//			string? holderNameContains = null,
//			string? fundNameContains = null,
//			bool? onlyActive = null,
//			int? holderId = null,
//			int? fundId = null,
//			int skip = 0,
//			int take = 25,
//			string sortBy = "FolioNumber",
//			bool desc = false,
//			CancellationToken ct = default)
//		{
//			var q = _db.Set<Folio>()
//					   .AsNoTracking()
//					   .Include(x => x.Holder)
//					   .Include(x => x.Fund)
//					   .AsQueryable();

//			// Filters (NOCASE)
//			if (!string.IsNullOrWhiteSpace(folioContains))
//			{
//				var pattern = $"%{folioContains}%";
//				q = q.Where(x => x.FolioNumber != null &&
//								 EF.Functions.Like(EF.Functions.Collate(x.FolioNumber, "NOCASE"), pattern));
//			}

//			if (!string.IsNullOrWhiteSpace(holderNameContains))
//			{
//				var pattern = $"%{holderNameContains}%";
//				q = q.Where(x =>
//					x.Holder != null &&
//					(
//						(x.Holder.FirstName != null && EF.Functions.Like(EF.Functions.Collate(x.Holder.FirstName, "NOCASE"), pattern)) ||
//						(x.Holder.LastName != null && EF.Functions.Like(EF.Functions.Collate(x.Holder.LastName, "NOCASE"), pattern))
//					));
//			}

//			if (!string.IsNullOrWhiteSpace(fundNameContains))
//			{
//				var pattern = $"%{fundNameContains}%";
//				q = q.Where(x =>
//					x.Fund != null &&
//					x.Fund.FundName != null &&
//					EF.Functions.Like(EF.Functions.Collate(x.Fund.FundName, "NOCASE"), pattern));
//			}

//			if (onlyActive is true) q = q.Where(x => x.IsActive);
//			if (holderId is int hid && hid > 0) q = q.Where(x => x.FolioHolderId == hid);
//			if (fundId is int fid && fid > 0) q = q.Where(x => x.FundId == fid);

//			var total = await q.CountAsync(ct);

//			// Sorting (typed + stable)
//			q = (sortBy, desc) switch
//			{
//				("FolioNumber", true) => q.OrderByDescending(x => EF.Functions.Collate(x.FolioNumber!, "NOCASE")).ThenBy(x => x.FolioId),
//				("FolioNumber", false) => q.OrderBy(x => EF.Functions.Collate(x.FolioNumber!, "NOCASE")).ThenBy(x => x.FolioId),

//				("Holder", true) => q.OrderByDescending(x => EF.Functions.Collate((x.Holder!.LastName + " " + x.Holder!.FirstName)!, "NOCASE")).ThenBy(x => x.FolioId),
//				("Holder", false) => q.OrderBy(x => EF.Functions.Collate((x.Holder!.LastName + " " + x.Holder!.FirstName)!, "NOCASE")).ThenBy(x => x.FolioId),

//				("Fund", true) => q.OrderByDescending(x => EF.Functions.Collate(x.Fund!.FundName!, "NOCASE")).ThenBy(x => x.FolioId),
//				("Fund", false) => q.OrderBy(x => EF.Functions.Collate(x.Fund!.FundName!, "NOCASE")).ThenBy(x => x.FolioId),

//				("Bank", true) => q.OrderByDescending(x => EF.Functions.Collate(x.AttachedBank ?? string.Empty, "NOCASE")).ThenBy(x => x.FolioId),
//				("Bank", false) => q.OrderBy(x => EF.Functions.Collate(x.AttachedBank ?? string.Empty, "NOCASE")).ThenBy(x => x.FolioId),

//				("Active", true) => q.OrderByDescending(x => x.IsActive).ThenBy(x => x.FolioId),
//				("Active", false) => q.OrderBy(x => x.IsActive).ThenBy(x => x.FolioId),

//				_ => q.OrderBy(x => EF.Functions.Collate(x.FolioNumber!, "NOCASE")).ThenBy(x => x.FolioId),
//			};

//			var items = await q.Skip(Math.Max(0, skip)).Take(Math.Max(1, take)).ToListAsync(ct);
//			return (items, total);
//		}

//		public Task<Folio?> GetByIdAsync(int id, CancellationToken ct = default) =>
//			_db.Set<Folio>().AsNoTracking().Include(x => x.Holder).Include(x => x.Fund)
//			   .FirstOrDefaultAsync(x => x.FolioId == id, ct);

//		public async Task<int> AddAsync(Folio entity, CancellationToken ct = default)
//		{
//			Normalize(entity);
//			Validate(entity);

//			await EnsureUniqueAsync(entity, false, ct);

//			var now = DateTime.UtcNow;
//			entity.InDate = now;
//			entity.UpdateDate = now;

//			_db.Set<Folio>().Add(entity);
//			await _db.SaveChangesAsync(ct);
//			return entity.FolioId;
//		}

//		public async Task UpdateAsync(Folio entity, CancellationToken ct = default)
//		{
//			var existing = await _db.Set<Folio>().FirstOrDefaultAsync(x => x.FolioId == entity.FolioId, ct);
//			if (existing is null) return;

//			Normalize(entity);
//			Validate(entity);

//			await EnsureUniqueAsync(entity, true, ct);

//			// map
//			existing.FolioNumber = entity.FolioNumber;
//			existing.FolioPurpose = entity.FolioPurpose;
//			existing.IsActive = entity.IsActive;
//			existing.AttachedBank = entity.AttachedBank;
//			existing.FolioHolderId = entity.FolioHolderId;
//			existing.FundId = entity.FundId;

//			existing.UpdateDate = DateTime.UtcNow;

//			await _db.SaveChangesAsync(ct);
//		}

//		public async Task DeleteAsync(int id, CancellationToken ct = default)
//		{
//			var row = await _db.Set<Folio>().FindAsync(new object?[] { id }, ct);
//			if (row is null) return;

//			_db.Set<Folio>().Remove(row);
//			await _db.SaveChangesAsync(ct);
//		}

//		// Helpers
//		private static void Normalize(Folio e)
//		{
//			e.FolioNumber = e.FolioNumber?.Trim() ?? string.Empty;
//			e.FolioPurpose = e.FolioPurpose?.Trim();
//			e.AttachedBank = e.AttachedBank?.Trim();
//		}

//		private static void Validate(Folio e)
//		{
//			if (string.IsNullOrWhiteSpace(e.FolioNumber))
//				throw new ArgumentException("Folio Number is required.", nameof(e.FolioNumber));
//			if (e.FolioNumber.Length < 5 || e.FolioNumber.Length > 50)
//				throw new ArgumentOutOfRangeException(nameof(e.FolioNumber), "Folio Number should be 5–50 characters.");
//			if (e.FolioHolderId <= 0)
//				throw new ArgumentException("Folio Holder is required.", nameof(e.FolioHolderId));
//			if (e.FundId <= 0)
//				throw new ArgumentException("Fund is required.", nameof(e.FundId));
//		}

//		private async Task EnsureUniqueAsync(Folio e, bool isUpdate, CancellationToken ct)
//		{
//			// composite check (case-insensitive)
//			var q = _db.Set<Folio>().AsNoTracking().Where(x => !isUpdate || x.FolioId != e.FolioId);

//			var dup = await q.AnyAsync(x =>
//				x.FolioHolderId == e.FolioHolderId &&
//				x.FundId == e.FundId &&
//				EF.Functions.Collate(x.FolioNumber, "NOCASE") == e.FolioNumber, ct);

//			if (dup)
//				throw new InvalidOperationException("A folio with the same holder, fund, and folio number already exists.");
//		}

//	}
//}
using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data.Models;

namespace OM.MFPTrackerV1.Data.Services
{
	//public interface IFolioRepo
	//{
	//	Task<(IReadOnlyList<Folio> Items, int TotalCount)> GetAsync(
	//		string? folioContains = null,
	//		string? holderNameContains = null,
	//		int? amcId = null,
	//		bool? onlyActive = null,
	//		int skip = 0,
	//		int take = 25,
	//		string sortBy = "FolioNumber",   // FolioNumber | Holder | AMC | Bank | Active
	//		bool desc = false,
	//		CancellationToken ct = default);

	//	Task<Folio?> GetByIdAsync(int id, CancellationToken ct = default);
	//	Task<int> AddAsync(Folio entity, CancellationToken ct = default);
	//	Task UpdateAsync(Folio entity, CancellationToken ct = default);
	//	Task DeleteAsync(int id, CancellationToken ct = default);
	//}

	//public class FolioRepo : IFolioRepo
	//{
	//	private readonly MFPTrackerDbContext _db;
	//	public FolioRepo(MFPTrackerDbContext db) => _db = db;

	//	public async Task<(IReadOnlyList<Folio> Items, int TotalCount)> GetAsync(
	//		string? folioContains = null,
	//		string? holderNameContains = null,
	//		int? amcId = null,
	//		bool? onlyActive = null,
	//		int skip = 0,
	//		int take = 25,
	//		string sortBy = "FolioNumber",
	//		bool desc = false,
	//		CancellationToken ct = default)
	//	{
	//		IQueryable<Folio> q = _db.Set<Folio>().AsNoTracking().Include(x => x.Holder).Include(x => x.AMC);
	//		//var q = _db.Set<Folio>().AsNoTracking().Include(x => x.Holder).Include(x => x.AMC);

	//		if (!string.IsNullOrWhiteSpace(folioContains))
	//		{
	//			var pattern = $"%{folioContains}%";
	//			q = q.Where(x => EF.Functions.Like(EF.Functions.Collate(x.FolioNumber, "NOCASE"), pattern));
	//		}

	//		if (!string.IsNullOrWhiteSpace(holderNameContains))
	//		{
	//			var pattern = $"%{holderNameContains}%";
	//			q = q.Where(x =>
	//				x.Holder != null &&
	//				(
	//					(x.Holder.FirstName != null && EF.Functions.Like(EF.Functions.Collate(x.Holder.FirstName, "NOCASE"), pattern)) ||
	//					(x.Holder.LastName != null && EF.Functions.Like(EF.Functions.Collate(x.Holder.LastName, "NOCASE"), pattern))
	//				));
	//		}

	//		if (amcId is int aid && aid > 0) q = q.Where(x => x.AMCId == aid);
	//		if (onlyActive is true) q = q.Where(x => x.IsActive);

	//		var total = await q.CountAsync(ct);

	//		q = (sortBy, desc) switch
	//		{
	//			("FolioNumber", true) => q.OrderByDescending(x => EF.Functions.Collate(x.FolioNumber!, "NOCASE")).ThenBy(x => x.FolioId),
	//			("FolioNumber", false) => q.OrderBy(x => EF.Functions.Collate(x.FolioNumber!, "NOCASE")).ThenBy(x => x.FolioId),

	//			("Holder", true) => q.OrderByDescending(x => EF.Functions.Collate((x.Holder!.LastName + " " + x.Holder!.FirstName)!, "NOCASE")).ThenBy(x => x.FolioId),
	//			("Holder", false) => q.OrderBy(x => EF.Functions.Collate((x.Holder!.LastName + " " + x.Holder!.FirstName)!, "NOCASE")).ThenBy(x => x.FolioId),

	//			("AMC", true) => q.OrderByDescending(x => EF.Functions.Collate(x.AMC!.AMCName!, "NOCASE")).ThenBy(x => x.FolioId),
	//			("AMC", false) => q.OrderBy(x => EF.Functions.Collate(x.AMC!.AMCName!, "NOCASE")).ThenBy(x => x.FolioId),

	//			("Bank", true) => q.OrderByDescending(x => EF.Functions.Collate(x.AttachedBank ?? string.Empty, "NOCASE")).ThenBy(x => x.FolioId),
	//			("Bank", false) => q.OrderBy(x => EF.Functions.Collate(x.AttachedBank ?? string.Empty, "NOCASE")).ThenBy(x => x.FolioId),

	//			("Active", true) => q.OrderByDescending(x => x.IsActive).ThenBy(x => x.FolioId),
	//			("Active", false) => q.OrderBy(x => x.IsActive).ThenBy(x => x.FolioId),

	//			_ => q.OrderBy(x => EF.Functions.Collate(x.FolioNumber!, "NOCASE")).ThenBy(x => x.FolioId),
	//		};

	//		var items = await q.Skip(Math.Max(0, skip)).Take(Math.Max(1, take)).ToListAsync(ct);
	//		return (items, total);
	//	}

	//	public Task<Folio?> GetByIdAsync(int id, CancellationToken ct = default) =>
	//		_db.Set<Folio>().AsNoTracking().Include(x => x.Holder).Include(x => x.AMC)
	//		   .FirstOrDefaultAsync(x => x.FolioId == id, ct);

	//	public async Task<int> AddAsync(Folio entity, CancellationToken ct = default)
	//	{
	//		Normalize(entity);
	//		Validate(entity);

	//		await EnsureUniqueAsync(entity, false, ct);

	//		var now = DateTime.UtcNow;
	//		entity.InDate = now;
	//		entity.UpdateDate = now;

	//		_db.Set<Folio>().Add(entity);
	//		await _db.SaveChangesAsync(ct);
	//		return entity.FolioId;
	//	}

	//	public async Task UpdateAsync(Folio entity, CancellationToken ct = default)
	//	{
	//		var existing = await _db.Set<Folio>().FirstOrDefaultAsync(x => x.FolioId == entity.FolioId, ct);
	//		if (existing is null) return;

	//		Normalize(entity);
	//		Validate(entity);
	//		await EnsureUniqueAsync(entity, true, ct);

	//		existing.FolioNumber = entity.FolioNumber;
	//		existing.FolioPurpose = entity.FolioPurpose;
	//		existing.AttachedBank = entity.AttachedBank;
	//		existing.IsActive = entity.IsActive;
	//		existing.AMCId = entity.AMCId;
	//		existing.FolioHolderId = entity.FolioHolderId;
	//		existing.UpdateDate = DateTime.UtcNow;

	//		await _db.SaveChangesAsync(ct);
	//	}

	//	public async Task DeleteAsync(int id, CancellationToken ct = default)
	//	{
	//		var hasTxns = await _db.Set<MutualFundTransaction>().AsNoTracking().AnyAsync(t => t.FolioId == id, ct);
	//		if (hasTxns) throw new InvalidOperationException("Cannot delete folio: transactions exist.");

	//		var row = await _db.Set<Folio>().FindAsync(new object?[] { id }, ct);
	//		if (row is null) return;

	//		_db.Set<Folio>().Remove(row);
	//		await _db.SaveChangesAsync(ct);
	//	}

	//	private static void Normalize(Folio e)
	//	{
	//		e.FolioNumber = (e.FolioNumber ?? "").Trim();
	//		e.FolioPurpose = (e.FolioPurpose ?? "").Trim();
	//		e.AttachedBank = (e.AttachedBank ?? "").Trim();
	//	}

	//	private static void Validate(Folio e)
	//	{
	//		if (string.IsNullOrWhiteSpace(e.FolioNumber))
	//			throw new ArgumentException("Folio Number is required.", nameof(e.FolioNumber));
	//		if (e.FolioNumber.Length < 5 || e.FolioNumber.Length > 50)
	//			throw new ArgumentOutOfRangeException(nameof(e.FolioNumber), "Folio Number should be 5–50 characters.");
	//		if (e.AMCId <= 0)
	//			throw new ArgumentException("AMC is required.", nameof(e.AMCId));
	//		if (e.FolioHolderId <= 0)
	//			throw new ArgumentException("Folio Holder is required.", nameof(e.FolioHolderId));
	//	}

	//	private async Task EnsureUniqueAsync(Folio e, bool isUpdate, CancellationToken ct)
	//	{
	//		var q = _db.Set<Folio>().AsNoTracking().Where(x => !isUpdate || x.FolioId != e.FolioId);

	//		var dup = await q.AnyAsync(x =>
	//			x.AMCId == e.AMCId &&
	//			EF.Functions.Collate(x.FolioNumber, "NOCASE") == e.FolioNumber, ct);

	//		if (dup) throw new InvalidOperationException("A folio with the same AMC and number already exists.");
	//	}
	//}

	public interface IFolioRepo
	{
		Task<(IReadOnlyList<Folio> Items, int TotalCount)> GetAsync(
			string? search,
			string? sortBy,
			bool sortDesc,
			int pageNumber,
			int pageSize);

		Task<Folio?> GetByIdAsync(int id);

		Task<bool> ExistsByFolioNumberAsync(
			string folioNumber,
			int amcId,
			int? excludeId = null);

		Task<bool> EnsureUniqueAsync(
			Folio entity,
			bool isUpdate,
			CancellationToken ct = default);

		Task<Folio> AddAsync(Folio entity);
		Task<Folio> UpdateAsync(Folio entity);
		Task DeleteAsync(int id);

		Task<int> CountAsync();
	}

	public class FolioRepo : IFolioRepo
	{
		private readonly MFPTrackerDbContext _db;

		public FolioRepo(MFPTrackerDbContext db)
		{
			_db = db;
		}

		// -------------------------------------------------
		// GET: Search + Sort + Paging
		// -------------------------------------------------
		public async Task<(IReadOnlyList<Folio> Items, int TotalCount)> GetAsync(
			string? search,
			string? sortBy,
			bool sortDesc,
			int pageNumber,
			int pageSize)
		{
			if (pageNumber < 1) pageNumber = 1;
			if (pageSize < 1) pageSize = 10;
			if (pageSize > 200) pageSize = 200;

			IQueryable<Folio> q = _db.Set<Folio>()
				.Include(x => x.AMC)
				.Include(x => x.Holder)
				.AsNoTracking();

			// -------- Filtering --------
			if (!string.IsNullOrWhiteSpace(search))
			{
				var term = search.Trim();
				var pattern = $"%{term}%";

				q = q.Where(x =>
					EF.Functions.Like(EF.Functions.Collate(x.FolioNumber, "NOCASE"), pattern) ||
					(x.Holder != null &&
					 EF.Functions.Like(EF.Functions.Collate(x.Holder.FirstName, "NOCASE"), pattern)) ||
					(x.AMC != null &&
					 EF.Functions.Like(EF.Functions.Collate(x.AMC.AMCName, "NOCASE"), pattern))
				);
			}

			// -------- Sorting --------
			var key = (sortBy ?? nameof(Folio.FolioNumber)).Trim();

			q = (key, sortDesc) switch
			{
				(nameof(Folio.FolioNumber), false) =>
					q.OrderBy(x => EF.Functions.Collate(x.FolioNumber, "NOCASE"))
					 .ThenBy(x => x.FolioId),

				(nameof(Folio.FolioNumber), true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.FolioNumber, "NOCASE"))
					 .ThenBy(x => x.FolioId),

				("AMC", false) =>
					q.OrderBy(x => EF.Functions.Collate(x.AMC!.AMCName, "NOCASE"))
					 .ThenBy(x => x.FolioId),

				("AMC", true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.AMC!.AMCName, "NOCASE"))
					 .ThenBy(x => x.FolioId),

				//("Holder", false) =>
				//	q.OrderBy(x => EF.Functions.Collate(x.Holder!.LastName, "NOCASE")),

				//("Holder", true) =>
				//	q.OrderByDescending(x => EF.Functions.Collate(x.Holder!.LastName, "NOCASE")),
				("Holder", false) =>
					q.OrderBy(x => EF.Functions.Collate(x.Holder!.LastName, "NOCASE"))
					 .ThenBy(x => EF.Functions.Collate(x.Holder!.FirstName, "NOCASE"))
					 .ThenBy(x => x.FolioId),

				("Holder", true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.Holder!.LastName, "NOCASE"))
					 .ThenByDescending(x => EF.Functions.Collate(x.Holder!.FirstName, "NOCASE"))
					 .ThenBy(x => x.FolioId),
				_ =>
					q.OrderBy(x => EF.Functions.Collate(x.FolioNumber, "NOCASE"))
			};

			var total = await q.CountAsync();
			var items = await q.Skip((pageNumber - 1) * pageSize)
							   .Take(pageSize)
							   .ToListAsync();

			return (items, total);
		}

		public Task<int> CountAsync() =>
			_db.Set<Folio>().CountAsync();

		public async Task<Folio?> GetByIdAsync(int id)
		{
			return await _db.Set<Folio>()
				.Include(x => x.AMC)
				.Include(x => x.Holder)
				.FirstOrDefaultAsync(x => x.FolioId == id);
		}

		// -------- Uniqueness --------
		public async Task<bool> ExistsByFolioNumberAsync(
			string folioNumber,
			int amcId,
			int? excludeId = null)
		{
			if (string.IsNullOrWhiteSpace(folioNumber))
				return false;

			var q = _db.Set<Folio>().AsNoTracking().Where(x => x.AMCId == amcId);

			if (excludeId.HasValue)
				q = q.Where(x => x.FolioId != excludeId.Value);

			return await q.AnyAsync(x =>
				EF.Functions.Collate(x.FolioNumber, "NOCASE") == folioNumber.Trim()
			);
		}

		public async Task<bool> EnsureUniqueAsync(
			Folio e,
			bool isUpdate,
			CancellationToken ct = default)
		{
			var q = _db.Set<Folio>()
				.AsNoTracking()
				.Where(x => !isUpdate || x.FolioId != e.FolioId);

			return await q.AnyAsync(x =>
				x.AMCId == e.AMCId &&
				EF.Functions.Collate(x.FolioNumber, "NOCASE") == e.FolioNumber,
				ct
			);
		}

		// -------- Add --------
		public async Task<Folio> AddAsync(Folio entity)
		{
			entity.FolioNumber = entity.FolioNumber.Trim();
			entity.InDate = DateTime.UtcNow;
			entity.UpdateDate = DateTime.UtcNow;

			_db.Set<Folio>().Add(entity);
			await _db.SaveChangesAsync();
			return entity;
		}

		// -------- Update --------
		public async Task<Folio> UpdateAsync(Folio entity)
		{
			var existing = await _db.Set<Folio>()
				.FirstOrDefaultAsync(x => x.FolioId == entity.FolioId);

			if (existing == null)
				throw new InvalidOperationException($"Folio {entity.FolioId} not found.");

			existing.FolioNumber = entity.FolioNumber.Trim();
			existing.FolioPurpose = entity.FolioPurpose;
			existing.AttachedBank = entity.AttachedBank;
			existing.IsActive = entity.IsActive;
			existing.AMCId = entity.AMCId;
			existing.FolioHolderId = entity.FolioHolderId;
			existing.UpdateDate = DateTime.UtcNow;

			await _db.SaveChangesAsync();
			return existing;
		}

		// -------- Delete --------
		public async Task DeleteAsync(int id)
		{
			var existing = await _db.Set<Folio>().FindAsync(id);
			if (existing == null) return;

			_db.Set<Folio>().Remove(existing);
			await _db.SaveChangesAsync();
		}
	}
}