using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data;
using OM.MFPTrackerV1.Data.Models;

namespace OM.MFPTrackerV1.Data.Services
{
	public interface IMutualFundTransactionRepo
	{
		Task<(IReadOnlyList<MutualFundTransaction> Items, int TotalCount)> GetAsync(
			int? folioId,
			int? fundId,
			string? sortBy,
			bool sortDesc,
			int pageNumber,
			int pageSize);

		Task<MutualFundTransaction?> GetByIdAsync(int transactionId);

		Task<decimal> GetAvailableUnitsAsync(int folioId, int fundId);

		Task<MutualFundTransaction> AddAsync(MutualFundTransaction entity);
		Task<MutualFundTransaction> UpdateAsync(MutualFundTransaction entity);
		Task DeleteAsync(int transactionId);

		Task<int> CountAsync(int? folioId = null);
	}
	public sealed class MutualFundTransactionRepo : IMutualFundTransactionRepo
	{
		private readonly MFPTrackerDbContext _db;

		// ---------- Transaction classification ----------
		private static readonly TransactionType[] AddUnitTypes =
		{
		TransactionType.BUY,
		TransactionType.SIP,
		TransactionType.DIV_REINVEST,
		TransactionType.SWITCH_IN
	};

		private static readonly TransactionType[] SubtractUnitTypes =
		{
		TransactionType.SELL,
		TransactionType.SWITCH_OUT
	};

		public MutualFundTransactionRepo(MFPTrackerDbContext db)
		{
			_db = db;
		}

		// -------------------------------------------------
		// GET: Transaction list (paging + sorting)
		// -------------------------------------------------
		public async Task<(IReadOnlyList<MutualFundTransaction> Items, int TotalCount)> GetAsync(
			int? folioId,
			int? fundId,
			string? sortBy,
			bool sortDesc,
			int pageNumber,
			int pageSize)
		{
			if (pageNumber < 1) pageNumber = 1;
			if (pageSize < 1) pageSize = 10;
			if (pageSize > 200) pageSize = 200;

			IQueryable<MutualFundTransaction> q = _db.Set<MutualFundTransaction>()
				.Include(x => x.Folio)
				.Include(x => x.Fund)
				.AsNoTracking();

			if (folioId.HasValue)
				q = q.Where(x => x.FolioId == folioId.Value);

			if (fundId.HasValue)
				q = q.Where(x => x.FundId == fundId.Value);

			var key = (sortBy ?? nameof(MutualFundTransaction.TransactionDate)).Trim();

			q = (key, sortDesc) switch
			{
				(nameof(MutualFundTransaction.TransactionDate), false) =>
					q.OrderBy(x => x.TransactionDate)
					 .ThenBy(x => x.TransactionId),

				(nameof(MutualFundTransaction.TransactionDate), true) =>
					q.OrderByDescending(x => x.TransactionDate)
					 .ThenBy(x => x.TransactionId),

				(nameof(MutualFundTransaction.TxnType), false) =>
					q.OrderBy(x => x.TxnType)
					 .ThenBy(x => x.TransactionDate),

				(nameof(MutualFundTransaction.TxnType), true) =>
					q.OrderByDescending(x => x.TxnType)
					 .ThenBy(x => x.TransactionDate),

				_ =>
					q.OrderByDescending(x => x.TransactionDate)
			};

			var total = await q.CountAsync();

			var items = await q
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (items, total);
		}

		// -------------------------------------------------
		public Task<int> CountAsync(int? folioId = null)
		{
			var q = _db.Set<MutualFundTransaction>().AsQueryable();
			if (folioId.HasValue)
				q = q.Where(x => x.FolioId == folioId.Value);

			return q.CountAsync();
		}

		// -------------------------------------------------
		public async Task<MutualFundTransaction?> GetByIdAsync(int transactionId)
		{
			return await _db.Set<MutualFundTransaction>()
				.Include(x => x.Folio)
				.Include(x => x.Fund)
				.FirstOrDefaultAsync(x => x.TransactionId == transactionId);
		}

