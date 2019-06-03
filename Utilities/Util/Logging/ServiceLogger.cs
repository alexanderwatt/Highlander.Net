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
using System.Diagnostics;

namespace Orion.Util.Logging
{
    public class ServiceLogger : BaseLogger
    {
        private readonly EventLog _systemEventLog;

        public ServiceLogger(string source)
            : base(null, null)
        {
            _systemEventLog = new EventLog("Application", Environment.MachineName, source);
            Initialize();
        }

        public ServiceLogger(EventLog eventLog)
            : base(null, null)
        {
            _systemEventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));
            Initialize();
        }

        private void Initialize()
        {
            Format = "{text}";
            SplitLines = false;
        }

        protected override void OnWrite(int severity, string text)
        {
            if (severity < LogSeverity.Info)
                return;
            EventLogEntryType entryType = EventLogEntryType.Information;
            if (severity >= LogSeverity.Warning)
                entryType = EventLogEntryType.Warning;
            if (severity >= LogSeverity.Error)
                entryType = EventLogEntryType.Error;

            _systemEventLog.WriteEntry(text, entryType);
        }
    }
}
