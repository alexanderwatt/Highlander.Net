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
using System.Threading;
using Highlander.Utilities.Logging;

namespace Highlander.Utilities.Threading
{
    public class LoggedCounter
    {
        private readonly ILogger _logger;
        private readonly string _name;
        private long _counter;
        public long Counter => Interlocked.Add(ref _counter, 0);

        // reporting variables
        private readonly long _logInterval = 1000;
        private int _spinLock;
        private long _lastCount;

        public LoggedCounter(ILogger logger, string name, long startValue, long logInterval)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _name = name ?? "Counter";
            _counter = startValue;
            _logInterval = logInterval;
        }

        public long Increment()
        {
            long counter = Interlocked.Increment(ref _counter);
            try
            {
                int spinLock = Interlocked.Increment(ref _spinLock);
                if ((_logger != null) && (spinLock == 1) && (counter != _lastCount) && (counter % _logInterval == 0))
                {
                    _lastCount = counter;
                    _logger.LogDebug("{0} incremented to: {1}", _name, counter.ToString("D7"));
                }
            }
            finally
            {
                Interlocked.Decrement(ref _spinLock);
            }
            return counter;
        }

        public long Decrement()
        {
            long counter = Interlocked.Decrement(ref _counter);
            try
            {
                int spinLock = Interlocked.Increment(ref _spinLock);
                if ((_logger != null) && (spinLock == 1) && (counter != _lastCount) && (counter % _logInterval == 0))
                {
                    _lastCount = counter;
                    _logger.LogDebug("{0} decremented to: {1}", _name, counter.ToString("D7"));
                }
            }
            finally
            {
                Interlocked.Decrement(ref _spinLock);
            }
            return counter;
        }
    }
}