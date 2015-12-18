namespace LoggingSample.Web.Filters {
	using System;
	using System.Web.Mvc;
	using LoggingSample.Service;
	using LoggingSample.Service.Models;
	using LoggingSample.Web.Models;

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class HandleAndLogErrorAttribute : FilterAttribute, IExceptionFilter {
		private readonly IErrorHandlerService errorHandlerService;

		public HandleAndLogErrorAttribute(IErrorHandlerService ErrorHandlerService) {
			if (ErrorHandlerService == null) {
				throw new ArgumentNullException("ErrorHandlerService");
			}
			this.errorHandlerService = ErrorHandlerService;
		}

		public virtual void OnException(ExceptionContext context) {
			if (context == null) {
				throw new ArgumentNullException("context");
			}

			Uri url = null;
			if (context.RequestContext != null && context.RequestContext.HttpContext != null && context.RequestContext.HttpContext.Request != null) {
				url = context.RequestContext.HttpContext.Request.Url;
			}
			ErrorHandledResult results = this.errorHandlerService.HandleError(context.Exception, url);

			if (context.IsChildAction) {
				return;
			}

			if (context.ExceptionHandled || !context.HttpContext.IsCustomErrorEnabled || !results.Handled) {
				return;
			}

			// Redirect to the "pretty" page
			context.Result = new ViewResult {
				ViewName = results.ViewName,
				ViewData = new ViewDataDictionary<ErrorModel>(new ErrorModel {ErrorId = results.ErrorId}),
				TempData = context.Controller.TempData
			};
			context.ExceptionHandled = true;
			context.HttpContext.Response.Clear();
			context.HttpContext.Response.TrySkipIisCustomErrors = true;
		}

	}
}
