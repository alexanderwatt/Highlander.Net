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
using Orion.Util.Logging;

namespace Orion.Util.Threading
{
    public class LoggedCounter
    {
        private readonly ILogger _Logger;
        private readonly string _Name;
        private long _Counter;
        public long Counter { get { return Interlocked.Add(ref _Counter, 0); } }
        // reporting variables
        private long _LogInterval = 1000;
        private int _SpinLock;
        private long _LastCount;

        public LoggedCounter(ILogger logger, string name, long startValue, long logInterval)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _Name = name ?? "Counter";
            _Counter = startValue;
            _LogInterval = logInterval;
        }

        public long Increment()
        {
            long counter = Interlocked.Increment(ref _Counter);
            try
            {
                int spinlock = Interlocked.Increment(ref _SpinLock);
                if ((_Logger != null) && (spinlock == 1) && (counter != _LastCount) && (counter % _LogInterval == 0))
                {
                    _LastCount = counter;
                    _Logger.LogDebug("{0} incremented to: {1}", _Name, counter.ToString("D7"));
                }
            }
            finally
            {
                Interlocked.Decrement(ref _SpinLock);
            }
            return counter;
        }

        public long Decrement()
        {
            long counter = Interlocked.Decrement(ref _Counter);
            try
            {
                int spinlock = Interlocked.Increment(ref _SpinLock);
                if ((_Logger != null) && (spinlock == 1) && (counter != _LastCount) && (counter % _LogInterval == 0))
                {
                    _LastCount = counter;
                    _Logger.LogDebug("{0} decremented to: {1}", _Name, counter.ToString("D7"));
                }
            }
            finally
            {
                Interlocked.Decrement(ref _SpinLock);
            }
            return counter;
        }
    }
}