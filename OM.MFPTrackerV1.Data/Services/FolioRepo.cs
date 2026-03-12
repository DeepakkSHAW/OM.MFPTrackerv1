using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OM.MFPTrackerV1.Data.Services
{
	public interface IFolioRepo
	{
		Task<(IReadOnlyList<Folio> Items, int TotalCount)> GetAsync(
			string? folioContains = null,
			string? holderNameContains = null,
			string? fundNameContains = null,
			bool? onlyActive = null,
			int? holderId = null,          // optional direct filter by holder
			int? fundId = null,            // optional direct filter by fund
			int skip = 0,
			int take = 25,
			string sortBy = "FolioNumber", // FolioNumber | Holder | Fund | Bank | Active
			bool desc = false,
			CancellationToken ct = default);

		Task<Folio?> GetByIdAsync(int id, CancellationToken ct = default);
		Task<int> AddAsync(Folio entity, CancellationToken ct = default);
		Task UpdateAsync(Folio entity, CancellationToken ct = default);
		Task DeleteAsync(int id, CancellationToken ct = default);
	}
	public class FolioRepo : IFolioRepo
	{
		private readonly MFPTrackerDbContext _db;
		public FolioRepo(MFPTrackerDbContext db) => _db = db;
		public async Task<(IReadOnlyList<Folio> Items, int TotalCount)> GetAsync(
			string? folioContains = null,
			string? holderNameContains = null,
			string? fundNameContains = null,
			bool? onlyActive = null,
			int? holderId = null,
			int? fundId = null,
			int skip = 0,
			int take = 25,
			string sortBy = "FolioNumber",
			bool desc = false,
			CancellationToken ct = default)
		{
			var q = _db.Set<Folio>()
					   .AsNoTracking()
					   .Include(x => x.Holder)
					   .Include(x => x.Fund)
					   .AsQueryable();

			// Filters (NOCASE)
			if (!string.IsNullOrWhiteSpace(folioContains))
			{
				var pattern = $"%{folioContains}%";
				q = q.Where(x => x.FolioNumber != null &&
								 EF.Functions.Like(EF.Functions.Collate(x.FolioNumber, "NOCASE"), pattern));
			}

			if (!string.IsNullOrWhiteSpace(holderNameContains))
			{
				var pattern = $"%{holderNameContains}%";
				q = q.Where(x =>
					x.Holder != null &&
					(
						(x.Holder.FirstName != null && EF.Functions.Like(EF.Functions.Collate(x.Holder.FirstName, "NOCASE"), pattern)) ||
						(x.Holder.LastName != null && EF.Functions.Like(EF.Functions.Collate(x.Holder.LastName, "NOCASE"), pattern))
					));
			}

			if (!string.IsNullOrWhiteSpace(fundNameContains))
			{
				var pattern = $"%{fundNameContains}%";
				q = q.Where(x =>
					x.Fund != null &&
					x.Fund.FundName != null &&
					EF.Functions.Like(EF.Functions.Collate(x.Fund.FundName, "NOCASE"), pattern));
			}

			if (onlyActive is true) q = q.Where(x => x.IsActive);
			if (holderId is int hid && hid > 0) q = q.Where(x => x.FolioHolderId == hid);
			if (fundId is int fid && fid > 0) q = q.Where(x => x.FundId == fid);

			var total = await q.CountAsync(ct);

			// Sorting (typed + stable)
			q = (sortBy, desc) switch
			{
				("FolioNumber", true) => q.OrderByDescending(x => EF.Functions.Collate(x.FolioNumber!, "NOCASE")).ThenBy(x => x.FolioId),
				("FolioNumber", false) => q.OrderBy(x => EF.Functions.Collate(x.FolioNumber!, "NOCASE")).ThenBy(x => x.FolioId),

				("Holder", true) => q.OrderByDescending(x => EF.Functions.Collate((x.Holder!.LastName + " " + x.Holder!.FirstName)!, "NOCASE")).ThenBy(x => x.FolioId),
				("Holder", false) => q.OrderBy(x => EF.Functions.Collate((x.Holder!.LastName + " " + x.Holder!.FirstName)!, "NOCASE")).ThenBy(x => x.FolioId),

				("Fund", true) => q.OrderByDescending(x => EF.Functions.Collate(x.Fund!.FundName!, "NOCASE")).ThenBy(x => x.FolioId),
				("Fund", false) => q.OrderBy(x => EF.Functions.Collate(x.Fund!.FundName!, "NOCASE")).ThenBy(x => x.FolioId),

				("Bank", true) => q.OrderByDescending(x => EF.Functions.Collate(x.AttachedBank ?? string.Empty, "NOCASE")).ThenBy(x => x.FolioId),
				("Bank", false) => q.OrderBy(x => EF.Functions.Collate(x.AttachedBank ?? string.Empty, "NOCASE")).ThenBy(x => x.FolioId),

				("Active", true) => q.OrderByDescending(x => x.IsActive).ThenBy(x => x.FolioId),
				("Active", false) => q.OrderBy(x => x.IsActive).ThenBy(x => x.FolioId),

				_ => q.OrderBy(x => EF.Functions.Collate(x.FolioNumber!, "NOCASE")).ThenBy(x => x.FolioId),
			};

			var items = await q.Skip(Math.Max(0, skip)).Take(Math.Max(1, take)).ToListAsync(ct);
			return (items, total);
		}

		public Task<Folio?> GetByIdAsync(int id, CancellationToken ct = default) =>
			_db.Set<Folio>().AsNoTracking().Include(x => x.Holder).Include(x => x.Fund)
			   .FirstOrDefaultAsync(x => x.FolioId == id, ct);

		public async Task<int> AddAsync(Folio entity, CancellationToken ct = default)
		{
			Normalize(entity);
			Validate(entity);

			await EnsureUniqueAsync(entity, false, ct);

			var now = DateTime.UtcNow;
			entity.InDate = now;
			entity.UpdateDate = now;

			_db.Set<Folio>().Add(entity);
			await _db.SaveChangesAsync(ct);
			return entity.FolioId;
		}

		public async Task UpdateAsync(Folio entity, CancellationToken ct = default)
		{
			var existing = await _db.Set<Folio>().FirstOrDefaultAsync(x => x.FolioId == entity.FolioId, ct);
			if (existing is null) return;

			Normalize(entity);
			Validate(entity);

			await EnsureUniqueAsync(entity, true, ct);

			// map
			existing.FolioNumber = entity.FolioNumber;
			existing.FolioPurpose = entity.FolioPurpose;
			existing.IsActive = entity.IsActive;
			existing.AttachedBank = entity.AttachedBank;
			existing.FolioHolderId = entity.FolioHolderId;
			existing.FundId = entity.FundId;

			existing.UpdateDate = DateTime.UtcNow;

			await _db.SaveChangesAsync(ct);
		}

		public async Task DeleteAsync(int id, CancellationToken ct = default)
		{
			var row = await _db.Set<Folio>().FindAsync(new object?[] { id }, ct);
			if (row is null) return;

			_db.Set<Folio>().Remove(row);
			await _db.SaveChangesAsync(ct);
		}

		// Helpers
		private static void Normalize(Folio e)
		{
			e.FolioNumber = e.FolioNumber?.Trim() ?? string.Empty;
			e.FolioPurpose = e.FolioPurpose?.Trim();
			e.AttachedBank = e.AttachedBank?.Trim();
		}

		private static void Validate(Folio e)
		{
			if (string.IsNullOrWhiteSpace(e.FolioNumber))
				throw new ArgumentException("Folio Number is required.", nameof(e.FolioNumber));
			if (e.FolioNumber.Length < 5 || e.FolioNumber.Length > 50)
				throw new ArgumentOutOfRangeException(nameof(e.FolioNumber), "Folio Number should be 5–50 characters.");
			if (e.FolioHolderId <= 0)
				throw new ArgumentException("Folio Holder is required.", nameof(e.FolioHolderId));
			if (e.FundId <= 0)
				throw new ArgumentException("Fund is required.", nameof(e.FundId));
		}

		private async Task EnsureUniqueAsync(Folio e, bool isUpdate, CancellationToken ct)
		{
			// composite check (case-insensitive)
			var q = _db.Set<Folio>().AsNoTracking().Where(x => !isUpdate || x.FolioId != e.FolioId);

			var dup = await q.AnyAsync(x =>
				x.FolioHolderId == e.FolioHolderId &&
				x.FundId == e.FundId &&
				EF.Functions.Collate(x.FolioNumber, "NOCASE") == e.FolioNumber, ct);

			if (dup)
				throw new InvalidOperationException("A folio with the same holder, fund, and folio number already exists.");
		}

	}
}
