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
using Core.Common;
using Core.V34;
using Orion.Constants;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using Orion.V5r3.Configuration;

#endregion

namespace FpML.V5r3.LoadConfigData
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
                        if (client.ClientInfo.ConfigEnv >= EnvId.Sit_SystemTest)
                        {
                            client.DeleteUntypedObjects(null, Expr.StartsWith(Expr.SysPropItemName, EnvironmentProp.DefaultNameSpace + ".Configuration."));
                        }
                        // load configuration data
                        //AppSettingsLoader.Load(logRef.Target, client);
                        LoadConfigDataHelper.LoadConfigurationData(logRef.Target, client, EnvironmentProp.DefaultNameSpace);
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
