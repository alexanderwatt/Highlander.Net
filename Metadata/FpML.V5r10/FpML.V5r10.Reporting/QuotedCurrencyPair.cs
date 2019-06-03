
namespace FpML.V5r10.Reporting
{
    public partial class QuotedCurrencyPair
    {
        public static QuotedCurrencyPair Create(string currency1, string currency2, QuoteBasisEnum quoteBasis)
        {
            var quotedCurrencyPair = new QuotedCurrencyPair
            {
                currency1 = Parse(currency1),
                currency2 = Parse(currency2),
                quoteBasis = quoteBasis
            };

            return quotedCurrencyPair;
        }

        private static Currency Parse(string currencyCode)
        {
            var currency = new Currency { Value = currencyCode };
            return currency;
        }
    }
}
