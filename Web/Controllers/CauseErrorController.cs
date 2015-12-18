namespace LoggingSample.Web.Controllers {
	using System;
	using System.Web.Mvc;

	public class CauseErrorController : Controller {

		public ActionResult Index() {
			return View();
		}

		public ActionResult ThrowUp() {
			throw new Exception("Throwing Up"); // Set a break point inside LoggerService.Log() and push play to watch it catch and log the error
		}

	}
}
