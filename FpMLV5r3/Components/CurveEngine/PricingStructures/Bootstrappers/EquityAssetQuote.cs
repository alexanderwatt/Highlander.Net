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
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Math = System.Math;
using Highlander.Reporting.Analytics.V5r3.Solvers;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public class EquityAssetQuote : IObjectiveFunction
    {
        private readonly IPriceableEquityAssetController _priceableAsset;
        private readonly DateTime                  _baseDate;
        private readonly IList<DateTime>           _dates;
        private readonly IList<double>                  _dfs;
        private readonly InterpolationMethod       _interpolationMethod;
        private readonly bool                      _extrapolation;
        private readonly Double                     _cTolerance;

        /// <summary>
        /// Initializes a new instance of the <see cref="EquityAssetQuote"/> class.
        /// </summary>
        /// <param name="priceableAsset">The priceable asset.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="interpolation">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="dates">The dates.</param>
        /// <param name="dfs">The DFS.</param>
        public EquityAssetQuote(IPriceableEquityAssetController priceableAsset, DateTime baseDate, InterpolationMethod interpolation,
                              bool extrapolation, IList<DateTime> dates, double[] dfs)
            : this(priceableAsset, baseDate, interpolation, extrapolation, dates, dfs, 0.0000001d)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="EquityAssetQuote"/> class.
        /// </summary>
        /// <param name="priceableAsset">The priceable asset.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="interpolation">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="dates">The dates.</param>
        /// <param name="dfs">The DFS.</param>
        /// <param name="tolerance">The tolerance.</param>
        public EquityAssetQuote(IPriceableEquityAssetController priceableAsset, DateTime baseDate, InterpolationMethod interpolation,
                            bool extrapolation, IList<DateTime> dates, IList<double> dfs, double tolerance)//TODO add tolerance/accuracy.
        {
            _priceableAsset = priceableAsset;
            _baseDate = baseDate;
            _dates = dates;
            _dfs = dfs;
            _interpolationMethod = interpolation;
            _extrapolation = extrapolation;
            _cTolerance = tolerance;
        }

        /// <summary>
        /// Values the specified discount factor.
        /// </summary>
        /// <param name="discountFactor">The discount factor.</param>
        /// <returns>Quote error - difference between market quote and implied (from the bootstrapped term structure) quote</returns>
        public double Value(double discountFactor)
        {
            _dfs[_dfs.Count - 1] = discountFactor;
            var curve = new SimpleFxCurve(_baseDate, _interpolationMethod, _extrapolation, _dates, _dfs);
            var impliedQuote = _priceableAsset.CalculateImpliedQuote(curve);
            var marketQuote = MarketQuoteHelper.NormalisePriceUnits(_priceableAsset.MarketQuote, "Price").value;
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
            var curve = new SimpleFxCurve(_baseDate, _interpolationMethod, _extrapolation, _dates, _dfs);
            var impliedQuote = _priceableAsset.CalculateImpliedQuote(curve);
            var marketQuote = MarketQuoteHelper.NormalisePriceUnits(_priceableAsset.MarketQuote, "Price").value;
            var quoteError = (double)(marketQuote - impliedQuote);
            return Math.Abs(quoteError) < _cTolerance;
        }
    }
}