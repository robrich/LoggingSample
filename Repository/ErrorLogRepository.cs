namespace LoggingSample.Repository {
	using System.Collections.Generic;
	using System.Linq;
	using LoggingSample.DataAccess;
	using LoggingSample.Entity;
	using LoggingSample.Library;

	public interface IErrorLogRepository : IRepository<ErrorLog> {
		PartialList<ErrorLog> GetPage(int PageNumber /* base 0 */, int PageSize);
	}

	public class ErrorLogRepository : Repository<ErrorLog>, IErrorLogRepository {

		public ErrorLogRepository(ILoggingSampleDbContextFactory Factory, IRetryinator Retryinator, IDbExceptionHandler DbExceptionHandler)
			: base(Factory, Retryinator, DbExceptionHandler) {
		}

		public PartialList<ErrorLog> GetPage(int PageNumber /* base 0 */, int PageSize) {
			int page = PageNumber;
			if (page < 0) {
				page = 0;
			}
			int pageSize = PageSize;
			if (pageSize < 1) {
				pageSize = 50;
			}
			using (ILoggingSampleDbContext db = this.Factory.GetContext()) {
				var query = (
					from e in db.ErrorLogs
					orderby e.Id descending
					select e
				);

				List<ErrorLog> data = query.Skip(pageSize * page).Take(pageSize).ToList();

				int totalItems = 0;
				if (data.Count < pageSize && (data.Count > 0 || page == 0)) {
					totalItems = data.Count + (pageSize * page); // We've got less than a full page and can figure out how many there are
				} else {
					totalItems = query.Count(); // Have to ask the db how many there are
				}

				return new PartialList<ErrorLog>(data, totalItems);
			}
		}

	}
}
