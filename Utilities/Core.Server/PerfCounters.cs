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

#region Using directives

using System;
using System.Collections.Generic;
using System.Threading;
using Orion.Util.Threading;

#endregion

namespace Core.Server
{
    public class StatsCounterSet
    {
        // constants
        public const char ListSep = ';';
        public const char NameSep = '.';
        private static readonly char[] ListSepChars = { ListSep };
        private static readonly char[] NameSepChars = { NameSep };

        private readonly object _lock = new object();
        private readonly GuardedDictionary<string, StatsCounter> _counters = new GuardedDictionary<string, StatsCounter>();

        // helpers
        /// <summary>
        /// Increments a single named counter.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public long Add(string name, long count)
        {
            return GetCounter(name).Add(count);
        }

        /// <summary>
        /// Increments multiple named counters (semicolon seperated list)
        /// </summary>
        /// <param name="names">The names.</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public void AddToMultiple(string names, long count)
        {
            foreach (string name in names.Split(ListSepChars))
            {
                GetCounter(name).Add(count);
            }
        }
        /// <summary>
        /// Increments multiple named counters by the count of others.
        /// </summary>
        /// <param name="counters">The counters.</param>
        public void AddToMultiple(IEnumerable<StatsCounter> counters)
        {
            foreach (StatsCounter counter in counters)
            {
                GetCounter(counter.Name).Add(counter.Count);
            }
        }
        /// <summary>
        /// Increments all counters in the dot-seperated named hierarchy
        /// </summary>
        /// <param name="name">The names list.</param>
        /// <returns></returns>
        public void AddToHierarchy(string name)
        {
            string[] nameParts = name.Split(NameSepChars);
            string subName = nameParts[0];
            GetCounter(subName).Add(1);
            for (int i = 1; i < nameParts.Length; i++)
            {
                subName += (NameSep + nameParts[i]);
                GetCounter(subName).Add(1);
            }
        }

        /// <summary>
        /// Gets the counter
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public StatsCounter GetCounter(string name)
        {
            return _counters.GetOrSet(name, () => new StatsCounter(name));
        }

        /// <summary>
        /// Gets the counters
        /// </summary>
        /// <param name="reset"></param>
        /// <returns></returns>
        public List<StatsCounter> GetCounters(bool reset)
        {
            return _counters.GetValues(reset);
        }
    }

    /// <summary>
    /// StatsCounter class
    /// </summary>
    public class StatsCounter : IComparer<StatsCounter>, IComparable<StatsCounter>
    {
        /// <summary>
        /// The name
        /// </summary>
        public readonly string Name;

        private long _count;

        /// <summary>
        /// The count
        /// </summary>
        public long Count => Interlocked.Add(ref _count, 0);

        /// <summary>
        /// Adds to the count
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public long Add(long count) { return Interlocked.Add(ref _count, count); }

        /// <summary>
        /// The Stats counter
        /// </summary>
        /// <param name="name"></param>
        public StatsCounter(string name) { Name = name; }

        /// <summary>
        /// The Stats counter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startCount"></param>
        public StatsCounter(string name, long startCount) { Name = name; _count = startCount; }

        /// <summary>
        /// Compares stats counters
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(StatsCounter x, StatsCounter y)
        {
            return String.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compares the provided stats counter with the current one.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(StatsCounter other)
        {
            return String.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
