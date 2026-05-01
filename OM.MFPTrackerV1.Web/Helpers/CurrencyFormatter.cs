namespace OM.MFPTrackerV1.Web.Helpers
{
	public static class CurrencyFormatter
	{
		/// <summary>
		/// Formats a decimal amount into Indian units (Lakh / Crore).
		/// Examples:
		/// 165967.71  → 1.66 L
		/// 12345678   → 1.23 Cr
		/// </summary>
		public static string FormatAmount(decimal value)
		{
			if (value >= 1_00_00_000) // 1 Crore
				return $"{value / 1_00_00_000M:0.##} Cr";

			if (value >= 1_00_000) // 1 Lakh
				return $"{value / 1_00_000M:0.##} L";

			return value.ToString("N0");
		}
	}
}