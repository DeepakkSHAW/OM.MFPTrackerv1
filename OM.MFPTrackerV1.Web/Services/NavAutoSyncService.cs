using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OM.MFPTrackerV1.Data.Models;
using OM.MFPTrackerV1.Data.Services;
namespace OM.MFPTrackerV1.Web.Services
{


	public sealed class NavAutoSyncService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<NavAutoSyncService> _logger;

		private readonly bool _autoSyncEnabled;
		private readonly int _minIntervalHours;
		private readonly int _startupDelayMinutes;

		public NavAutoSyncService(
			IServiceScopeFactory scopeFactory, ILogger<NavAutoSyncService> logger, IConfiguration configuration)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;

			// Read configuration directly
			_autoSyncEnabled = configuration.GetValue<bool>("NavSync:AutoSyncEnabled");
			_minIntervalHours = configuration.GetValue<int>("NavSync:MinIntervalHours", 22);
			_startupDelayMinutes = configuration.GetValue<int>("NavSync:StartupDelayMinutes", 1);

		}
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("NAV Auto-Sync background service started");

			if (!_autoSyncEnabled)
			{
				_logger.LogInformation("NAV auto-sync is DISABLED via configuration");
				return;
			}

			try
			{
				// Small startup delay
				await Task.Delay(TimeSpan.FromMinutes(_startupDelayMinutes), stoppingToken);

				while (!stoppingToken.IsCancellationRequested)
				{
					using var scope = _scopeFactory.CreateScope();

					var navService = scope.ServiceProvider.GetRequiredService<IAmfiNavService>();
					var state = scope.ServiceProvider.GetRequiredService<ISystemStateRepo>();

					try
					{
						_logger.LogInformation("NAV auto-sync started");

						var lastRun = await state.GetLastNavSyncUtcAsync();
						var now = DateTime.UtcNow;

						if (lastRun.HasValue && lastRun.Value > now.AddHours(-_minIntervalHours))
						{
							_logger.LogInformation(
								"NAV auto-sync skipped — already synced at {Time}", lastRun);
						}
						else
						{
							_logger.LogInformation("NAV auto-sync started at {Time}", now);

							var summary =
								await navService.FetchAndStoreLatestNavWithSummaryAsync();

							await state.SetLastNavSyncUtcAsync(now);

							_logger.LogInformation(
								"NAV auto-sync completed. Inserted={Inserted}, Skipped={Skipped}",
								summary.Inserted,
								summary.SkippedAsDuplicate);
						}
					}
					catch (Exception ex)
					{
						// Real failure (not cancellation)
						_logger.LogError(ex, "NAV auto-sync failed");
					}

					// Run once per 24 hours
					await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
				}
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				// ✅ Expected on shutdown — DO NOT log as error
				_logger.LogInformation("NAV Auto-Sync background service is stopping");
			}
		}

		//protected override async Task ExecuteAsync_redundent(CancellationToken stoppingToken)
		//{
		//	_logger.LogInformation("NAV Auto-Sync background service started");
		//	var summary = new NavSyncSummary();

		//	if (!_autoSyncEnabled)
		//	{
		//		_logger.LogInformation("NAV auto-sync is DISABLED via configuration");
		//		return;
		//	}

		//	// Small startup delay
		//	await Task.Delay(TimeSpan.FromMinutes(_startupDelayMinutes), stoppingToken);

		//	while (!stoppingToken.IsCancellationRequested)
		//	{
		//		using var scope = _scopeFactory.CreateScope();

		//		var navService = scope.ServiceProvider.GetRequiredService<IAmfiNavService>();
		//		var state = scope.ServiceProvider.GetRequiredService<ISystemStateRepo>();

		//		try
		//		{
		//			_logger.LogInformation("NAV auto-sync started");
		//			var lastRun = await state.GetLastNavSyncUtcAsync();
		//			var now = DateTime.UtcNow;

		//			if (lastRun.HasValue && lastRun.Value > now.AddHours(-_minIntervalHours))
		//			{
		//				_logger.LogInformation("NAV auto-sync skipped — already synced at {Time}", lastRun);
		//			}
		//			else
		//			{

		//				_logger.LogInformation("NAV auto-sync started at {Time}", DateTime.UtcNow);
		//				summary = await navService.FetchAndStoreLatestNavWithSummaryAsync();

		//				await state.SetLastNavSyncUtcAsync(now);

		//				_logger.LogInformation("NAV auto-sync completed successfully");
		//			}

		//			_logger.LogInformation(
		//				"NAV auto-sync completed. Inserted={Inserted}, Skipped={Skipped}",
		//				summary.Inserted,
		//				summary.SkippedAsDuplicate);
		//		}
		//		catch (Exception ex)
		//		{
		//			_logger.LogError(ex, "NAV auto-sync failed");
		//		}

		//		// Run once per 24 hours
		//		await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
		//	}
		//}
	}
}
