namespace LoggingSample.Web.Models {
	using System.Collections.Generic;
	using LoggingSample.Entity;
	using LoggingSample.Entity.Helpers;
	using LoggingSample.Entity.Models;

	public class ErrorLogViewModel : ErrorLog {
		public ErrorLogViewModel(ErrorLog ErrorLog) {
			this.Id = ErrorLog.Id;
			this.CreateDate = ErrorLog.CreateDate;
			this.ModifyDate = ErrorLog.ModifyDate;
			this.UserId = ErrorLog.UserId;
			this.IsActive = ErrorLog.IsActive;

			this.UserMessage = ErrorLog.UserMessage;
			this.ExceptionDetails = ErrorLog.ExceptionDetails;
			this.HttpMethod = ErrorLog.HttpMethod;
			this.Url = ErrorLog.Url;
			this.ReferrerUrl = ErrorLog.ReferrerUrl;
			this.UserAgent = ErrorLog.UserAgent;
			this.ClientAddress = ErrorLog.ClientAddress;
			this.Headers = ErrorLog.Headers;
			this.Body = ErrorLog.Body;
		}
		public string UserEmail { get; set; }
		public string UserFirstName { get; set; }
		public ExceptionInfo ExceptionInfo { get; set; }
		public List<HeaderInfo> HeaderInfo { get; set; }
	}
}
