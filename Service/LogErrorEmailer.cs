namespace LoggingSample.Service {
	using System;
	using System.Diagnostics;
	using System.Net.Mail;
	using System.Text;
	using System.Threading;
	using System.Web;
	using System.Web.Hosting;
	using LoggingSample.Entity;
	using LoggingSample.Repository;

	public interface ILogErrorEmailer {
		/// <summary>
		/// Doesn't use Mailman.SendEmail() to avoid loop, Ignores SettingRepository.EmailEnabled
		/// </summary>
		void SendErrorEmail(ErrorLog ErrorLog, User CurrentUser);
	}

	public class LogErrorEmailer : ILogErrorEmailer {
		private readonly ISettingRepository settingRepository;

		public LogErrorEmailer(ISettingRepository SettingRepository) {
			if (SettingRepository == null) {
				throw new ArgumentNullException("SettingRepository");
			}
			this.settingRepository = SettingRepository;
		}

		/// <summary>
		/// Doesn't use MailService.SendEmail() to avoid dependency loop, Ignores SettingRepository.EmailEnabled
		/// </summary>
		public void SendErrorEmail(ErrorLog ErrorLog, User CurrentUser) {

			StringBuilder sb = new StringBuilder();

			string subject = ErrorLog.UserMessage;
			if (string.IsNullOrWhiteSpace(subject)) {
				subject = ErrorLog.ExceptionDetails;
			}
			if (!string.IsNullOrEmpty(subject)) {
				subject = subject.Replace('\r', ' ').Replace('\n', ' ');
			}

			sb.AppendLine("ErrorLogId: " + ErrorLog.Id);
			sb.AppendLine("Date: " + ErrorLog.CreateDate.ToString("G"));
			sb.AppendLine("Url: " + ErrorLog.HttpMethod+" "+ ErrorLog.Url);
			sb.AppendLine("Referrer: " + ErrorLog.ReferrerUrl);
			sb.AppendLine("User: " + (CurrentUser != null ? (CurrentUser.Id + " " + CurrentUser.FirstName + " " + CurrentUser.Email) : null));
			sb.AppendLine("User-Agent: " + ErrorLog.UserAgent);
			sb.AppendLine();
			sb.AppendLine("UserMessage: " + ErrorLog.UserMessage);
			sb.AppendLine();
			sb.AppendLine("Exception: " + (ErrorLog.ExceptionDetails ?? "").Replace("{", "\n{").Replace("}", "\n}\n"));

			MailMessage message = new MailMessage(this.settingRepository.EmailFrom, this.settingRepository.ErrorEmail) {
				Subject = string.Format("[LoggingSample.com-Error] in {0} id {1}: {2}", this.settingRepository.EnvorionmentName, ErrorLog.Id, subject),
				Body = sb.ToString()
			};

#if DEBUG
			if (HostingEnvironment.IsHosted && HttpContext.Current != null && HttpContext.Current.Request.IsLocal) {
				return; // No need to email locally run requests
			}
			if (Debugger.IsAttached) {
				return; // No need to email when debugging
			}
#endif

			try {

				SmtpClient client = new SmtpClient(); // TODO: authenticate to the mail server
				client.Send(message);

			} catch (Exception ex) {
				// Don't error trying to error
				if (ex is ThreadAbortException) {
					throw;
				}
#if DEBUG
				throw;
#endif
			}
		}

	}
}
