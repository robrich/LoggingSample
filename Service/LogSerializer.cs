namespace LoggingSample.Service {
	using System;
	using System.Collections.Generic;
	using System.Data.Entity.Validation;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Web;
	using System.Web.Hosting;
	using LoggingSample.Entity;
	using LoggingSample.Entity.Helpers;
	using LoggingSample.Entity.Models;
	using LoggingSample.Library;
	using Newtonsoft.Json;

	public interface ILogSerializer {
		void GetRequestDetails(ErrorLog li);
		ExceptionInfo GetExceptionInfo(Exception ex);
	}

	public class LogSerializer : ILogSerializer {

		public void GetRequestDetails(ErrorLog li) {
			if (!HostingEnvironment.IsHosted) {
				return;
			}
			HttpContext context = HttpContext.Current;
			if (context == null || context.Request == null) {
				return;
			}

			li.HttpMethod = context.Request.HttpMethod;
			li.Url = context.Request.Url.OriginalString;
			li.UserAgent = context.Request.UserAgent;
			li.ClientAddress = context.Request.UserHostAddress;
			if (context.Request.UrlReferrer != null) {
				li.ReferrerUrl = context.Request.UrlReferrer.OriginalString;
			}

			List<HeaderInfo> headers = new List<HeaderInfo>();
			for (int i = 0; i < context.Request.Headers.Count; i++) {
				headers.Add(new HeaderInfo {
					Name = context.Request.Headers.Keys[i],
					Value = context.Request.Headers[i]
				});
			}
			li.Headers = JsonConvert.SerializeObject(headers);

			using (StreamReader reader = new StreamReader(context.Request.InputStream)) {
				try {
					context.Request.InputStream.Position = 0;
					li.Body = reader.ReadToEnd();
				} finally {
					context.Request.InputStream.Position = 0;
				}
			}
		}

		public ExceptionInfo GetExceptionInfo(Exception ex) {
			if (ex == null) {
				return null;
			}
			ExceptionInfo exInfo = new ExceptionInfo {
				Message = ex.Message,
				StackTrace = ex.StackTrace,
				ExceptionType = ex.GetType().ToString()
			};
			if (ex.Data != null && ex.Data.Count > 0) {
				List<string> data = new List<string>();
				foreach (string key in ex.Data.Keys) {
					var value = ex.Data[key];
					if (value != null) {
						data.Add(key + ": " + value);
					}
				}
				exInfo.Data = data;
			}

			AggregateException aEx = ex as AggregateException;
			if (aEx != null) {
				exInfo.InnerExceptions = (
					from i in aEx.InnerExceptions
					select this.GetExceptionInfo(i) // recurse
				).ToList();
			}

			DbEntityValidationException dbEx = ex as DbEntityValidationException;
			if (dbEx != null) {
				List<string> validationErrors = (
					from e in dbEx.EntityValidationErrors
					from f in e.ValidationErrors
					select f.PropertyName + ": " + f.ErrorMessage
				).ToList();
				if (!validationErrors.IsNullOrEmpty()) {
					exInfo.Data.AddRange(validationErrors);
				}
			}

			WebException wEx = ex as WebException;
			if (wEx != null) {
				HttpWebResponse resp = wEx.Response as HttpWebResponse;
				if (resp != null) {

					StringBuilder content = new StringBuilder();
					if (resp.ResponseUri != null) {
						content.AppendLine("URL: " + resp.ResponseUri.OriginalString);
					}
					content.AppendLine("HttpMethod: " + resp.Method);
					content.AppendLine("HttpStatus: " + (int)resp.StatusCode);
					foreach (string header in resp.Headers.AllKeys) {
						content.AppendLine(header + ": " + resp.Headers[header]);
					}
					try {
						string body = null;
						Stream respStream = null;
						try {
							respStream = resp.GetResponseStream();
							if (respStream != null) {
								using (StreamReader sr = new StreamReader(respStream)) {
									respStream = null; // sr will dispose it https://msdn.microsoft.com/query/dev12.query?appId=Dev12IDEF1&l=EN-US&k=k(CA2202);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.5)&rd=true
									body = sr.ReadToEnd();
								}
							}
						} finally {
							if (respStream != null) {
								respStream.Dispose();
							}
						}
						if (!string.IsNullOrEmpty(body)) {
							content.AppendLine();
							content.AppendLine(body);
						}
					} catch (Exception webEx) {
						content.AppendLine("Error getting HttpWebRequest content: " + webEx.Message);
						// Don't error when trying to error
					}

					exInfo.Data.Add(content.ToString());
				}
			}

			if (ex.InnerException != null) {
				exInfo.InnerException = this.GetExceptionInfo(ex.InnerException); // recurse
			}
			return exInfo;
		}

	}
}
