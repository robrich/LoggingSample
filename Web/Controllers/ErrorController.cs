namespace LoggingSample.Web.Controllers {
	using System;
	using System.Web.Mvc;
	using LoggingSample.Service;
	using LoggingSample.Web.Models;

	public class ErrorController : Controller {
		private readonly ILoggerService loggerService;

		public ErrorController(ILoggerService LoggerService) {
			if (LoggerService == null) {
				throw new ArgumentNullException("LoggerService");
			}
			this.loggerService = LoggerService;
		}

		/// <summary>
		/// FRAGILE: ErrorHandlerService references this actions by name
		/// </summary>
		public ActionResult Gone() {
			return this.View();
		}

		/// <summary>
		/// FRAGILE: ErrorHandlerService references this actions by name
		/// </summary>
		public ActionResult NotFound() {
			return this.View();
		}

		/// <summary>
		/// FRAGILE: ErrorHandlerService references this actions by name
		/// </summary>
		public ActionResult Error(int? id) {
			return this.View(new ErrorModel {ErrorId = id});
		}

		// Parameters match object passed from jquery.log.js
		// Avoid the "no such url" error: //[HttpPost]
		public ActionResult Log(string message, string errorUrl, string referrerUrl) {
			int? errorId = null;
			if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(errorUrl)) { // else there's nothing to log
				string mess = "from JS: " + message;
				if (!string.IsNullOrEmpty(errorUrl)) {
					mess += ", URL: " + errorUrl;
				}
				if (!string.IsNullOrEmpty(referrerUrl)) {
					mess += ", Referrer: " + referrerUrl;
				}
				errorId = this.loggerService.Log(mess, RequestUrlOverride: errorUrl);
			}
			return this.Json(new {errorId = errorId}, JsonRequestBehavior.AllowGet);
		}

	}
}
