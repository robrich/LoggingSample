namespace LoggingSample.Repository {
	using System.Linq;
	using LoggingSample.DataAccess;
	using LoggingSample.Entity;
	using LoggingSample.Library;

	public interface IUserRepository : IRepository<User> {
		User GetActiveUserBySessionToken(string SessionToken);
	}

	public class UserRepository : Repository<User>, IUserRepository {

		public UserRepository(ILoggingSampleDbContextFactory Factory, IRetryinator Retryinator, IDbExceptionHandler DbExceptionHandler)
			: base(Factory, Retryinator, DbExceptionHandler) {
		}

		public User GetActiveUserBySessionToken(string SessionToken) {
			if (string.IsNullOrWhiteSpace(SessionToken)) {
				return null; // You asked for nothing, you got it
			}
			using (ILoggingSampleDbContext db = this.Factory.GetContext()) {
				return (
					from u in db.Users
					where u.SessionToken == SessionToken
					&& u.IsActive
					select u
				).FirstOrDefault();
			}
		}

	}
}
