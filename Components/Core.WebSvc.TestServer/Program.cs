/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Highlander.Build;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.Core.WebServer;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;

namespace Highlander.Core.WebClient
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
                    using (IServerBase2 server = new HighlanderWebProxyServer())
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
