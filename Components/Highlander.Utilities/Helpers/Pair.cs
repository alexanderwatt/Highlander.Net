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

namespace Highlander.Utilities.Helpers
{
    /// <summary>
    /// Pairs to types
    /// </summary>
    /// <typeparam name="T1">The type of the 1.</typeparam>
    /// <typeparam name="T2">The type of the 2.</typeparam>
    [Serializable]
    public sealed class Pair<T1, T2>
    {
        /// <summary>
        /// first item
        /// </summary>
        public T1 First;

        /// <summary>
        /// Second Item
        /// </summary>
        public T2 Second;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pair&lt;T1, T2&gt;"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        public Pair(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
    }
}
