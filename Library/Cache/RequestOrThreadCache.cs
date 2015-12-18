namespace LoggingSample.Library.Cache {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Web;
	using System.Web.Hosting;

	public interface IRequestOrThreadCache<T> : ICache<T> {
	}

	/// <summary>
	/// Cache for this request (if hosted) or this thread (if not hosted)
	/// FRAGILE: Ensure this is rigged as a singleton in the IoC container to ensure all uses leverage the same dictionary
	/// </summary>
	public class RequestOrThreadCache<T> : CacheDict<T>, IRequestOrThreadCache<T> {

		// ReSharper disable once StaticFieldInGenericType
		[ThreadStatic]
		private static Dictionary<string, object> _threadDictionary;

		protected override IDictionary DataStore {
			get {
				if (HostingEnvironment.IsHosted) {
					HttpContext context = HttpContext.Current;
					if (context == null) {
						throw new ArgumentNullException("HttpContext.Current", "Not in a request");
					}
					return context.Items;
				} else {
					// Don't need to lock because only this thread can get to it anyway
					if (_threadDictionary == null) {
						_threadDictionary = new Dictionary<string, object>();
					}
					return _threadDictionary;
				}
			}
		}

	}
}
