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
using System.IO;
using System.Reflection;
using Highlander.Build;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Exception = System.Exception;

#endregion

namespace Highlander.Core.TradeDump.V5r3
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new ConsoleLogger("Highlander.CoreTradeDump.V5r3: ")))
            {
                loggerRef.Target.LogInfo("Running...");
                const int exitCode = 0;
                try
                {
                    // set defaults
                    string coreAddr = null;
                    // process arguments
                    var filterProps = new NamedValueSet();
                    foreach (string arg in args)
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
                                case "p":
                                    string[] propParts = argValue.Split('=');
                                    filterProps.Set(new NamedValue(propParts[0], propParts[1]));
                                    loggerRef.Target.LogInfo("  Property: {0}={1}", propParts[0], propParts[1]);
                                    break;
                                case "s":
                                    coreAddr = argValue;
                                    loggerRef.Target.LogInfo("  Server  : {0}", coreAddr);
                                    break;
                                default:
                                    argProcessed = false;
                                    break;
                            }
                        }
                        if (!argProcessed)
                        {
                            loggerRef.Target.LogInfo(" Dumps trade metadata and FpML content");
                            loggerRef.Target.LogInfo(" usage:");
                            loggerRef.Target.LogInfo("   CoreDump [options]");
                            loggerRef.Target.LogInfo("      /s:address        the server address to connect to eg. /s:sydwadqds01:8213");
                            loggerRef.Target.LogInfo("      /p:name=value     adds a filter property value e.g. /p:CounterpartyId=13142");
                            loggerRef.Target.LogInfo("          Examples: /p:CounterpartyId=13142 /p:OriginatingPartyId=1636 /p:ProductType=CrossCurrencySwap");
                            loggerRef.Target.LogInfo(" returns:");
                            loggerRef.Target.LogInfo("  0: success");
                            loggerRef.Target.LogInfo("  2: failed - see output for details");
                            throw new ArgumentException("Unknown argument '" + arg + "'");
                        }
                    }

                    // connect and query
                    using (ICoreClient client = new CoreClientFactory(loggerRef)
                        .SetEnv(BuildConst.BuildEnv)
                        .SetServers(coreAddr)
                        .SetApplication(Assembly.GetExecutingAssembly())
                        .Create())
                    {
                        IExpression filterExpr = Expr.BoolAND(filterProps);
                        loggerRef.Target.LogInfo("Selecting trades where: {0}", filterExpr.DisplayString());
                        ICoreItem[] tradeItems = client.LoadItems<Trade>(filterExpr).ToArray();
                        // dump results
                        loggerRef.Target.LogInfo("Saving {0} trades...", tradeItems.Length);
                        for (int i = 0; i < tradeItems.Length; i++)
                        {
                            ICoreItem tradeItem = tradeItems[i];
                            // save item
                            string baseFilename = Path.GetFullPath($@".\{tradeItem.Name}.xxx");
                            string xmlFilename = Path.ChangeExtension(baseFilename, ".xml");
                            string nvsFilename = Path.ChangeExtension(xmlFilename, ".nvs");
                            using (var sr = new StreamWriter(xmlFilename))
                            {
                                sr.Write(tradeItem.Text);
                            }
                            using (var sr = new StreamWriter(nvsFilename))
                            {
                                sr.Write(tradeItem.AppProps.Serialise());
                            }
                            // done
                            tradeItems[i] = null; // allows early GC of decompressed/deserialised xml content
                            if (i % 1000 == 0)
                            {
                                loggerRef.Target.LogInfo("Saved {0} trades...", i);
                                GC.Collect();
                            }
                        }
                        loggerRef.Target.LogInfo("Saved {0} trades.", tradeItems.Length);
                    }
                    loggerRef.Target.LogInfo("Success");
                    Environment.ExitCode = exitCode;
                }
                catch (Exception e)
                {
                    loggerRef.Target.Log(e);
                    for (int i = 0; i < args.Length; i++)
                    {
                        loggerRef.Target.LogDebug("  args[{0}]='{1}'", i, args[i]);
                    }
                    loggerRef.Target.LogInfo("FAILED");
                    Environment.ExitCode = 2;
                }
            }
        }
    }
}
