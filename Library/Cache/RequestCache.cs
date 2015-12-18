namespace LoggingSample.Library.Cache {
	using System;
	using System.Collections;
	using System.Web;
	using System.Web.Hosting;

	public interface IRequestCache<T> : ICache<T> {
	}

	/// <summary>
	/// Cache for this request
	/// </summary>
	public class RequestCache<T> : CacheDict<T>, IRequestCache<T> {

		/// <summary>
		/// If you're not in a request, shame on you
		/// </summary>
		protected override IDictionary DataStore {
			get {
				if (!HostingEnvironment.IsHosted) {
					throw new ArgumentNullException("HostingEnvironment.IsHosted", "Must be hosted to get a RequestCache");
				}
				HttpContext context = HttpContext.Current;
				if (context == null) {
					throw new ArgumentNullException("HttpContext.Current", "Not in a request");
				}
				return context.Items;
			}
		}

	}
}
