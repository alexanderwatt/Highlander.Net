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

#region Usings

using System;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Helpers;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;
using Orion.Analytics.DayCounters;

#endregion

namespace Orion.ValuationEngine.Helpers
{
    public class ProductFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fraInputRange"></param>
        /// <returns></returns>
        public static Fra GetFpMLFra(FraInputRange fraInputRange)
        {
            var fra = new Fra
                          {
                              adjustedEffectiveDate =
                                  DateTypesHelper.ToRequiredIdentifierDate(fraInputRange.AdjustedEffectiveDate),
                              adjustedTerminationDate = fraInputRange.AdjustedTerminationDate,
                              paymentDate =
                                  DateTypesHelper.ToAdjustableDate(fraInputRange.UnadjustedPaymentDate,
                                                                   fraInputRange.PaymentDateBusinessDayConvention,
                                                                   fraInputRange.PaymentDateBusinessCenters)
                          };
            if ("resetDate" != fraInputRange.FixingDayOffsetDateRelativeTo)
            {
                throw new ArgumentException("The fixing date must be specified as 'resetDate'-relative!", nameof(fraInputRange));
            }
            var fixingDayType = EnumHelper.Parse<DayTypeEnum>(fraInputRange.FixingDayOffsetDayType);
            fra.fixingDateOffset = RelativeDateOffsetHelper.Create(fraInputRange.FixingDayOffsetPeriod, fixingDayType,
                                                                   fraInputRange.FixingDayOffsetBusinessDayConvention,
                                                                   fraInputRange.FixingDayOffsetBusinessCenters,
                                                                   fraInputRange.FixingDayOffsetDateRelativeTo);
            fra.dayCountFraction = DayCountFractionHelper.Parse(fraInputRange.DayCountFraction);
            IDayCounter dayCounter = DayCounterHelper.Parse(fra.dayCountFraction.Value);
            fra.calculationPeriodNumberOfDays = dayCounter.DayCount(fra.adjustedEffectiveDate.Value, fra.adjustedTerminationDate).ToString();
            fra.notional = MoneyHelper.GetAmount(fraInputRange.NotionalAmount, fraInputRange.NotionalCurrency);
            fra.fixedRate = (decimal)fraInputRange.FixedRate;
            fra.floatingRateIndex = FloatingRateIndexHelper.Parse(fraInputRange.FloatingRateIndex);
            fra.indexTenor = new[] { PeriodHelper.Parse(fraInputRange.IndexTenor) };
            fra.fraDiscounting = fraInputRange.FraDiscounting;
            PartyReference nabParty = PartyReferenceFactory.Create("NAB");
            PartyReference counterParty = PartyReferenceFactory.Create("COUNTERPARTY");
            if (bool.Parse(fraInputRange.Sell))
            {
                fra.sellerPartyReference = nabParty;
                fra.buyerPartyReference = counterParty;
            }
            else
            {
                fra.sellerPartyReference = counterParty;
                fra.buyerPartyReference = nabParty;
            }
            return fra;
        }

    }
}