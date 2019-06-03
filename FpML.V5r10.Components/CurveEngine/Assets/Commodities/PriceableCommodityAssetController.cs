using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Analytics.Interpolations.Points;

namespace Orion.CurveEngine.Assets
{
    ///<summary>
    ///</summary>
    public abstract class PriceableCommodityAssetController : AssetControllerBase, IPriceableCommodityAssetController
    {
        /// <summary>
        /// The Rate quotation
        /// </summary>
        public BasicQuotation CommodityValue => MarketQuote;

        //TODO change to a commodity sometime as per 4.7.
        /// <summary>
        /// 
        /// </summary>
        public Commodity CommodityAsset { get; protected set; }

        #region IPriceableAssetController Members

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public abstract decimal IndexAtMaturity { get; }


        #endregion


        /// <summary>
        /// Gets the forward spot date.
        /// </summary>
        /// <returns></returns>
        protected DateTime GetForwardDate(DateTime spotDate, IBusinessCalendar paymentCalendar, Period tenor, BusinessDayConventionEnum businessDayConvention)
        {
            return paymentCalendar.Advance(spotDate, OffsetHelper.FromInterval(tenor, DayTypeEnum.Calendar), businessDayConvention);
        }

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        protected decimal GetIndex(ICommodityCurve discountFactorCurve, DateTime targetDate,
                                         DateTime valuationDate)
        {
            IPoint point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = (decimal)discountFactorCurve.Value(point);
            return discountFactor;
        }

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        protected decimal GetIndex(IInterpolatedSpace discountFactorCurve, DateTime targetDate,
                                         DateTime valuationDate)
        {
            IPoint point = new DateTimePoint1D(valuationDate, targetDate);
            var discountFactor = (decimal)discountFactorCurve.Value(point);
            return discountFactor;
        }
    }
}