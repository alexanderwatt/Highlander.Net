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
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.FX.Cash
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