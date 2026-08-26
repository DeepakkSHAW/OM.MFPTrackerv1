using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data;
using OM.MFPTrackerV1.Data.Helper;
using OM.MFPTrackerV1.Data.Models;
using System;
using System.Buffers.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OM.MFPTrackerV1.Data.Services
{
	public interface IPortfolioReturnService
	{
		Task<PortfolioReturnResultDto> CalculatePortfolioReturnsAsync(int folioId, int fundId);
		Task<List<PortfolioRowDto>> CalculatePortfolioReturnsForAllAsync();
		Task<decimal?> CalculateOverallXirrAsync();
		Task<List<MonthlyPortfolioValueDto>> GetLast12MonthPortfolioValueTrendAsync();
		Task<List<FundPerformanceTrendDto>> GetTopFundPerformanceTrendLast12MonthsAsync(int count = 3);
		Task<List<PortfolioRowDto>> CalculatePortfolioReturnsByFilterAsync(PortfolioAdvancedFilter filter);
	}
	public class PortfolioReturnService : IPortfolioReturnService
	{
		private readonly MFPTrackerDbContext _db;
		private readonly IMutualFundTransactionRepo _txnRepo;
		private readonly IFundNavRepo _navRepo;
		private readonly IFolioRepo _folioRepo;
		private readonly IMFFundRepo _fundRepo;


		public PortfolioReturnService(MFPTrackerDbContext db, IMutualFundTransactionRepo txnRepo, IFundNavRepo navRepo, IFolioRepo folioRepo, IMFFundRepo fundRepo)
		{
			_db = db;
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
					var holderName = $"{folio.Holder?.FirstName} {folio.Holder?.LastName}".Trim();

					row.FolioDisplay =
						string.IsNullOrWhiteSpace(holderName)
							? folio.FolioNumber
							: $"{folio.FolioNumber} - {holderName}";
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
		public async Task<decimal?> CalculateOverallXirrAsync()
		{
			var pairs = await _txnRepo.GetDistinctFolioFundPairsAsync();
			var xirrFlows = new List<(DateTime date, decimal amount)>();

			foreach (var (folioId, fundId) in pairs)
			{
				var calc = await CalculatePortfolioReturnsAsync(folioId, fundId);

				xirrFlows.AddRange(
					calc.CashFlows.Select(x => (x.Date, x.Amount)));
			}

			return XirrCalculator.Calculate(xirrFlows);
		}
		public async Task<List<MonthlyPortfolioValueDto>> GetLast12MonthPortfolioValueTrendAsync()
		{
			var today = DateTime.Today;
			var currentMonth = new DateTime(today.Year, today.Month, 1);

			var months = Enumerable.Range(0, 12)
				.Select(i => currentMonth.AddMonths(i - 11))
				.Select(month => new
				{
					Month = month,
					MonthEnd = month.Year == today.Year && month.Month == today.Month
						? today
						: month.AddMonths(1).AddDays(-1)
				})
				.ToList();

			var latestMonthEnd = months.Max(x => x.MonthEnd);

			var transactions = await _db.MutualFundTransactions
				.AsNoTracking()
				.Where(t => t.TransactionDate <= latestMonthEnd)
				.Select(t => new
				{
					t.FundId,
					t.TransactionDate,
					t.TxnType,
					t.Units,
					t.AmountPaid
				})
				.ToListAsync();

			var navsByFund = await _db.FundNavs
				.AsNoTracking()
				.Where(n => n.NavDate <= latestMonthEnd)
				.Select(n => new
				{
					n.FundId,
					n.NavDate,
					n.NavValue
				})
				.ToListAsync();

			var navLookup = navsByFund
				.GroupBy(n => n.FundId)
				.ToDictionary(
					g => g.Key,
					g => g.OrderByDescending(n => n.NavDate).ToList());

			var result = new List<MonthlyPortfolioValueDto>();

			foreach (var month in months)
			{
				var monthTransactions = transactions
					.Where(t => t.TransactionDate <= month.MonthEnd)
					.ToList();

				var totalInvestment = monthTransactions.Sum(t =>
					IsCashInflow(t.TxnType) ? -t.AmountPaid : t.AmountPaid);

				var unitsByFund = monthTransactions
					.GroupBy(t => t.FundId)
					.Select(g => new
					{
						FundId = g.Key,
						Units = g.Sum(t => AddsUnits(t.TxnType) ? t.Units : 0m)
							- g.Sum(t => SubtractsUnits(t.TxnType) ? t.Units : 0m)
					})
					.Where(x => x.Units > 0)
					.ToList();

				decimal currentValue = 0m;

				foreach (var holding in unitsByFund)
				{
					if (!navLookup.TryGetValue(holding.FundId, out var navs))
						continue;

					var latestNav = navs
						.FirstOrDefault(n => n.NavDate <= month.MonthEnd);

					if (latestNav != null)
						currentValue += holding.Units * latestNav.NavValue;
				}

				result.Add(new MonthlyPortfolioValueDto
				{
					Month = month.Month,
					TotalInvestment = Math.Round(totalInvestment, 2),
					CurrentValue = Math.Round(currentValue, 2)
				});
			}

			return result;
		}
		public async Task<List<FundPerformanceTrendDto>> GetTopFundPerformanceTrendLast12MonthsAsync(int count = 3)
		{
			var today = DateTime.Today;
			var currentMonth = new DateTime(today.Year, today.Month, 1);

			var months = Enumerable.Range(0, 12)
				.Select(i => currentMonth.AddMonths(i - 11))
				.Select(month => new
				{
					Month = month,
					MonthEnd = month.Year == today.Year && month.Month == today.Month
						? today
						: month.AddMonths(1).AddDays(-1)
				})
				.ToList();

			var latestMonthEnd = months.Max(x => x.MonthEnd);

			var transactions = await _db.MutualFundTransactions
				.AsNoTracking()
				.Where(t => t.TransactionDate <= latestMonthEnd)
				.Select(t => new
				{
					t.FundId,
					t.Fund.FundName,
					t.TransactionDate,
					t.TxnType,
					t.Units,
					t.AmountPaid
				})
				.ToListAsync();

			var navsByFund = await _db.FundNavs
				.AsNoTracking()
				.Where(n => n.NavDate <= latestMonthEnd)
				.Select(n => new
				{
					n.FundId,
					n.NavDate,
					n.NavValue
				})
				.ToListAsync();

			var navLookup = navsByFund
				.GroupBy(n => n.FundId)
				.ToDictionary(
					g => g.Key,
					g => g.OrderByDescending(n => n.NavDate).ToList());

			var fundTrends = transactions
				.GroupBy(t => new { t.FundId, t.FundName })
				.Select(g =>
				{
					var fundTransactions = g.ToList();
					var points = new List<MonthlyFundPerformancePointDto>();

					foreach (var month in months)
					{
						var startDate = month.Month.AddMonths(-12);

						var openingTransactions = fundTransactions
							.Where(t => t.TransactionDate < startDate)
							.ToList();

						var periodTransactions = fundTransactions
							.Where(t => t.TransactionDate >= startDate &&
										t.TransactionDate <= month.MonthEnd)
							.ToList();

						var allTransactionsToMonthEnd = fundTransactions
							.Where(t => t.TransactionDate <= month.MonthEnd)
							.ToList();

						var totalInvestment = allTransactionsToMonthEnd.Sum(t =>
							IsCashInflow(t.TxnType) ? -t.AmountPaid : t.AmountPaid);

						var openingUnits = openingTransactions.Sum(t => AddsUnits(t.TxnType) ? t.Units : 0m)
							- openingTransactions.Sum(t => SubtractsUnits(t.TxnType) ? t.Units : 0m);

						var units = allTransactionsToMonthEnd.Sum(t => AddsUnits(t.TxnType) ? t.Units : 0m)
							- allTransactionsToMonthEnd.Sum(t => SubtractsUnits(t.TxnType) ? t.Units : 0m);

						decimal currentValue = 0m;
						decimal openingValue = 0m;

						if (navLookup.TryGetValue(g.Key.FundId, out var navs))
						{
							if (openingUnits > 0)
							{
								var openingNav = navs
									.FirstOrDefault(n => n.NavDate <= startDate);

								if (openingNav != null)
									openingValue = openingUnits * openingNav.NavValue;
							}

							if (units > 0)
							{
								var latestNav = navs
								.FirstOrDefault(n => n.NavDate <= month.MonthEnd);

								if (latestNav != null)
									currentValue = units * latestNav.NavValue;
							}
						}

						var cashFlows = new List<(DateTime date, decimal amount)>();

						if (openingValue > 0)
							cashFlows.Add((startDate, -openingValue));

						cashFlows.AddRange(periodTransactions.Select(t =>
						{
							var amount = IsCashInflow(t.TxnType)
								? t.AmountPaid
								: -t.AmountPaid;

							return (t.TransactionDate, amount);
						}));

						if (currentValue > 0)
							cashFlows.Add((month.MonthEnd, currentValue));

						var hasPositiveFlow = cashFlows.Any(x => x.amount > 0);
						var hasNegativeFlow = cashFlows.Any(x => x.amount < 0);

						var xirr = hasPositiveFlow && hasNegativeFlow
							? XirrCalculator.Calculate(cashFlows)
							: null;

						points.Add(new MonthlyFundPerformancePointDto
						{
							Month = month.Month,
							TotalInvestment = Math.Round(totalInvestment, 2),
							CurrentValue = Math.Round(currentValue, 2),
							Xirr = xirr
						});
					}

					return new FundPerformanceTrendDto
					{
						FundName = g.Key.FundName,
						Points = points
					};
				})
				.Where(x => x.Points.LastOrDefault()?.TotalInvestment > 0)
				.OrderByDescending(x => x.Points.Last().Xirr ?? decimal.MinValue)
				.Take(count)
				.ToList();

			return fundTrends;
		}

		private static bool IsCashInflow(TransactionType transactionType) =>
			transactionType is TransactionType.SELL or TransactionType.SWITCH_OUT or TransactionType.DIV_PAYOUT;

		private static bool AddsUnits(TransactionType transactionType) =>
			transactionType is TransactionType.BUY or TransactionType.SIP or TransactionType.DIV_REINVEST or TransactionType.SWITCH_IN;

		private static bool SubtractsUnits(TransactionType transactionType) =>
			transactionType is TransactionType.SELL or TransactionType.SWITCH_OUT;

		public async Task<List<PortfolioRowDto>> CalculatePortfolioReturnsByFilterAsync(PortfolioAdvancedFilter filter)
		{
			var results = new List<PortfolioRowDto>();

			var fromDate = filter.FromDate ?? new DateTime(1900, 1, 1);
			var toDate = filter.ToDate ?? DateTime.Today;

			// ✅ Load ALL data
			var (transactions, _) = await _txnRepo.GetAsync(
				null, null,
				sortBy: nameof(MutualFundTransaction.TransactionDate),
				sortDesc: false,
				pageNumber: 1,
				pageSize: int.MaxValue);

			if (transactions == null || !transactions.Any())
				return results;

			var folios = (await _folioRepo
				.GetAsync(null, null, false, 1, int.MaxValue))
				.Items.ToList();

			var funds = (await _fundRepo
				.GetAsync(null, null, false, 1, int.MaxValue))
				.Items.ToList();

			// ✅ Apply MASTER filtering (NOT transactions yet)

			if (filter.AmcId.HasValue)
				folios = folios.Where(f => f.AMCId == filter.AmcId).ToList();

			if (filter.HolderId.HasValue)
				folios = folios.Where(f => f.FolioHolderId == filter.HolderId).ToList();

			if (filter.FolioId.HasValue)
				folios = folios.Where(f => f.FolioId == filter.FolioId).ToList();

			if (filter.CategoryId.HasValue)
				funds = funds.Where(f => f.MFCatId == filter.CategoryId).ToList();

			if (filter.FundId.HasValue)
				funds = funds.Where(f => f.FundId == filter.FundId).ToList();

			// ✅ Build ALL POSSIBLE COMBINATIONS (key fix 🔥)
			var combos = (from f in folios
						  from fd in funds
						  select new { f.FolioId, fd.FundId })
						  .ToList();

			// ✅ Apply transaction filtering (date + text)
			var filteredTxns = transactions
				.Where(t => t.TransactionDate >= fromDate &&
							t.TransactionDate <= toDate)
				.ToList();

			if (!string.IsNullOrWhiteSpace(filter.FreeText))
			{
				var txt = filter.FreeText.ToLower();

				filteredTxns = filteredTxns.Where(t =>
					(t.ReferenceNo != null && t.ReferenceNo.ToLower().Contains(txt)) ||
					(t.Source != null && t.Source.ToLower().Contains(txt)) ||
					(t.Note != null && t.Note.ToLower().Contains(txt))
				).ToList();
			}

			// Lookups
			var folioLookup = folios.ToDictionary(f => f.FolioId);
			var fundLookup = funds.ToDictionary(f => f.FundId);

			foreach (var c in combos)
			{
				var calc = await CalculatePortfolioReturnsAsync(c.FolioId, c.FundId);

				// ✅ skip ONLY if truly no investment
				if (calc.InvestedAmount == 0 && calc.CurrentValue == 0)
					continue;

				var row = new PortfolioRowDto
				{
					FolioId = c.FolioId,
					FundId = c.FundId,

					Xirr = calc.Xirr,
					InvestedAmount = calc.InvestedAmount,
					CurrentValue = calc.CurrentValue,
					AbsoluteReturn = calc.AbsoluteReturn,
					AbsoluteReturnAmount = calc.AbsoluteReturnAmount,
					WeightedAverageDays = calc.WeightedAverageDays
				};

				if (folioLookup.TryGetValue(c.FolioId, out var f))
				{
					var holderName = $"{f.Holder?.FirstName} {f.Holder?.LastName}".Trim();

					row.FolioDisplay =
						string.IsNullOrWhiteSpace(holderName)
							? f.FolioNumber
							: $"{f.FolioNumber} - {holderName}";
				}

				if (fundLookup.TryGetValue(c.FundId, out var fd))
				{
					row.FundDisplay = fd.FundName;
				}

				results.Add(row);
			}
			return results;
		}
		//I need a portfolio query method where filters(AMC, Folio, Fund, etc.) are applied on master data to generate a full matrix of all folio–fund combinations, and the system must always return all those rows(no grouping-based elimination), while calculating returns(Invested, Absolute Return, XIRR) strictly using only the transactions within the selected date range(From/To); if a combination has no transactions in that period, it should still appear with zero values, ensuring no rows are dropped, no duplicates are created, and calculations are accurate and consistent for strict period-based analysis.
		private async Task<PortfolioReturnResultDto> CalculateFromTransactionsAsync(int fundId, List<MutualFundTransaction> txns)
		{
			var result = new PortfolioReturnResultDto();

			if (txns == null || !txns.Any())
				return result;

			var xirrFlows = new List<(DateTime date, decimal amount)>();
			decimal invested = 0m;

			// ✅ 1️⃣ Build cashflows from period transactions
			foreach (var t in txns)
			{
				bool isInflow = t.TxnType is
					TransactionType.SELL or
					TransactionType.SWITCH_OUT or
					TransactionType.DIV_PAYOUT;

				var amt = t.AmountPaid;
				var signed = isInflow ? amt : -amt;

				if (!isInflow)
					invested += amt;
				else
					invested -= amt;

				xirrFlows.Add((t.TransactionDate, signed));
			}

			// ✅ 2️⃣ Calculate remaining units ONLY from period txns
			decimal units = 0m;

			foreach (var t in txns)
			{
				bool isInflow = t.TxnType is
					TransactionType.SELL or
					TransactionType.SWITCH_OUT or
					TransactionType.DIV_PAYOUT;

				units += isInflow ? -t.Units : t.Units;
			}

			decimal currentValue = 0m;
			DateTime valuationDate = DateTime.Today;

			// ✅ 3️⃣ Add closing value (CRITICAL FIX)
			if (units > 0)
			{
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

					// ✅ ADD FINAL CASHFLOW (THIS FIXES YOUR ISSUE)
					xirrFlows.Add((valuationDate, currentValue));
				}
			}

			// ✅ 4️⃣ Calculate XIRR
			var xirr = xirrFlows.Count > 1
				? XirrCalculator.Calculate(xirrFlows)
				: null;

			// ✅ 5️⃣ Absolute Return
			var absAmt = currentValue - invested;

			var absPct = invested > 0
				? (absAmt / invested) * 100
				: 0;

			// ✅ 6️⃣ Populate result
			result.InvestedAmount = Math.Round(invested, 2);
			result.CurrentValue = Math.Round(currentValue, 2);
			result.AbsoluteReturnAmount = Math.Round(absAmt, 2);
			result.AbsoluteReturn = Math.Round(absPct, 2);
			result.Xirr = xirr;
			result.WeightedAverageDays = 0; // optional for strict period

			return result;
		}
	}
}
