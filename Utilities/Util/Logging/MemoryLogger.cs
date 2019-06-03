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

using System.Collections.Generic;

namespace Orion.Util.Logging
{
    public class MemoryLogger : BaseLogger
    {
        public Dictionary<int, List<string>> Logs;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        public MemoryLogger(string prefix)
            : base(prefix, null)
        {
            Format = "({severity}) {user} {prefix}{indent}{text}{suffix}";
            Prefix = prefix;
            Clear();
        }

        public new void Clear()
        {
            Logs = new Dictionary<int, List<string>>
                       {
                           {LogSeverity.Fatal, new List<string>()},
                           {LogSeverity.Error, new List<string>()},
                           {LogSeverity.Warning, new List<string>()},
                           {LogSeverity.Info, new List<string>()},
                           {LogSeverity.Debug, new List<string>()},
                           {LogSeverity.DebugL2, new List<string>()},
                           {LogSeverity.DebugL3, new List<string>()}
                       };
            base.Clear();
        }

        public MemoryLogger()
            : this(null)
        {
        }

        protected override void OnWrite(int severity, string text)
        {
            Logs[severity].Add(text);
        }
    }
}
