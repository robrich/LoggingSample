namespace LoggingSample.Web.App_Start {
	using System.Web.Mvc;
	using LoggingSample.Library;
	using LoggingSample.Web.Filters;

	public static class FilterConfig {

		public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
			filters.Add(ServiceLocator.GetService<HandleAndLogErrorAttribute>());
		}

	}
}
