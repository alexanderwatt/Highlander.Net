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
using Highlander.Utilities.Logging;

namespace Highlander.Metadata.Common
{
    public partial class DebugCacheStats
    {
        public string ItemName => $"CacheStats.{(hostNameField ?? "(null-host)").Replace('.', '_')}.{(userWNIdField ?? "(null-user)").Replace('.', '_')}.{(applNameField ?? "(null-appl)").Replace('.', '_')}.{(nodeIdField ?? "(null-node)").Replace('.', '_')}";
    }

    public partial class DebugLogEvent
    {
        public string ItemName => $"Logger.{(hostNameField ?? "(null-host)").Replace('.', '_')}.{(userNameField ?? "(null-user)").Replace('.', '_')}.{(applNameField ?? "(null-appl)").Replace('.', '_')}.{processIdField}.{seqNumberField}";

        public TimeSpan CalcLifetime()
        {
            switch (severityField)
            {
                case LogSeverity.Fatal:
                case LogSeverity.Error:
                    return TimeSpan.FromDays(30);
                case LogSeverity.Warning:
                case LogSeverity.Info:
                    return TimeSpan.FromDays(7);
                default:
                    return TimeSpan.FromMinutes(10);
            }
        }
    }
}
