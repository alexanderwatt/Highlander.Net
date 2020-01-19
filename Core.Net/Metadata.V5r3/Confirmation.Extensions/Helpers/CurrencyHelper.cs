namespace nab.QDS.FpML.V47
{
    public class CurrencyHelper
    {
        public static Currency Parse(string currencyCode)
        {
            var currency = new Currency {Value = currencyCode};
            return currency;
        }

        public static Currency Copy(Currency baseCurrency)
        {
            Currency result = null;
            if (baseCurrency!=null)
            {
                result = new Currency {Value = baseCurrency.Value};
            }
            return result;
        }

    }
}
