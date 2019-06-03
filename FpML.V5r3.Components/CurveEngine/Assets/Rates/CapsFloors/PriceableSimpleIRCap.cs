#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;
using Orion.CalendarEngine.Schedulers;
using Orion.ModelFramework;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;

#endregion

namespace Orion.CurveEngine.Assets.Rates.CapsFloors
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
                    Analytics.Schedulers.DateScheduler.GetUnadjustedDateSchedule(spotDate, SimpleIRCap.term,
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