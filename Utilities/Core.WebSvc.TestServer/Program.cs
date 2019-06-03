using System;
using Core.Common;
using Core.V34;
using Core.WebSvc.Server;
using Orion.Build;
using Orion.Util.Logging;
using Orion.Util.RefCounting;

namespace Core.WebSvc.SvrCli
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new ConsoleLogger(null)))
            {
                try
                {
                    using (Reference<ICoreClient> client = Reference<ICoreClient>.Create(new CoreClientFactory(loggerRef).SetEnv(BuildConst.BuildEnv).Create()))
                    using (IServerBase2 server = new WebProxyServer())
                    {
                        server.LoggerRef = loggerRef;
                        server.Client = client;
                        server.Start();
                        loggerRef.Target.LogInfo("Running... Press ENTER to exit.");
                        Console.ReadLine();
                    }
                }
                catch (Exception excp)
                {
                    loggerRef.Target.Log(excp);
                    loggerRef.Target.LogInfo("Failed. Press ENTER to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
