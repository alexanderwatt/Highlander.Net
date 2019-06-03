#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;
using Orion.Analytics.Solvers;
using Orion.CurveEngine.Helpers;
using Math = System.Math;

#endregion

namespace Orion.CurveEngine.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public class ClearedRateAssetQuote : IObjectiveFunction
    {
        private IPriceableClearedRateAssetController PriceableClearedRateAsset { get; }
        public IInterpolatedSpace BaseCurve { get; set; }
        protected DateTime BaseDate { get; set; }
        protected IDictionary<DateTime, double> Dfs { get; set; }
        protected InterpolationMethod InterpolationMethod { get; set; }
        protected bool Extrapolation { get; set; }
        protected double Tolerance { get; set; }
        private const double DefaultTolerance = 0.0000001d;
        public decimal Quote { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateAssetQuote"/> class.
        /// </summary>
        /// <param name="priceableAsset">The priceable asset.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="interpolation">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="dfs">The discount factors.</param>
        /// <param name="baseCurve">The base curve i.e. the OIS dicount curve.</param>
        public ClearedRateAssetQuote(IPriceableClearedRateAssetController priceableAsset, DateTime baseDate, InterpolationMethod interpolation,
                              bool extrapolation, IDictionary<DateTime, double> dfs, IInterpolatedSpace baseCurve)
            : this(priceableAsset, baseDate, interpolation, extrapolation, dfs, baseCurve, DefaultTolerance)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="RateAssetQuote"/> class.
        /// </summary>
        /// <param name="priceableAsset">The priceable asset.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="interpolation">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="dfs">The discount factors.</param>
        /// <param name="baseCurve">The base curve i.e. the OIS dicount curve.</param>
        /// <param name="tolerance">The tolerance.</param>
        public ClearedRateAssetQuote(IPriceableClearedRateAssetController priceableAsset, DateTime baseDate, InterpolationMethod interpolation,
                              bool extrapolation, IDictionary<DateTime, double> dfs, IInterpolatedSpace baseCurve, double tolerance)
        {
            PriceableClearedRateAsset = priceableAsset;
            BaseCurve = baseCurve;
            BaseDate = baseDate;
            Dfs = dfs;
            InterpolationMethod = interpolation;
            Extrapolation = extrapolation;
            Tolerance = tolerance;
            Quote = MarketQuoteHelper.NormalisePriceUnits(priceableAsset.MarketQuote, "DecimalRate").value;
        }

        /// <summary>
        /// Values the specified discount factor.
        /// </summary>
        /// <param name="discountFactor">The discount factor.</param>
        /// <returns></returns>
        public double Value(double discountFactor)
        {
            Dfs[Dfs.Keys.Last()] = discountFactor;
            return CalculateQuoteError();
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
            double quoteError = CalculateQuoteError();
            return Math.Abs(quoteError) < Tolerance;
        }

        public double CalculateQuoteError()
        {
            var curve = new SimpleDiscountFactorCurve(BaseDate, InterpolationMethod, Extrapolation, Dfs);
            var impliedQuote = PriceableClearedRateAsset.CalculateImpliedQuote(curve, BaseCurve);
            return (double)(Quote - impliedQuote);
        }
    }
}