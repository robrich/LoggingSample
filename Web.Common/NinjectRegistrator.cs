namespace LoggingSample.Web.Common {
	using System.Collections.Generic;
	using System.Reflection;
	using LoggingSample.DataAccess;
	using LoggingSample.Infrastructure;
	using LoggingSample.Library;
	using LoggingSample.Repository;
	using LoggingSample.Service;
	using Ninject;
	using Ninject.Extensions.Conventions;

	public static class NinjectRegistrator {

		public static void RegisterServices(IKernel kernel, Assembly GuiAssembly) {

			// .FromAssembliesInPath() doesn't work in WebJobs because System.UnauthorizedAccessException: Access to the path is denied
			// FRAGILE: ASSUME: these are all the assemblies I want to bind
			List<Assembly> assemblies = new List<Assembly> {
				typeof(LoggingSampleDbContext).Assembly, // DataAccess
				typeof(ILogger).Assembly, // Infrastructure
				typeof(NullHelpers).Assembly, // Library
				typeof(Repository<>).Assembly, // Repository
				typeof(Logger).Assembly, // Service
				typeof(NinjectRegistrator).Assembly, // This project
				GuiAssembly, // The GUI project
			};

			kernel.Bind(x => x
				.From(assemblies)
				.Select(type => type.IsClass && !type.IsAbstract)
				.BindDefaultInterface()
				.Configure(b => b.InSingletonScope()) // FRAGILE: THERE SHALL BE NO OBJECT SCOPED PROPERTIES IN LOGIC CLASSES !!
			);

			// Initialize global service locator
			ServiceLocator.Initialize(kernel.GetService);

			// Add other bindings as necessary
			kernel.Rebind<ILoggingSampleDbContext>().ToMethod(_ => new LoggingSampleDbContext()); // Create a new one each time

		}

	}
}
