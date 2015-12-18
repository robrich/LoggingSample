namespace LoggingSample.Web.Helpers {
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;

	public static class ResponseContentTypeHelper {

		public static bool RequestedJson(this HtmlHelper HtmlHelper) {
			HttpRequestBase Request = HtmlHelper.ViewContext.RequestContext.HttpContext.Request;
			bool returnJson = (Request.AcceptTypes ?? new string[0]).Any(t => t != null && t.Contains("json")) || Request.ContentType.Contains("json");
			bool returnHtml = (Request.AcceptTypes ?? new string[0]).Any(t => t != null && t.Contains("html")) || Request.ContentType.Contains("html");
			if (returnHtml) {
				returnJson = false;
			}
			return returnJson;
		}

	}
}
