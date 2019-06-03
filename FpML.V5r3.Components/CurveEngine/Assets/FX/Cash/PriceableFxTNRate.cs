#region Using directives

using System;
using FpML.V5r3.Reporting.Helpers;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.CurveEngine.Assets
{
    ///<summary>
    ///</summary>
    public class PriceableFxTNRate : PriceableFxForwardRate//, IPriceableRateSpreadAssetController
    {
        ///// <summary>
        ///// Initializes a new instance of the <see cref="PriceableFxSpotRate"/> class.
        ///// </summary>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="spotDateOffset">The business day adjustments.</param>
        ///// <param name="fxRateAsset"></param>
        ///// <param name="fxForward">The forward points.</param>
        //public PriceableFxTNRate(DateTime baseDate, RelativeDateOffset spotDateOffset, FxRateAsset fxRateAsset, BasicQuotation fxForward)
        //    : this(1.0m, baseDate, fxRateAsset,
        //           spotDateOffset, fxForward)
        //{}

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFxSpotRate"/> class.
        /// </summary>
        /// <param name="notionalAmount">The notional.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="fxRateAsset">The asset itself</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="fxForward">The forward points.</param>
        public PriceableFxTNRate(DateTime baseDate, decimal notionalAmount, FxSpotNodeStruct nodeStruct,
                                      FxRateAsset fxRateAsset, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fxForward)
            : base(baseDate, "1D", notionalAmount, nodeStruct, fxRateAsset, fixingCalendar, paymentCalendar, fxForward)
        {
            AdjustedStartDate = baseDate;
            //AdjustedEffectiveDate = fixingCalendar.Advance(AdjustedStartDate, OffsetHelper.FromInterval(Tenor, DayTypeEnum.Business), SpotDateOffset.businessDayConvention);
            RiskMaturityDate = fixingCalendar.Advance(AdjustedStartDate, OffsetHelper.FromInterval(Tenor, DayTypeEnum.Business), SpotDateOffset.businessDayConvention);
        }

        ///// <summary>
        ///// Gets the forward spot date.
        ///// </summary>
        ///// <returns></returns>
        //protected override DateTime GetForwardDate()
        //{
        //    return FixingCalendar.Advance(AdjustedStartDate, OffsetHelper.FromInterval(Tenor, DayTypeEnum.Business), SpotDateOffset.businessDayConvention);
        //}
    }
}