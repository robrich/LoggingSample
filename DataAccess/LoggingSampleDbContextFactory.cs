namespace LoggingSample.DataAccess {
	using LoggingSample.Library;

	public interface ILoggingSampleDbContextFactory {
		ILoggingSampleDbContext GetContext();
	}

	public class LoggingSampleDbContextFactory : ILoggingSampleDbContextFactory {
		public ILoggingSampleDbContext GetContext() {
			return ServiceLocator.GetService<ILoggingSampleDbContext>();
		}
	}
}
