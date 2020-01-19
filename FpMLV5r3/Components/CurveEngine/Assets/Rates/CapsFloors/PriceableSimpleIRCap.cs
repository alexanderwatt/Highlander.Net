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
using Highlander.CalendarEngine.V5r3.Schedulers;
using Highlander.Reporting.Analytics.V5r3.Schedulers;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.Serialisation;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates.CapsFloors
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableSimpleIRCap : PriceableCapRateAsset
    {
        /// <summary>
        /// Gets the discounting type.
        /// </summary>
        /// <value>The discounting type.</value>
        public DiscountingTypeEnum? DiscountingType => Calculation.discounting?.discountingType;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRCap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="interestRateCap">An interest Rate Cap.</param>
        /// <param name="properties">THe properies, including strike information.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="marketQuotes">The market Quote: premium, normal flat volatility or lognormal flat volatility.</param>
        public PriceableSimpleIRCap(DateTime baseDate, SimpleIRCapNodeStruct interestRateCap, NamedValueSet properties, 
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicAssetValuation marketQuotes)
            : base(baseDate, interestRateCap.DateAdjustments, ReversePeriodMultiplier(interestRateCap.SpotDate), interestRateCap.Calculation, marketQuotes)
        {
            Id = interestRateCap.SimpleIRCap.id;
            IsCap = true;
            SimpleIRCap = interestRateCap.SimpleIRCap;
            var spotDate = GetSpotDate(baseDate, fixingCalendar, interestRateCap.SpotDate);
            if (SimpleIRCap != null)
            {
                var unadjustedDates =
                    DateScheduler.GetUnadjustedDateSchedule(spotDate, SimpleIRCap.term,
                        SimpleIRCap.paymentFrequency);
                AdjustedPeriodDates = AdjustedDateScheduler.GetAdjustedDateSchedule(unadjustedDates,
                    PaymentBusinessDayAdjustments.businessDayConvention, paymentCalendar);
                //Adjust for the spot period backwards!
                ExpiryDates = AdjustedDateScheduler.GetAdjustedDateSchedule(AdjustedPeriodDates,
                    FixingBusinessDayOffset, fixingCalendar);
                //Remove the last date, which is not an expiry date.
                ExpiryDates.RemoveAt(ExpiryDates.Count-1);
                TimesToExpiry = GetTimesToExpiry(ExpiryDates, baseDate);              
                //Set the strike scalar. This must be after the number of expiry dates has been set.
                var quotes = new List<BasicQuotation>(marketQuotes.quote);
                //For the default cap the spot rate is used.
                SetQuote("SpotRate", quotes);
                //If the strike is not provided it must be calculated!!
                SetQuote("Strike", quotes);
                //Set the spot date.
                AdjustedStartDate = AdjustedPeriodDates[0];
                RiskMaturityDate = ExpiryDates[ExpiryDates.Count - 1];
                OptionsExpiryDate = ExpiryDates[ExpiryDates.Count - 1];
                if (Strike != null && Strikes == null) Strikes = CreateList((decimal) Strike, TimesToExpiry.Count);
                if(Notionals == null) Notionals = CreateList(InitialNotional, TimesToExpiry.Count);
            }
            if (ExpiryDates[0] <= BaseDate)
            {
                IncludeFirstPeriod = false;
            }
            if (DiscountingType != null)
            {
                ModelIdentifier = "DiscountCapAsset";
            }
        }

        private static RelativeDateOffset ReversePeriodMultiplier(RelativeDateOffset spotDate)
        {
            var reversedPeriodDays = XmlSerializerHelper.Clone(spotDate);
            Int32 spotDays = Int32.Parse(spotDate.periodMultiplier) * -1;
            reversedPeriodDays.periodMultiplier = spotDays.ToString();
            return reversedPeriodDays;
        }
    }
}