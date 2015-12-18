namespace LoggingSample.Library.Cache {
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public interface IThreadCache<T> : ICache<T> {
	}

	/// <summary>
	/// Cache for this thread<br />
	/// FRAGILE: Ensure this is rigged as a singleton in the IoC container to ensure all uses leverage the same dictionary
	/// </summary>
	public class ThreadCache<T> : CacheDict<T>, IThreadCache<T> {

		// ReSharper disable once StaticFieldInGenericType
		[ThreadStatic]
		private static Dictionary<string, object> threadDictionary;

		protected override IDictionary DataStore {
			get {
				// Don't need to lock because only this thread can get to it anyway
				if (threadDictionary == null) {
					threadDictionary = new Dictionary<string, object>();
				}
				return threadDictionary;
			}
		}

	}
}
