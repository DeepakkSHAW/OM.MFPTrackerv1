using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data;
using OM.MFPTrackerV1.Data.Models;
using System.Linq;

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
		Task<TransactionImportSummary> ImportFolioTransactionsAsync(int amcId, TransactionType transactionType, IEnumerable<FolioTransactionPreviewRow> rows);
		Task<Dictionary<int, decimal>> GetTotalInvestmentByFundAsync(CancellationToken ct = default);
		Task<Dictionary<int, decimal>> GetTotalInvestmentByFolioAsync(CancellationToken ct = default);
		Task<List<FolioHolder>> GetHoldersForTransactionsAsync(int? folioId, int? fundId, int? amcId, int? categoryId);
		Task<List<BubblePointDto>> GetBubbleDataAsync(TransactionFilter filter);
		Task<List<BubblePointDto>> GetTransactionExplorerAsync(TransactionExplorerFilter filter);
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

		public async Task<TransactionImportSummary> ImportFolioTransactionsAsync(int amcId, TransactionType transactionType, IEnumerable<FolioTransactionPreviewRow> rows)
		{
			int inserted = 0;
			int skippedDuplicate = 0;
			int invalid = 0;

			// Load DB reference data ONCE
			var folios = await _db.Folios
				.Where(f => f.AMCId == amcId)
				.ToDictionaryAsync(f => f.FolioNumber);

			var funds = await _db.Funds
				.Where(f => f.AMCId == amcId)
				.ToDictionaryAsync(f => f.ISIN ?? f.SchemeCode!);

			// Load existing transaction keys for duplicate protection
			var existingKeys = new HashSet<(string Folio, int FundId, DateTime Date, TransactionType Type, decimal Units)>(
				await _db.MutualFundTransactions
					.Where(t => t.Folio.AMCId == amcId)
					.Select(t => new
					{
						t.Folio.FolioNumber,
						t.FundId,
						t.TransactionDate,
						t.TxnType,
						t.Units
					})
					.AsNoTracking()
					.ToListAsync()
					.ContinueWith(t => t.Result.Select(x =>
						(x.FolioNumber, x.FundId, x.TransactionDate, x.TxnType, x.Units)))
			);

			foreach (var r in rows.Where(r => r.Status == PreviewRowStatus.Valid))
			{
				// Defensive lookup
				if (!folios.TryGetValue(r.FolioNumber, out var folio) ||
					!funds.TryGetValue(r.FundCode, out var fund))
				{
					invalid++;
					continue;
				}

				// Final AMC safety check
				if (folio.AMCId != fund.AMCId)
				{
					invalid++;
					continue;
				}

				var key = (
					r.FolioNumber,
					fund.FundId,
					r.TransactionDate.Date,
					transactionType,
					r.Units
				);

				if (existingKeys.Contains(key))
				{
					skippedDuplicate++;
					continue;
				}

				_db.MutualFundTransactions.Add(new MutualFundTransaction
				{
					FolioId = folio.FolioId,
					FundId = fund.FundId,
					TransactionDate = r.TransactionDate.Date,
					TxnType = transactionType,
					Units = r.Units,
					NAV = r.Nav,
					AmountPaid = r.Amount,
					Source = string.IsNullOrWhiteSpace(r.Source) ? "CSV" : r.Source,
					InDate = DateTime.UtcNow,

					// ✅ NEW
					ReferenceNo = r.ReferenceNo,
					Note = r.Note
				});

				existingKeys.Add(key);
				inserted++;
			}

			if (inserted > 0)
				await _db.SaveChangesAsync();

			return new TransactionImportSummary
			{
				Inserted = inserted,
				SkippedAsDuplicate = skippedDuplicate,
				InvalidRows = invalid
			};
		}
		public async Task<Dictionary<int, decimal>> GetTotalInvestmentByFundAsync(CancellationToken ct = default)
		{
			var result =
				await _db.MutualFundTransactions
					.Where(tx =>
						tx.TxnType == TransactionType.BUY ||
						tx.TxnType == TransactionType.SIP ||
						tx.TxnType == TransactionType.SWITCH_IN ||
						tx.TxnType == TransactionType.DIV_REINVEST)
					.GroupBy(tx => tx.FundId)
					.Select(g => new
					{
						FundId = g.Key,
						Total = g.Sum(x => x.AmountPaid)
					})
					.ToDictionaryAsync(
						x => x.FundId,
						x => x.Total,
						ct);

			return result;
		}
		public async Task<Dictionary<int, decimal>> GetTotalInvestmentByFolioAsync(CancellationToken ct = default)
		{
			return await _db.MutualFundTransactions
				.Where(t =>
					t.TxnType == TransactionType.BUY ||
					t.TxnType == TransactionType.SIP ||
					t.TxnType == TransactionType.SWITCH_IN ||
					t.TxnType == TransactionType.DIV_REINVEST)
				.GroupBy(t => t.FolioId)
				.Select(g => new
				{
					FolioId = g.Key,
					Total = g.Sum(x => x.AmountPaid)
				})
				.ToDictionaryAsync(x => x.FolioId, x => x.Total, ct);
		}
		public async Task<List<FolioHolder>> GetHoldersForTransactionsAsync(
			int? folioId,
			int? fundId,
			int? amcId,
			int? categoryId)
		{
			var q = _db.MutualFundTransactions
				.Include(t => t.Folio)
					.ThenInclude(f => f.Holder)
				.Include(t => t.Fund)
				.AsNoTracking()
				.AsQueryable();

			if (folioId.HasValue)
				q = q.Where(t => t.FolioId == folioId);

			if (fundId.HasValue)
				q = q.Where(t => t.FundId == fundId);

			if (amcId.HasValue)
				q = q.Where(t => t.Fund.AMCId == amcId);

			if (categoryId.HasValue)
				q = q.Where(t => t.Fund.MFCatId == categoryId);

			return await q
                .Select(t => t.Folio.Holder)
				.Distinct()
				.OrderBy(h => h.FirstName)
				.ToListAsync();
		}

		public async Task<List<BubblePointDto>> GetBubbleDataAsync(TransactionFilter filter)
		{
			var q = _db.MutualFundTransactions
				.Include(t => t.Fund)
				.Include(t => t.Folio)
					.ThenInclude(f => f.Holder)
				.AsNoTracking()
				.AsQueryable();

			if (filter.StartDate.HasValue)
				q = q.Where(t => t.TransactionDate >= filter.StartDate);

			if (filter.EndDate.HasValue)
				q = q.Where(t => t.TransactionDate <= filter.EndDate);

			if (filter.CategoryId.HasValue)
				q = q.Where(t => t.Fund.MFCatId == filter.CategoryId);

			if (filter.AMCId.HasValue)
				q = q.Where(t => t.Fund.AMCId == filter.AMCId);

			if (filter.FundId.HasValue)
				q = q.Where(t => t.FundId == filter.FundId);

			if (filter.FolioId.HasValue)
				q = q.Where(t => t.FolioId == filter.FolioId);

			if (filter.HolderId.HasValue)
				q = q.Where(t => t.Folio.FolioHolderId == filter.HolderId);

			return await q
				.GroupBy(t => new
				{
					Date = t.TransactionDate.Date,
					t.FundId,
					t.Fund.FundName,
					HolderId = t.Folio.FolioHolderId,
					Holder = t.Folio.Holder.FirstName + " " + t.Folio.Holder.LastName
				})
				.Select(g => new BubblePointDto
				{
					Date = g.Key.Date,
					FundId = g.Key.FundId,
					FundName = g.Key.FundName,
					HolderId = g.Key.HolderId,
					HolderName = g.Key.Holder,
					TotalInvestment = g.Sum(x => x.AmountPaid),
					Units = g.Sum(x => x.Units),
					Nav = g.Average(x => x.NAV)
				})
				.ToListAsync();
		}

		public async Task<List<BubblePointDto>> GetTransactionExplorerAsync(	TransactionExplorerFilter filter)
		{
			// Base query (no cascade assumptions)
			var query = _db.MutualFundTransactions
				.AsNoTracking()
				.Include(t => t.Fund)
					.ThenInclude(f => f.Category)
				.Include(t => t.Fund)
					.ThenInclude(f => f.AMC)
				.Include(t => t.Folio)
					.ThenInclude(f => f.Holder)
				.AsQueryable();

			/* ===========================
			   DATE RANGE
			   =========================== */
			if (filter.StartDate.HasValue)
				query = query.Where(t =>
					t.TransactionDate.Date >= filter.StartDate.Value.Date);

			if (filter.EndDate.HasValue)
				query = query.Where(t =>
					t.TransactionDate.Date <= filter.EndDate.Value.Date);

			/* ===========================
			   STRUCTURAL FILTERS
			   (ALL OPTIONAL & INDEPENDENT)
			   =========================== */
			if (filter.CategoryId.HasValue)
				query = query.Where(t =>
					t.Fund.Category.MFCatId == filter.CategoryId.Value);

			if (filter.AmcId.HasValue)
				query = query.Where(t =>
					t.Fund.AMC.AMCId == filter.AmcId.Value);

			if (filter.FundId.HasValue)
				query = query.Where(t =>
					t.FundId == filter.FundId.Value);

			if (filter.FolioId.HasValue)
				query = query.Where(t =>
					t.FolioId == filter.FolioId.Value);

			if (filter.HolderId.HasValue)
				query = query.Where(t =>
					t.Folio.Holder.FolioHolderId == filter.HolderId.Value);

			if (filter.TxnType.HasValue)
				query = query.Where(t =>
					t.TxnType == filter.TxnType.Value);

			/* ===========================
			   FREE TEXT SEARCH (EXCEL STYLE)
			   =========================== */
			if (!string.IsNullOrWhiteSpace(filter.FreeText))
			{
				var ft = filter.FreeText.Trim();

				query = query.Where(t =>
					(t.ReferenceNo != null && t.ReferenceNo.Contains(ft)) ||
					(t.Source != null && t.Source.Contains(ft)) ||
					(t.Note != null && t.Note.Contains(ft))
				);
			}

			/* ===========================
			   AGGREGATION (BUBBLE‑READY)
			   =========================== */
			var result = await query
				.GroupBy(t => new
				{
					Date = t.TransactionDate.Date,
					t.FundId,
					t.Fund.FundName,
					HolderId = t.Folio.Holder.FolioHolderId,
					HolderName =
						t.Folio.Holder.FirstName + " " +
						t.Folio.Holder.LastName
				})
				.Select(g => new BubblePointDto
				{
					Date = g.Key.Date,
					Nav = g.Average(x => x.NAV),
					Units = g.Sum(x => x.Units),
					TotalInvestment = g.Sum(x => x.AmountPaid),

					FundId = g.Key.FundId,
					HolderId = g.Key.HolderId,
					FundName = g.Key.FundName,
					HolderName = g.Key.HolderName
				})
				.OrderBy(x => x.Date)
				.ToListAsync();

			return result;
		}
	}
}
