using Hangfire;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EventMangementSystem.Startup))]
namespace EventMangementSystem
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration
            .UseSqlServerStorage("DefaultConnection");
            app.MapSignalR();
            app.UseHangfireServer();
            app.UseHangfireDashboard();
            ConfigureAuth(app);
        }
    }
}
