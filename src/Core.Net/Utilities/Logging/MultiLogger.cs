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
using System.Collections.Generic;

namespace Highlander.Utilities.Logging
{
    public class MultiLogger : ILogger
    {
        private readonly IEnumerable<ILogger> loggers;

        public MultiLogger(IEnumerable<ILogger> loggers)
        {
            this.loggers = loggers ?? throw new ArgumentNullException(nameof(loggers));
        }

        // factories
        public MultiLogger(ILogger logger1, ILogger logger2)
            : this(new[] { logger1, logger2 })
        { }

        public MultiLogger(ILogger logger1, ILogger logger2, ILogger logger3)
            : this(new[] { logger1, logger2, logger3 })
        { }

        public void Dispose()
        { }

        public void Clear()
        {
            foreach (var logger in loggers)
            {
                logger?.Clear();
            }
        }

        public void Flush()
        {
            foreach (var logger in loggers)
            {
                logger?.Flush();
            }
        }

        public void Log(Exception e)
        {
            Log(LogSeverity.Error, "EXCEPTION: " + e);
        }

        public void LogDebug(string msg)
        {
            Log(LogSeverity.Debug, msg);
        }

        public void LogDebug(string format, object arg0)
        {
            Log(LogSeverity.Debug, String.Format(format, arg0));
        }

        public void LogDebug(string format, object arg0, object arg1)
        {
            Log(LogSeverity.Debug, String.Format(format, arg0, arg1));
        }

        public void LogDebug(string format, object arg0, object arg1, object arg2)
        {
            Log(LogSeverity.Debug, String.Format(format, arg0, arg1, arg2));
        }

        public void LogDebug(string format, params object[] args)
        {
            Log(LogSeverity.Debug, String.Format(format, args));
        }

        public void LogInfo(string msg)
        {
            Log(LogSeverity.Info, msg);
        }

        public void LogInfo(string format, object arg0)
        {
            Log(LogSeverity.Info, String.Format(format, arg0));
        }

        public void LogInfo(string format, object arg0, object arg1)
        {
            Log(LogSeverity.Info, String.Format(format, arg0, arg1));
        }

        public void LogInfo(string format, object arg0, object arg1, object arg2)
        {
            Log(LogSeverity.Info, String.Format(format, arg0, arg1, arg2));
        }

        public void LogInfo(string format, params object[] args)
        {
            Log(LogSeverity.Info, String.Format(format, args));
        }

        public void LogWarning(string msg)
        {
            Log(LogSeverity.Warning, msg);
        }

        public void LogWarning(string format, object arg0)
        {
            Log(LogSeverity.Warning, String.Format(format, arg0));
        }

        public void LogWarning(string format, object arg0, object arg1)
        {
            Log(LogSeverity.Warning, String.Format(format, arg0, arg1));
        }

        public void LogWarning(string format, object arg0, object arg1, object arg2)
        {
            Log(LogSeverity.Warning, String.Format(format, arg0, arg1, arg2));
        }

        public void LogWarning(string format, params object[] args)
        {
            Log(LogSeverity.Warning, String.Format(format, args));
        }

        public void LogError(string msg)
        {
            Log(LogSeverity.Error, msg);
        }

        public void LogError(string format, object arg0)
        {
            Log(LogSeverity.Error, String.Format(format, arg0));
        }

        public void LogError(string format, object arg0, object arg1)
        {
            Log(LogSeverity.Error, String.Format(format, arg0, arg1));
        }

        public void LogError(string format, object arg0, object arg1, object arg2)
        {
            Log(LogSeverity.Error, String.Format(format, arg0, arg1, arg2));
        }

        public void LogError(string format, params object[] args)
        {
            Log(LogSeverity.Error, String.Format(format, args));
        }

        public void LogError(Exception ex)
        {
            Log(LogSeverity.Error, ex.ToString());
        }

        public void LogFatal(string msg)
        {
            Log(LogSeverity.Fatal, msg);
        }

        public void LogFatal(string format, object arg0)
        {
            Log(LogSeverity.Fatal, String.Format(format, arg0));
        }

        public void LogFatal(string format, object arg0, object arg1)
        {
            Log(LogSeverity.Fatal, String.Format(format, arg0, arg1));
        }

        public void LogFatal(string format, object arg0, object arg1, object arg2)
        {
            Log(LogSeverity.Fatal, String.Format(format, arg0, arg1, arg2));
        }

        public void LogFatal(string format, params object[] args)
        {
            Log(LogSeverity.Fatal, String.Format(format, args));
        }

        public void LogFatal(Exception ex)
        {
            Log(LogSeverity.Fatal, ex.ToString());
        }

        public void Log(int severity, string msg)
        {
            Log(severity, "", msg);
        }

        public void Log(int severity, string indent, string msg)
        {
            foreach (var logger in loggers)
            {
                if (logger != null)
                    logger.Log(severity, indent, msg);
            }
        }
    }
}