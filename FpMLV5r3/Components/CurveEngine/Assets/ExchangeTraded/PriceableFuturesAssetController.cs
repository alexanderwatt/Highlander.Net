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
using Highlander.CurveEngine.V5r3.Assets.Rates;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.Models.V5r3.Futures;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.ExchangeTraded
{
    ///<summary>
    ///</summary>
    public abstract class PriceableFuturesAssetController : AssetControllerBase, IPriceableFuturesAssetController
    {
        ///<summary>
        ///</summary>
        ///<typeparam name="TR"></typeparam>
        public delegate TR DelegateToCalculateMethod<out TR>();

        /// <summary>
        /// 
        /// </summary>
        public Future Future { get; protected set; }

        /// <summary>
        /// The exchange code for the contract.
        /// </summary>
        public string Code { get; protected set; }

        /// <summary>
        /// AdjustedStartDate
        /// </summary>
        public DateTime AdjustedStartDate { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public IFuturesAssetResults AnalyticResults { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime BaseDate { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime RiskMaturityDate { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public BusinessDayAdjustments BusinessDayAdjustments { get; protected set; }

        /// <summary>
        /// Gets the day counter.
        /// </summary>
        /// <value>The day counter.</value>
        public IDayCounter DayCounter { get; protected set; }

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>The position.</value>
        public int Position { get; protected set; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public decimal TimeToExpiry { get; protected set; }

        /// <summary>
        /// Gets or sets the name of the curve.
        /// </summary>
        /// <value>The name of the curve.</value>
        public string CurveName { get; set; }

        /// <summary>
        /// Gets the time to expiry.
        /// </summary>
        /// <returns></returns>
        public decimal GetTimeToExpiry()
        {
            return TimeToExpiry;
        }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public decimal Volatility { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public Decimal Strike { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public RelativeDateOffset FuturesLag { get; protected set; }

        /// <summary>
        /// Gets the exchange initial margin.
        /// </summary>
        /// <value>The exchange initial margin.</value>
        public decimal InitialMargin { get; protected set; }

        /// <summary>
        /// Gets the last settled value.
        /// </summary>
        /// <value>The last settled value.</value>
        public decimal LastSettledValue { get; protected set; }

        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <value>The current value.</value>
        public decimal IndexAtMaturity { get; protected set; }

        /// <summary>
        /// Gets the last trading date.
        /// </summary>
        /// <value>The last trading dat.</value>
        public DateTime LastTradeDate { get; protected set; }

        /// <summary>
        /// Returns the futures expiry or the option expiry date.
        /// </summary>
        /// <returns></returns>
        public DateTime GetBootstrapDate() => LastTradeDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFuturesAssetController"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="position">The number of contracts.</param>
        /// <param name="nodeStruct"></param>
        /// <param name="marketQuote">In the case of a future, this is a rate.</param>
        /// <param name="extraQuote">In the case of a future, this is the futures convexity volatility.</param>
        protected PriceableFuturesAssetController(DateTime baseDate, int position, FutureNodeStruct nodeStruct,
            BasicQuotation marketQuote, Decimal extraQuote)
        {
            Id = nodeStruct.Future.id;
            Position = position;
            BaseDate = baseDate;
            SetQuote(marketQuote);
            //This handles the case where the underlying index is pat of an IR future node type.
            Volatility = extraQuote;
            BusinessDayAdjustments = nodeStruct.BusinessDayAdjustments;
            FuturesLag = nodeStruct.SpotDate;
            Future = nodeStruct.Future;        
        }

        /// <summary>
        /// Sets the market quote, which is a rate for a future and a lognormal volatility for an option.
        /// </summary>
        /// <param name="marketQuote">The fixed rate.</param>
        private void SetQuote(BasicQuotation marketQuote)
        {
            if (String.Compare(marketQuote.measureType.Value, PriceableSimpleRateAsset.RateQuotationType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                marketQuote.measureType.Value = PriceableSimpleRateAsset.RateQuotationType;
            }
            else
            {
                throw new ArgumentException("Quotation must be of type {0}", PriceableSimpleRateAsset.RateQuotationType);
            }
            MarketQuote = marketQuote.measureType.Value == PriceableSimpleRateAsset.RateQuotationType
                ? marketQuote
                : null;
        }

        #region GetIndex Helpers

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="discountFactorCurve">The discount factor curve.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="valuationDate">The valuation date.</param>
        /// <returns></returns>
        protected decimal GetIndex(IRateCurve discountFactorCurve, DateTime targetDate,
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
        protected decimal GetIndex(IInterpolatedSpace discountFactorCurve, DateTime targetDate,
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