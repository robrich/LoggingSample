namespace LoggingSample.Service {
	using System;
	using System.Threading;
	using LoggingSample.Entity;
	using LoggingSample.Entity.Helpers;
	using LoggingSample.Infrastructure;
	using LoggingSample.Repository;
	using Newtonsoft.Json;

	public class Logger : LoggerService, ILogger {

		public Logger(IErrorLogRepository ErrorLogRepository, ILogSerializer LogSerializer, ILogErrorEmailer LogErrorEmailer, IUserCurrentService UserCurrentService)
			: base(ErrorLogRepository, LogSerializer, LogErrorEmailer, UserCurrentService) {
		}

		public int? Log(string Message, Exception ex = null, bool SkipEmail = false) {
			return base.Log(Message, ex, SkipEmail, RequestUrlOverride: null);
		}

		public int? Log(Exception ex, bool SkipEmail = false) {
			return this.Log(null, ex, SkipEmail);
		}
	}

	/// <summary>
	/// For all but the weirdest uses, use ILogger instead
	/// </summary>
	public interface ILoggerService {
		int? Log(string Message, Exception ex = null, bool SkipEmail = false, string RequestUrlOverride = null);
	}

	/// <summary>
	/// For all but the weirdest uses, use ILogger instead
	/// </summary>
	public class LoggerService : ILoggerService {
		private readonly IErrorLogRepository errorLogRepository;
		private readonly ILogSerializer logSerializer;
		private readonly ILogErrorEmailer logErrorEmailer;
		private readonly IUserCurrentService userCurrentService;

		public LoggerService(IErrorLogRepository ErrorLogRepository, ILogSerializer LogSerializer, ILogErrorEmailer LogErrorEmailer, IUserCurrentService UserCurrentService) {
			if (ErrorLogRepository == null) {
				throw new ArgumentNullException("ErrorLogRepository");
			}
			if (LogSerializer == null) {
				throw new ArgumentNullException("LogSerializer");
			}
			if (LogErrorEmailer == null) {
				throw new ArgumentNullException("LogErrorEmailer");
			}
			if (UserCurrentService == null) {
				throw new ArgumentNullException("UserCurrentService");
			}
			this.errorLogRepository = ErrorLogRepository;
			this.logSerializer = LogSerializer;
			this.logErrorEmailer = LogErrorEmailer;
			this.userCurrentService = UserCurrentService;
		}

		public int? Log(string Message, Exception ex = null, bool SkipEmail = false, string RequestUrlOverride = null) {
			try {
				return this.PrivateLog(Message, ex, SkipEmail, RequestUrlOverride);
			} catch (Exception ex2) {
				// Don't error trying to error
				if (ex2 is ThreadAbortException) {
					throw;
				}

				// Try again
				try {
					const bool skipEmail = false; // always send an email if we failed to log
					return this.PrivateLog("Error saving to error log", ex2, skipEmail, RequestUrlOverride);
				} catch (Exception ex3) {
					// Don't error trying to error
					if (ex3 is ThreadAbortException) {
						throw;
					}
#if DEBUG
					throw;
#else
					return null;
#endif
				}
			}
		}

		private int? PrivateLog(string Message, Exception ex, bool SkipEmail, string RequestUrlOverride) {

			ErrorLog li = new ErrorLog {
				UserMessage = Message
			};

			// Date, UserId, etc are inherint in IEntity and Repository<T>

			this.logSerializer.GetRequestDetails(li);

			if (!string.IsNullOrEmpty(RequestUrlOverride)) {
				li.Url = RequestUrlOverride;
			}

			li.UserId = this.userCurrentService.UserId;
			User currentUser = null;
			try {
				currentUser = this.userCurrentService.User;
			} catch (Exception ex3) {
				// Don't error trying to error
				if (ex3 is ThreadAbortException) {
					throw;
				}
				// Swallow
			}

			ExceptionInfo exInfo = this.logSerializer.GetExceptionInfo(ex);
			li.ExceptionDetails = JsonConvert.SerializeObject(exInfo);

			this.errorLogRepository.Save(li);

			if (!SkipEmail) {
				this.logErrorEmailer.SendErrorEmail(li, currentUser);
			}

			if (ex != null && ex is ThreadAbortException) {
				throw ex; // Can't just "throw" because we aren't necessarily inside a catch, so you'll lose the original stack trace
			}

			return li.Id;
		}

	}
}
