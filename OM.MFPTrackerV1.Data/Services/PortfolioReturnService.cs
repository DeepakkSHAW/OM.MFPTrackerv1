using OM.MFPTrackerV1.Data.Models;
using OM.MFPTrackerV1.Data.Helper;

namespace OM.MFPTrackerV1.Data.Services
{
	public interface IPortfolioReturnService
	{
		Task<PortfolioReturnResultDto> CalculatePortfolioReturnsAsync(int folioId, int fundId);
		Task<List<PortfolioRowDto>> CalculatePortfolioReturnsForAllAsync();
		Task<List<PortfolioRowDto>> CalculatePortfolioReturnsByFilterAsync(PortfolioAdvancedFilter filter);
	}
	public class PortfolioReturnService : IPortfolioReturnService
	{
		private readonly IMutualFundTransactionRepo _txnRepo;
		private readonly IFundNavRepo _navRepo;
		private readonly IFolioRepo _folioRepo;
		private readonly IMFFundRepo _fundRepo;


		public PortfolioReturnService(IMutualFundTransactionRepo txnRepo, IFundNavRepo navRepo, IFolioRepo folioRepo, IMFFundRepo fundRepo)
		{
			_txnRepo = txnRepo;
			_navRepo = navRepo;
			_folioRepo = folioRepo;
			_fundRepo = fundRepo;
		}

