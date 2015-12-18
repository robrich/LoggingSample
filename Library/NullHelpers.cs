namespace LoggingSample.Library {
	using System.Collections.Generic;

	public static class NullHelpers {

		public static bool IsNullOrEmpty(this string source) {
			return string.IsNullOrEmpty(source);
		}

		public static bool IsNullOrEmpty<T>(this List<T> source) {
			return (source == null || source.Count == 0);
		}

		public static bool IsNullOrEmpty<T>(this T[] source) {
			return (source == null || source.Length == 0);
		}

		public static bool IsNullOrEmpty<T, U>(this Dictionary<T, U> source) {
			return (source == null || source.Count == 0);
		}

	}
}