		// -------------------------------------------------
		// Holdings calculation (CRITICAL)
		// -------------------------------------------------
		public async Task<decimal> GetAvailableUnitsAsync(int folioId, int fundId)
		{
			var txns = await _db.Set<MutualFundTransaction>()
				.Where(x => x.FolioId == folioId && x.FundId == fundId)
				.AsNoTracking()
				.ToListAsync();

			var addedUnits = txns
				.Where(x => AddUnitTypes.Contains(x.TxnType))
				.Sum(x => x.Units);

			var removedUnits = txns
				.Where(x => SubtractUnitTypes.Contains(x.TxnType))
				.Sum(x => x.Units);

			return addedUnits - removedUnits;
		}

		// -------------------------------------------------
		// ADD (BUY / SELL / SIP / SWITCH / DIV)
		// -------------------------------------------------
		public async Task<MutualFundTransaction> AddAsync(MutualFundTransaction entity)
		{
			ValidateTransaction(entity);

			if (SubtractUnitTypes.Contains(entity.TxnType))
			{
				var availableUnits =
					await GetAvailableUnitsAsync(entity.FolioId, entity.FundId);

				if (availableUnits <= 0)
					throw new InvalidOperationException("No units available to sell.");

				if (entity.Units > availableUnits)
					throw new InvalidOperationException(
						$"Cannot sell {entity.Units}. Available units: {availableUnits}");
			}

			// Compute AmountPaid centrally (never trust UI)
			entity.AmountPaid = Math.Round(
				entity.Units * entity.NAV,
				2,
				MidpointRounding.AwayFromZero);

			entity.InDate = DateTime.UtcNow;
			entity.UpdateDate = DateTime.UtcNow;

			_db.Set<MutualFundTransaction>().Add(entity);
			await _db.SaveChangesAsync();

			return entity;
		}

		// -------------------------------------------------
		// UPDATE (restricted but supported)
		// -------------------------------------------------
		public async Task<MutualFundTransaction> UpdateAsync(MutualFundTransaction entity)
		{
			var existing = await _db.Set<MutualFundTransaction>()
				.FirstOrDefaultAsync(x => x.TransactionId == entity.TransactionId);

			if (existing == null)
				throw new InvalidOperationException("Transaction not found.");

			ValidateTransaction(entity);

			existing.TransactionDate = entity.TransactionDate;
			existing.TxnType = entity.TxnType;
			existing.Units = entity.Units;
			existing.NAV = entity.NAV;
			existing.ReferenceNo = entity.ReferenceNo;
			existing.Source = entity.Source;
			existing.Note = entity.Note;

			existing.AmountPaid = Math.Round(
				existing.Units * existing.NAV,
				2,
				MidpointRounding.AwayFromZero);

			existing.UpdateDate = DateTime.UtcNow;

			await _db.SaveChangesAsync();
			return existing;
		}

		// -------------------------------------------------
		public async Task DeleteAsync(int transactionId)
		{
			var existing = await _db.Set<MutualFundTransaction>()
				.FirstOrDefaultAsync(x => x.TransactionId == transactionId);

			if (existing == null)
				return;

			_db.Set<MutualFundTransaction>().Remove(existing);
			await _db.SaveChangesAsync();
		}

		// -------------------------------------------------
		// Domain validation (centralised & explicit)
		// -------------------------------------------------
		private static void ValidateTransaction(MutualFundTransaction txn)
		{
			if (txn.TransactionDate == default)
				throw new InvalidOperationException("Transaction date is required.");

			if (txn.TxnType == TransactionType.DIV_PAYOUT)
			{
				if (txn.Units != 0)
					throw new InvalidOperationException(
						"Dividend payout must not contain units.");
			}
			else
			{
				if (txn.Units <= 0)
					throw new InvalidOperationException(
						"Units must be greater than zero.");
			}

			if (txn.NAV < 0)
				throw new InvalidOperationException("NAV cannot be negative.");
		}
	}
}
