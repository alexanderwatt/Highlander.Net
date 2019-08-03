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

namespace Orion.Util.Logging
{
    public class ConsoleLogger : BaseLogger
    {
        public ConsoleLogger()
            : base(null, null)
        {
            Format = "{text}{crlf}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        public ConsoleLogger(string prefix)
            : base(prefix, null)
        {
            Format = "{prefix}{indent}{text}{crlf}";
        }

        protected override void OnWrite(int severity, string text)
        {
            if (severity >= LogSeverity.Warning)
                Console.Error.Write(text);
            else
                Console.Out.Write(text);
        }

        protected override void OnFlush()
        {
            Console.Out.Flush();
            Console.Error.Flush();
        }
    }
}
