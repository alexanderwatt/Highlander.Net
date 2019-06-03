using Core.Common;
using Core.Server;
using Core.WebSvc.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Logging;
using Orion.Util.RefCounting;

namespace Core.V34.Tests
{
    [TestClass]
    public class WebServiceTests
    {
        [TestMethod]
        public void TestWebServiceStartStop()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                using (var coreServer = new CoreServer(loggerRef, "UTT", NodeType.Router))
                {
                    coreServer.Start();
                    using (Reference<ICoreClient> clientRef = Reference<ICoreClient>.Create(new CoreClientFactory(loggerRef).SetEnv("UTT").Create()))
                    using (var httpServer = new WebProxyServer())
                    {
                        httpServer.LoggerRef = loggerRef;
                        httpServer.Client = clientRef;
                        httpServer.Start();
                    }
                }
            }
        }
    }
}
