using System;

namespace FpML.V5r10.Reporting
{
    public partial class ExchangeRate
    {
        public static ExchangeRate Create(string currency1, string currency2, QuoteBasisEnum quoteBasis,
                                         Decimal spotRate, Decimal forwardRate, Decimal? forwardPoints)
        {
            var exchangeRate = new ExchangeRate
            {
                quotedCurrencyPair = QuotedCurrencyPair.Create(currency1, currency2, quoteBasis),
                rate = forwardRate,
                rateSpecified = true,
                spotRate = spotRate,
                spotRateSpecified = true
            };
            if(forwardPoints != null)
            {
                exchangeRate.forwardPointsField = (decimal)forwardPoints;
            }
            return exchangeRate;
        }

        public static ExchangeRate Create(string currency1, string currency2, QuoteBasisEnum quoteBasis,
                                         Decimal spotRate)
        {
            var exchangeRate = new ExchangeRate
            {
                quotedCurrencyPair = QuotedCurrencyPair.Create(currency1, currency2, quoteBasis),
                rate = spotRate,
                rateSpecified = true
            };

            return exchangeRate;
        }
    }
}
