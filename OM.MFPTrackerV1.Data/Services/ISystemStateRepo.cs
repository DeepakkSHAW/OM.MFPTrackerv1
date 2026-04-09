using OM.MFPTrackerV1.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OM.MFPTrackerV1.Data.Services
{
	public static class SystemStateKeys
	{
		public const string NavLastSuccessSyncUtc =
			"NAV_LAST_SUCCESS_SYNC_UTC";

		public const string TransactionLastSuccessSyncUtc =
			"TRANSACTION_LAST_SUCCESS_SYNC_UTC";

		public const string AppInfo =
			"APP_INFO";
	}
	public interface ISystemStateRepo
	{
		// 1️: NAV sync
		Task<DateTime?> GetLastNavSyncUtcAsync();
		Task SetLastNavSyncUtcAsync(DateTime utcTime);

		// 2️: Transaction sync (future use)
		Task<DateTime?> GetLastTransactionSyncUtcAsync();
		Task SetLastTransactionSyncUtcAsync(DateTime utcTime);

		// 3️: Application info (name + version)
		Task<(string AppName, string AppVersion)?> GetAppInfoAsync();
		Task SetAppInfoAsync(string appName, string appVersion);

	}
	public sealed class SystemStateRepo : ISystemStateRepo
	{
		private readonly MFPTrackerDbContext _db;

		public SystemStateRepo(MFPTrackerDbContext db)
		{
			_db = db;
		}

		// --------------------
		// 1️: NAV sync
		// --------------------

		public async Task<DateTime?> GetLastNavSyncUtcAsync()
			=> await GetDateTimeUtcAsync(
				SystemStateKeys.NavLastSuccessSyncUtc);

		public async Task SetLastNavSyncUtcAsync(DateTime utcTime)
			=> await SetDateTimeUtcAsync(
				SystemStateKeys.NavLastSuccessSyncUtc, utcTime);

		// --------------------
		// 2️: Transaction sync (future)
		// --------------------

		public async Task<DateTime?> GetLastTransactionSyncUtcAsync()
			=> await GetDateTimeUtcAsync(
				SystemStateKeys.TransactionLastSuccessSyncUtc);

		public async Task SetLastTransactionSyncUtcAsync(DateTime utcTime)
			=> await SetDateTimeUtcAsync(
				SystemStateKeys.TransactionLastSuccessSyncUtc, utcTime);

		// --------------------
		// 3️: App info
		// --------------------

		public async Task<(string AppName, string AppVersion)?>
			GetAppInfoAsync()
		{
			var row = await _db.SystemStates
				.FindAsync(SystemStateKeys.AppInfo);

			if (row?.Value == null)
				return null;

			// Stored as JSON: {"name":"MFT","version":"1.0.0"}
			var info = System.Text.Json.JsonSerializer
				.Deserialize<AppInfoDto>(row.Value);

			return info == null
				? null
				: (info.Name, info.Version);
		}

		public async Task SetAppInfoAsync(
			string appName,
			string appVersion)
		{
			var value = System.Text.Json.JsonSerializer
				.Serialize(new AppInfoDto
				{
					Name = appName,
					Version = appVersion
				});

			await UpsertAsync(
				SystemStateKeys.AppInfo,
				value);
		}

		// ====================
		// PRIVATE HELPERS
		// ====================

		private async Task<DateTime?> GetDateTimeUtcAsync(string key)
		{
			var row = await _db.SystemStates.FindAsync(key);

			if (row?.Value == null)
				return null;

			return DateTime.TryParse(
				row.Value,
				out var parsed)
				? DateTime.SpecifyKind(parsed, DateTimeKind.Utc)
				: null;
		}

		private async Task SetDateTimeUtcAsync(
			string key,
			DateTime utcTime)
			=> await UpsertAsync(
				key,
				utcTime.ToString("O")); // ISO‑8601

		private async Task UpsertAsync(
			string key,
			string value)
		{
			var row = await _db.SystemStates.FindAsync(key);

			if (row == null)
			{
				row = new SystemState
				{
					Key = key
				};
				_db.SystemStates.Add(row);
			}

			row.Value = value;
			row.UpdateDate = DateTime.UtcNow;

			await _db.SaveChangesAsync();
		}

		private sealed class AppInfoDto
		{
			public string Name { get; set; } = "";
			public string Version { get; set; } = "";
		}

	}
}
