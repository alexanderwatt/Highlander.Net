#region Using directives

using System.Collections.Generic;

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public  class CashflowsHelper
    {
        public static Money GetForecastValue(Cashflows cashflows)
        {
            List<Money> forecastValueList = new List<Money>();

            foreach (PaymentCalculationPeriod period in cashflows.paymentCalculationPeriod)
            {
                forecastValueList.Add(period.forecastPaymentAmount);                
            }

            return MoneyHelper.Sum(forecastValueList);
        }

        public static Money GetPresentValue(Cashflows cashflows)
        {
            List<Money> presentValueList = new List<Money>();

            foreach (PaymentCalculationPeriod period in cashflows.paymentCalculationPeriod)
            {
                presentValueList.Add(period.presentValueAmount);                
            }

            return MoneyHelper.Sum(presentValueList);
        }

    }
}