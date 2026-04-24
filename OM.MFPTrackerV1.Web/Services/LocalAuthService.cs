//using OM.MFPTrackerV1.Web.Models;

//namespace OM.MFPTrackerV1.Web.Services
//{
//	public class LocalAuthService
//	{
//		private readonly IConfiguration _config;

//		public LocalAuthService(IConfiguration config)
//		{
//			_config = config;
//		}

//		public AppUser Authenticate(string username, string password)
//		{



//			var users = _config.GetSection("Auth:Users").Get<List<AppUser>>();

//			var user = users.FirstOrDefault(u =>
//				u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

//			if (user == null)
//				return null;

//			if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
//				return null;

//			return user;
//		}
//	}
//}
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using OM.MFPTrackerV1.Web.Models;

namespace OM.MFPTrackerV1.Web.Services
{
	public sealed class LocalAuthService
	{
		private readonly IConfiguration _configuration;
		//Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("Admin@123!"));
		//Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("User@123!"));
		public LocalAuthService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public bool IsLoginEnabled()
		{
			return _configuration.GetValue<bool>("Auth:LoginEnabled");
		}

		public AppUser? Authenticate(string username, string password)
		{
			if (!IsLoginEnabled())
				return null;

			var users = _configuration
				.GetSection("Auth:Users")
				.Get<List<AppUser>>();

			if (users == null || users.Count == 0)
				return null;

			var user = users.FirstOrDefault(u =>
				string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

			if (user == null)
				return null;

			if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
				return null;

			return new AppUser
			{
				Username = user.Username,
				Role = user.Role
			};
		}
	}
}
