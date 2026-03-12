using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data;
using OM.MFPTrackerV1.Data.Models;

namespace OM.MFPTrackerV1.Data.Services
{
	public interface IMFTransactionRepo
	{
		Task<(IReadOnlyList<MutualFundTransaction> Items, int TotalCount)> GetAsync(
			DateTime? from = null,
			DateTime? to = null,
			int? folioId = null,
			int? fundId = null,
			string? folioNumberContains = null,
			string? fundNameContains = null,
			TransactionType? txnType = null,
			string? sourceContains = null,
			decimal? minAmount = null,
			decimal? maxAmount = null,
			int skip = 0,
			int take = 25,
			string sortBy = "Date",           // Date | Amount | Units | NAV | Fund | Folio
			bool desc = true,
			CancellationToken ct = default);

		Task<MutualFundTransaction?> GetByIdAsync(int id, CancellationToken ct = default);
		Task<int> AddAsync(MutualFundTransaction entity, CancellationToken ct = default);
		Task UpdateAsync(MutualFundTransaction entity, CancellationToken ct = default);
		Task DeleteAsync(int id, CancellationToken ct = default);
	}

	public class MFTransactionRepo : IMFTransactionRepo
	{
		private readonly MFPTrackerDbContext _db;
		public MFTransactionRepo(MFPTrackerDbContext db) => _db = db;

		public async Task<(IReadOnlyList<MutualFundTransaction> Items, int TotalCount)> GetAsync(
			DateTime? from = null,
			DateTime? to = null,
			int? folioId = null,
			int? fundId = null,
			string? folioNumberContains = null,
			string? fundNameContains = null,
			TransactionType? txnType = null,
			string? sourceContains = null,
			decimal? minAmount = null,
			decimal? maxAmount = null,
			int skip = 0,
			int take = 25,
			string sortBy = "Date",
			bool desc = true,
			CancellationToken ct = default)
		{
			IQueryable<MutualFundTransaction> q = _db.Set<MutualFundTransaction>().AsNoTracking().Include(t => t.Folio).Include(t => t.Fund);

			if (from is DateTime f) q = q.Where(t => t.Date >= f);
			if (to is DateTime tEnd) q = q.Where(t => t.Date <= tEnd);

			if (folioId is int fid && fid > 0) q = q.Where(t => t.FolioId == fid);
			if (fundId is int fu && fu > 0) q = q.Where(t => t.FundId == fu);

			if (!string.IsNullOrWhiteSpace(folioNumberContains))
			{
				var pattern = $"%{folioNumberContains}%";
				q = q.Where(t => t.Folio != null &&
								 EF.Functions.Like(EF.Functions.Collate(t.Folio.FolioNumber, "NOCASE"), pattern));
			}

			if (!string.IsNullOrWhiteSpace(fundNameContains))
			{
				var pattern = $"%{fundNameContains}%";
				q = q.Where(t => t.Fund != null &&
								 EF.Functions.Like(EF.Functions.Collate(t.Fund.FundName, "NOCASE"), pattern));
			}

			if (txnType.HasValue) q = q.Where(t => t.TxnType == txnType.Value);

			if (!string.IsNullOrWhiteSpace(sourceContains))
			{
				var pattern = $"%{sourceContains}%";
				q = q.Where(t => t.Source != null && EF.Functions.Like(EF.Functions.Collate(t.Source, "NOCASE"), pattern));
			}

			if (minAmount.HasValue) q = q.Where(t => t.AmountPaid >= minAmount.Value);
			if (maxAmount.HasValue) q = q.Where(t => t.AmountPaid <= maxAmount.Value);

			var total = await q.CountAsync(ct);

			q = (sortBy, desc) switch
			{
				("Date", true) => q.OrderByDescending(t => t.Date).ThenBy(t => t.Id),
				("Date", false) => q.OrderBy(t => t.Date).ThenBy(t => t.Id),

				("Amount", true) => q.OrderByDescending(t => t.AmountPaid).ThenBy(t => t.Id),
				("Amount", false) => q.OrderBy(t => t.AmountPaid).ThenBy(t => t.Id),

				("Units", true) => q.OrderByDescending(t => t.Units).ThenBy(t => t.Id),
				("Units", false) => q.OrderBy(t => t.Units).ThenBy(t => t.Id),

				("NAV", true) => q.OrderByDescending(t => t.NAV).ThenBy(t => t.Id),
				("NAV", false) => q.OrderBy(t => t.NAV).ThenBy(t => t.Id),

				("Fund", true) => q.OrderByDescending(t => EF.Functions.Collate(t.Fund!.FundName, "NOCASE")).ThenBy(t => t.Id),
				("Fund", false) => q.OrderBy(t => EF.Functions.Collate(t.Fund!.FundName, "NOCASE")).ThenBy(t => t.Id),

				("Folio", true) => q.OrderByDescending(t => EF.Functions.Collate(t.Folio!.FolioNumber, "NOCASE")).ThenBy(t => t.Id),
				("Folio", false) => q.OrderBy(t => EF.Functions.Collate(t.Folio!.FolioNumber, "NOCASE")).ThenBy(t => t.Id),

				_ => q.OrderByDescending(t => t.Date).ThenBy(t => t.Id),
			};

			var items = await q.Skip(Math.Max(0, skip)).Take(Math.Max(1, take)).ToListAsync(ct);
			return (items, total);
		}

