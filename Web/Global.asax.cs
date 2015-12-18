namespace LoggingSample.Web {
	using System;
	using System.Web;
	using System.Web.Http;
	using System.Web.Mvc;
	using System.Web.Routing;
	using LoggingSample.Library;
	using LoggingSample.Service;
	using LoggingSample.Service.Models;
	using LoggingSample.Web.App_Start;
	using LoggingSample.Web.Controllers;

	public class MvcApplication : HttpApplication {
		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
		}

		protected void Application_Error(object sender, EventArgs e) {
			Exception ex = this.Server.GetLastError();
			if (ex == null) {
				return;
			}
			IErrorHandlerService svc = ServiceLocator.GetService<IErrorHandlerService>();
			Uri url = null;
			HttpContext context = HttpContext.Current;
			if (context != null) {
				url = context.Request.Url;
			}
			ErrorHandledResult result = svc.HandleError(ex, url);
			if (!result.Handled) {
				return;
			}
			this.Server.ClearError();
			this.Response.Clear();
			// Server.Transfer() doesn't work here -- iduno why
			// Response.Redirect() polutes SEO
			// So just execute the appropriate action right here instead
			// http://stackoverflow.com/questions/1171035/asp-net-mvc-custom-error-handling-application-error-global-asax
			RouteData routeData = new RouteData();
			routeData.Values.Add("controller", "Error");
			routeData.Values.Add("action", result.ViewName);
			if (result.ErrorId != null) {
				routeData.Values.Add("ErrorId", result.ErrorId);
			}
			IController errorController = ServiceLocator.GetService<ErrorController>();
			errorController.Execute(new RequestContext(new HttpContextWrapper(this.Context), routeData));
		}

	}
}
