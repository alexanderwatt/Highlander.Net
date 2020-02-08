
namespace nab.QDS.FpML.V47
{
    public partial class QuotedCurrencyPair
    {
        public static QuotedCurrencyPair Create(string currency1, string currency2, QuoteBasisEnum quoteBasis)
        {
            var quotedCurrencyPair = new QuotedCurrencyPair
            {
                currency1 = CurrencyHelper.Parse(currency1),
                currency2 = CurrencyHelper.Parse(currency2),
                quoteBasis = quoteBasis
            };

            return quotedCurrencyPair;
        }
    }
}
