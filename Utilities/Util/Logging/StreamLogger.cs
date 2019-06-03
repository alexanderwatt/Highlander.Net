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
using System.IO;
using Orion.Util.Helpers;

namespace Orion.Util.Logging
{
    public class StreamLogger : BaseLogger
    {
        private StreamWriter _streamWriter;
        public StreamLogger(Stream stream, string prefix)
            : base(prefix, null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            _streamWriter = new StreamWriter(stream);
            Format = "{dt:d},{dt:T},{severity},{host},{user},{prefix}{indent}{text}{suffix}{crlf}";
            // do async IO
            DoAsyncIo = true;
        }

        protected override void OnWrite(int severity, string text)
        {
            _streamWriter.Write(text);
        }

        protected override void OnFlush()
        {
            _streamWriter.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // clean up managed resources
                DisposeHelper.SafeDispose(ref _streamWriter);
            }
            // no unmanaged resources to clean up
            base.Dispose(disposing);
        }
    }
}
