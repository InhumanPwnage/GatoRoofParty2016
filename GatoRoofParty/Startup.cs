using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GatoRoofParty.Startup))]
namespace GatoRoofParty
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
