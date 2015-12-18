namespace LoggingSample.Infrastructure {
	using System;

	public interface ILogger {
		// TODO: include log level?
		int? Log(string Message, Exception ex = null, bool SkipEmail = false);
		int? Log(Exception ex, bool SkipEmail = false);
	}
}
