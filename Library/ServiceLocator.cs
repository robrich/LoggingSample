namespace LoggingSample.Library {
	using System;

	public static class ServiceLocator {
		private static Func<Type, object> getService;
		public static void Initialize(Func<Type, object> GetService) {
			getService = GetService;
		}
		public static object GetService(Type Type) {
			return getService(Type);
		}
		public static T GetService<T>() {
			return (T)GetService(typeof(T));
		}
	}
}