		public Task<MutualFundTransaction?> GetByIdAsync(int id, CancellationToken ct = default) =>
			_db.Set<MutualFundTransaction>().AsNoTracking()
			   .Include(t => t.Folio).Include(t => t.Fund)
			   .FirstOrDefaultAsync(t => t.Id == id, ct);

		public async Task<int> AddAsync(MutualFundTransaction e, CancellationToken ct = default)
		{
			Normalize(e);
			Validate(e);

			await EnforceAmcMatch(e, ct);

			e.AmountPaid = Math.Round(e.Units * e.NAV, 2, MidpointRounding.ToEven);
			var now = DateTime.UtcNow;
			e.InDate = now;
			e.UpdateDate = now;

			_db.Set<MutualFundTransaction>().Add(e);
			await _db.SaveChangesAsync(ct);
			return e.Id;
		}

		public async Task UpdateAsync(MutualFundTransaction e, CancellationToken ct = default)
		{
			var existing = await _db.Set<MutualFundTransaction>().FirstOrDefaultAsync(x => x.Id == e.Id, ct);
			if (existing is null) return;

			Normalize(e);
			Validate(e);

			// If FolioId or FundId changed, still enforce AMC match
			await EnforceAmcMatch(e, ct);

			existing.Date = e.Date;
			existing.FolioId = e.FolioId;
			existing.FundId = e.FundId;
			existing.TxnType = e.TxnType;
			existing.Units = e.Units;
			existing.NAV = e.NAV;
			existing.Source = e.Source;
			existing.Note = e.Note;

			existing.AmountPaid = Math.Round(existing.Units * existing.NAV, 2, MidpointRounding.ToEven);
			existing.UpdateDate = DateTime.UtcNow;

			await _db.SaveChangesAsync(ct);
		}

		public async Task DeleteAsync(int id, CancellationToken ct = default)
		{
			var row = await _db.Set<MutualFundTransaction>().FindAsync(new object?[] { id }, ct);
			if (row is null) return;
			_db.Set<MutualFundTransaction>().Remove(row);
			await _db.SaveChangesAsync(ct);
		}

		// Helpers
		private static void Normalize(MutualFundTransaction e)
		{
			e.Source = (e.Source ?? "").Trim();
			e.Note = (e.Note ?? "").Trim();
		}

		private static void Validate(MutualFundTransaction e)
		{
			if (e.FolioId <= 0) throw new ArgumentException("Folio is required.", nameof(e.FolioId));
			if (e.FundId <= 0) throw new ArgumentException("Fund is required.", nameof(e.FundId));
			if (e.Units <= 0) throw new ArgumentOutOfRangeException(nameof(e.Units), "Units must be greater than zero.");
			if (e.NAV <= 0) throw new ArgumentOutOfRangeException(nameof(e.NAV), "NAV must be greater than zero.");
		}

		private async Task EnforceAmcMatch(MutualFundTransaction e, CancellationToken ct)
		{
			var folio = await _db.Set<Folio>().AsNoTracking().FirstOrDefaultAsync(x => x.FolioId == e.FolioId, ct)
						?? throw new ArgumentException("Invalid Folio.", nameof(e.FolioId));
			var fund = await _db.Set<Fund>().AsNoTracking().FirstOrDefaultAsync(x => x.FundId == e.FundId, ct)
						?? throw new ArgumentException("Invalid Fund.", nameof(e.FundId));

			if (folio.AMCId != fund.AMCId)
				throw new InvalidOperationException("Folio AMC and Fund AMC do not match. Select a fund under the same AMC as the folio.");
		}
	}
}
