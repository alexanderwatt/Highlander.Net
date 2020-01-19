#region Using directives

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
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
                                          currency = new IdentifiedCurrency() { Value = currency },
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
                                          currency = new IdentifiedCurrency() { Value = currency },
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
                currency = new IdentifiedCurrency() { Value = currency },
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
            RateIndex rateIndex = new RateIndex();
            rateIndex.currency = new IdentifiedCurrency() { Value = currency.Value };
            rateIndex.dayCountFraction = dayCountFraction;
            rateIndex.floatingRateIndex = floatingRateIndex;
            rateIndex.id = instrumentId; 
            rateIndex.instrumentId = InstrumentIdArrayHelper.Parse(instrumentId);
            rateIndex.paymentFrequency = paymentFrequency;
            rateIndex.term = term;

            return rateIndex;
        }
    }

}
