using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data;
using OM.MFPTrackerV1.Data.Services;
using OM.MFPTrackerV1.Web.API.Auth;
using OM.MFPTrackerV1.Web.Components;
using OM.MFPTrackerV1.Web.Services;

namespace OM.MFPTrackerV1.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			////////////DK Added /////////////
			var dbFolder = builder.Configuration["Database:Folder"] ?? "data";
			var dbFile = builder.Configuration["Database:FileName"] ?? "portfolio.db";
			var urlAmf = builder.Configuration["Amfi:NavUrl"] ?? "https://portal.amfiindia.com/spages/NAVOpen.txt";

			var dbPath = Path.Combine(builder.Environment.ContentRootPath, dbFolder, dbFile);
			Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

			// Register DbContext
			builder.Services.AddDbContext<MFPTrackerDbContext>(options => options.UseSqlite($"Data Source={dbPath}")
			.EnableDetailedErrors()             //For DEV ONLY
			.EnableSensitiveDataLogging()       //For DEV ONLY
			);
			//Register HTTP factory
			//builder.Services.AddHttpClient("AmfiNavClient", client =>
			//{
			//	client.BaseAddress = new Uri(urlAmf);
			//	client.Timeout = TimeSpan.FromSeconds(30);

			//	client.DefaultRequestHeaders.Add("User-Agent", "MFPTracker/1.0");
			//	client.DefaultRequestHeaders.Add("Accept", "text/plain");
			//});

			builder.Services.AddHttpClient("AmfiNavClient", (sp, client) =>
			{
				var config = sp.GetRequiredService<IConfiguration>();
				var url = config["Amfi:NavUrl"];

				if (string.IsNullOrWhiteSpace(url))
					throw new InvalidOperationException("Amfi:NavUrl not configured.");

				client.BaseAddress = new Uri(url);
				client.Timeout = TimeSpan.FromSeconds(120);
				client.DefaultRequestHeaders.Add("User-Agent", "MFPTracker/1.0");
			});

			// Register repository
			builder.Services.AddScoped<IAMCRepo, AMCRepo>();
			builder.Services.AddScoped<IMFCategoryRepo, MFCategoryRepo>();
			builder.Services.AddScoped<IMFFundRepo, MFFundRepo>();
			builder.Services.AddScoped<IFolioHolderRepo, FolioHolderRepo>();
			builder.Services.AddScoped<IFolioRepo, FolioRepo>();
			builder.Services.AddScoped<IMutualFundTransactionRepo, MutualFundTransactionRepo>();
			builder.Services.AddScoped<IAmfiNavService, AmfiNavService>();
			builder.Services.AddScoped<ISpecialEventRepo, SpecialEventRepo>();
			builder.Services.AddScoped<IFundNavRepo, FundNavRepo>();
			builder.Services.AddScoped<ISystemStateRepo, SystemStateRepo>();
			builder.Services.AddHostedService<NavAutoSyncService>(); //BackgroundService to auto-sync NAV data from AMFI daily
			builder.Services.AddScoped<IPortfolioReturnService, PortfolioReturnService>(); // delete after PortfolioReturnService is merged into FolioService
			builder.Services.AddScoped<IFolioOwnerRepository, FolioOwnerRepository>(); // delete after FolioOwner is removed

			builder.Services.AddScoped(sp =>
			{
				var nav = sp.GetRequiredService<NavigationManager>();
				return new HttpClient
				{
					BaseAddress = new Uri(nav.BaseUri)
				};
			});

			//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
			//		.AddCookie(options =>
			//		{
			//			options.Cookie.Name = "OMNamahShivay-Auth";
			//			options.LoginPath = "/login";
			//			options.LogoutPath = "/logout";
			//			options.AccessDeniedPath = "/login/access-denied";
			//			//options.ExpireTimeSpan = TimeSpan.FromHours(8);
			//			//options.SlidingExpiration = true;


			//			//options.Cookie.HttpOnly = true;
			//			//options.Cookie.SameSite = SameSiteMode.Lax;

			//			//// ✅ THIS IS THE IMPORTANT PART
			//			//options.Cookie.SecurePolicy =
			//			//	builder.Environment.IsDevelopment()
			//			//		? CookieSecurePolicy.SameAsRequest
			//			//		: CookieSecurePolicy.Always;


			//		});

			builder.Services
				.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					// -------------------------------
					// Cookie identity
					// -------------------------------
					options.Cookie.Name = ".OM.MFPTracker.Auth";

					// -------------------------------
					// Navigation paths
					// -------------------------------
					options.LoginPath = "/login";
					options.LogoutPath = "/logout";
					options.AccessDeniedPath = "/account/access-denied";

					// -------------------------------
					// Lifetime & expiration
					// -------------------------------
					options.ExpireTimeSpan = TimeSpan.FromHours(8);
					options.SlidingExpiration = true;

					// -------------------------------
					// Security best practices
					// -------------------------------
					options.Cookie.HttpOnly = true;
					options.Cookie.SameSite = SameSiteMode.Lax;

					options.Cookie.SecurePolicy =
						builder.Environment.IsDevelopment()
							? CookieSecurePolicy.SameAsRequest
							: CookieSecurePolicy.Always;

					// -------------------------------
					// Ensure APIs return 401/403 instead of redirects
					// -------------------------------
					options.Events = new CookieAuthenticationEvents
					{
						OnRedirectToLogin = context =>
						{
							if (context.Request.Path.StartsWithSegments("/api"))
							{
								context.Response.StatusCode = StatusCodes.Status401Unauthorized;
								return Task.CompletedTask;
							}

							context.Response.Redirect(context.RedirectUri);
							return Task.CompletedTask;
						},

						OnRedirectToAccessDenied = context =>
						{
							if (context.Request.Path.StartsWithSegments("/api"))
							{
								context.Response.StatusCode = StatusCodes.Status403Forbidden;
								return Task.CompletedTask;
							}

							context.Response.Redirect(context.RedirectUri);
							return Task.CompletedTask;
						}
					};
				});

			builder.Services.AddAuthorization();
			//builder.Services.AddHttpContextAccessor();
			builder.Services.AddScoped<LocalAuthService>();
			builder.Services.AddAuthorizationCore();
			builder.Services.AddCascadingAuthenticationState();

			/////////////END DK Added /////////////

			// Add services to the container.
			builder.Services.AddRazorComponents()
				.AddInteractiveServerComponents();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			//DK-Added//
			app.UseStaticFiles();          // if present
			app.MapStaticAssets();
			app.UseRouting();              // <-- ADD THIS LINE

			app.UseAuthentication();
			app.UseAuthorization();
			app.MapAuthEndpoints(); // login/logout APIs

			app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
			app.UseHttpsRedirection();

			app.UseAntiforgery();/* REQUIRED FOR BLAZOR */

			app.MapRazorComponents<App>()
				.AddInteractiveServerRenderMode();

			app.Run();
		}
	}
}
