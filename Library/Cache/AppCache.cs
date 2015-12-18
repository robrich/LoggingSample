namespace LoggingSample.Library.Cache {
	using System;
	using System.Collections;
	using System.Web;
	using System.Web.Caching;
	using System.Web.Hosting;

	public interface IAppCache<T> : ICache<T> {
		int MinutesDefault { get; set; }
		void Add(T Value, string Key = null, int? minutes = null, CacheDependency dependancy = null);
		T Retrieve(Func<T> GetFunc, string Key = null, int? Minutes = null, CacheDependency Dependancy = null);
	}

	/// <summary>
	/// Cache for the whole app
	/// </summary>
	public class AppCache<T> : Cache<T>, IAppCache<T> {
		private readonly object getLock = new object();
		private readonly object runtimeLock = new object();

		public AppCache() {
			this.MinutesDefault = 120;
		}

		// TODO: Get from Setting like AppSettings in Web.Config
		public int MinutesDefault { get; set; }

		// HttpRuntime Cache in non ASP.NET scenarios
		// http://netcode.ru/dotnet/?lang=&katID=30&skatID=283&artID=7851
		private HttpRuntime httpRuntime = null;
		private readonly object httpRuntimeLock = new object();

		private void EnsureHttpRuntime() {
			if (HostingEnvironment.IsHosted) {
				return; // It already exists
			}
			if (this.httpRuntime == null) {
				lock (this.runtimeLock) {
					if (this.httpRuntime == null) {
						// FRAGILE: This doesn't kick off IIS's cleanup processes so things stay in cache much longer
						this.httpRuntime = new HttpRuntime();
					}
				}
			}
		}

		protected Cache DataStore {
			get {
				this.EnsureHttpRuntime();
				return HttpRuntime.Cache;
			}
		}

		protected override object PrivateGet(string KeyPrefixAndKey) {
			return this.DataStore[KeyPrefixAndKey];
		}

		public override void Add(T Value, string Key = null) {
			this.Add(Value, Key, Minutes: null, Dependancy: null);
		}

		public void Add(T Value, string Key = null, int? Minutes = null, CacheDependency Dependancy = null) {
			if (this.Equals(Value, this.tNull)) {
				this.Remove(Key);
				return;
			}
			T localValue = this.WrapList(Value);
			// HttpRuntime.Cache allows inserting over the top if it's already there
			if (Minutes != null && Minutes < 0) {
				this.DataStore.Insert(this.KeyPrefix + Key, localValue, Dependancy, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);
			} else {
				TimeSpan duration = new TimeSpan(0, 0, Minutes ?? this.MinutesDefault);
				this.DataStore.Insert(this.KeyPrefix + Key, localValue, Dependancy, Cache.NoAbsoluteExpiration, duration);
			}
		}

		public override void Remove(string Key) {
			this.DataStore.Remove(this.KeyPrefix + Key);
		}

		// Clears all matches, not just if it's a T
		public override void Clear() {
			Cache cache = this.DataStore;
			lock (cache) {
				IDictionaryEnumerator enumerator = cache.GetEnumerator();
				while (enumerator.MoveNext()) {
					string key = enumerator.Key as string;
					if (key != null) {
						if (!key.StartsWith(this.KeyPrefix, StringComparison.InvariantCultureIgnoreCase)) {
							continue; // Not this one
						}
						try {
							cache.Remove(key);
// ReSharper disable once EmptyGeneralCatchClause
						} catch (Exception /*ex*/) {
							// It expired out from under us
						}
					}
				}
			}
		}

		public override T Retrieve(Func<T> GetFunc, string Key = null) {
			return this.Retrieve(GetFunc, Key, Minutes: null, Dependancy: null);
		}

		public T Retrieve(Func<T> GetFunc, string Key = null, int? Minutes = null, CacheDependency Dependancy = null) {
			// TODO: find a way to use more of base.Get()
			T result = this.Get(Key);
			if (this.Equals(result, this.tNull)) {
				lock (this.getLock) {
					result = this.Get(Key);
					if (this.Equals(result, this.tNull)) {
						result = GetFunc();
						this.Add(result, Key, Minutes, Dependancy);
					}
				}
			}
			return result;
		}

	}
}
