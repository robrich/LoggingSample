namespace LoggingSample.DataAccess {
	using System;
	using System.Data.Common;
	using System.Data.SqlClient;

	public interface IDbExceptionHandler {
		bool RetryException(Exception ex);
	}

	public class DbExceptionHandler : IDbExceptionHandler {

		public bool RetryException(Exception ex) {
			Exception innerEx = ex;
			while (innerEx != null) {
				if (innerEx is DbException && innerEx.Message.ToLowerInvariant().Contains("deadlock")) {
					return true;
				} else if (innerEx is SqlException && innerEx.Message.ToLowerInvariant().Contains("timeout")) {
					return true;
				} else if (innerEx is InvalidOperationException && innerEx.Message.ToLowerInvariant().Contains("reader is closed")) {
					return true;
				} else {
					innerEx = innerEx.InnerException;
				}
			}
			return false;
		}

	}
}
