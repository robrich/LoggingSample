namespace LoggingSample.Service {
	using System;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Hosting;
	using LoggingSample.Entity;
	using LoggingSample.Library.Cache;
	using LoggingSample.Repository;

	public interface IUserCurrentService {
		User User { get; }
		int? UserId { get; }
		bool IsAuthenticated { get; }
		string Email { get; }
		string FirstName { get; }
	}

	/// <summary>
	/// Get the user currently authenticated to the website<br />
	/// FRAGILE: Though technically they're more like methods, everyone uses them like properties, so let's just call them properties.
	/// </summary>
	public class UserCurrentService : IUserCurrentService {
		private readonly IUserRepository userRepository;
		private readonly IThreadCache<string> identityCache;
		private readonly IRequestOrThreadCache<User> currentUserCache;

		private const string IDENTITY_KEY = "IdentityName";

		public UserCurrentService(IUserRepository UserRepository, IThreadCache<string> IdentityCache, IRequestOrThreadCache<User> CurrentUserCache) {
			if (UserRepository == null) {
				throw new ArgumentNullException("UserRepository");
			}
			if (IdentityCache == null) {
				throw new ArgumentNullException("IdentityCache");
			}
			if (CurrentUserCache == null) {
				throw new ArgumentNullException("CurrentUserCache");
			}
			this.userRepository = UserRepository;
			this.identityCache = IdentityCache;
			this.currentUserCache = CurrentUserCache;

			this.identityCache.KeyPrefix = IDENTITY_KEY;
		}

		// public for testing, not part of interface
		public string CookieToken {
			get {
				if (HostingEnvironment.IsHosted) {
					HttpContext context = HttpContext.Current;
					if (context != null) {
						IPrincipal principal = context.User;
						if (principal != null && principal.Identity.IsAuthenticated) {
							return principal.Identity.Name;
						}
					}
				} else {
					return this.identityCache.Get(IDENTITY_KEY);
				}
				return null;
			}
		}

		public User User {
			get {
				// FRAGILE: ASSUME: There isn't another thread modifying another copy
				string sessionToken = this.CookieToken;
				if (string.IsNullOrEmpty(sessionToken)) {
					return null;
				}
				User user = this.currentUserCache.Retrieve(() => this.userRepository.GetActiveUserBySessionToken(sessionToken));
				if (user != null && user.IsActive) {
					return user;
				} else {
					return null;
				}
			}
		}

		public int? UserId {
			get {
				User u = this.User;
				return u != null ? u.Id : (int?)null;
			}
		}

		public bool IsAuthenticated {
			get {
				User u = this.User;
				return u != null;
			}
		}

		public string Email {
			get {
				User u = this.User;
				return u != null ? u.Email : null;
			}
		}

		public string FirstName {
			get {
				User u = this.User;
				return u != null ? (u.FirstName ?? u.Email) : null;
			}
		}

	}
}
