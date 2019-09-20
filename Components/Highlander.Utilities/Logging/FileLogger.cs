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
using System.IO;
using Highlander.Utilities.Helpers;

namespace Highlander.Utilities.Logging
{
    public class FileLogger : BaseLogger
    {
        private readonly string _templateFilename;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        public FileLogger(string filename)
            : base(null, null)
        {
            Format = "{dt:d},{dt:T},{severity},{host},{user},{prefix}{indent}{text}{suffix}{crlf}";
            _templateFilename = filename ?? throw new ArgumentNullException(nameof(filename));
            // do async IO
            DoAsyncIo = true;
            EnsureFileIsOpen();
        }
        // managed state
        private StreamWriter _streamWriter;
        private string _currentFilename;

        private void EnsureFileIsOpen()
        {
            // replace date tokens in template with values
            string proposedFilename = StringHelper.ReplaceDateTimeTokens(_templateFilename, DateTimeOffset.Now);
            if (String.Compare(proposedFilename, _currentFilename, StringComparison.OrdinalIgnoreCase) != 0)
            {
                // filename new or changed
                DisposeHelper.SafeDispose(ref _streamWriter);
                // open the new stream
                // - create directory if required
                // - append only if first open
                string dirName = Path.GetDirectoryName(proposedFilename);
                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);
                _streamWriter = new StreamWriter(proposedFilename, (_currentFilename == null));
                _currentFilename = proposedFilename;
            }
        }

        protected override void OnWrite(int severity, string text)
        {
            EnsureFileIsOpen();
            _streamWriter.Write(text);
            _streamWriter.Flush();
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
