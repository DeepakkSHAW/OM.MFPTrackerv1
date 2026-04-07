using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data.Models;

namespace OM.MFPTrackerV1.Data.Services
{
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
		Task<IEnumerable<Folio>> GetByAmcAsync(int amcId);
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
		public async Task<IEnumerable<Folio>> GetByAmcAsync(int amcId)
		{
			return await _db.Folios
				.Where(f => f.AMCId == amcId)
				.AsNoTracking()
				.ToListAsync();
		}
	}
}