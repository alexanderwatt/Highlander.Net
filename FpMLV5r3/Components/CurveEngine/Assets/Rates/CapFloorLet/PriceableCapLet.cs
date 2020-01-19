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
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.Models.V5r3.Rates.Options;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.NamedValues;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates.CapFloorLet
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableCaplet : PriceableSimpleRateOptionAsset
    {
        ///<summary>
        ///</summary>
        public RateIndex UnderlyingRateIndex { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCaplet"/> class.
        /// This is a special case for use with the factry for bootstrapping, as it
        /// uses no calendar logic. This is done by the factory.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">A special class containing all salient data required.</param>
        /// <param name="properties">The properties set This includes strike information.</param>
        /// <param name="fixingCalendar">The fixing/expiry calendar></param>
        /// <param name="paymentCalendar">The paymentCalendar calendar.</param>
        /// <param name="notional">The notional. The default value is 1,000,000.00m.</param>
        /// <param name="marketQuotes">The market quotes, including the volatility and possibly the fixed rate as a decimal contained in a basic quotation.</param>
        public PriceableCaplet(DateTime baseDate, RateOptionNodeStruct nodeStruct, NamedValueSet properties, IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, Decimal notional, BasicAssetValuation marketQuotes)
            : base(baseDate, nodeStruct.ResetDateAdjustment, nodeStruct.SimpleRateOption, notional, 1000000.0m, nodeStruct.BusinessDayAdjustments, marketQuotes)
        {
            Id = nodeStruct.SimpleRateOption.id;
            IsCap = true;
            IsDiscounted = false;
            RateOption = nodeStruct.SimpleRateOption;
            ResetDateOffset = nodeStruct.ResetDateAdjustment;
            SpotDateOffset = nodeStruct.SpotDate;
            UnderlyingRateIndex = nodeStruct.RateIndex;
            AdjustedStartDate = GetSpotDate(baseDate, fixingCalendar, nodeStruct.SpotDate);
            AdjustedEffectiveDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, nodeStruct.SimpleRateOption.startTerm, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            OptionsExpiryDate = GetFixingDate(AdjustedEffectiveDate, fixingCalendar, nodeStruct.ResetDateAdjustment);
            MaturityDate = GetEffectiveDate(AdjustedStartDate, paymentCalendar, nodeStruct.SimpleRateOption.endTerm, nodeStruct.BusinessDayAdjustments.businessDayConvention);
            PaymentDate = MaturityDate;
            YearFraction = GetYearFraction(RateOption.dayCountFraction.Value, AdjustedEffectiveDate, MaturityDate);
            TimeToExpiry = GetTimeToExpiry(baseDate, OptionsExpiryDate);
            //Set the strike scalar. This must be after the number of expiry dates has been set.
            var quotes = new List<BasicQuotation>(marketQuotes.quote);
            //For the default cap the spot rate is used.
            SetQuote("ForwardRate", quotes);
            SetQuote("Strike", quotes);
            //decimal? strike = properties.GetValue<decimal>("Strike", false);
            //Strike = (decimal)strike;
        }

        /// <summary>
        /// Calculates the specified metric for the fast bootstrapper.
        /// </summary>
        /// <param name="interpolatedSpace">The intepolated Space.</param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            ISimpleOptionAssetParameters analyticModelParameters = new SimpleRateOptionAssetParameters();
            //{
            //    YearFraction = YearFraction,
            //    TimeToExpiry = TimeToExpiry,
            //    StartDiscountFactor =
            //        GetDiscountFactor(interpolatedSpace,
            //            AdjustedStartDate, BaseDate),
            //    EndDiscountFactor =
            //        GetDiscountFactor(interpolatedSpace,
            //            GetRiskMaturityDate(), BaseDate),
            //    Volatility = Volatility
            //};
            //3. Get the Rate
            //
            //if (Volatility != null)
            //{
            //    analyticModelParameters.Rate = MarketQuoteHelper.NormalisePriceUnits(FixedRate, "DecimalRate").value;
            //}
            AnalyticResults = new SimpleRateOptionAssetResults();
            //4. Set the anaytic input parameters and Calculate the respective metrics
            //
            AnalyticResults = AnalyticsModel.Calculate<ISimpleRateOptionAssetResults, SimpleRateOptionAssetResults>(analyticModelParameters, new[] { RateOptionMetrics.ImpliedQuote });
            return AnalyticResults.ImpliedQuote;
        }

        /// <summary>
        /// Gets the adjusted termination date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()//TODO This should be switched to the actual rate maturity once the bootstrapper has been fixed.
        {
            return MaturityDate;
        }
    }
}
