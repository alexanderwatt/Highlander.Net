#region Using directives

using System.Collections.Generic;

#endregion

namespace FpML.V5r3.Confirmation
{
    public  class CashflowsFactory
    {
        public static Cashflows Create(List<PaymentCalculationPeriod> paymentCalculationPeriods)
        {
            return Create(paymentCalculationPeriods, true);
        }

        public static Cashflows Create(List<PaymentCalculationPeriod> paymentCalculationPeriods, bool cashflowsMatchParameters)
        {
            var cashflows = new Cashflows
                                      {
                                          cashflowsMatchParameters = cashflowsMatchParameters,
                                          paymentCalculationPeriod = paymentCalculationPeriods.ToArray()
                                      };

            return cashflows;
        }

        public static Cashflows Create(List<PaymentCalculationPeriod> paymentCalculationPeriods, List<PrincipalExchange> principalExchanges)
        {
            return Create(paymentCalculationPeriods, principalExchanges, true);
        }

        public static Cashflows Create(List<PaymentCalculationPeriod> paymentCalculationPeriods, List<PrincipalExchange> principalExchanges, bool cashflowsMatchParameters)
        {
            var cashflows = new Cashflows
                                      {
                                          cashflowsMatchParameters = cashflowsMatchParameters,
                                          paymentCalculationPeriod = paymentCalculationPeriods.ToArray(),
                                          principalExchange = principalExchanges.ToArray()
                                      };

            return cashflows;
        }

    }
}
