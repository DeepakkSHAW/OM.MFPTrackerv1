using OM.MFPTrackerV1.Data.Models;
using OM.MFPTrackerV1.Data.Helper;

namespace OM.MFPTrackerV1.Data.Services
{
	public interface IPortfolioReturnService
	{
		Task<PortfolioReturnResultDto> CalculatePortfolioReturnsAsync(int folioId, int fundId);
	}
	public class PortfolioReturnService : IPortfolioReturnService
	{
		private readonly IMutualFundTransactionRepo _txnRepo;
		private readonly IFundNavRepo _navRepo;

		public PortfolioReturnService(
			IMutualFundTransactionRepo txnRepo,
			IFundNavRepo navRepo)
		{
			_txnRepo = txnRepo;
			_navRepo = navRepo;
		}

		public async Task<PortfolioReturnResultDto> CalculatePortfolioReturnsAsync(int folioId, int fundId)
		{
			var (transactions, _) = await _txnRepo.GetAsync(
				folioId,
				fundId,
				sortBy: "TransactionDate",
				sortDesc: false,
				pageNumber: 1,
				pageSize: int.MaxValue);

			var cashFlows = new List<PortfolioCashFlowDto>();
			var xirrFlows = new List<(DateTime, decimal)>();

			decimal investedAmount = 0m;

			foreach (var t in transactions)
			{
				bool isInflow = t.TxnType is
					TransactionType.SELL or
					TransactionType.SWITCH_OUT or
					TransactionType.DIV_PAYOUT;

				var signedAmount = isInflow
					? t.AmountPaid
					: -t.AmountPaid;

				// ✅ Track Invested Amount
				if (!isInflow)
					investedAmount += t.AmountPaid;
				else
					investedAmount -= t.AmountPaid;

				cashFlows.Add(new PortfolioCashFlowDto
				{
					TransactionId = t.TransactionId,
					Date = t.TransactionDate,
					Amount = signedAmount,
					TxnType = t.TxnType.ToString()
				});

				xirrFlows.Add((t.TransactionDate, signedAmount));
			}

			// ✅ Get remaining units
			var units = await _txnRepo.GetAvailableUnitsAsync(folioId, fundId);

			decimal currentValue = 0m;

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
					currentValue = units * latestNav.NavValue;

					cashFlows.Add(new PortfolioCashFlowDto
					{
						TransactionId = null,
						Date = latestNav.NavDate,
						Amount = currentValue,
						TxnType = "CURRENT_VALUE"
					});

					xirrFlows.Add((latestNav.NavDate, currentValue));
				}
			}

			var xirr = XirrCalculator.Calculate(xirrFlows);

			// ✅ Absolute Return
			var absoluteReturnAmount = currentValue - investedAmount;
			var absoluteReturnPct = investedAmount > 0
				? (absoluteReturnAmount / investedAmount) * 100
				: 0;

			return new PortfolioReturnResultDto
			{
				Xirr = xirr,
				AnnualizedReturn = xirr,

				InvestedAmount = investedAmount,
				CurrentValue = currentValue,
				AbsoluteReturnAmount = absoluteReturnAmount,
				AbsoluteReturn = absoluteReturnPct,

				CashFlows = cashFlows
			};
		}
	}
}
