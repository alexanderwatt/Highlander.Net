#region Using directives



#endregion

namespace FpML.V5r10.Reporting.Helpers
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
                                          instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                                          paymentFrequency = PeriodHelper.Parse(paymentFrequency),
                                          term = PeriodHelper.Parse(term)
                                      };

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
