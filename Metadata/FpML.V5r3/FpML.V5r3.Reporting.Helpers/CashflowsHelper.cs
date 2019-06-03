#region Using directives

using System.Linq;

#endregion

namespace FpML.V5r3.Reporting.Helpers
{
    public  class CashflowsHelper
    {
        public static Money GetForecastValue(Cashflows cashflows)
        {
            var forecastValueList = cashflows.paymentCalculationPeriod.Select(period => period.forecastPaymentAmount).ToList();
            return MoneyHelper.Sum(forecastValueList);
        }

        public static Money GetPresentValue(Cashflows cashflows)
        {
            var presentValueList = cashflows.paymentCalculationPeriod.Select(period => period.presentValueAmount).ToList();
            return MoneyHelper.Sum(presentValueList);
        }

    }
}