		public async Task<PortfolioReturnResultDto> CalculatePortfolioReturnsAsync(int folioId, int fundId)
		{
			// 1️⃣ Get ALL transactions (no paging)
			var (transactions, _) = await _txnRepo.GetAsync(
				folioId,
				fundId,
				sortBy: nameof(MutualFundTransaction.TransactionDate),
				sortDesc: false,
				pageNumber: 1,
				pageSize: int.MaxValue);

			var cashFlows = new List<PortfolioCashFlowDto>();
			var xirrFlows = new List<(DateTime date, decimal amount)>();

			decimal investedAmount = 0m;

			// ✅ For weighted avg days
			var investmentTxns = new List<MutualFundTransaction>();

			foreach (var t in transactions)
			{
				bool isInflow = t.TxnType is
					TransactionType.SELL or
					TransactionType.SWITCH_OUT or
					TransactionType.DIV_PAYOUT;

				var signedAmount = isInflow
					? t.AmountPaid
					: -t.AmountPaid;

				// ✅ Track invested amount (net)
				if (!isInflow)
				{
					investedAmount += t.AmountPaid;
					investmentTxns.Add(t);
				}
				else
				{
					investedAmount -= t.AmountPaid;
				}

				// ✅ Debug cashflows
				cashFlows.Add(new PortfolioCashFlowDto
				{
					TransactionId = t.TransactionId,
					Date = t.TransactionDate,
					Amount = signedAmount,
					TxnType = t.TxnType.ToString()
				});

				xirrFlows.Add((t.TransactionDate, signedAmount));
			}

			// 2️⃣ Get remaining units
			var units = await _txnRepo
				.GetAvailableUnitsAsync(folioId, fundId);

			decimal currentValue = 0m;
			DateTime valuationDate = DateTime.Today;

			if (units > 0)
			{
				// 3️⃣ Get latest NAV
				var navs = await _navRepo.GetNavHistoryAsync(
					fundId,
					DateTime.MinValue,
					DateTime.Today);

				var latestNav = navs
					.OrderByDescending(n => n.NavDate)
					.FirstOrDefault();

				if (latestNav != null)
				{
					valuationDate = latestNav.NavDate;

					currentValue = units * latestNav.NavValue;

					// ✅ Add synthetic final cashflow
					cashFlows.Add(new PortfolioCashFlowDto
					{
						TransactionId = null,
						Date = valuationDate,
						Amount = currentValue,
						TxnType = "CURRENT_VALUE"
					});

					xirrFlows.Add((valuationDate, currentValue));
				}
			}

			// 4️⃣ XIRR
			var xirr = XirrCalculator.Calculate(xirrFlows);

			// 5️⃣ Absolute Return
			var absoluteReturnAmount = currentValue - investedAmount;

			var absoluteReturnPct = investedAmount > 0
				? (absoluteReturnAmount / investedAmount) * 100
				: 0;

			// ✅ 6️⃣ Weighted Average Days
			decimal totalWeightedDays = 0m;

			foreach (var t in investmentTxns)
			{
				var daysHeld = (valuationDate - t.TransactionDate).TotalDays;

				if (daysHeld < 0)
					continue;

				totalWeightedDays += t.AmountPaid * (decimal)daysHeld;
			}

			decimal weightedAvgDays = investedAmount > 0
				? totalWeightedDays / investedAmount
				: 0;

			// ✅ 7️⃣ Return DTO
			return new PortfolioReturnResultDto
			{
				Xirr = xirr,
				AnnualizedReturn = xirr,

				InvestedAmount = Math.Round(investedAmount, 2),
				CurrentValue = Math.Round(currentValue, 2),

				AbsoluteReturnAmount = Math.Round(absoluteReturnAmount, 2),
				AbsoluteReturn = Math.Round(absoluteReturnPct, 2),

				WeightedAverageDays = Math.Round(weightedAvgDays, 0),

				CashFlows = cashFlows
			};
		}
		public async Task<List<PortfolioRowDto>> CalculatePortfolioReturnsForAllAsync()
		{
			var results = new List<PortfolioRowDto>();

			// 1️⃣ Get distinct (FolioId, FundId)
			var pairs = await _txnRepo.GetDistinctFolioFundPairsAsync();

			// 2️⃣ Load folios (for display)
			var folios = (await _folioRepo
				.GetAsync(null, null, false, 1, int.MaxValue))
				.Items
				.ToDictionary(f => f.FolioId);

			// 3️⃣ Load funds (for display)
			var funds = (await _fundRepo
				.GetAsync(null, null, false, 1, int.MaxValue))
				.Items
				.ToDictionary(f => f.FundId);

			// 4️⃣ Loop and reuse YOUR method
			foreach (var (folioId, fundId) in pairs)
			{
				var calc = await CalculatePortfolioReturnsAsync(folioId, fundId);

				var row = new PortfolioRowDto
				{
					FolioId = folioId,
					FundId = fundId,

					Xirr = calc.Xirr,
					InvestedAmount = calc.InvestedAmount,
					CurrentValue = calc.CurrentValue,
					AbsoluteReturn = calc.AbsoluteReturn,
					AbsoluteReturnAmount = calc.AbsoluteReturnAmount,
					WeightedAverageDays = calc.WeightedAverageDays
				};

				// ✅ Attach Folio Display
				if (folios.TryGetValue(folioId, out var folio))
				{
					row.FolioDisplay =
						$"{folio.FolioNumber} - {folio.Holder.FirstName} {folio.Holder.LastName}";
				}

				// ✅ Attach Fund Display
				if (funds.TryGetValue(fundId, out var fund))
				{
					row.FundDisplay = fund.FundName;
				}

				results.Add(row);
			}

			return results;
		}
		public async Task<List<PortfolioRowDto>> CalculatePortfolioReturnsByFilterAsync(PortfolioAdvancedFilter filter)
		{
			var results = new List<PortfolioRowDto>();

			// ✅ 1️⃣ Normalize dates (as per your requirement)
			var fromDate = filter.FromDate ?? new DateTime(1900, 1, 1);
			var toDate = filter.ToDate ?? DateTime.Today;

			// ✅ 2️⃣ Get ALL transactions (filtered at repo level where possible)
			var (transactions, _) = await _txnRepo.GetAsync(
				filter.FolioId,
				filter.FundId,
				sortBy: nameof(MutualFundTransaction.TransactionDate),
				sortDesc: false,
				pageNumber: 1,
				pageSize: int.MaxValue);

			// ✅ 3️⃣ Apply date filter
			var filtered = transactions
				.Where(t =>
					t.TransactionDate >= fromDate &&
					t.TransactionDate <= toDate);

			// ✅ 4️⃣ Apply free-text filter
			if (!string.IsNullOrWhiteSpace(filter.FreeText))
			{
				var txt = filter.FreeText.ToLower();

				filtered = filtered.Where(t =>
					(t.ReferenceNo != null && t.ReferenceNo.ToLower().Contains(txt)) ||
					(t.Source != null && t.Source.ToLower().Contains(txt)) ||
					(t.Note != null && t.Note.ToLower().Contains(txt))
				);
			}

			// ✅ 5️⃣ Group by (Folio, Fund)
			var groups = filtered
				.GroupBy(t => new { t.FolioId, t.FundId })
				.Select(g => new
				{
					g.Key.FolioId,
					g.Key.FundId
				})
				.ToList();

			if (!groups.Any())
				return results;

			// ✅ 6️⃣ Load display data
			var folios = (await _folioRepo
				.GetAsync(null, null, false, 1, int.MaxValue))
				.Items
				.ToDictionary(f => f.FolioId);

			var funds = (await _fundRepo
				.GetAsync(null, null, false, 1, int.MaxValue))
				.Items
				.ToDictionary(f => f.FundId);

			// ✅ 7️⃣ Reuse existing calculation method
			foreach (var g in groups)
			{
				var calc = await CalculatePortfolioReturnsAsync(g.FolioId, g.FundId);

				var row = new PortfolioRowDto
				{
					FolioId = g.FolioId,
					FundId = g.FundId,

					Xirr = calc.Xirr,
					InvestedAmount = calc.InvestedAmount,
					CurrentValue = calc.CurrentValue,
					AbsoluteReturn = calc.AbsoluteReturn,
					AbsoluteReturnAmount = calc.AbsoluteReturnAmount,
					WeightedAverageDays = calc.WeightedAverageDays
				};

				// ✅ Add display values
				if (folios.TryGetValue(g.FolioId, out var f))
				{
					row.FolioDisplay =
						$"{f.FolioNumber} - {f.Holder.FirstName} {f.Holder.LastName}";
				}

				if (funds.TryGetValue(g.FundId, out var fd))
				{
					row.FundDisplay = fd.FundName;
				}

				results.Add(row);
			}

			return results;
		}
	}
}
