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
    /// <summary>
    /// Cox-Ross-Rubinstein binomial tree.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The binomial method is the simplest numerical method that can be used to
    /// price path-independent derivatives. It is usually the preferred lattice
    /// method under the Black-Scholes-Merton model. 
    /// See <see cref="BlackScholesLattice"/> for a class based on 
    /// a <see cref="BinomialTree"/>.
    /// </para>
    /// <para>
    /// There are several approaches to build the underlying binomial tree,
    /// like <see cref="JarrowRudd">Jarrow-Rudd</see> or 
    /// <see cref="CoxRossRubinstein">Cox-Ross-Rubinstein</see>.
    /// </para>
    /// </remarks>
    public class CoxRossRubinstein : BinomialTree 
    {
        ///<summary>
        ///</summary>
        ///<param name="volatility"></param>
        ///<param name="riskFreeRate"></param>
        ///<param name="underlying"></param>
        ///<param name="endTime"></param>
        ///<param name="steps"></param>
        public CoxRossRubinstein(double volatility, double riskFreeRate,
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
        public override double Probability(int i, int index, int branch)
        {
            if (branch == 1)
                return 0.5 + 0.5*(_mu/_sigma)*Math.Sqrt(_dt);
            return 0.5 - 0.5*(_mu/_sigma)*Math.Sqrt(_dt);
        }

        public override double Underlying(int i, int index)
        {
            int j = (2*index - i);
            return _x0 * Math.Exp(          j*_sigma*Math.Sqrt(_dt));
        }
        #endregion
		
    }
}