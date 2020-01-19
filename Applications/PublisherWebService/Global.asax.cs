using System;
using System.Reflection;
using System.Web;
//using National.QRSC.ObjectCache;
using Core.Common;
using Core.V34;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.RefCounting;

namespace Orion.PublisherWebService
{
    public class Global : HttpApplication
    {
        public static Reference<ILogger> LoggerRef = Reference<ILogger>.Create(new TraceLogger(true));
        //private CoreServer _server;
        private ICoreClient _client;
        private ICoreCache _cache;

        #region Application Methods

        protected void Application_Start(object sender, EventArgs e)
        {
            const string fullAppName = "PublisherWebService";
            LoggerRef.Target.LogInfo("Starting up...");
            try
            {
                //var stopwatch = new Stopwatch();
                //stopwatch.Start();
                CoreClientFactory factory = new CoreClientFactory(LoggerRef)
                    .SetEnv("DEV")
                    .SetApplication(Assembly.GetExecutingAssembly())
                    .SetProtocols(WcfConst.AllProtocolsStr);
                _client = factory.SetServers("localhost").Create();
                _cache = _client.CreateCache();
                //LoadConfigDataHelper.LoadConfigurationData(LogRef.Target, _Cache);//TODO get rid of this
                //stopwatch.Stop();
                //Debug.Print("Initialized test environment, in {0} seconds", stopwatch.Elapsed.TotalSeconds);
                LoggerRef.Target.LogInfo("Loaded..." + fullAppName);
            }
            catch (Exception excp)
            {
                LoggerRef.Target.Log(excp);
            }
        }

        protected void Application_End(object sender, EventArgs e)
        {
            try
            {
                LoggerRef.Target.LogInfo("Stopped.");
                DisposeHelper.SafeDispose(ref _client);
                DisposeHelper.SafeDispose(ref _cache);
            }
            catch (Exception ex)
            {
                LoggerRef.Target.LogError(ex);
                throw;
            }
        }

        #endregion
    }
}