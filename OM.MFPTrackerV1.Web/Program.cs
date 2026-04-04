using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data;
using OM.MFPTrackerV1.Data.Models;
using OM.MFPTrackerV1.Data.Services;
using OM.MFPTrackerV1.Web.Components;

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
			builder.Services.AddScoped<IFolioOwnerRepository, FolioOwnerRepository>(); // delete after FolioOwner is removed
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

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
