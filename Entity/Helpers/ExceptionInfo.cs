namespace LoggingSample.Entity.Helpers {
	using System.Collections.Generic;

	public class ExceptionInfo {

		public ExceptionInfo() {
			this.Data = new List<string>();
		}

		public string Message { get; set; }
		public string StackTrace { get; set; }
		public string ExceptionType { get; set; }
		public List<string> Data { get; set; }
		public ExceptionInfo InnerException { get; set; }
		public List<ExceptionInfo> InnerExceptions { get; set; }

	}
}
