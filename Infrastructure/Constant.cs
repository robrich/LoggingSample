namespace LoggingSample.Infrastructure {
	using System;
	using System.Configuration;
	using LoggingSample.Library;

	public static class Constant {

		public const string CONNECTION_STRING_NAME = "DefaultConnection";
		public static readonly int COMMAND_TIMEOUT = ConfigurationManager.AppSettings["CommandTimeout"].ToIntOrNull() ?? 30; // seconds

	}
}
