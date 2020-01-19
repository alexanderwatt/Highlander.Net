using System;
using nab.QDS.Core.Common;
using nab.QDS.Core.V34;
using nab.QDS.Util.Logging;

namespace National.QRSC.Grid.Handler
{
    class Program
    {
        static void Main(string[] args)
        {
            // exit codes:
            //   1  success
            //   0  failed (exception logged to network)
            //  -1  catastrophic failure (logged to local file)
            //  -2  catastrophic failure (logged to console only)
            using (ILogger logger = new ConsoleLogger("RequestHandler: "))
            {
                // argument defaults
                bool attended = false;
                Guid requestId = Guid.Empty;
                EnvId env = EnvId.Undefined;
                string hostInstance = null;
                // note: project debugging guid is: {6B70756E-16D8-4E9D-B104-3BE5830F229A}

                try
                {
                    // process arguments
                    logger.LogDebug("Arguments:");
                    for (int i = 0; i < args.Length; i++)
                    {
                        logger.LogDebug("  Argument[{0}]='{1}'", i, args[i]);
                        string[] argParts = args[i].Split(':');
                        string argName = argParts[0];
                        switch (argName.ToLower())
                        {
                            case "/debug": attended = true; break;
                            case "/reqid": requestId = Guid.Parse(argParts[1]); break;
                            case "/hiid":
                                {
                                    hostInstance = argParts[1];
                                    if (hostInstance.Equals("Default", StringComparison.OrdinalIgnoreCase))
                                        hostInstance = null;
                                }
                                break;
                            case "/env": env = EnvHelper.ParseEnvName(argParts[1]); break;
                            default: break;
                        }
                    }

                    using(ICoreClient client = new CoreClientFactory(logger).SetEnv(env).Create())
                    //using (ILogger netLogger = new NetworkLogger(client, null))
                    using (ICoreCache cache = client.CreateCache())
                    {
                        //Environment.ExitCode = HandleRequest(new MultiLogger(logger, netLogger), cache, requestId);
                        Environment.ExitCode = RequestHandler.HandleRequest(logger, cache, requestId, hostInstance);
                    }
                }
                catch (Exception e1)
                {
                    // catastrophic failure
                    // - try to log to local file
                    try
                    {
                        Environment.ExitCode = -2;
                        using (ILogger errorLog = new MultiLogger(logger,
                            new FileLogger(String.Format(@".\RequestHandler.Error.{0}.log", requestId))))
                        {
                            errorLog.LogDebug("Arguments:");
                            for (int i = 0; i < args.Length; i++)
                                errorLog.LogDebug("  Argument[{0}]='{1}'", i, args[i]);
                            errorLog.LogError(e1);
                        }
                    }
                    catch (Exception e2)
                    {
                        // I/O failure creating error log?
                        Environment.ExitCode = -3;
                        logger.LogDebug("Arguments ({0}):", args.Length);
                        for (int i = 0; i < args.Length; i++)
                            logger.LogDebug("  Argument[{0}]='{1}'", i, args[i]);
                        logger.LogError(e2);
                    }
                }
                if (attended)
                {
                    Console.WriteLine("Press ENTER to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
