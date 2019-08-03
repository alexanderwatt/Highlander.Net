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
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Orion.Util.Logging
{
    public class TextBoxLogger : BaseLogger
    {
        private readonly SynchronizationContext _syncContext;
        private readonly TextBox _control;
        private const int MaxLines = 5000;

        public TextBoxLogger(TextBox control)
            : base(null, null)
        {
            _control = control ?? throw new ArgumentNullException(nameof(control));
            _syncContext = SynchronizationContext.Current;
            Format = "{dt:T} [{severity}] {prefix}{indent}{text}{suffix}{crlf}";
            DoAsyncIo = true;
        }

        protected override void OnClear()
        {
            _syncContext.Post(AsyncHandler, null);
        }

        protected override void OnWrite(int severity, string text)
        {
            _syncContext.Post(AsyncHandler, text);
        }

        private void AsyncHandler(object state)
        {
            // null means clear, otherwise append
            try
            {
                if (!_control.IsDisposed)
                {
                    if (state == null)
                        _control.Clear();
                    else
                    {
                        if (_control.Lines.Length > MaxLines)
                            _control.Clear();
                        _control.AppendText((string)state);
                    }
                }
            }
            catch (Exception excp)
            {
                Trace.WriteLine($"TextBoxLogger.AsyncHandler: Unhandled exception: {excp}");
            }
        }
    }
}
