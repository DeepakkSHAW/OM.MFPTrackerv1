using System.ComponentModel.DataAnnotations;

namespace OM.MFPTrackerV1.Web.Models
{
	public class AppUser
	{
		public string Username { get; init; } = string.Empty;
		public string PasswordHash { get; init; } = string.Empty;
		public string Role { get; init; } = string.Empty;

		// Optional internal fields
		//public bool IsActive { get; init; }
	}
	public sealed class LoginRequest
	{
		[Required(ErrorMessage = "Username is required")]
		public string Username { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password is required")]
		public string Password { get; set; } = string.Empty;
	}

	public sealed class LoginResult
	{
		public bool Success { get; set; }
		public string? Reason { get; set; }
	}
}
