namespace LoggingSample.Web.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using LoggingSample.Entity;
	using LoggingSample.Entity.Helpers;
	using LoggingSample.Entity.Models;
	using LoggingSample.Library;
	using LoggingSample.Repository;
	using LoggingSample.Web.Models;
	using Newtonsoft.Json;

	// TODO: Secure this [Authorize]
	public class ErrorLogController : Controller {
		private readonly IErrorLogRepository errorLogRepository;
		private readonly IUserRepository userRepository;

		public ErrorLogController(IErrorLogRepository ErrorLogRepository, IUserRepository UserRepository) {
			if (ErrorLogRepository == null) {
				throw new ArgumentNullException("ErrorLogRepository");
			}
			if (UserRepository == null) {
				throw new ArgumentNullException("UserRepository");
			}
			this.errorLogRepository = ErrorLogRepository;
			this.userRepository = UserRepository;
		}

		public ActionResult Index(int? id) {
			int page = id ?? 0;
			PartialList<ErrorLog> results = this.errorLogRepository.GetPage(page, PageSize: 50);
			PartialListPageInfo<ErrorLogViewModel> model = new PartialListPageInfo<ErrorLogViewModel> {
				PageNumber = page,
				PageSize = 50,
				TotalCount = results.TotalCount,
			};
			if (results.Count > 0) {
				List<User> userCache = new List<User>();
				foreach (ErrorLog result in results) {
					ErrorLogViewModel m = new ErrorLogViewModel(result);
					if (m.UserId > 0) {
						User user = userCache.FirstOrDefault(c => c.Id == m.UserId);
						if (user == null) {
							user = this.userRepository.GetById(m.UserId ?? 0);
							if (user != null) {
								userCache.Add(user);
							}
						}
						if (user != null) {
							m.UserEmail = user.Email;
							m.UserFirstName = user.FirstName;
						}
					}
					model.Add(m);
				}
			}
			return View(model);
		}

		public ActionResult Detail(int id) {
			ErrorLog log = this.errorLogRepository.GetById(id);
			if (log == null) {
				return this.View("NotFound");
			}
			ErrorLogViewModel model = new ErrorLogViewModel(log);
			User user = this.userRepository.GetById(model.UserId ?? 0);
			if (user != null) {
				model.UserEmail = user.Email;
				model.UserFirstName = user.FirstName;
			}
			if (!string.IsNullOrEmpty(log.ExceptionDetails) && log.ExceptionDetails.IndexOf("{", StringComparison.InvariantCultureIgnoreCase) > -1) {
				try {
					model.ExceptionInfo = JsonConvert.DeserializeObject<ExceptionInfo>(log.ExceptionDetails);
				} catch {
					// FRAGILE: Swallow, just show the string
					model.ExceptionInfo = null;
				}
			}
			if (!string.IsNullOrEmpty(model.Headers) && model.Headers.IndexOf("{", StringComparison.InvariantCultureIgnoreCase) > -1) {
				try {
					model.HeaderInfo = JsonConvert.DeserializeObject<List<HeaderInfo>>(model.Headers);
				} catch {
					// FRAGILE: Swallow, just show the string
					model.HeaderInfo = null;
				}
			}
			return View(model);
		}

	}
}
