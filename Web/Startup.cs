using LoggingSample.Web;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace LoggingSample.Web {
	using Owin;

	public partial class Startup {
		public void Configuration(IAppBuilder app) {
		}
	}
}
