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
using System.Diagnostics;
using System.Threading;
using Orion.Util.Logging;
using Orion.Util.NamedValues;

namespace Core.Common
{
    public class NetworkLogger : BaseLogger
    {
        private static readonly int ProcessId = Process.GetCurrentProcess().Id;
        private static int _seqNumber;
        private readonly ICoreClient _client;
        public NetworkLogger(ICoreClient client, string prefix)
            : base(prefix, null)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            Format = "{prefix}{indent}{text}{suffix}";
        }

        protected override void OnWrite(int severity, string text)
        {
            DateTimeOffset dtoNow = DateTimeOffset.Now;
            var props = new NamedValueSet();
            props.Set("Severity", severity);
            var data = new DebugLogEvent
                           {
                               Created = dtoNow.ToString("o"),
                               Message = text,
                               ProcessId = ProcessId,
                               SeqNumber = Interlocked.Increment(ref _seqNumber),
                               Severity = severity,
                               ApplName = _client.ClientInfo.ApplName,
                               HostName = _client.ClientInfo.HostName,
                               UserName = _client.ClientInfo.UserName,
                               OrigEnv = EnvHelper.EnvName(_client.ClientInfo.ConfigEnv)
                           };
            _client.SaveDebug(data, data.ItemName, props);
        }
    }
}
