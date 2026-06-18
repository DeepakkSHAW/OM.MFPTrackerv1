using System.Globalization;
using System;

namespace OM.MFPTrackerV1.Web.Helpers
{
	public static class DateParser
	{
		// ✅ Explicit, supported formats
		private static readonly string[] Formats =
		{
        // Common human-readable
        "dd-MMM-yyyy",    // 03-May-2024
        "d-MMM-yyyy",
		"MMM d, yyyy",    // Apr 1, 2026
        "MMM dd, yyyy",

        // ISO / system-friendly
        "yyyy-MM-dd",     // 2024-01-01
        "yyyy-MMM-dd",    // 2024-Jan-01

        // Slashed formats
        "dd/MM/yyyy",     // 01/01/2024
        "d/M/yyyy",

        // Dashed numerics
        "dd-MM-yyyy",     // 09-09-2025
        "d-M-yyyy",

        // US safety (optional)
        "MM/dd/yyyy",
		"M/d/yyyy",
		
        // -----------------------------
        // Day-Month-Year (named month)
        // -----------------------------
        "dd-MMM-yyyy",       // 03-May-2024
        "d-MMM-yyyy",
		"dd-MMM-yy",
		"d-MMM-yy",

		"dd MMM yyyy",       // 03 May 2024
        "d MMM yyyy",

        // -----------------------------
        // Month-Day-Year (named month)
        // -----------------------------
        "MMM d, yyyy",       // Apr 1, 2026
        "MMM dd, yyyy",
		"MMM d yyyy",
		"MMMM d, yyyy",     // April 1, 2026
        "MMMM dd, yyyy",

        // -----------------------------
        // ISO / system formats
        // -----------------------------
        "yyyy-MM-dd",        // 2024-01-01
        "yyyy-M-d",
		"yyyy/MM/dd",
		"yyyyMMdd",          // 20240101
        "yyyy-MMM-dd",       // 2024-Jan-01

        // -----------------------------
        // Slash formats
        // -----------------------------
        "dd/MM/yyyy",        // 01/01/2024 (India/UK)
        "d/M/yyyy",
		"MM/dd/yyyy",        // US format
        "M/d/yyyy",

        // -----------------------------
        // Dash numeric formats
        // -----------------------------
        "dd-MM-yyyy",        // 09-09-2025
        "d-M-yyyy",
		"MM-dd-yyyy",
		"M-d-yyyy",

        // -----------------------------
        // Dot formats (Europe)
        // -----------------------------
        "dd.MM.yyyy",        // 31.12.2024
        "d.M.yyyy",

        // -----------------------------
        // Month-Year only (optional)
        // -----------------------------
        "MMM yyyy",          // Jan 2024
        "MMMM yyyy",

        // -----------------------------
        // Excel-style timestamps
        // -----------------------------
        "dd/MM/yyyy HH:mm:ss",
		"MM/dd/yyyy HH:mm:ss",
		"yyyy-MM-dd HH:mm:ss",
		"yyyy-MM-ddTHH:mm:ss",       // ISO w/o timezone
        "yyyy-MM-ddTHH:mm:ssZ",      // ISO UTC
	};
		private static (DateTime, DateTime) GetLastQuarterRange(DateTime now)
		{
			var currentQuarter = (now.Month - 1) / 3 + 1;
			var firstMonthOfCurrentQuarter = (currentQuarter - 1) * 3 + 1;

			var startOfCurrentQuarter = new DateTime(now.Year, firstMonthOfCurrentQuarter, 1);

			var startOfLastQuarter = startOfCurrentQuarter.AddMonths(-3);
			var endOfLastQuarter = startOfCurrentQuarter.AddDays(-1);

			return (startOfLastQuarter, endOfLastQuarter);
		}
		private static (DateTime, DateTime) GetCurrentQuarterRange(DateTime now)
		{
			var currentQuarter = (now.Month - 1) / 3 + 1;
			var firstMonth = (currentQuarter - 1) * 3 + 1;

			var start = new DateTime(now.Year, firstMonth, 1);

			return (start, now);
		}

		public static IReadOnlyList<KeyValuePair<string, string>> TimeRanges { get; } = new List<KeyValuePair<string, string>>
		{
			new("1M", "Current Month"),
			new("LM", "Last Month"),
			new("CQ", "Current Quarter"),
			new("LQ", "Last Quarter"),
			new("6M", "Last 6 Months"),
			new("1Y", "Last Year"),
			new("3Y", "Last 3 Years"),
			new("ALL", "Since Inception")
		};
		public static string GetGrouping(string selectedRange)
		{
			if (string.IsNullOrWhiteSpace(selectedRange))
				return "M";

			selectedRange = selectedRange.Trim().ToUpper();

			return selectedRange switch
			{
				"1M" => "W",   // weekly for short range
				"LM" => "W",
				"CQ" => "M",   // monthly for quarter
				"LQ" => "M",
				"6M" => "M",
				"1Y" => "M",
				"3Y" => "Q",   // quarterly for long range
				"ALL" => "Y",  // yearly for very long
				_ => "M"
			};
		}
		public static (DateTime from, DateTime to) GetDateRange(this string selectedRange)
		{
			var now = DateTime.Today;

			if (string.IsNullOrWhiteSpace(selectedRange))
				return (DateTime.MinValue, now);

			selectedRange = selectedRange.Trim().ToUpper();


			return selectedRange switch
			{
				"1M" => (new DateTime(now.Year, now.Month, 1), now),

				"LM" => (
					new DateTime(now.Year, now.Month, 1).AddMonths(-1),
					new DateTime(now.Year, now.Month, 1).AddDays(-1)
				),

				"CQ" => GetCurrentQuarterRange(now),

				"LQ" => GetLastQuarterRange(now),

				"6M" => (now.AddMonths(-6), now),

				"1Y" => (now.AddYears(-1), now),

				"3Y" => (now.AddYears(-3), now),

				"ALL" => (DateTime.MinValue, now),

				_ => (DateTime.MinValue, now)
			};

		}
		
		/// <summary>
		/// Safely parses common date formats used in files, UI, and Excel.
		/// Deterministic first, tolerant fallback last.
		/// </summary>
		public static bool TryParseFlexible(string input, out DateTime result)
		{
			result = default;

			if (string.IsNullOrWhiteSpace(input))
				return false;

			input = input.Trim();

			// ✅ Tier 1: Strict & deterministic
			if (DateTime.TryParseExact(
				input,
				Formats,
				CultureInfo.InvariantCulture,
				DateTimeStyles.AllowWhiteSpaces,
				out result))
			{
				return true;
			}

			// ✅ Tier 2: Last-resort tolerance (still invariant)
			return DateTime.TryParse(
				input,
				CultureInfo.InvariantCulture,
				DateTimeStyles.AllowWhiteSpaces,
				out result);
		}
	}
}