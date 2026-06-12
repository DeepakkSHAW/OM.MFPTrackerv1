using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OM.MFPTrackerV1.Data;
using OM.MFPTrackerV1.Data.Models;
using System.Diagnostics.CodeAnalysis;

namespace OM.MFPTrackerV1.Data.Services
{
	public interface IFolioHolderRepo
	{
		Task<(IReadOnlyList<FolioHolder> Items, int TotalCount)> GetAsync(
			string? search,
			string? sortBy,
			bool sortDesc,
			int pageNumber,
			int pageSize);

		Task<FolioHolder?> GetByIdAsync(int id);

		Task<bool> ExistsBySignatureAsync(string signature, int? excludeId = null);

		/// <summary>
		/// Checks uniqueness of (FirstName + LastName + DateOfBirth)
		/// </summary>
		Task<bool> EnsureUniqueAsync(FolioHolder entity, bool isUpdate, CancellationToken ct = default);

		Task<FolioHolder> AddAsync(FolioHolder entity);
		Task<FolioHolder> UpdateAsync(FolioHolder entity);
		Task DeleteAsync(int id);
		Task<int> CountAsync();
	}

	public class FolioHolderRepo : IFolioHolderRepo
	{
		private readonly MFPTrackerDbContext _db;

		public FolioHolderRepo(MFPTrackerDbContext db)
		{
			_db = db;
		}

		// ---------------------------------------------------------
		// GET WITH SEARCH + SORT + PAGING
		// ---------------------------------------------------------
		public async Task<(IReadOnlyList<FolioHolder> Items, int TotalCount)> GetAsync(
			string? search,
			string? sortBy,
			bool sortDesc,
			int pageNumber,
			int pageSize)
		{
			if (pageNumber < 1) pageNumber = 1;
			if (pageSize < 1) pageSize = 10;
			if (pageSize > 200) pageSize = 200;

			//IQueryable<FolioHolder> q = _db.Set<FolioHolder>().AsNoTracking();
			IQueryable<FolioHolder> q = _db.Set<FolioHolder>().Include(x => x.Folios).AsNoTracking();

			// ---------------------- Filtering ----------------------
			if (!string.IsNullOrWhiteSpace(search))
			{
				var term = search.Trim();

				static string Esc(string s) =>
					s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

				var pattern = $"%{Esc(term)}%";

				q = q.Where(x =>
					EF.Functions.Like(EF.Functions.Collate(x.FirstName, "NOCASE"), pattern) ||
					EF.Functions.Like(EF.Functions.Collate(x.LastName, "NOCASE"), pattern) ||
					EF.Functions.Like(EF.Functions.Collate(x.Signature, "NOCASE"), pattern)
				);
			}

			// ---------------------- Sorting ------------------------
			var key = (sortBy ?? nameof(FolioHolder.LastName)).Trim();

			q = (key, sortDesc) switch
			{
				// FirstName
				(nameof(FolioHolder.FirstName), false) =>
					q.OrderBy(x => EF.Functions.Collate(x.FirstName, "NOCASE"))
					 .ThenBy(x => x.FolioHolderId),

				(nameof(FolioHolder.FirstName), true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.FirstName, "NOCASE"))
					 .ThenBy(x => x.FolioHolderId),

				// LastName
				(nameof(FolioHolder.LastName), false) =>
					q.OrderBy(x => EF.Functions.Collate(x.LastName, "NOCASE"))
					 .ThenBy(x => x.FolioHolderId),

				(nameof(FolioHolder.LastName), true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.LastName, "NOCASE"))
					 .ThenBy(x => x.FolioHolderId),

				// Date of Birth
				(nameof(FolioHolder.DateOfBirth), false) =>
					q.OrderBy(x => x.DateOfBirth).ThenBy(x => x.FolioHolderId),

				(nameof(FolioHolder.DateOfBirth), true) =>
					q.OrderByDescending(x => x.DateOfBirth).ThenBy(x => x.FolioHolderId),

