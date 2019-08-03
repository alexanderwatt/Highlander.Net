/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

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

namespace Orion.Analytics.Lattices
{
    public abstract class EquityBinomialTree : BinomialTree, ICloneable
    {
        protected EquityBinomialTree(int nColumns)
            : base(nColumns) 
        {}
      

        /// <summary>
        /// The forward rate applying at the i-th node to the (i+1)-th node of the tree.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public abstract double Rate(int i);

        /// <summary>
        /// The present value of dividends at the i-th node of the tree
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public abstract double Dividend(int i);


        /// <summary>
        /// Volatility on tree
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public abstract double Volatility { get; }

        /// <summary>
        /// Gets the time.
        /// </summary>
        /// <value>The time.</value>
        public abstract double Time { get; }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return this;
        }
    }
}
