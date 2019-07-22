/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

// COM interop attributes
// some useful attributes

namespace Highlander.Numerics.Lattices
{
    /// <summary>
    /// Jarrow-Rudd binomial tree.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The binomial method is the simplest numerical method that can be used to
    /// price path-independent derivatives. It is usually the preferred lattice
    /// method under the Black-Scholes-Merton model. 
    /// See <see cref="BinomialTree"/> for a class based on 
    /// a <see cref="JarrowRudd"/>.
    /// </para>
    /// <para>
    /// There are several approaches to build the underlying binomial tree,
    /// like <see cref="CoxRossRubinstein">Jarrow-Rudd</see> or 
    /// <see cref="BlackScholesLattice">Cox-Ross-Rubinstein</see>.
    /// </para>
    /// </remarks>
    public class JarrowRudd : BinomialTree 
    {
        ///<summary>
        ///</summary>
        ///<param name="volatility"></param>
        ///<param name="riskFreeRate"></param>
        ///<param name="underlying"></param>
        ///<param name="endTime"></param>
        ///<param name="steps"></param>
        public JarrowRudd(double volatility, double riskFreeRate,
                          double underlying, double endTime, int steps)
            : base(steps + 1)
        {
            _x0 = underlying;
            _sigma = volatility;
            _mu = riskFreeRate - 0.5*_sigma*_sigma;
            _dt = endTime/steps;
        }

        private readonly double _x0;
        private readonly double _sigma;
        private readonly double _mu;
        private readonly double _dt;

        #region Tree implementation
        public override double Underlying(int i, int index)
        {
            int j = (2*index - i);
            return _x0 * Math.Exp(i*_mu*_dt + j*_sigma*Math.Sqrt(_dt));
        }

        public override double Probability(int i, int index, int branch)
        {
            return 0.5;
        }
        #endregion
		
    }
}