				// Signature
				(nameof(FolioHolder.Signature), false) =>
					q.OrderBy(x => EF.Functions.Collate(x.Signature, "NOCASE"))
					 .ThenBy(x => x.FolioHolderId),

				(nameof(FolioHolder.Signature), true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.Signature, "NOCASE"))
					 .ThenBy(x => x.FolioHolderId),

				// Default: LastName ASC
				_ =>
					q.OrderBy(x => EF.Functions.Collate(x.LastName, "NOCASE"))
					 .ThenBy(x => x.FolioHolderId)
			};

			// Count + paging
			var total = await q.CountAsync();
			var items = await q
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (items, total);
		}

		// ---------------------------------------------------------
		public Task<int> CountAsync() =>
			_db.Set<FolioHolder>().CountAsync();

		// ---------------------------------------------------------
		public async Task<FolioHolder?> GetByIdAsync(int id)
		{
			return await _db.Set<FolioHolder>()
				.Include(x => x.Folios) // if you need folios loaded
				.FirstOrDefaultAsync(x => x.FolioHolderId == id);
		}

		// ---------------------------------------------------------
		// SIGNATURE uniqueness check
		// ---------------------------------------------------------
		public async Task<bool> ExistsBySignatureAsync(string signature, int? excludeId = null)
		{
			if (string.IsNullOrWhiteSpace(signature))
				return false;

			var query = _db.Set<FolioHolder>().AsQueryable();

			if (excludeId.HasValue)
				query = query.Where(x => x.FolioHolderId != excludeId.Value);

			return await query.AnyAsync(
				x => EF.Functions.Collate(x.Signature, "NOCASE") == signature.Trim()
			);
		}

		// ---------------------------------------------------------
		// FirstName + LastName + DOB uniqueness check
		// ---------------------------------------------------------
		public async Task<bool> EnsureUniqueAsync(FolioHolder e, bool isUpdate, CancellationToken ct = default)
		{
			var q = _db.Set<FolioHolder>()
					   .AsNoTracking()
					   .Where(x => !isUpdate || x.FolioHolderId != e.FolioHolderId);

			return await q.AnyAsync(x =>
				EF.Functions.Collate(x.FirstName, "NOCASE") == e.FirstName &&
				EF.Functions.Collate(x.LastName, "NOCASE") == e.LastName &&
				x.DateOfBirth.Date == e.DateOfBirth.Date,
				ct
			);
		}

		// ---------------------------------------------------------
		// ADD
		// ---------------------------------------------------------
		public async Task<FolioHolder> AddAsync(FolioHolder entity)
		{
			entity.InDate = DateTime.UtcNow;
			entity.UpdateDate = DateTime.UtcNow;

			_db.Set<FolioHolder>().Add(entity);
			await _db.SaveChangesAsync();
			return entity;
		}

		// ---------------------------------------------------------
		// UPDATE
		// ---------------------------------------------------------
		public async Task<FolioHolder> UpdateAsync(FolioHolder entity)
		{
			var existing = await _db.Set<FolioHolder>()
				.FirstOrDefaultAsync(x => x.FolioHolderId == entity.FolioHolderId);

			if (existing == null)
				throw new InvalidOperationException($"FolioHolder id {entity.FolioHolderId} not found.");

			existing.FirstName = entity.FirstName;
			existing.LastName = entity.LastName;
			existing.DateOfBirth = entity.DateOfBirth;
			existing.Signature = entity.Signature;

			existing.UpdateDate = DateTime.UtcNow;

			await _db.SaveChangesAsync();
			return existing;
		}

		// ---------------------------------------------------------
		// DELETE
		// ---------------------------------------------------------
		public async Task DeleteAsync(int id)
		{
			var existing = await _db.Set<FolioHolder>()
				.FirstOrDefaultAsync(x => x.FolioHolderId == id);

			if (existing == null)
				return;

			_db.Set<FolioHolder>().Remove(existing);
			await _db.SaveChangesAsync();
		}
	}
}
