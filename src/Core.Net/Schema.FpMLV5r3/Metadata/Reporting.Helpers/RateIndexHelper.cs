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



#endregion

using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.Helpers.V5r3
{
    public class RateIndexHelper
    {
        public static RateIndex Parse(string instrumentId,
            string floatingRateIndex,
            string currency,
            string dayCountFraction,
            string paymentFrequency, 
            string term)
        {
            var rateIndex = new RateIndex
                                      {
                                          currency = new IdentifiedCurrency { Value = currency },
                                          dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                                          floatingRateIndex = FloatingRateIndexHelper.Parse(floatingRateIndex),
                                          id = instrumentId,
                                          instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                                          paymentFrequency = PeriodHelper.Parse(paymentFrequency),
                                          term = PeriodHelper.Parse(term)
                                      };
            return rateIndex;
        }

        public static RateIndex Parse(string id, 
            string instrumentId,
            string floatingRateIndex,
            string currency,
            string dayCountFraction,
            string paymentFrequency,
            string term)
        {
            var rateIndex = new RateIndex
                                      {
                                          currency = new IdentifiedCurrency { Value = currency },
                                          dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                                          floatingRateIndex = FloatingRateIndexHelper.Parse(floatingRateIndex),
                                          id = id,
                                          instrumentId = InstrumentIdArrayHelper.Parse(instrumentId)
                                      };
            Period frequency = null;
            if (paymentFrequency != null)
            {
                frequency = PeriodHelper.Parse(paymentFrequency);
            }
            rateIndex.paymentFrequency = frequency;
            Period period = null;
            if (term != null)
            {
                period = PeriodHelper.Parse(term);
            }
            rateIndex.term = period;
            return rateIndex;
        }

        public static RateIndex Parse(string floatingRateIndex,
            string currency,
            string dayCountFraction)
        {
            var rateIndex = new RateIndex
            {
                currency = new IdentifiedCurrency { Value = currency },
                dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                floatingRateIndex = FloatingRateIndexHelper.Parse(floatingRateIndex),
            };
            return rateIndex;
        }

        public static RateIndex Parse(string floatingRateIndex,
            string currency,
            string dayCountFraction, string term)
        {
            var rateIndex = new RateIndex
            {
                currency = new IdentifiedCurrency { Value = currency },
                dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                floatingRateIndex = FloatingRateIndexHelper.Parse(floatingRateIndex),
            };
            Period period = null;
            if (term != null)
            {
                period = PeriodHelper.Parse(term);
            }
            rateIndex.term = period;
            return rateIndex;
        }

        public static RateIndex Create(string instrumentId, 
            FloatingRateIndex floatingRateIndex,
            Currency currency,
            DayCountFraction dayCountFraction,
            Period paymentFrequency, 
            Period term)
        {
            var rateIndex = new RateIndex
                {
                    currency = new IdentifiedCurrency {Value = currency.Value},
                    dayCountFraction = dayCountFraction,
                    floatingRateIndex = floatingRateIndex,
                    id = instrumentId,
                    instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                    paymentFrequency = paymentFrequency,
                    term = term
                };
            return rateIndex;
        }
    }
}
