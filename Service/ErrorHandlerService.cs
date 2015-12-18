namespace LoggingSample.Service {
	using System;
	using System.Configuration;
	using System.Web;
	using System.Web.Configuration;
	using System.Web.Hosting;
	using LoggingSample.Infrastructure;
	using LoggingSample.Service.Models;

	public interface IErrorHandlerService {
		ErrorHandledResult HandleError(Exception ex, Uri RequestUrl);
	}

	public class ErrorHandlerService : IErrorHandlerService {
		private readonly ILogger logger;
		private readonly IErrorIdentifierService errorIdentifierService;

		private bool errorIsHandled { get; set; }
		private bool errorIsHandledSet { get; set; }

		public ErrorHandlerService(ILogger Logger, IErrorIdentifierService ErrorIdentifierService) {
			if (Logger == null) {
				throw new ArgumentNullException("Logger");
			}
			if (ErrorIdentifierService == null) {
				throw new ArgumentNullException("ErrorIdentifierService");
			}
			this.logger = Logger;
			this.errorIdentifierService = ErrorIdentifierService;
		}

		// public to be testable, not part of interface
		/// <summary>
		/// Cache the answer of the relatively expensive ErrorIsHandledInternal()
		/// </summary>
		public bool ErrorIsHandled() {
			if (!this.errorIsHandledSet) {
				// Yeah, I could lock, but it isn't that expensive to get the answer, so let both threads do it
				this.errorIsHandled = this.ErrorIsHandledInternal();
				this.errorIsHandledSet = true;
			}
			return this.errorIsHandled;
		}

		// public to be testable, not part of interface
		public bool ErrorIsHandledInternal() {

			if (!HostingEnvironment.IsHosted) {
				return false; // Services don't get web.config handling
			}

			CustomErrorsMode redirectMode = CustomErrorsMode.RemoteOnly;

			bool handled = true;

			// Read web.config's line like so:
			// <customErrors mode="RemoteOnly" />
			Configuration configuration = WebConfigurationManager.OpenWebConfiguration("/");
			SystemWebSectionGroup systemWeb = (SystemWebSectionGroup)configuration.GetSectionGroup("system.web");
			if (systemWeb != null && systemWeb.CustomErrors != null) {
				redirectMode = systemWeb.CustomErrors.Mode;
			}

			switch (redirectMode) {
				case CustomErrorsMode.Off:
					handled = false;
					break;
				case CustomErrorsMode.On:
					handled = true;
					break;
				case CustomErrorsMode.RemoteOnly:
					if (HostingEnvironment.IsHosted) {
						try {
							handled = !HttpContext.Current.Request.IsLocal;
						} catch {
							// Don't error while trying to handle an error
							handled = false;
						}
					}
					break;
			}

			return handled;
		}

		public ErrorHandledResult HandleError(Exception ex, Uri RequestUrl) {

			if (ex != null && ex.InnerException != null && ex is HttpUnhandledException) {
				ex = ex.InnerException;
			}

			int? errorId = null;

			if (this.errorIdentifierService.ShouldLogException(ex, RequestUrl)) {
				errorId = this.logger.Log(ex: ex);
			}

			string destViewName = "Error";
			bool handled = false;
			if (this.errorIdentifierService.IsNotFoundException(ex) || this.errorIdentifierService.IsNoRouteException(ex) || this.errorIdentifierService.IsNullParameterException(ex)) {
				// No need to bubble up a 404
				destViewName = "NotFound";
				handled = true;
			} else if (this.errorIdentifierService.IsHackAttempt(ex, RequestUrl) || this.errorIdentifierService.IsPotentiallyDangerous(ex)) {
				// No need to bubble up a 410
				destViewName = "Gone";
				handled = true;
			} else {
				// Log
				destViewName = "Error";
				handled = this.ErrorIsHandled();
			}
			return new ErrorHandledResult {
				Handled = handled,
				ViewName = destViewName,
				ErrorId = errorId
			};
		}

	}
}
