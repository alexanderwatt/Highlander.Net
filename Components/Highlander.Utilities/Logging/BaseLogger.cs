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

#region Usings

using System;
using System.Diagnostics;
using Highlander.Utilities.Threading;

#endregion

namespace Highlander.Utilities.Logging
{
    public class BaseLogger : ILogger
    {
        private readonly AsyncThreadQueue _asyncWriteQueue;
        protected bool DoAsyncIo;

        public string Format { get; set; }

        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public bool SplitLines { get; set; }

        private readonly string _userName = IdentityHelper.GetIdentity().Name;
        private readonly string _hostName = IdentityHelper.GetHostName();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        public BaseLogger(string prefix, string suffix)
        {
            // note: the following async thread queue must be constructed
            //       with a NULL logger, otherwise recursion occurs!
            _asyncWriteQueue = new AsyncThreadQueue(null);
            Format = "{dt:o},{severity},{host},{user},{prefix}{indent}{text}{suffix}{crlf}";
            Prefix = prefix;
            Suffix = suffix;
            SplitLines = true;
            try
            {
                AppDomain.CurrentDomain.UnhandledException +=
                    UnhandledDomainException;
            }
            catch (Exception excp)
            {
                Debug.WriteLine($"Unable to register AppDomain exception handler: {excp}");
            }
            // not used
            //try
            //{
            //    Application.ThreadException += new ThreadExceptionEventHandler(UnhandledThreadException);
            //}
            //catch (Exception excp)
            //{
            //    Trace.WriteLine(String.Format("Unable to register Application exception handler: {0}", excp);
            //}
        }

