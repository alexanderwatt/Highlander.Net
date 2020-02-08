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

namespace Highlander.Core.Common
{
    public static class WcfConst
    {
        public const string NetPipe = "net.pipe";

        //public const string NetMsmq = "net.msmq";
        public const string NetgRPC = "net.grpc";

        public const string NetTcp = "net.tcp";

        public const string Http = "http";

        public const string WebHttp = "web.http";

        public const string NetHttp = "net.http";

        public const string NetHttps = "net.https";

        public const string NetWebsockets = "net.websockets";

        //public static string[] AllProtocols { get { return new string[] { NetMsmq, NetPipe, NetTcp }; } }
        public static string[] AllProtocols => new[] { NetTcp };

        public static string AllProtocolsStr => string.Join(";", AllProtocols);

        public const int LimitMultiplier = 16;

        public const int MaxMessageSize = LimitMultiplier * 64 * 1024;

        public static readonly TimeSpan ExpressMsgLifetime = TimeSpan.FromMinutes(5);
    }
}
