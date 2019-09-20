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

namespace Highlander.Utilities.Threading
{
    public interface ILock
    {
        void Enter();
        void Leave();
    }

    /// <summary>
    /// Implements a non-blocking non-nestable non-re-entrant spinlock.
    /// Notes:
    ///   - Non-blocking does not imply "wait-free" - the Enter method will
    ///     "spin" waiting (but not blocking) until it acquires the spinlock. 
    ///   - Order through the spinlock is not guaranteed.
    ///   - On a single core the spinlock will yield the CPU.
    /// </summary>
    public class NonblockingSpinlock : ILock
    {
        private int _lock;
        private readonly bool _isSingleCore = (Environment.ProcessorCount == 1);

        /// <summary>
        /// 
        /// </summary>
        public void Enter()
        {
            while (Interlocked.CompareExchange(ref _lock, 1, 0) != 0)
            {
                // spin
                if (_isSingleCore)
                    Thread.Sleep(0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Leave()
        {
            Interlocked.Exchange(ref _lock, 0);
        }
    }
}
