using System;

namespace nab.QDS.FpML.V47
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
                spotRate = spotRate
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
                rate = spotRate
            };

            return exchangeRate;
        }
    }
}
