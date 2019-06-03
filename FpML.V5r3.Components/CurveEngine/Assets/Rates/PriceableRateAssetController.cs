#region Using directives

using System;
using Orion.Analytics.Interpolations.Points;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.CurveEngine.Assets
{
    ///<summary>
    ///</summary>
    public abstract class PriceableRateAssetController : AssetControllerBase, IPriceableRateAssetController
    {
        #region Properties

        /// <summary>
        /// The Rate quotation
        /// </summary>
        public BasicQuotation FixedRate => MarketQuote;

        #endregion

        #region IPriceableAssetController Members

        ///<summary>
        ///</summary>
        ///<param name="interpolatedSpace"></param>
        ///<returns></returns>
        public abstract decimal CalculateDiscountFactorAtMaturity(IInterpolatedSpace interpolatedSpace);

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public abstract decimal DiscountFactorAtMaturity { get; }

        /// <summary>
        /// AdjustedStartDate
        /// </summary>
        public DateTime AdjustedStartDate { get; protected set; }

        #endregion

        #region Discount Factor Helpers

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        protected decimal GetDiscountFactor(IRateCurve discountFactorCurve, DateTime targetDate,
                                         DateTime valuationDate)
        {
            if (targetDate == valuationDate)
            {
                return 1.0m;
            }
            IPoint point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = (decimal)discountFactorCurve.Value(point);
            return discountFactor;
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        protected decimal GetDiscountFactor(IInterpolatedSpace discountFactorCurve, DateTime targetDate,
                                         DateTime valuationDate)
        {
            if (targetDate == valuationDate)
            {
                return 1.0m;
            }
            IPoint point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = (decimal)discountFactorCurve.Value(point);
            return discountFactor;
        }

        #endregion
    }
}