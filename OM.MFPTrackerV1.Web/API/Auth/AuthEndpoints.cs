using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.WebUtilities;
using OM.MFPTrackerV1.Web.Models;
using OM.MFPTrackerV1.Web.Services;
using System.Security.Claims;

namespace OM.MFPTrackerV1.Web.API.Auth
{
	public static class AuthEndpoints
	{
		public static void MapAuthEndpoints(this WebApplication app)
		{
			app.MapGet("/api/login-status", (LocalAuthService auth) =>
			{
				return Results.Json(new
				{
					enabled = auth.IsLoginEnabled(),
					message = "Login is temporarily disabled for maintenance."
				});
			});

			app.MapPost("/api/auth/login", async (LoginRequest request, string? returnUrl, LocalAuthService authService, HttpContext context) =>
			{
				//if (!authService.IsLoginEnabled())
				//	return Results.Redirect("/login");

				if (!authService.IsLoginEnabled())
					return Results.Problem(
					statusCode: StatusCodes.Status403Forbidden,
					title: "Login temporarily unavailable",
					detail: "The system is currently under maintenance. Please try again later."
				);

				var user = authService.Authenticate(request.Username, request.Password);
				//if (user is null)
				//	return Results.Redirect("/login");
				if (user is null)
					//return Results.Unauthorized();
					return Results.Problem(
						statusCode: StatusCodes.Status401Unauthorized,
						title: "login failed",
						detail: "An error occurred while processing your login request."
						);

				var claims = new[]
						{
							new Claim(ClaimTypes.Name, user.Username),
							new Claim(ClaimTypes.Role, user.Role),
						};

				var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

				await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

				return Results.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
			}).DisableAntiforgery();

			app.MapPost("/api/auth/login1", async (LoginRequest request, LocalAuthService authService, HttpContext context) =>
			{
				// ✅ Feature flag check
				if (!authService.IsLoginEnabled())
				{
					return Results.Forbid();
				}

				// ✅ Authenticate credentials
				var user = authService.Authenticate(request.Username, request.Password);
				if (user is null)
				{
					return Results.Unauthorized();
				}

				// ✅ Build claims
				var claims = new List<Claim>
					{
						new Claim(ClaimTypes.Name, user.Username),
						new Claim(ClaimTypes.Role, user.Role)
					};

				var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

				var principal = new ClaimsPrincipal(identity);

				// ✅ Issue authentication cookie
				Console.WriteLine("SignInAsync about to be executed");
				await context.SignInAsync(
					CookieAuthenticationDefaults.AuthenticationScheme,
					principal);

				return Results.Ok();
			});

			app.MapPost("/api/auth/logout", async (HttpContext context) =>
			{
				await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

				return Results.Ok();
			}).DisableAntiforgery();

			app.MapGet("/api/login", async (HttpContext http, LocalAuthService authService, string? redirect) =>
			{
				var query = http.Request.Query["query"];
				if (string.IsNullOrEmpty(query)) return Results.BadRequest("Missing query parameter.");

				// 1. Decode the Base64 string back into a plain text query string
				byte[] decodedBytes = Convert.FromBase64String(query);
				string decodedQuery = System.Text.Encoding.UTF8.GetString(decodedBytes);

				// 2. Parse the decoded query string (e.g., "?username=abc&password=123")
				// Use QueryHelpers to handle the parsing safely
				var queryParams = QueryHelpers.ParseQuery(decodedQuery);

				string? username = queryParams["username"];
				string? password = queryParams["password"];

				var user = authService.Authenticate(username, password);
				if (user is null)
					return Results.Redirect("/login");

				var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, "deepak")
			};

				var identity = new ClaimsIdentity(claims, "Cookies");
				var principal = new ClaimsPrincipal(identity);

				await http.SignInAsync("Cookies", principal);

				return Results.Redirect(redirect ?? "/");
			});
			app.MapGet("/api/logout", async (HttpContext http, string? redirect) =>
			{
				await http.SignOutAsync("Cookies");

				return Results.Redirect(redirect ?? "/");
			});
		}
	}
}
