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

namespace Highlander.Utilities.Logging
{
    public static class LogSeverity
    {
        public const int Fatal = 3;
        public const int Error = 2;
        public const int Warning = 1;
        public const int Info = 0; // default
        public const int Debug = -1;
        public const int DebugL2 = -2;
        public const int DebugL3 = -3;

        public static int Highest(int severity1, int severity2)
        {
            if (severity1 >= severity2)
                return severity1;
            return severity2;
        }

        public static bool IsWarning(int severity)
        {
            return (severity <= Warning);
        }
        public static bool IsInfo(int severity)
        {
            return (severity <= Info);
        }
        public static bool IsDebug(int severity)
        {
            return (severity <= Debug);
        }
    }

    public static class LoggerHelper
    {
        public static string SeverityName(int severity)
        {
            switch (severity)
            {
                case LogSeverity.Fatal:
                    return "FATAL";
                case LogSeverity.Error:
                    return "ERROR";
                case LogSeverity.Warning:
                    return "Warning";
                case LogSeverity.Info:
                    return "Information";
                case LogSeverity.Debug:
                    return "Debug";
                default:
                    return $"Debug{severity}";
            }
        }
        public static string SeverityName(int severity, int maxChars)
        {
            string s = SeverityName(severity);
            if (s.Length > maxChars)
                s = s.Substring(0, (maxChars-1)) + ".";
            return s;
        }
    }

    public delegate int GetLogSeverityDelegate();
}
