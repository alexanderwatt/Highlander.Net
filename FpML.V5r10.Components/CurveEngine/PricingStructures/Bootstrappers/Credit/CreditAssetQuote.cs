#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using Math = System.Math;
using Orion.Analytics.Solvers;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.CurveEngine.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public class CreditAssetQuote : IObjectiveFunction
    {
        private readonly IPriceableCreditAssetController _priceableAsset;
        private readonly DateTime                  _baseDate;
        private readonly IList<DateTime>           _dates;
        private readonly double[]                  _dfs;
        private readonly InterpolationMethod       _interpolationMethod;
        private readonly bool                      _extrapolation;
        private readonly Double _cTolerance;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditAssetQuote"/> class.
        /// </summary>
        /// <param name="priceableAsset">The priceable asset.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="interpolation">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="dates">The dates.</param>
        /// <param name="dfs">The DFS.</param>
        public CreditAssetQuote(IPriceableCreditAssetController priceableAsset, DateTime baseDate, InterpolationMethod interpolation,
                              bool extrapolation, IList<DateTime> dates, double[] dfs)
            : this(priceableAsset, baseDate, interpolation, extrapolation, dates, dfs, 0.0000001d)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditAssetQuote"/> class.
        /// </summary>
        /// <param name="priceableAsset">The priceable asset.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="interpolation">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="dates">The dates.</param>
        /// <param name="dfs">The DFS.</param>
        /// <param name="tolerance">The tolerance.</param>
        public CreditAssetQuote(IPriceableCreditAssetController priceableAsset, DateTime baseDate, InterpolationMethod interpolation,
                          bool extrapolation, IList<DateTime> dates, double[] dfs, double tolerance)//TODO add tolerance/accuracy.
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
        /// <returns></returns>
        public double Value(double discountFactor)
        {
            _dfs[_dfs.Length - 1] = discountFactor;
            var curve = new SimpleDiscountFactorCurve(_baseDate, _interpolationMethod, _extrapolation, _dates, _dfs);
            var impliedQuote = _priceableAsset.CalculateImpliedQuote(curve);
            var marketQuote = MarketQuoteHelper.NormalisePriceUnits(_priceableAsset.MarketQuote, "DecimalRate").value;
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
            var curve = new SimpleDiscountFactorCurve(_baseDate, _interpolationMethod, _extrapolation, _dates, _dfs);
            var impliedQuote = _priceableAsset.CalculateImpliedQuote(curve);
            var marketQuote = MarketQuoteHelper.NormalisePriceUnits(_priceableAsset.MarketQuote, "DecimalRate").value;
            var quoteError = (double)(marketQuote - impliedQuote);
            return Math.Abs(quoteError) < _cTolerance;
        }
    }
}