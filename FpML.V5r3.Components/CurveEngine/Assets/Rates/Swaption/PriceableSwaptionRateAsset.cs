/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections.Generic;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;
using Orion.Models.Assets;
using Orion.Models.Rates.Options;

#endregion

namespace Orion.CurveEngine.Assets
{
    ///<summary>
    ///</summary>
    public abstract class PriceableSwaptionRateAsset : PriceableSimpleRateOptionAsset
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public new IModelAnalytic<ISwaptionAssetParameters, RateOptionMetrics> AnalyticsModel { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCaplet"/> class.
        /// This is a special case for use with the factry for bootstrapping, as it
        /// uses no calendar logic. This is done by the factory.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">A special class containing all salient data required.</param>
        /// <param name="fixingCalendar">The fixing/expiry calendar></param>
        /// <param name="paymentCalendar">The paymentCalendar calendar.</param>
        /// <param name="notional">The notional. The default value is 1,000,000.00m.</param>
        /// <param name="marketQuotes">The market quotes, including the volatility and possibly the fixed rate as a decimal contained in a basic quotation.</param>
        protected PriceableSwaptionRateAsset(DateTime baseDate, RateOptionNodeStruct nodeStruct, IBusinessCalendar fixingCalendar, 
            IBusinessCalendar paymentCalendar, Decimal notional, BasicAssetValuation marketQuotes) :
            base(baseDate, nodeStruct.ResetDateAdjustment, nodeStruct.SimpleRateOption, notional, 1000000.0m, nodeStruct.BusinessDayAdjustments, marketQuotes)
        {
            ModelIdentifier = "SimpleSwaptionAsset";
            Id = nodeStruct.SimpleRateOption.id;
            IsCap = true;
            IsDiscounted = false;
            RateOption = nodeStruct.SimpleRateOption;
            ResetDateOffset = nodeStruct.ResetDateAdjustment;
            SpotDateOffset = nodeStruct.SpotDate;
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
        }

        #endregion

        #region Calculation

        #endregion

        #region Interface IAssetController

        #endregion

        #region Helper Functions


        #endregion     
    }
}