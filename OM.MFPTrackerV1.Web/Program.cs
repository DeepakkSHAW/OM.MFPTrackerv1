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

			var dbPath = Path.Combine(builder.Environment.ContentRootPath, dbFolder, dbFile);
			Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

			// Register DbContext
			builder.Services.AddDbContext<MFPTrackerDbContext>(options => options.UseSqlite($"Data Source={dbPath}")
			.EnableDetailedErrors()             //For DEV ONLY
			.EnableSensitiveDataLogging()       //For DEV ONLY
			);

			// Register repository
			builder.Services.AddScoped<IFolioHolderRepo, FolioHolderRepo>();
			builder.Services.AddScoped<IMFCategoryRepo, MFCategoryRepo>();
			builder.Services.AddScoped<IFundRepo, FundRepo>();
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
