using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(jCPCFeed.Startup))]
namespace jCPCFeed
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
