namespace LoggingSample.Service {
	using System;
	using System.Text.RegularExpressions;
	using System.Web;

	public interface IErrorIdentifierService {
		bool IsNotFoundException(Exception ex);
		bool IsNoRouteException(Exception ex);
		bool IsNullParameterException(Exception ex);
		bool IsHackAttempt(Exception ex, Uri RequestUrl);
		bool IsPotentiallyDangerous(Exception ex);
		bool IsDatabaseTimeoutException(Exception ex);
		bool ShouldLogException(Exception ex, Uri RequestUrl);
	}

	public class ErrorIdentifierService : IErrorIdentifierService {

		public bool IsNotFoundException(Exception ex) {
			bool result = false;
			if (ex != null && ex.GetType() == typeof(HttpException)) {
				if (!string.IsNullOrEmpty(ex.Message)
					&& (ex.Message.EndsWith(" does not exist.") || ex.Message.Contains(" was not found"))) {
					result = true;
				}
			}
			return result;
		}

		public bool IsNoRouteException(Exception ex) {
			bool result = false;
			if (ex != null && ex.GetType() == typeof(InvalidOperationException)) {
				if (!string.IsNullOrEmpty(ex.Message) && ex.Message.StartsWith("No route in the route table ")) {
					result = true;
				}
			}
			return result;
		}

		public bool IsNullParameterException(Exception ex) {
			bool result = false;
			if (ex != null && ex.GetType() == typeof(ArgumentException)) {
				if (!string.IsNullOrEmpty(ex.Message) && ex.Message.StartsWith("The parameters dictionary contains a null entry for parameter ")) {
					result = true;
				}
			}
			return result;
		}

		private static Regex ipRegex = new Regex(@"(\d{1,3}\.){3}\d{1,3}");
		public bool IsHackAttempt(Exception ex, Uri RequestUrl) {
			bool result = false;
			if (ex != null && ex.GetType() == typeof(HttpException)) {
				if (RequestUrl != null && ipRegex.IsMatch(RequestUrl.Authority)) {
					result = true;
				}
			}
			return result;
		}

		public bool IsPotentiallyDangerous(Exception ex) {
			bool result = false;
			if (ex != null && ex.GetType() == typeof(HttpException)) {
				if (!string.IsNullOrEmpty(ex.Message) && ex.Message.StartsWith("A potentially dangerous Request.Path value was detected ")) {
					result = true;
				}
			}
			return result;
		}

		public bool IsDatabaseTimeoutException(Exception ex) {
			bool result = false;
			Exception innerEx = ex;
			while (innerEx != null) {
				if (innerEx != null && !string.IsNullOrEmpty(innerEx.Message)) {
					if (innerEx.Message.StartsWith("A transport-level error has occurred ") ||
						innerEx.Message.StartsWith("The semaphore timeout period has expired") ||
						innerEx.Message.StartsWith("The wait operation timed out")) {
						result = true;
						break;
					}
				}
				innerEx = innerEx.InnerException;
			}
			return result;
		}

		public bool ShouldLogException(Exception ex, Uri RequestUrl) {

			if (ex != null && ex.InnerException != null && ex is HttpUnhandledException) {
				ex = ex.InnerException;
			}

			bool logIt = true; // nothing has disputed it yet
			if (this.IsNotFoundException(ex) || this.IsNoRouteException(ex) || this.IsNullParameterException(ex)) {
				// No need to log a 404
				logIt = false;
			} else if (this.IsHackAttempt(ex, RequestUrl) || this.IsPotentiallyDangerous(ex)) {
				// No need to log a 410
				logIt = false;
			} else {
				// Log
				logIt = true;
			}
			return logIt;
		}

	}
}
