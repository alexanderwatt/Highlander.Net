#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r3.Reporting;
using Orion.CurveEngine.PricingStructures;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using Orion.Numerics.Functions;
using Math = System.Math;

#endregion

namespace Orion.CurveEngine.PricingStructures.Bootstrappers
{
    ///<summary>
    ///</summary>
    public class CommoditySpreadAssetQuote2 : IObjectiveFunction
    {
        private IPriceableCommoditySpreadAssetController PriceableAsset { get; set; }
        private DateTime BaseDate { get; set; }
        private IDictionary<DateTime, double> Dfs { get; set; }
        private bool Extrapolation { get; set; }
        private ICommodityCurve BaseCurve { get; set; }
        private decimal MarketQuote { get; set; }
        private double Tolerance { get; set; }
        private const double DefaultTolerance = 0.0000001d;


        /// <summary>
        /// Initializes a new instance of the <see cref="RateSpreadAssetQuote"/> class.
        /// </summary>
        /// <param name="priceableAsset">The priceable asset.</param>
        /// <param name="referenceCurve">The reference curve.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="dfs">The DFS.</param>
        public CommoditySpreadAssetQuote2(IPriceableCommoditySpreadAssetController priceableAsset, ICommodityCurve referenceCurve, DateTime baseDate,
                              bool extrapolation, IDictionary<DateTime, double> dfs)
            : this(priceableAsset, referenceCurve, baseDate, extrapolation, dfs, DefaultTolerance)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="RateAssetQuote"/> class.
        /// </summary>
        /// <param name="priceableAsset">The priceable asset.</param>
        /// <param name="referenceCurve">The reference curve.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="dfs">The DFS.</param>
        /// <param name="tolerance">The tolerance.</param>
        public CommoditySpreadAssetQuote2(IPriceableCommoditySpreadAssetController priceableAsset,
                            ICommodityCurve referenceCurve, DateTime baseDate,
                              bool extrapolation, IDictionary<DateTime, double> dfs, double tolerance)
        {
            PriceableAsset = priceableAsset;
            BaseDate = baseDate;
            Dfs = dfs;
            Extrapolation = extrapolation;
            Tolerance = tolerance;
            BaseCurve = referenceCurve;
            MarketQuote = PriceableAsset.CalculateImpliedQuoteWithSpread(BaseCurve);
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
            var quoteError = CalculateQuoteError();
            return Math.Abs(quoteError) < Tolerance;
        }

        private double CalculateQuoteError()
        {
            var curve = new SimpleSpreadDiscountFactorCurve(BaseCurve, BaseDate, Extrapolation, Dfs);
            var impliedQuote = PriceableAsset.CalculateImpliedQuoteWithSpread(curve);
            return (double)(MarketQuote - impliedQuote);
        }
    }
}