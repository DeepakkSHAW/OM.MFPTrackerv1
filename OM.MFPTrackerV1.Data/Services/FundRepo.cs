using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data.Models;

namespace OM.MFPTrackerV1.Data.Services
{
	//public interface IMFFundRepo
	//{
	//	Task<(IReadOnlyList<Fund> Items, int TotalCount)> GetAsync(
	//		string? nameContains = null,
	//		string? amcContains = null,
	//		string? codeContains = null,       // SchemeCode contains
	//		string? isinContains = null,       // ISIN contains
	//		int? amcId = null,
	//		int? mfcCatId = null,
	//		bool? onlyTxnAllowed = null,
	//		bool? onlyNavAllowed = null,
	//		int skip = 0,
	//		int take = 25,
	//		string sortBy = "FundName",
	//		bool desc = false,
	//		CancellationToken ct = default);

	//	Task<Fund?> GetByIdAsync(int id, CancellationToken ct = default);
	//	Task<int> AddAsync(Fund entity, CancellationToken ct = default);
	//	Task UpdateAsync(Fund entity, CancellationToken ct = default);
	//	Task DeleteAsync(int id, CancellationToken ct = default);
	//}

	//public class MFFundRepo : IMFFundRepo
	//{
	//	private readonly MFPTrackerDbContext _db;
	//	public MFFundRepo(MFPTrackerDbContext db) => _db = db;

	//	public async Task<(IReadOnlyList<Fund> Items, int TotalCount)> GetAsync(
	//		string? nameContains = null,
	//		string? amcContains = null,
	//		string? codeContains = null,
	//		string? isinContains = null,
	//		int? amcId = null,
	//		int? mfcCatId = null,
	//		bool? onlyTxnAllowed = null,
	//		bool? onlyNavAllowed = null,
	//		int skip = 0,
	//		int take = 25,
	//		string sortBy = "FundName",
	//		bool desc = false,
	//		CancellationToken ct = default)
	//	{
	//		IQueryable<Fund> q = _db.Set<Fund>().AsNoTracking().Include(f => f.AMC).Include(f => f.Category);

	//		if (!string.IsNullOrWhiteSpace(nameContains))
	//		{
	//			var pattern = $"%{nameContains}%";
	//			q = q.Where(f => EF.Functions.Like(EF.Functions.Collate(f.FundName, "NOCASE"), pattern));
	//		}
	//		//if (!string.IsNullOrWhiteSpace(amcContains))
	//		//{
	//		//	var pattern = $"%{amcContains}%";
	//		//	q = q.Where(f => EF.Functions.Like(EF.Functions.Collate(f.AMCName, "NOCASE"), pattern));
	//		//}
	//		if (!string.IsNullOrWhiteSpace(codeContains))
	//		{
	//			var pattern = $"%{codeContains}%";
	//			q = q.Where(f => EF.Functions.Like(EF.Functions.Collate(f.SchemeCode, "NOCASE"), pattern));
	//		}
	//		if (!string.IsNullOrWhiteSpace(isinContains))
	//		{
	//			var pattern = $"%{isinContains}%";
	//			q = q.Where(f => EF.Functions.Like(EF.Functions.Collate(f.ISIN, "NOCASE"), pattern));
	//		}

	//		if (amcId is int aid && aid > 0) q = q.Where(f => f.AMCId == aid);
	//		if (mfcCatId is int cid && cid > 0) q = q.Where(f => f.MFCatId == cid);
	//		if (onlyTxnAllowed is true) q = q.Where(f => f.IsTransactionAllowed);
	//		if (onlyNavAllowed is true) q = q.Where(f => f.IsNavAllowed);

	//		var total = await q.CountAsync(ct);

	//		q = (sortBy, desc) switch
	//		{
	//			("FundName", true) => q.OrderByDescending(f => EF.Functions.Collate(f.FundName, "NOCASE")).ThenBy(f => f.FundId),
	//			("FundName", false) => q.OrderBy(f => EF.Functions.Collate(f.FundName, "NOCASE")).ThenBy(f => f.FundId),

	//			//("AMCName", true) => q.OrderByDescending(f => EF.Functions.Collate(f.AMCName, "NOCASE")).ThenBy(f => f.FundId),
	//			//("AMCName", false) => q.OrderBy(f => EF.Functions.Collate(f.AMCName, "NOCASE")).ThenBy(f => f.FundId),

	//			("SchemeCode", true) => q.OrderByDescending(f => EF.Functions.Collate(f.SchemeCode, "NOCASE")).ThenBy(f => f.FundId),
	//			("SchemeCode", false) => q.OrderBy(f => EF.Functions.Collate(f.SchemeCode, "NOCASE")).ThenBy(f => f.FundId),

	//			("ISIN", true) => q.OrderByDescending(f => EF.Functions.Collate(f.ISIN, "NOCASE")).ThenBy(f => f.FundId),
	//			("ISIN", false) => q.OrderBy(f => EF.Functions.Collate(f.ISIN, "NOCASE")).ThenBy(f => f.FundId),

