using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using Microsoft.Owin;
using Owin;
using System.Reflection;
using System.Web.Http;

[assembly: OwinStartup(typeof(Sender.Service.Startup))]

namespace Sender.Service
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            var builder = new ContainerBuilder();
            var config = new HttpConfiguration();
            WebApiConfig.Register(config);
            builder.RegisterWebApiFilterProvider(GlobalConfiguration.Configuration);
            var log = LogManager.GetLogger("SenderApi");
            builder.Register(x => log).As<ILog>();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            var container = builder.Build();
            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(config);
            app.UseWebApi(config);
        }
    }
}
