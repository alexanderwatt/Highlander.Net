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

#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Orion.ModelFramework.Assets;
using Math = System.Math;
using FpML.V5r3.Reporting;
using Orion.Analytics.Solvers;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.CurveEngine.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public class BondAssetQuote : IObjectiveFunction
    {
        private IPriceableBondAssetController PriceableAsset { get; }
        private DateTime BaseDate { get; }
        private IDictionary<DateTime, double> Dfs { get; }
        private InterpolationMethod InterpolationMethod { get; }
        private bool Extrapolation { get; }
        private double Tolerance { get; }
        private const double DefaultTolerance = 0.0000001d;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommodityAssetQuote"/> class.
        /// </summary>
        /// <param name="priceableAsset">The priceable asset.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="interpolation">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="dfs">The DFS.</param>
        public BondAssetQuote(IPriceableBondAssetController priceableAsset, DateTime baseDate, InterpolationMethod interpolation,
                              bool extrapolation, IDictionary<DateTime, double> dfs)
            : this(priceableAsset, baseDate, interpolation, extrapolation, dfs, DefaultTolerance)
        {}


        /// <summary>
        /// Initializes a new instance of the <see cref="RateAssetQuote"/> class.
        /// </summary>
        /// <param name="priceableAsset">The priceable asset.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="interpolation">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="dfs">The discount factors.</param>
        /// <param name="tolerance">The tolerance.</param>
        public BondAssetQuote(IPriceableBondAssetController priceableAsset, DateTime baseDate, InterpolationMethod interpolation,
                              bool extrapolation, IDictionary<DateTime, double> dfs, double tolerance)
        {
            PriceableAsset = priceableAsset;
            BaseDate = baseDate;
            Dfs = dfs;
            InterpolationMethod = interpolation;
            Extrapolation = extrapolation;
            Tolerance = tolerance;
        }

        /// <summary>
        /// Values the specified discount factor.
        /// </summary>
        /// <param name="discountFactor">The discount factor.</param>
        /// <returns>Quote error - difference between market quote and implied (from the bootstrapped term structure) quote</returns>
        public double Value(double discountFactor)
        {
            Dfs[Dfs.Keys.Last()] = discountFactor;
            var curve = new SimpleCommodityCurve(BaseDate, InterpolationMethod, Extrapolation, Dfs);
            var impliedQuote = PriceableAsset.CalculateImpliedQuote(curve);
            var marketQuote = MarketQuoteHelper.NormalisePriceUnits(PriceableAsset.MarketQuote, "DecimalRate").value;
            var quoteError = (double)(marketQuote - impliedQuote);
            return quoteError;
        }

        ///<summary>
        /// This will need to be implemented if used in certain solvers.
        ///</summary>
        ///<param name="x"></param>
        ///<returns></returns>
        public double Derivative(double x)
        {
            return 0.0;
        }

        /// <summary>
        /// An inital test to see if the guess was good..
        /// </summary>
        /// <returns></returns>
        public Boolean InitialValue()
        {
            var curve = new SimpleCommodityCurve(BaseDate, InterpolationMethod, Extrapolation, Dfs);
            var impliedQuote = PriceableAsset.CalculateImpliedQuote(curve);
            var marketQuote = MarketQuoteHelper.NormalisePriceUnits(PriceableAsset.MarketQuote, "DecimalRate").value;
            var quoteError = (double)(marketQuote - impliedQuote);
            return Math.Abs(quoteError) < Tolerance;
        }
    }
}