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

using System.Diagnostics;

namespace Orion.Util.Logging
{
    public class TraceLogger : BaseLogger
    {
        public TraceLogger(bool asyncIO)
            : base(null, null)
        {
            DoAsyncIo = asyncIO;
            Format = "({severity}) {user} {prefix}{indent}{text}{suffix}";
        }

        protected override void OnWrite(int severity, string text)
        {
            Trace.WriteLine(text);
        }

        protected override void OnFlush()
        {
            Trace.Flush();
        }
    }
}
