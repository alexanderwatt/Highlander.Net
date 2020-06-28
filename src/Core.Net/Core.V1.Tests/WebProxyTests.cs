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

using Highlander.Core.Common;
using Highlander.Core.Server;
using Highlander.Core.WebServer;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Core.V1.Tests
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
                    using (var httpServer = new HighlanderWebProxyServer())
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
