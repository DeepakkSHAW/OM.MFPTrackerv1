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