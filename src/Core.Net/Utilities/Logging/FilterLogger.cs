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
using Highlander.Utilities.RefCounting;

namespace Highlander.Utilities.Logging
{
    public class FilterLogger : ILogger
    {
        private readonly Reference<ILogger> _loggerRef;
        private readonly ILogger _logger;
        private readonly string _indent;

        public int MinLogSeverity { get; set; }

        private readonly GetLogSeverityDelegate _logSeverityCallback;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerRef"></param>
        /// <param name="indent"></param>
        public FilterLogger(Reference<ILogger> loggerRef, string indent)
        {
            MinLogSeverity = LogSeverity.DebugL3;
            _loggerRef = loggerRef.Clone();
            _logger = _loggerRef.Target;
            _indent = indent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="indent"></param>
        public FilterLogger(ILogger logger, string indent)
        {
            MinLogSeverity = LogSeverity.DebugL3;
            _logger = logger;
            _indent = indent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="indent"></param>
        /// <param name="minLogSeverity"></param>
        public FilterLogger(ILogger logger, string indent, int minLogSeverity)
        {
            _logger = logger;
            _indent = indent;
            MinLogSeverity = minLogSeverity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="indent"></param>
        /// <param name="logSeverityCallback"></param>
        public FilterLogger(ILogger logger, string indent, GetLogSeverityDelegate logSeverityCallback)
        {
            MinLogSeverity = LogSeverity.DebugL3;
            _logger = logger;
            _indent = indent;
            _logSeverityCallback = logSeverityCallback;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _loggerRef?.Dispose();
        }

        // filtering
        private bool LogSeverityEnabled(int severity)
        {
            int currentSeverity = MinLogSeverity;
            // get dynamic log level
            if (_logSeverityCallback != null)
                currentSeverity = _logSeverityCallback();
            return (severity >= currentSeverity);
        }

        // ILogger methods
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            _logger.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Flush()
        {
            _logger.Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void Log(Exception e)
        {
            Log(LogSeverity.Error, "EXCEPTION: " + e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void LogDebug(string msg)
        {
            Log(LogSeverity.Debug, msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
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

        private void Log(int severity, string msg)
        {
            if (LogSeverityEnabled(severity))
                _logger.Log(severity, _indent, msg);
        }

        public void Log(int severity, string indent, string msg)
        {
            if (LogSeverityEnabled(severity))
                _logger.Log(severity, _indent + indent, msg);
        }
    }
}