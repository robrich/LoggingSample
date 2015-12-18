namespace LoggingSample.Web.Helpers {
	using System.Web;
	using System.Web.Mvc;

	public static class StringHelpers {

		/// <summary>
		/// Ouutput the string the way one would just by outputing the content (@Content) but also turn \\n into &lt;br /&gt;
		/// </summary>
		public static IHtmlString CRLF(this HtmlHelper Html, string Content) {
			string result = null;
			if (!string.IsNullOrEmpty(Content)) {
				result = Html.Encode(Content).Replace("\n", "<br />");
			} else {
				result = "";
			}
			return new HtmlString(result);
		}

	}
}
