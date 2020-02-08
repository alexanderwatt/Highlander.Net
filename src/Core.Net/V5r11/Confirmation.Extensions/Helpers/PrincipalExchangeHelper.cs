using System;

namespace nab.QDS.FpML.V47
{
    public static class PrincipalExchangeHelper
    {
        public static PrincipalExchange Create(DateTime adjustedPrincipalExchangeDate, decimal amount)
        {
            PrincipalExchange principalExchange = Create(adjustedPrincipalExchangeDate);

            principalExchange.principalExchangeAmount = amount;
            principalExchange.principalExchangeAmountSpecified = true;

            return principalExchange;
        }

        public static PrincipalExchange Create(DateTime adjustedPrincipalExchangeDate)
        {
            var principalExchange = new PrincipalExchange
                                        {
                                            adjustedPrincipalExchangeDate = adjustedPrincipalExchangeDate,
                                            adjustedPrincipalExchangeDateSpecified = true
                                        };

            return principalExchange;
        }
    }
}