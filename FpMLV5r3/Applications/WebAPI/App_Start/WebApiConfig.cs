using System.Web.Http;
using System.Web.Http.Tracing;

namespace Highlander.WebAPI.V5r3
{
    /// <summary>
    /// The Highlander Web API
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Register the configuration
        /// </summary>
        /// <param name="config"></param>
        public static void Register(HttpConfiguration config)
        {
            // Diagnostics
            //SystemDiagnosticsTraceWriter traceWriter = config.EnableSystemDiagnosticsTracing();
            //traceWriter.IsVerbose = true;
            //traceWriter.MinimumLevel = TraceLevel.Debug;

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
