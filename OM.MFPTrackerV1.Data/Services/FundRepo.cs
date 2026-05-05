using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data.Models;

namespace OM.MFPTrackerV1.Data.Services
{
	public interface IMFFundRepo
	{
		Task<(IReadOnlyList<Fund> Items, int TotalCount)> GetAsync(string? search, string? sortBy, bool sortDesc, int pageNumber, int pageSize);

		Task<Fund?> GetByIdAsync(int id);
		Task<List<Fund>> GetAllAsync(CancellationToken ct = default);
		Task<bool> ExistsByISINAsync(string isin, int? excludeId = null);
		Task<bool> ExistsBySchemeCodeAsync(string schemeCode, int amcId, int? excludeId = null);

		/// <summary>
		/// Checks uniqueness: (AMCId + SchemeCode)
		/// </summary>
		Task<bool> EnsureUniqueAsync(Fund entity, bool isUpdate, CancellationToken ct = default);

		Task<Fund> AddAsync(Fund entity);
		Task<Fund> UpdateAsync(Fund entity);
		Task DeleteAsync(int id);

		Task<int> CountAsync();
		Task<IReadOnlyList<FundLookupDto>> GetFundsAsync();
		/// <summary>
		/// Returns distinct funds that have transactions under the given folio
		/// </summary>
		Task<IReadOnlyList<FundLookupDto>> GetFundsByFolioAsync(int folioId, CancellationToken ct = default);
	}
	public class MFFundRepo : IMFFundRepo
	{
		private readonly MFPTrackerDbContext _db;
		public MFFundRepo(MFPTrackerDbContext db) => _db = db;


		// -------------------------------------------------
		// GET: Search + Sort + Paging
		// -------------------------------------------------
		public async Task<(IReadOnlyList<Fund> Items, int TotalCount)> GetAsync(
			string? search,
			string? sortBy,
			bool sortDesc,
			int pageNumber,
			int pageSize)
		{
			if (pageNumber < 1) pageNumber = 1;
			if (pageSize < 1) pageSize = 10;
			if (pageSize > 200) pageSize = 200;

			IQueryable<Fund> q = _db.Set<Fund>()
				.Include(x => x.AMC)
				.Include(x => x.Category)
				.AsNoTracking();

			// ---------------- Filtering ----------------
			if (!string.IsNullOrWhiteSpace(search))
			{
				var term = search.Trim();

				static string Esc(string s) =>
					s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

				var pattern = $"%{Esc(term)}%";

				q = q.Where(x =>
					EF.Functions.Like(EF.Functions.Collate(x.FundName, "NOCASE"), pattern) ||
					EF.Functions.Like(EF.Functions.Collate(x.ISIN, "NOCASE"), pattern) ||
					EF.Functions.Like(EF.Functions.Collate(x.SchemeCode, "NOCASE"), pattern) ||
					(x.AMC != null &&
					 EF.Functions.Like(EF.Functions.Collate(x.AMC.AMCName!, "NOCASE"), pattern))
				);
			}

			// ---------------- Sorting ----------------
			var key = (sortBy ?? nameof(Fund.FundName)).Trim();

			q = (key, sortDesc) switch
			{
				(nameof(Fund.FundName), false) =>
					q.OrderBy(x => EF.Functions.Collate(x.FundName, "NOCASE"))
					 .ThenBy(x => x.FundId),

				(nameof(Fund.FundName), true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.FundName, "NOCASE"))
					 .ThenBy(x => x.FundId),

				(nameof(Fund.SchemeCode), false) =>
					q.OrderBy(x => EF.Functions.Collate(x.SchemeCode, "NOCASE")),

				(nameof(Fund.SchemeCode), true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.SchemeCode, "NOCASE")),

				(nameof(Fund.ISIN), false) =>
					q.OrderBy(x => EF.Functions.Collate(x.ISIN, "NOCASE")),

				(nameof(Fund.ISIN), true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.ISIN, "NOCASE")),

				("AMC", false) =>
					q.OrderBy(x => EF.Functions.Collate(x.AMC!.AMCName!, "NOCASE")),

