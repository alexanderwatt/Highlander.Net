/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using Highlander.Configuration.Data.V5r3;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;

#endregion

namespace Highlander.Data.Loader.V5r3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var logRef = Reference<ILogger>.Create(new ConsoleLogger("Highlander.Data.Loader.V5r3: ")))
            {
                logRef.Target.LogInfo("Running...");
                int exitCode = 0;
                try
                {
                    //Use the unit test environment rather than the DEV environment.
                    var env = EnvHelper.EnvName(EnvId.Dev_Development);//Utt_UnitTest
                    //var random = new Random(Environment.TickCount);
                    //int port = random.Next(8000, 8099);
                    //IBasicServer coreServer = new CoreServer(logRef, env, NodeType.Router, port, WcfConst.NetTcp);
                    //coreServer.Start();
                    using (var client = new CoreClientFactory(logRef)
                        .SetEnv(env)
                        //.SetServers("localhost:" + port.ToString(CultureInfo.InvariantCulture))//For testing only
                        //.SetProtocols(WcfConst.NetTcp)//For testing only
                        .Create())
                    {
                        // delete 'old' configuration data in all environments greater than DEV
                        if (client.ClientInfo.ConfigEnv >= EnvId.Sit_SystemTest)
                        {
                            client.DeleteUntypedObjects(null, Expr.StartsWith(Expr.SysPropItemName, EnvironmentProp.DefaultNameSpace + ".Configuration."));
                        }
                        // load configuration data
                        var nameSpace = EnvironmentProp.DefaultNameSpace;
                        if (args != null && args.Length > 0)
                        {
                            nameSpace = args[0];
                        }
                        LoadConfigDataHelper.LoadConfigurationData(logRef.Target, client, nameSpace);
                        // done
                        logRef.Target.LogInfo("Success");
                    }
                    //DisposeHelper.SafeDispose(ref coreServer);
                }
                catch (Exception e)
                {
                    logRef.Target.Log(e);
                    for (var i = 0; i < args?.Length; i++)
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
