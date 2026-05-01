using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data.Models;


namespace OM.MFPTrackerV1.Data.Services
{
	public interface IAMCRepo
	{
		Task<(IReadOnlyList<AMC> Items, int TotalCount)> GetAsync(
			string? nameContains = null,
			int skip = 0,
			int take = 25,
			string sortBy = "AMCName",
			bool desc = false,
			CancellationToken ct = default);
		Task<IReadOnlyList<AMC>> GetAllAsync();
		Task<AMC?> GetByIdAsync(int id, CancellationToken ct = default);
		Task<int> AddAsync(AMC entity, CancellationToken ct = default);
		Task UpdateAsync(AMC entity, CancellationToken ct = default);
		Task DeleteAsync(int id, CancellationToken ct = default);
		Task<Dictionary<int, int>> GetFundCountsAsync(CancellationToken ct = default);
		Task<Dictionary<int, int>> GetFolioCountsAsync(CancellationToken ct = default);
		Task<Dictionary<int, decimal>> GetTotalInvestmentByAmcAsync(CancellationToken ct = default);

	}

	public class AMCRepo : IAMCRepo
	{
		private readonly MFPTrackerDbContext _db;
		public AMCRepo(MFPTrackerDbContext db) => _db = db;

		public async Task<(IReadOnlyList<AMC> Items, int TotalCount)> GetAsync(
			string? nameContains = null,
			int skip = 0,
			int take = 25,
			string sortBy = "AMCName",
			bool desc = false,
			CancellationToken ct = default)
		{
			var q = _db.Set<AMC>().AsNoTracking();

			if (!string.IsNullOrWhiteSpace(nameContains))
			{
				var pattern = $"%{nameContains}%";
				q = q.Where(x => EF.Functions.Like(EF.Functions.Collate(x.AMCName, "NOCASE"), pattern));
			}

			var total = await q.CountAsync(ct);

			q = (sortBy, desc) switch
			{
				("AMCName", true) => q.OrderByDescending(x => EF.Functions.Collate(x.AMCName, "NOCASE")).ThenBy(x => x.AMCId),
				_ => q.OrderBy(x => EF.Functions.Collate(x.AMCName, "NOCASE")).ThenBy(x => x.AMCId),
			};

			var items = await q.Skip(Math.Max(0, skip)).Take(Math.Max(1, take)).ToListAsync(ct);
			return (items, total);
		}

		public async Task<IReadOnlyList<AMC>> GetAllAsync()
		{
			return await _db.Set<AMC>()
				.AsNoTracking()
				.OrderBy(x => x.AMCName)
				.ToListAsync();
		}

		public Task<AMC?> GetByIdAsync(int id, CancellationToken ct = default) =>
			_db.Set<AMC>().AsNoTracking().FirstOrDefaultAsync(x => x.AMCId == id, ct);

		public async Task<int> AddAsync(AMC entity, CancellationToken ct = default)
		{
			entity.AMCName = (entity.AMCName ?? "").Trim();
			if (string.IsNullOrWhiteSpace(entity.AMCName))
				throw new ArgumentException("AMC Name is required.", nameof(entity.AMCName));

			var dup = await _db.Set<AMC>().AsNoTracking()
				.AnyAsync(x => EF.Functions.Collate(x.AMCName, "NOCASE") == entity.AMCName, ct);
			if (dup) throw new InvalidOperationException("AMC already exists. Please choose a unique name.");

			_db.Set<AMC>().Add(entity);
			await _db.SaveChangesAsync(ct);
			return entity.AMCId;
		}

		public async Task UpdateAsync(AMC entity, CancellationToken ct = default)
		{
			var existing = await _db.Set<AMC>().FirstOrDefaultAsync(x => x.AMCId == entity.AMCId, ct);
			if (existing is null) return;

			var name = (entity.AMCName ?? "").Trim();
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("AMC Name is required.", nameof(entity.AMCName));

			var dup = await _db.Set<AMC>().AsNoTracking()
				.AnyAsync(x => x.AMCId != entity.AMCId && EF.Functions.Collate(x.AMCName, "NOCASE") == name, ct);
			if (dup) throw new InvalidOperationException("AMC already exists. Please choose a unique name.");

			existing.AMCName = name;
			// audit not needed; table has no UpdateDate. If you add, set existing.UpdateDate = DateTime.UtcNow;

			await _db.SaveChangesAsync(ct);
		}

		public async Task DeleteAsync(int id, CancellationToken ct = default)
		{
			// Guard: prevent deleting if Funds or Folios exist
			var hasFunds = await _db.Set<Fund>().AsNoTracking().AnyAsync(f => f.AMCId == id, ct);
			var hasFolios = await _db.Set<Folio>().AsNoTracking().AnyAsync(f => f.AMCId == id, ct);
			if (hasFunds || hasFolios)
				throw new InvalidOperationException("Cannot delete AMC because it has linked Funds or Folios.");

			var row = await _db.Set<AMC>().FindAsync(new object?[] { id }, ct);
			if (row is null) return;
			_db.Set<AMC>().Remove(row);
			await _db.SaveChangesAsync(ct);
		}

		public async Task<Dictionary<int, int>> GetFundCountsAsync(CancellationToken ct = default)
		{
			return await _db.Set<Fund>()
							.AsNoTracking()
							.GroupBy(f => f.AMCId)
							.Select(g => new { g.Key, Cnt = g.Count() })
							.ToDictionaryAsync(x => x.Key, x => x.Cnt, ct);
		}

		public async Task<Dictionary<int, int>> GetFolioCountsAsync(CancellationToken ct = default)
		{
			return await _db.Set<Folio>()
				 .AsNoTracking()
				 .GroupBy(f => f.AMCId)
				 .Select(g => new { g.Key, Cnt = g.Count() })
				 .ToDictionaryAsync(x => x.Key, x => x.Cnt, ct);
		}
		public async Task<Dictionary<int, decimal>> GetTotalInvestmentByAmcAsync(CancellationToken ct = default)
		{
			var result =
				await (from tx in _db.MutualFundTransactions
					   join fund in _db.Funds
						   on tx.FundId equals fund.FundId
					   join amc in _db.AMCs
						   on fund.AMCId equals amc.AMCId
					   where
						   tx.TxnType == TransactionType.BUY ||
						   tx.TxnType == TransactionType.SIP ||
						   tx.TxnType == TransactionType.SWITCH_IN ||
						   tx.TxnType == TransactionType.DIV_REINVEST
					   group tx by amc.AMCId into g
					   select new
					   {
						   AmcId = g.Key,
						   Total = g.Sum(x => x.AmountPaid)
					   })
				.ToDictionaryAsync(
					x => x.AmcId,
					x => x.Total,
					ct);

			return result;
		}
	}
}