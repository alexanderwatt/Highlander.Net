using Autofac;
using Autofac.Integration.WebApi;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using Highlander.Web.API.V5r3.Auth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Highlander.Web.API.V5r3
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            var builder = new ContainerBuilder();

            // Get your HttpConfiguration.
            var config = GlobalConfiguration.Configuration;

            //auth
            var apiKeyHeader = ConfigurationManager.AppSettings["auth:apiKeyHeader"];
            var apiKey = ConfigurationManager.AppSettings["auth:apiKey"];
            builder.RegisterType<ApiKeyAuthFilter>()
                .WithParameter("header", apiKeyHeader)
                .WithParameter("privateKey", apiKey)
                .AsWebApiAuthorizationFilterFor<ApiController>()
                .SingleInstance();
            builder.RegisterWebApiFilterProvider(config);

            //logging
            var logLocation = ConfigurationManager.AppSettings["logs:path"];
            var logger = Reference<ILogger>.Create(new FileLogger($@"{logLocation}\CoreSvcWeb.{DateTime.Now.ToShortDateString()}.log"));

            //highlander client
            var environment = ConfigurationManager.AppSettings["env"];
            builder.Register(c =>
            {
                var factory = new CoreClientFactory(logger)
                    .SetEnv(environment)
                    .SetApplication(Assembly.GetExecutingAssembly())
                    .SetProtocols(WcfConst.AllProtocolsStr);
                var client = factory.SetServers("localhost").Create();
                //_syncContext.Post(OnClientStateChange, new CoreStateChange(CoreStateEnum.Initial, _client.CoreState));
                //client.OnStateChange += _Client_OnStateChange;
                return client;
            }).As<ICoreClient>().SingleInstance();

            builder.Register(c =>
            {
                return c.Resolve<ICoreClient>().CreateCache();
            }).As<ICoreCache>().SingleInstance();

            // Register your Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // Set the dependency resolver to be Autofac.
            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            //RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