        protected virtual void Dispose(bool disposing)
        {
            _asyncWriteQueue.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            // first, wait a bit for (if enabled) the write queue to empty
            DoAsyncIo = false;
            long remaining = _asyncWriteQueue.WaitUntilEmpty(TimeSpan.FromSeconds(30));
            // todo - what to do if writes remain in the queue - ignore for now
            if (remaining > 0)
            {
                Debug.WriteLine($"BaseLogger: {remaining} log entries remain in queue!");
            }
            Flush();
            Dispose(true);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public void LogDebug(string format, object arg0, object arg1)
        {
            Log(LogSeverity.Debug, String.Format(format, arg0, arg1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public void LogDebug(string format, object arg0, object arg1, object arg2)
        {
            Log(LogSeverity.Debug, String.Format(format, arg0, arg1, arg2));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void LogDebug(string format, params object[] args)
        {
            Log(LogSeverity.Debug, String.Format(format, args));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void LogInfo(string msg)
        {
            Log(LogSeverity.Info, msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        public void LogInfo(string format, object arg0)
        {
            Log(LogSeverity.Info, string.Format(format, arg0));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public void LogInfo(string format, object arg0, object arg1)
        {
            Log(LogSeverity.Info, string.Format(format, arg0, arg1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public void LogInfo(string format, object arg0, object arg1, object arg2)
        {
            Log(LogSeverity.Info, string.Format(format, arg0, arg1, arg2));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void LogInfo(string format, params object[] args)
        {
            Log(LogSeverity.Info, string.Format(format, args));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void LogWarning(string msg)
        {
            Log(LogSeverity.Warning, msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        public void LogWarning(string format, object arg0)
        {
            Log(LogSeverity.Warning, String.Format(format, arg0));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public void LogWarning(string format, object arg0, object arg1)
        {
            Log(LogSeverity.Warning, string.Format(format, arg0, arg1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public void LogWarning(string format, object arg0, object arg1, object arg2)
        {
            Log(LogSeverity.Warning, string.Format(format, arg0, arg1, arg2));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void LogWarning(string format, params object[] args)
        {
            Log(LogSeverity.Warning, string.Format(format, args));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void LogError(string msg)
        {
            Log(LogSeverity.Error, msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        public void LogError(string format, object arg0)
        {
            Log(LogSeverity.Error, string.Format(format, arg0));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public void LogError(string format, object arg0, object arg1)
        {
            Log(LogSeverity.Error, string.Format(format, arg0, arg1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public void LogError(string format, object arg0, object arg1, object arg2)
        {
            Log(LogSeverity.Error, string.Format(format, arg0, arg1, arg2));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void LogError(string format, params object[] args)
        {
            Log(LogSeverity.Error, string.Format(format, args));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        public void LogError(Exception ex)
        {
            Log(LogSeverity.Error, ex.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void LogFatal(string msg)
        {
            Log(LogSeverity.Fatal, msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        public void LogFatal(string format, object arg0)
        {
            Log(LogSeverity.Fatal, string.Format(format, arg0));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public void LogFatal(string format, object arg0, object arg1)
        {
            Log(LogSeverity.Fatal, string.Format(format, arg0, arg1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public void LogFatal(string format, object arg0, object arg1, object arg2)
        {
            Log(LogSeverity.Fatal, string.Format(format, arg0, arg1, arg2));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void LogFatal(string format, params object[] args)
        {
            Log(LogSeverity.Fatal, string.Format(format, args));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        public void LogFatal(Exception ex)
        {
            Log(LogSeverity.Fatal, ex.ToString());
        }

        protected virtual void OnFlush() { }

        /// <summary>
        /// 
        /// </summary>
        public void Flush()
        {
            if (DoAsyncIo)
                _asyncWriteQueue.Dispatch<AsyncLogData>(null, AsyncFlush);
            else
                OnFlush();
        }

        private void AsyncFlush(object notUsed)
        {
            OnFlush();
        }

        private void Log(int severity, string input)
        {
            Log(severity, "", input);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="indent"></param>
        /// <param name="input"></param>
        public void Log(int severity, string indent, string input)
        {
            // split multiple lines and indent
            var inputLines = SplitLines ? input.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries) : new[] {input};
            foreach (string inputLine in inputLines)
            {
                try
                {
                    string format = Format.ToLower()
                        .Replace("{text}", "{0}")
                        .Replace("{dt:s}", "{1}")
                        .Replace("{dt:d}", "{2}")
                        .Replace("{dt:t}", "{3}")
                        .Replace("{dt:o}", "{4}")
                        .Replace("{severity}", "{5}")
                        .Replace("{host}", "{6}")
                        .Replace("{user}", "{7}")
                        .Replace("{prefix}", "{8}")
                        .Replace("{suffix}", "{9}")
                        .Replace("{indent}", "{10}")
                        .Replace("{crlf}", "{11}");
                    DateTimeOffset dt = DateTimeOffset.Now;
                    string datetime = dt.ToString("s");
                    string date = dt.ToString("d");
                    string time = dt.ToString("T");
                    string utc = dt.ToUniversalTime().ToString("o");
                    string severityStr = LoggerHelper.SeverityName(severity, 5);
                    string resultLine = String.Format(
                        format,
                        inputLine, // 0
                        datetime, // 1
                        date, // 2
                        time, // 3
                        utc, // 4
                        severityStr, // 5
                        _hostName, // 6
                        _userName, // 7
                        Prefix, // 8
                        Suffix, // 9
                        indent, // 10
                        Environment.NewLine); // 11
                    if(DoAsyncIo)
                        _asyncWriteQueue.Dispatch(new AsyncLogData { Severity = severity, Text = resultLine }, AsyncWrite);
                    else
                        OnWrite(severity, resultLine);
                }
                catch (Exception excp)
                {
                    Debug.WriteLine($"BaseLogger.Log: Severity={severity},Message={input}");
                    Debug.WriteLine($"BaseLogger.Log: {GetType().Name}.OnWrite threw Exception: {excp}");
                    break; // don't bother with remaining lines
                }
            }
        }

        internal class AsyncLogData
        {
            public int Severity;
            public string Text;
        }

        private void AsyncWrite(AsyncLogData asyncData)
        {
            OnWrite(asyncData.Severity, asyncData.Text);
        }

        protected virtual void OnClear() { }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            if (DoAsyncIo)
                _asyncWriteQueue.Dispatch<AsyncLogData>(null, AsyncClear);
            else
                OnClear();
        }
        private void AsyncClear(object notUsed)
        {
            OnClear();
        }

        protected virtual void OnWrite(int severity, string text)
        {
            throw new NotImplementedException();
        }

        // unhandled exception catchers
        //private void UnhandledThreadException(object sender, ThreadExceptionEventArgs args)
        //{
        //    try
        //    {
        //        Exception excp = args.Exception;
        //        Log(excp);
        //        throw excp;
        //    }
        //    catch (Exception e2)
        //    {
        //        Trace.WriteLine(String.Format("UnhandledThreadException: Secondary exception: {0}", e2));
        //    }
        //}

        private void UnhandledDomainException(object sender, UnhandledExceptionEventArgs args)
        {
            try
            {
                var excp = args.ExceptionObject as Exception;
                Log(excp);
            }
            catch (Exception e2)
            {
                Debug.WriteLine($"UnhandledDomainException: Secondary exception: {e2}");
            }
        }
    }
}