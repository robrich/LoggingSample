namespace LoggingSample.Library {
	public static class StringHelpers {

		public static string TrimToLength(this string Content, int Length) {
			string result = null;
			if (Content != null) {
				if (Content.Length > Length) {
					result = Content.Substring(0, Length);
				} else {
					result = Content;
				}
			}
			return result;
		}

	}
}
