using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OM.MFPTrackerV1.Data.Models;
using System.Globalization;

namespace OM.MFPTrackerV1.Data.Services
{
	public interface IAmfiNavService
	{
		/// <summary>
		/// Downloads latest NAV file from AMFI and saves NAV history
		/// for funds already present in the system.
		/// </summary>
		/// <returns>Number of NAV rows inserted</returns>
		Task<int> FetchAndStoreLatestNavAsync();

		/// <summary>
		/// Fetches NAV data from AMFI and stores NAV history
		/// for funds already present in the system.
		/// </summary>
		Task<NavSyncSummary> FetchAndStoreLatestNavWithSummaryAsync();


		/// <summary>
		/// Imports NAV data from a CSV file (admin/manual source)
		/// // ✅ Manual / admin source (CSV)
		/// </summary>
		Task<NavSyncSummary> ImportNavFromCsvAsync(Stream csvStream);

		Task<NavSyncSummary> ImportNavForFundAsync(int fundId, IEnumerable<(DateTime Date, decimal Nav)> navs);
	}

	public sealed class AmfiNavService : IAmfiNavService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly MFPTrackerDbContext _db;
		private readonly ILogger<AmfiNavService> _logger;
		private readonly char[] _delimiters = { ',', ';', ' ', '|' };
		public AmfiNavService(IHttpClientFactory httpClientFactory, MFPTrackerDbContext db, ILogger<AmfiNavService> logger)
		{
			_httpClientFactory = httpClientFactory;
			_db = db;
			_logger = logger;
		}
		public async Task<NavSyncSummary> FetchAndStoreLatestNavWithSummaryAsync()
		{
			var client = _httpClientFactory.CreateClient("AmfiNavClient");
			//var content = await client.GetStringAsync("");
			//var lines = content	.Split('\n', StringSplitOptions.RemoveEmptyEntries);

			using var response = await client.GetAsync("", HttpCompletionOption.ResponseHeadersRead);

			response.EnsureSuccessStatusCode();

			using var stream = await response.Content.ReadAsStreamAsync();
			using var reader = new StreamReader(stream);

			var lines = new List<string>();
			while (!reader.EndOfStream)
			{
				var line = await reader.ReadLineAsync();
				if (!string.IsNullOrWhiteSpace(line))
					lines.Add(line);
			}

			var fundBySchemeCode = await _db.Funds
				.AsNoTracking()
				.ToDictionaryAsync(f => f.SchemeCode);

			int inserted = 0;
			int skippedDuplicate = 0;
			int fundNotFound = 0;
			int invalidRows = 0;

			DateTime fetchedAt = DateTime.UtcNow;
			foreach (var line in lines)
			{
				// Skip empty lines
				if (string.IsNullOrWhiteSpace(line)) continue;
				if (line.StartsWith("-")) continue;

				var parts = line.Split(';');

				// Skip non-data lines
				if (parts.Length < 6)
					continue;

				// Extract fields
				var schemeCode = parts[0].Trim();
				var navText = parts[4].Trim();
				var dateText = parts[5].Trim();

				// SchemeCode MUST be numeric
				if (!int.TryParse(schemeCode, out _))
					continue;

				// NAV must be decimal
				if (!decimal.TryParse(navText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal navValue))
				{
					invalidRows++;
					continue;
				}

				// Date must be dd-MMM-yyyy
				if (!DateTime.TryParseExact(dateText, "dd-MMM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime navDate))
				{
					invalidRows++;
					continue;
				}

				// Match fund by SchemeCode
				if (!fundBySchemeCode.TryGetValue(schemeCode, out var fund))
				{
					fundNotFound++;
					continue;
				}

				// Check duplicate
				bool exists = await _db.FundNavs.AnyAsync(n =>
					n.FundId == fund.FundId &&
					n.NavDate == navDate);

				if (exists)
				{
					skippedDuplicate++;
					continue;
				}

				// Insert NAV
				_db.FundNavs.Add(new FundNav
				{
					FundId = fund.FundId,
					NavDate = navDate,
					NavValue = navValue,
					Source = "AMFI",
					FetchedAt = fetchedAt
				});

				inserted++;
			}

			if (inserted > 0)
				await _db.SaveChangesAsync();

			return new NavSyncSummary
			{
				Inserted = inserted,
				SkippedAsDuplicate = skippedDuplicate,
				FundNotFound = fundNotFound,
				InvalidRows = invalidRows
			};
		}
		public async Task<int> FetchAndStoreLatestNavAsync()
		{
			var client = _httpClientFactory.CreateClient("AmfiNavClient");
			var navUrl = client.BaseAddress?.ToString() ?? "";
			string content;
			try
			{
				content = await client.GetStringAsync(""); // BaseAddress already set
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to download AMFI NAV data from {Url}", navUrl);
				throw;
			}

			var lines = content
				.Split('\n', StringSplitOptions.RemoveEmptyEntries);

			if (lines.Length <= 1)
			{
				_logger.LogWarning("AMFI NAV file contained no data.");
				return 0;
			}

			var fundBySchemeCode = await _db.Funds
				.AsNoTracking()
				.ToDictionaryAsync(f => f.SchemeCode);

			int inserted = 0;
			DateTime fetchedAt = DateTime.UtcNow;

			foreach (var line in lines.Skip(1)) // skip header
			{
				var parts = line.Split(_delimiters, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 6)
					continue;

				var schemeCode = parts[0].Trim();
				var navText = parts[4].Trim();
				var dateText = parts[5].Trim();

				if (!fundBySchemeCode.TryGetValue(schemeCode, out var fund))
					continue;

				if (!decimal.TryParse(
						navText,
						NumberStyles.Any,
						CultureInfo.InvariantCulture,
						out var navValue))
					continue;

				if (!DateTime.TryParseExact(
						dateText,
						"dd-MMM-yyyy",
						CultureInfo.InvariantCulture,
						DateTimeStyles.None,
						out var navDate))
					continue;

				bool exists = await _db.FundNavs.AnyAsync(n =>
					n.FundId == fund.FundId &&
					n.NavDate == navDate);

				if (exists)
					continue;

				_db.FundNavs.Add(new FundNav
				{
					FundId = fund.FundId,
					NavDate = navDate,
					NavValue = navValue,
					Source = "AMFI",
					FetchedAt = fetchedAt
				});

				inserted++;
			}

			if (inserted > 0)
			{
				await _db.SaveChangesAsync();
				_logger.LogInformation(
					"Inserted {Count} new NAV records from AMFI.",
					inserted);
			}
			else
			{
				_logger.LogInformation("No new AMFI NAV data found.");
			}

			return inserted;
		}

		// =====================================================
		// CSV INGESTION
		// =====================================================
		public async Task<NavSyncSummary> ImportNavFromCsvAsync(Stream csvStream)
		{
			int inserted = 0;
			int skippedDuplicate = 0;
			int fundNotFound = 0;
			int invalidRows = 0;

			// 1️⃣ Resolve fund codes → FundId
			var fundMap = await _db.Funds
				.AsNoTracking()
				.ToDictionaryAsync(
					f => (f.ISIN ?? f.SchemeCode)!.Trim().ToUpperInvariant(),
					f => f.FundId);

			// 2️⃣ Load existing NAV keys ONCE
			var existingKeys = new HashSet<(int FundId, DateTime NavDate)>(
				(await _db.FundNavs
					.AsNoTracking()
					.Select(n => new
					{
						n.FundId,
						NavDate = n.NavDate
					})
					.ToListAsync())
				.Select(x => (x.FundId, x.NavDate.Date))
			);

			// 3️⃣ Track keys inserted in this batch
			var batchKeys = new HashSet<(int FundId, DateTime NavDate)>();

			using var reader = new StreamReader(csvStream);
			bool header = true;
			string? line;

			while ((line = await reader.ReadLineAsync()) != null)
			{
				if (header)
				{
					header = false;
					continue;
				}

				if (string.IsNullOrWhiteSpace(line))
					continue;

				var parts = line.Split(',');
				if (parts.Length != 3)
				{
					invalidRows++;
					continue;
				}

				var fundCode = parts[0].Trim().ToUpperInvariant();

				if (!fundMap.TryGetValue(fundCode, out var fundId))
				{
					fundNotFound++;
					continue;
				}

				if (!DateTime.TryParse(parts[1], out var navDate) ||
					!decimal.TryParse(parts[2], out var navValue) ||
					navValue <= 0)
				{
					invalidRows++;
					continue;
				}

				navDate = navDate.Date;
				var key = (fundId, navDate);

				// ✅ DB duplicate OR batch duplicate
				if (existingKeys.Contains(key) || batchKeys.Contains(key))
				{
					skippedDuplicate++;
					continue;
				}

				_db.FundNavs.Add(new FundNav
				{
					FundId = fundId,
					NavDate = navDate,
					NavValue = navValue,
					Source = "CSV",
					FetchedAt = DateTime.UtcNow
				});

				batchKeys.Add(key);
				inserted++;
			}

			if (inserted > 0)
				await _db.SaveChangesAsync();

			return new NavSyncSummary
			{
				Inserted = inserted,
				SkippedAsDuplicate = skippedDuplicate,
				FundNotFound = fundNotFound,
				InvalidRows = invalidRows
			};
		}


		// =====================================================
		// BULK INGESTION
		// =====================================================
		public async Task<NavSyncSummary> ImportNavForFundAsync(int fundId, IEnumerable<(DateTime Date, decimal Nav)> navs)
		{
			int inserted = 0;
			int skippedDuplicate = 0;
			int invalid = 0;

			foreach (var (date, nav) in navs)
			{
				if (nav <= 0)
				{
					invalid++;
					continue;
				}

				var navDate = date.Date;

				bool exists = await _db.FundNavs.AnyAsync(n =>
					n.FundId == fundId &&
					n.NavDate == navDate);

				if (exists)
				{
					skippedDuplicate++;
					continue;
				}

				_db.FundNavs.Add(new FundNav
				{
					FundId = fundId,
					NavDate = navDate,
					NavValue = nav,
					Source = "BULK",
					FetchedAt = DateTime.UtcNow
				});

				inserted++;
			}

			if (inserted > 0)
				await _db.SaveChangesAsync();

			return new NavSyncSummary
			{
				Inserted = inserted,
				SkippedAsDuplicate = skippedDuplicate,
				InvalidRows = invalid
			};
		}

	}
}
