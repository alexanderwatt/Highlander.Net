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

namespace Orion.Util.Logging
{
    public interface ILogger : IDisposable
    {
        void Clear();
        void Flush();
        void Log(int severity, string indent, string msg);
        void Log(Exception e);
        void LogDebug(string msg);
        void LogDebug(string format, object arg0);
        void LogDebug(string format, object arg0, object arg1);
        void LogDebug(string format, object arg0, object arg1, object arg2);
        void LogDebug(string format, params object[] args);
        void LogInfo(string msg);
        void LogInfo(string format, object arg0);
        void LogInfo(string format, object arg0, object arg1);
        void LogInfo(string format, object arg0, object arg1, object arg2);
        void LogInfo(string format, params object[] args);
        void LogWarning(string msg);
        void LogWarning(string format, object arg0);
        void LogWarning(string format, object arg0, object arg1);
        void LogWarning(string format, object arg0, object arg1, object arg2);
        void LogWarning(string format, params object[] args);
        void LogError(string msg);
        void LogError(Exception ex);
        void LogError(string format, object arg0);
        void LogError(string format, object arg0, object arg1);
        void LogError(string format, object arg0, object arg1, object arg2);
        void LogError(string format, params object[] args);
        void LogFatal(string msg);
        void LogFatal(Exception ex);
        void LogFatal(string format, object arg0);
        void LogFatal(string format, object arg0, object arg1);
        void LogFatal(string format, object arg0, object arg1, object arg2);
        void LogFatal(string format, params object[] args);
    }
}