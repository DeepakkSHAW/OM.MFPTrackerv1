namespace OM.MFPTrackerV1.Data.Helper
{
	public static class XirrCalculator
	{
		public static decimal? Calculate(List<(DateTime date, decimal amount)> cashFlows, decimal guess = 0.1m)
		{
			if (cashFlows.Count < 2)
				return null;

			const int maxIterations = 100;
			const double tolerance = 1e-6;

			double rate = (double)guess;
			var baseDate = cashFlows.Min(c => c.date);

			for (int i = 0; i < maxIterations; i++)
			{
				double f = 0;
				double df = 0;

				foreach (var (date, amount) in cashFlows)
				{
					var days = (date - baseDate).TotalDays / 365.0;
					var pow = Math.Pow(1 + rate, days);

					f += (double)amount / pow;
					df += -days * (double)amount / (pow * (1 + rate));
				}

				var nextRate = rate - f / df;

				if (Math.Abs(nextRate - rate) < tolerance)
					return (decimal)nextRate;

				rate = nextRate;
			}

			return null;
		}

	}
}