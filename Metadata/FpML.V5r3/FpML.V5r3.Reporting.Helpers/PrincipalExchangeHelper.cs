using System;

namespace FpML.V5r3.Reporting.Helpers
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