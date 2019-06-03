#region Usings

using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Orion.ValuationEngine.Instruments;

#endregion

namespace Orion.ValuationEngine.Helpers
{
    public class CashflowReportHelper
    {
        public static object DoReport(PriceableCashflow expectedcashflow)
        {
            if (expectedcashflow != null)
            {
                Debug.Print("Coupon: coupon type: {0},payment date: {1}, npv : {2}, multiplier : {3}, payment amount: {4}, forecast amount: {5}",
                    expectedcashflow.CashflowType, expectedcashflow.PaymentDate,
                            expectedcashflow.NPV, expectedcashflow.Multiplier, expectedcashflow.PaymentAmount, expectedcashflow.ForecastAmount);
            }
            return null;
        }

        public static object[] DoCashflowReport(string identifier, PriceableCashflow expectedcashflow)
        {
            if (expectedcashflow != null)
            {
                var result = new object[11];
                result[0] = identifier;
                result[1] = expectedcashflow.CashflowType.Value;
                result[2] = expectedcashflow.PaymentDate;
                result[3] = expectedcashflow.PaymentAmount.currency.Value;
                if (expectedcashflow.ForecastAmount != null)
                {
                    result[4] = expectedcashflow.ForecastAmount.amount;
                }
                else
                {
                    result[4] = expectedcashflow.PaymentAmount.amount;
                }
                result[5] = expectedcashflow.ModelIdentifier;
                result[6] = expectedcashflow.PaymentDiscountFactor;
                result[7] = expectedcashflow.NPV.amount;
                result[8] = expectedcashflow.YearFractionToCashFlowPayment;
                result[9] = expectedcashflow.IsCollateralised;
                result[10] = expectedcashflow.PayerIsBaseParty;
                return result;
            }
            return null;
        }

        public static List<object[]> DoCashflowReport(string identifier, PriceableInterestRateStream interestRateStream)
        {
            if (interestRateStream != null)
            {
                var result = new List<object[]>();
                if (interestRateStream.Coupons != null)
                {
                    result.AddRange(interestRateStream.Coupons.Select(cashflow => DoCashflowReport(identifier, cashflow)));
                }
                if (interestRateStream.Exchanges != null)
                {
                    result.AddRange(interestRateStream.Exchanges.Select(principal => DoCashflowReport(identifier, principal)));
                }
                return result;
            }
            return null;
        }
    }
}