	//			("Category", true) => q.OrderByDescending(f => EF.Functions.Collate(f.Category!.CategoryName, "NOCASE")).ThenBy(f => f.FundId),
	//			("Category", false) => q.OrderBy(f => EF.Functions.Collate(f.Category!.CategoryName, "NOCASE")).ThenBy(f => f.FundId),

	//			_ => q.OrderBy(f => EF.Functions.Collate(f.FundName, "NOCASE")).ThenBy(f => f.FundId),
	//		};

	//		var items = await q.Skip(Math.Max(0, skip)).Take(Math.Max(1, take)).ToListAsync(ct);
	//		return (items, total);
	//	}

	//	public Task<Fund?> GetByIdAsync(int id, CancellationToken ct = default) =>
	//		_db.Set<Fund>().AsNoTracking().Include(f => f.AMC).Include(f => f.Category).FirstOrDefaultAsync(f => f.FundId == id, ct);

	//	public async Task<int> AddAsync(Fund entity, CancellationToken ct = default)
	//	{
	//		Normalize(entity);
	//		Validate(entity);

	//		await EnsureUniqueAsync(entity, false, ct);

	//		var now = DateTime.UtcNow;
	//		entity.InDate = now;
	//		entity.UpdateDate = now;

	//		_db.Set<Fund>().Add(entity);
	//		await _db.SaveChangesAsync(ct);
	//		return entity.FundId;
	//	}

	//	public async Task UpdateAsync(Fund entity, CancellationToken ct = default)
	//	{
	//		var existing = await _db.Set<Fund>().FirstOrDefaultAsync(f => f.FundId == entity.FundId, ct);
	//		if (existing is null) return;

	//		Normalize(entity);
	//		Validate(entity);
	//		await EnsureUniqueAsync(entity, true, ct);

	//		existing.FundName = entity.FundName;
	//		existing.SchemeCode = entity.SchemeCode;
	//		existing.ISIN = entity.ISIN;
	//		//existing.AMCName = entity.AMCName;
	//		existing.IsTransactionAllowed = entity.IsTransactionAllowed;
	//		existing.IsNavAllowed = entity.IsNavAllowed;
	//		existing.AMCId = entity.AMCId;
	//		existing.MFCatId = entity.MFCatId;
	//		existing.UpdateDate = DateTime.UtcNow;

	//		await _db.SaveChangesAsync(ct);
	//	}

	//	public async Task DeleteAsync(int id, CancellationToken ct = default)
	//	{
	//		// Guard: prevent delete if transactions reference this fund
	//		var hasTxns = await _db.Set<MutualFundTransaction>().AsNoTracking().AnyAsync(t => t.FundId == id, ct);
	//		if (hasTxns) throw new InvalidOperationException("Cannot delete fund: transactions exist.");

	//		var row = await _db.Set<Fund>().FindAsync(new object?[] { id }, ct);
	//		if (row is null) return;

	//		_db.Set<Fund>().Remove(row);
	//		await _db.SaveChangesAsync(ct);
	//	}

	//	// Helpers
	//	private static void Normalize(Fund e)
	//	{
	//		e.FundName = (e.FundName ?? "").Trim();
	//		e.SchemeCode = (e.SchemeCode ?? "").Trim();
	//		e.ISIN = (e.ISIN ?? "").Trim();
	//		//e.AMCName = (e.AMCName ?? "").Trim();
	//	}

	//	private static void Validate(Fund e)
	//	{
	//		if (string.IsNullOrWhiteSpace(e.FundName)) throw new ArgumentException("Fund Name is required.", nameof(e.FundName));
	//		if (string.IsNullOrWhiteSpace(e.SchemeCode)) throw new ArgumentException("Scheme Code is required.", nameof(e.SchemeCode));
	//		if (string.IsNullOrWhiteSpace(e.ISIN)) throw new ArgumentException("ISIN is required.", nameof(e.ISIN));
	//		//if (string.IsNullOrWhiteSpace(e.AMCName)) throw new ArgumentException("AMC Name is required.", nameof(e.AMCName));
	//		if (e.AMCId <= 0) throw new ArgumentException("AMC is required.", nameof(e.AMCId));
	//		if (e.MFCatId <= 0) throw new ArgumentException("Category is required.", nameof(e.MFCatId));
	//	}

	//	private async Task EnsureUniqueAsync(Fund e, bool isUpdate, CancellationToken ct)
	//	{
	//		var q = _db.Set<Fund>().AsNoTracking().Where(x => !isUpdate || x.FundId != e.FundId);

	//		var dupName = await q.AnyAsync(x => EF.Functions.Collate(x.FundName, "NOCASE") == e.FundName, ct);
	//		if (dupName) throw new InvalidOperationException("Fund Name already exists.");

	//		var dupScheme = await q.AnyAsync(x => EF.Functions.Collate(x.SchemeCode, "NOCASE") == e.SchemeCode, ct);
	//		if (dupScheme) throw new InvalidOperationException("Scheme Code already exists.");

	//		var dupIsin = await q.AnyAsync(x => EF.Functions.Collate(x.ISIN, "NOCASE") == e.ISIN, ct);
	//		if (dupIsin) throw new InvalidOperationException("ISIN already exists.");
	//	}
	//}

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

	}
}