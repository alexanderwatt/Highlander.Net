using System;
using Core.Common;
using Core.V34;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using Orion.Configuration;

namespace FpML.V5r3.Confirmation.LoadConfigData
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Reference<ILogger> logRef = Reference<ILogger>.Create(new ConsoleLogger("FpML.V5r3.ConfigDataLoader: ")))
            {
                logRef.Target.LogInfo("Running...");
                int exitCode = 0;
                try
                {
                    using (ICoreClient client = new CoreClientFactory(logRef).SetEnv("DEV").Create())
                    {
                        // delete 'old' configuration data in all environments greater than DEV
                        if (client.ClientInfo.ConfigEnv >= EnvId.SIT_SystemTest)
                        {
                            client.DeleteUntypedObjects(null, Expr.StartsWith(Expr.SysPropItemName, "Orion.FpML.V5r3.Configuration."));
                        }

                        // load configuration data
                        //AppSettingsLoader.Load(logRef.Target, client);
                        LoadConfigDataHelper.LoadConfigurationData(logRef.Target, client);
                        // done
                        logRef.Target.LogInfo("Success");
                    }
                }
                catch (Exception e)
                {
                    logRef.Target.Log(e);
                    for (int i = 0; i < args.Length; i++)
                    {
                        logRef.Target.LogDebug("  args[{0}]='{1}'", i, args[i]);
                    }
                    logRef.Target.LogInfo("FAILED");
                    exitCode = 2;
                }
                finally
                {
                    Environment.ExitCode = exitCode;
                }
            }
        }
    }
}
