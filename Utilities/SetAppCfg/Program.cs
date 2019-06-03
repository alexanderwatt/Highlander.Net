/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Core.Common;
using Core.V34;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.RefCounting;

namespace Core.SetAppCfg
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ConsoleLogger logger = new ConsoleLogger("SetAppCfg: "))
            {
                logger.LogInfo("Running...");
                int exitCode = 0;
                try
                {
                    // set defaults
                    string coreAddr = null;
                    string applName = null;
                    string userName = null;
                    string hostName = null;
                    bool replace = false;
                    //bool debugOn = false;
                    // process arguments
                    NamedValueSet appCfg = new NamedValueSet();
                    foreach (var arg in args)
                    {
                        bool argProcessed = true;
                        // switches - begin with /
                        if (arg.StartsWith("/"))
                        {
                            // arg is a switch
                            string[] argParts = arg.Split(':');
                            string argName = argParts[0].Substring(1);
                            string argValue = "";
                            for (int j = 1; j < argParts.Length; j++)
                            {
                                if (j > 1) argValue += ':';
                                argValue += argParts[j];
                            }
                            switch (argName.ToLower())
                            {
                                case "a":
                                    applName = argValue;
                                    logger.LogInfo("  ApplName: {0}", applName);
                                    break;
                                case "u":
                                    userName = argValue;
                                    logger.LogInfo("  UserName: {0}", userName);
                                    break;
                                case "h":
                                    hostName = argValue;
                                    logger.LogInfo("  HostName: {0}", hostName);
                                    break;
                                case "p": // simple string property
                                    string[] propParts = argValue.Split('=');
                                    appCfg.Set(new NamedValue(propParts[0], propParts[1]));
                                    logger.LogInfo("  Property: {0}={1}", propParts[0], propParts[1]);
                                    break;
                                case "nv": // typed serialised named value
                                    appCfg.Set(new NamedValue(argValue));
                                    logger.LogInfo("  Property: {0}", argValue);
                                    break;
                                case "replace":
                                    replace = true;
                                    logger.LogInfo("  Replace Old");
                                    break;
                                case "debug":
                                    //debugOn = true;
                                    logger.LogInfo("  Debug On");
                                    break;
                                case "s":
                                    coreAddr = argValue;
                                    logger.LogInfo("  Server  : {0}", coreAddr);
                                    break;
                                default:
                                    argProcessed = false;
                                    break;
                            }
                        }
                        if (!argProcessed)
                        {
                            logger.LogInfo(" Sets configuration values for applications, users and machines");
                            logger.LogInfo(" usage:");
                            logger.LogInfo("   SetAppCfg [options]");
                            logger.LogInfo("      /a:applname           the application name (default: all applications)");
                            logger.LogInfo("      /u:username           the user name (default: all users)");
                            logger.LogInfo("      /h:hostname           the host (machine) name (default: all machines)");
                            logger.LogInfo("      /p:name=value         sets a string property value");
                            logger.LogInfo("      /nv:name/type=value   sets a typed property value with a serialised named value");
                            logger.LogInfo("      /s:address            the address of the core server to connect to eg. localhost:8114");
                            logger.LogInfo("      /replace              old values for the same application/user/host are deleted");
                            logger.LogInfo("      /debug                logs the internal query and results to the debug port");
                            logger.LogInfo(" returns:");
                            logger.LogInfo("  0: success - settings updated");
                            logger.LogInfo("  2: error - see error output for details");
                            throw new ArgumentException("Unknown argument '" + arg + "'");
                        }
                    }
                    // save the app config
                    var refLogger = Reference<ILogger>.Create(logger);
                    using (ICoreClient client = new CoreClientFactory(refLogger).SetServers(coreAddr).Create())
                    {
                        logger.LogInfo("Old/new settings comparison for:");
                        logger.LogInfo("  Application: {0}", applName ?? "(all)");
                        logger.LogInfo("  User name  : {0}", userName ?? "(all)");
                        logger.LogInfo("  Host name  : {0}", hostName ?? "(all)");
                        // get old settings
                        NamedValueSet oldAppCfg = client.LoadAppSettings(applName, userName, hostName);
                        logger.LogInfo("Old settings:");
                        oldAppCfg.LogValues(delegate(string text) { logger.LogInfo("  " + text); });
                        // set new settings
                        client.SaveAppSettings(appCfg, applName, userName, hostName, replace);
                        // get new settings
                        NamedValueSet newAppCfg = client.LoadAppSettings(applName, userName, hostName);
                        logger.LogInfo("New settings:");
                        newAppCfg.LogValues(delegate(string text) { logger.LogInfo("  " + text); });
                    }
                    logger.LogInfo("Success");
                    Environment.ExitCode = exitCode;
                }
                catch (Exception e)
                {
                    logger.Log(e);
                    for (int i = 0; i < args.Length; i++)
                    {
                        logger.LogDebug("  args[{0}]='{1}'", i, args[i]);
                    }
                    logger.LogInfo("FAILED");
                    Environment.ExitCode = 2;
                }
            }
        }
    }
}
