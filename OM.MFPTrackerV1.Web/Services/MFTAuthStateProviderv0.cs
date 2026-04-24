
//using Microsoft.AspNetCore.Components.Authorization;
//using OM.MFPTrackerV1.Web.Models;
//using System.Security.Claims;

//namespace OM.MFPTrackerV1.Web.Services
//{
//	public class MFTAuthStateProviderv0 : AuthenticationStateProvider
//	{
//		private ClaimsPrincipal _currentUser =
//			new ClaimsPrincipal(new ClaimsIdentity());

//		public override Task<AuthenticationState> GetAuthenticationStateAsync()
//		{
//			return Task.FromResult(new AuthenticationState(_currentUser));
//		}

//		public void Login(AppUser user)
//		{
//			var identity = new ClaimsIdentity(new[]
//			{
//			new Claim(ClaimTypes.Name, user.Username),
//			new Claim(ClaimTypes.Role, user.Role)
//		}, authenticationType: "LocalAuth");

//			_currentUser = new ClaimsPrincipal(identity);

//			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
//		}

//		public void Logout()
//		{
//			_currentUser = new ClaimsPrincipal(new ClaimsIdentity());
//			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
//		}
//	}
//}