				("AMC", true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.AMC!.AMCName!, "NOCASE")),

				("Category", false) =>
					q.OrderBy(x => EF.Functions.Collate(x.Category!.CategoryName!, "NOCASE")),

				("Category", true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.Category!.CategoryName!, "NOCASE")),

				_ =>
					q.OrderBy(x => EF.Functions.Collate(x.FundName, "NOCASE"))
			};

			var total = await q.CountAsync();

			var items = await q
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (items, total);
		}

		// -------------------------------------------------
		public Task<int> CountAsync() =>
			_db.Set<Fund>().CountAsync();

		public async Task<List<Fund>> GetAllAsync(CancellationToken ct = default)
		{
			return await _db.Funds
				.Include(f => f.AMC)
				.Include(f => f.Category)
				.AsNoTracking()
				.OrderBy(f => f.FundName)
				.ToListAsync(ct);
		}

		// -------------------------------------------------
		public async Task<Fund?> GetByIdAsync(int id)
		{
			return await _db.Set<Fund>()
				.Include(x => x.AMC)
				.Include(x => x.Category)
				.FirstOrDefaultAsync(x => x.FundId == id);
		}

		// -------------------------------------------------
		// Uniqueness Checks
		// -------------------------------------------------
		public async Task<bool> ExistsByISINAsync(string isin, int? excludeId = null)
		{
			if (string.IsNullOrWhiteSpace(isin))
				return false;

			var q = _db.Set<Fund>().AsQueryable();

			if (excludeId.HasValue)
				q = q.Where(x => x.FundId != excludeId.Value);

			return await q.AnyAsync(
				x => EF.Functions.Collate(x.ISIN, "NOCASE") == isin.Trim()
			);
		}

		public async Task<bool> ExistsBySchemeCodeAsync(string schemeCode, int amcId, int? excludeId = null)
		{
			if (string.IsNullOrWhiteSpace(schemeCode))
				return false;

			var q = _db.Set<Fund>().AsQueryable()
				.Where(x => x.AMCId == amcId);

			if (excludeId.HasValue)
				q = q.Where(x => x.FundId != excludeId.Value);

			return await q.AnyAsync(
				x => EF.Functions.Collate(x.SchemeCode, "NOCASE") == schemeCode.Trim()
			);
		}

		public async Task<bool> EnsureUniqueAsync(Fund e, bool isUpdate, CancellationToken ct = default)
		{
			var q = _db.Set<Fund>()
				.AsNoTracking()
				.Where(x => !isUpdate || x.FundId != e.FundId);

			return await q.AnyAsync(x =>
				x.AMCId == e.AMCId &&
				EF.Functions.Collate(x.SchemeCode, "NOCASE") == e.SchemeCode,
				ct
			);
		}

		// -------------------------------------------------
		// ADD
		// -------------------------------------------------
		public async Task<Fund> AddAsync(Fund entity)
		{
			entity.SchemeCode = entity.SchemeCode.Trim().ToUpperInvariant();
			entity.ISIN = entity.ISIN.Trim().ToUpperInvariant();

			entity.InDate = DateTime.UtcNow;
			entity.UpdateDate = DateTime.UtcNow;

			_db.Set<Fund>().Add(entity);
			await _db.SaveChangesAsync();

			return entity;
		}

		// -------------------------------------------------
		// UPDATE
		// -------------------------------------------------
		public async Task<Fund> UpdateAsync(Fund entity)
		{
			var existing = await _db.Set<Fund>()
				.FirstOrDefaultAsync(x => x.FundId == entity.FundId);

			if (existing == null)
				throw new InvalidOperationException($"Fund id {entity.FundId} not found.");

			existing.FundName = entity.FundName.Trim();
			existing.SchemeCode = entity.SchemeCode.Trim().ToUpperInvariant();
			existing.ISIN = entity.ISIN.Trim().ToUpperInvariant();
			existing.AMCId = entity.AMCId;
			existing.MFCatId = entity.MFCatId;
			existing.IsTransactionAllowed = entity.IsTransactionAllowed;
			existing.IsNavAllowed = entity.IsNavAllowed;
			existing.UpdateDate = DateTime.UtcNow;

			await _db.SaveChangesAsync();
			return existing;
		}

		// -------------------------------------------------
		// DELETE (safe)
		// -------------------------------------------------
		public async Task DeleteAsync(int id)
		{
			var existing = await _db.Set<Fund>()
				.FirstOrDefaultAsync(x => x.FundId == id);

			if (existing == null)
				return;

			_db.Set<Fund>().Remove(existing);
			await _db.SaveChangesAsync();
		}


		public async Task<IReadOnlyList<FundLookupDto>> GetFundsAsync()
		{
			var items = await _db.Funds
				.AsNoTracking()                     // ✅ lookup = read‑only
				.Where(f => f.IsNavAllowed)             // ✅ only active funds (adjust if needed)
				.OrderBy(f => f.FundName)               // ✅ stable UX ordering
				.Select(f => new FundLookupDto(
					f.FundId,
					f.FundName
				))
				.ToListAsync();

			return items;
		}
		//public async Task<IReadOnlyList<FundLookupDto>> GetFundsByFolioAsync(int folioId, CancellationToken ct = default)
		//{
		//	return await (
		//		from t in _db.MutualFundTransactions
		//		join f in _db.Funds on t.FundId equals f.FundId
		//		where t.FolioId == folioId
		//		select new FundLookupDto(f.FundId, f.FundName))
		//		.Distinct()
		//		.OrderBy(x => x.Name)
		//		.ToListAsync(ct);
		//}
		public async Task<IReadOnlyList<FundLookupDto>> GetFundsByFolioAsync(
			int folioId,
			CancellationToken ct = default)
		{
			return await _db.MutualFundTransactions
				.Where(t => t.FolioId == folioId)
				.Select(t => new
				{
					t.FundId,
					t.Fund.FundName
				})
				.Distinct()
				.OrderBy(x => x.FundName)
				.Select(x => new FundLookupDto(
					x.FundId,
					x.FundName
				))
				.ToListAsync(ct);
		}
	}
}