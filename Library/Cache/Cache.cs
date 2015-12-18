namespace LoggingSample.Library.Cache {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	public interface ICache<T> {
		/// <summary>
		/// Retrieve gets from cache, and falls back to the GetFunc if the cache doesn't have it.<br />
		/// Probably this is the only method you'll ever need to use.
		/// </summary>
		T Retrieve(Func<T> GetFunc, string Key = null);
		/// <summary>
		/// Set this at startup (if needed) to make this cache section more unique<br />
		/// Changing it while running will lead to unexpected results
		/// </summary>
		string KeyPrefix { get; set; }
		T Get(string Key);
		void Add(T Value, string Key = null);
		void Remove(string Key = null);
		/// <summary>
		/// Clears all that match KeyPrefix, not just if it's a T
		/// </summary>
		void Clear();
	}

	public abstract class Cache<T> : ICache<T> {
		protected readonly Type tType = typeof(T);
		protected readonly T tNull = default(T);
		private readonly object getLock = new object();

		protected Cache() {
			this.KeyPrefix = this.tType.FullName + "-"; // FRAGILE: Presume the type we're caching is descriptive
			if (this.tType.IsValueType) {
				this.KeyPrefix += Guid.NewGuid().ToString("N") + "-"; // Make the cache key more descriptive
			}
		}

		public virtual T Get(string Key) {
			T result = default(T);
			object o = this.PrivateGet(this.KeyPrefix + Key);
			if (o != null && o is T) {
				result = (T)o;
				result = this.WrapList(result);
			}
			return result;
		}

		protected abstract object PrivateGet(string KeyPrefixAndKey);
		public abstract void Add(T Value, string Key = null);
		public abstract void Remove(string Key = null);
		/// <summary>
		/// Clears all that match KeyPrefix, not just if it's a T
		/// </summary>
		public abstract void Clear();

		public string KeyPrefix { get; set; }

		public virtual T Retrieve(Func<T> GetFunc, string Key = null) {
			T result = this.Get(Key);
			if (this.Equals(result, this.tNull)) {
				lock (this.getLock) {
					result = this.Get(Key);
					if (this.Equals(result, this.tNull)) {
						result = GetFunc();
						this.Add(result, Key);
					}
				}
			}
			return result;
		}

		/// <summary>
		/// If you cached a List&lt;T&gt; then what you want back is a new list, not the original list -- so if you modify your copy you don't break everyone else
		/// </summary>
		protected T WrapList(T result) {
			if (!this.Equals(result, this.tNull)) {
				if (this.tType.IsGenericType && typeof(List<>).IsAssignableFrom(this.tType.GetGenericTypeDefinition())) {
					T newList = (T)Activator.CreateInstance(this.tType, new object[] { result });
					result = newList;
				}
			}
			return result;
		}

		protected bool Equals(T a, T b) {
			return EqualityComparer<T>.Default.Equals(a, b);
		}

	}

	/// <summary>
	/// This class helps shim to Dictionary data stores like Request and Thread
	/// </summary>
	public abstract class CacheDict<T> : Cache<T> {

		protected abstract IDictionary DataStore { get; }

		protected override object PrivateGet(string KeyPrefixAndKey) {
			if (this.DataStore.Contains(KeyPrefixAndKey)) {
				return this.DataStore[KeyPrefixAndKey];
			} else {
				return null;
			}
		}

		public override void Add(T Value, string Key = null) {
			if (this.Equals(Value, this.tNull)) {
				this.Remove(Key);
				return;
			}
			T localValue = this.WrapList(Value);
			if (this.DataStore.Contains(this.KeyPrefix + Key)) {
				this.DataStore[this.KeyPrefix + Key] = localValue;
			} else {
				this.DataStore.Add(this.KeyPrefix + Key, localValue);
			}
		}

		public override void Remove(string Key = null) {
			if (this.DataStore.Contains(this.KeyPrefix + Key)) {
				this.DataStore.Remove(this.KeyPrefix + Key);
			}
		}

		// Clears all matches, not just if it's a T
		public override void Clear() {
			IDictionary cache = this.DataStore;
			lock (cache) {
				List<string> keys = (
					from k in cache.Keys.Cast<string>()
					where !string.IsNullOrEmpty(k)
					&& k.StartsWith(this.KeyPrefix)
					select k
				).ToList();
				foreach (string key in keys) {
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

}
