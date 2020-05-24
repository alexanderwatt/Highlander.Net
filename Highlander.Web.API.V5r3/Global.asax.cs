using Autofac;
using Autofac.Integration.WebApi;
using Highlander.Configuration.Data.V5r3;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Core.Interface.V5r3;
using Highlander.Core.V34;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using Highlander.Web.API.V5r3.Auth;
using Highlander.Web.API.V5r3.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
            var logger = Reference<ILogger>.Create(new MultiLogger(
                new TraceLogger(true),
                new FileLogger($@"{logLocation}\CoreSvcWeb.{DateTime.Now.ToShortDateString()}.log")
            ));
            builder.Register(c => logger).As<Reference<ILogger>>().SingleInstance();

            var environment = ConfigurationManager.AppSettings["env"];

            builder.Register(c => {
                try
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    var cache = new PricingCache();
                    stopwatch.Stop();
                    Debug.Print($"Initialized pricing cache in {stopwatch.Elapsed.TotalSeconds} seconds");
                    return cache;
                }
                catch(Exception ex)
                {
                    logger.Target.Log(ex);
                    throw ex;
                }
            }).AsSelf().SingleInstance();

            builder.RegisterType<PropertyService>().AsSelf().SingleInstance();
            builder.RegisterType<LeaseService>().AsSelf().SingleInstance();
            builder.RegisterType<CurveService>().AsSelf().SingleInstance();

            var factory = new CoreClientFactory(logger)
                .SetEnv(environment)
                .SetApplication(Assembly.GetExecutingAssembly())
                .SetProtocols(WcfConst.AllProtocolsStr);
            var client = factory.SetServers("localhost").Create();

            builder.Register(c => client).As<ICoreClient>().SingleInstance();

            builder.Register(c => c.Resolve<ICoreClient>().CreateCache()).As<ICoreCache>().SingleInstance();

            try
            {
                logger.Target.LogInfo("Loading data into cache...");
                var nameSpace = EnvironmentProp.DefaultNameSpace;
                LoadConfigDataHelper.LoadConfigurationData(logger.Target, client, nameSpace);
                logger.Target.LogInfo("Loaded data into cache successfully");
            }
            catch (Exception exception)
            {
                logger.Target.Log(exception);
                throw exception;
            }
            
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
