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
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.V5r3;
// Model Analytics

namespace Highlander.CurveEngine.V5r3.Assets.Commodities
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