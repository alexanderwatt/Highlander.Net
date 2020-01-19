using System.Reflection;
using Core.Common;
using Core.V34;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.RefCounting;

namespace Viewer.Tests.Integration
{
    [TestClass]
    public class LoggerTest
    {
        [TestMethod]
        public void LogTest()
        {
            const string appName = "ExcelWrapper";
            using (var logger = new TraceLogger(false))
            {
                var coreClientFactory = new CoreClientFactory(Reference<ILogger>.Create(logger))
                .SetEnv("DEV")
                .SetApplication(Assembly.GetExecutingAssembly())
                .SetProtocols(WcfConst.AllProtocolsStr)
                .SetServers("localhost");
                ICoreClient client = coreClientFactory.Create();
                // write the string
                var namedValueSet = new NamedValueSet();
                namedValueSet.Set("ConnectionString", "Data Source=SYDWDDQUR02;Initial Catalog=QR;Integrated Security=True");
                client.SaveAppSettings(namedValueSet, appName, "*", "*", false);

                // read the string
                string conn = client.LoadAppSettings(appName).Get("ConnectionString").ValueString;
                Assert.IsTrue(!string.IsNullOrEmpty(conn));
            }
        }
    }
}
