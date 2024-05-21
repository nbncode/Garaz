using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Garaaz.Startup))]
namespace Garaaz
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
