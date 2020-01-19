#region Using directives

using System.Collections.Generic;
using Orion.ValuationEngine.Instruments;

#endregion

namespace Orion.ValuationEngine.Helpers
{
    internal static class Utilities
    {
        public static object[,] GetCashflowsOneUnderAnotherExcelRange(List<PriceableRateCoupon> payCashflows, List<PriceableRateCoupon> recCashflows)
        {
            var result = new object[payCashflows.Count + recCashflows.Count + 4, 9];
            int currentRow = 0;
            //  Pay
            //
            result[currentRow, 0] = "Pay cashflows:";
            result[currentRow, 1] = "";
            result[currentRow, 2] = "";
            result[currentRow, 3] = "";
            result[currentRow, 4] = "";
            result[currentRow, 5] = "";
            result[currentRow, 6] = "";
            result[currentRow, 7] = "";
            result[currentRow, 8] = "";
            ++currentRow;

            result[currentRow, 0] = "Accrual start";
            result[currentRow, 1] = "Accrual end";
            result[currentRow, 2] = "Payment";
            result[currentRow, 3] = "Year fraction";
            result[currentRow, 4] = "Interest FV";
            result[currentRow, 5] = "Interest PV";
            result[currentRow, 6] = "Notional";
            result[currentRow, 7] = "Forward rate";
            result[currentRow, 8] = "Discount factor";
            ++currentRow;

            foreach (var payCashflow in payCashflows)
            {
                result[currentRow, 0] = payCashflow.AccrualStartDate;
                result[currentRow, 1] = payCashflow.AccrualEndDate;
                result[currentRow, 2] = payCashflow.PaymentDate;
                result[currentRow, 3] = payCashflow.CouponYearFraction;
                result[currentRow, 4] = payCashflow.AccruedInterest;
                result[currentRow, 5] = payCashflow.AccruedInterestPV;
                result[currentRow, 6] = payCashflow.Notional;
                result[currentRow, 7] = payCashflow.Rate;
                result[currentRow, 8] = payCashflow.PaymentDiscountFactor;
                ++currentRow;
            }

            //  Rec  
            //
            result[currentRow, 0] = "Rec cashflows:";
            result[currentRow, 1] = "";
            result[currentRow, 2] = "";
            result[currentRow, 3] = "";
            result[currentRow, 4] = "";
            result[currentRow, 5] = "";
            result[currentRow, 6] = "";
            result[currentRow, 7] = "";
            result[currentRow, 8] = "";
            ++currentRow;

            result[currentRow, 0] = "Accrual start";
            result[currentRow, 1] = "Accrual end";
            result[currentRow, 2] = "Payment";
            result[currentRow, 3] = "Year fraction";
            result[currentRow, 4] = "Interest FV";
            result[currentRow, 5] = "Interest PV";
            result[currentRow, 6] = "Notional";
            result[currentRow, 7] = "Forward rate";
            result[currentRow, 8] = "Discount factor";
            ++currentRow;

            foreach (var recCashflow in recCashflows)
            {
                result[currentRow, 0] = recCashflow.AccrualStartDate;
                result[currentRow, 1] = recCashflow.AccrualEndDate;
                result[currentRow, 2] = recCashflow.PaymentDate;
                result[currentRow, 3] = recCashflow.CouponYearFraction;
                result[currentRow, 4] = recCashflow.AccruedInterest;
                result[currentRow, 5] = recCashflow.AccruedInterestPV;
                result[currentRow, 6] = recCashflow.Notional;
                result[currentRow, 7] = recCashflow.Rate;
                result[currentRow, 8] = recCashflow.PaymentDiscountFactor;
                ++currentRow;
            }
      
            return result;
        }

        public static object[,] GetCashflowsSideBySideExcelRange(List<PriceableRateCoupon> payFloatingCashflows,
                                                                 List<PriceableRateCoupon> recFixedCashflows)
        {
            var result2 = new List<CashflowCombined>();
            payFloatingCashflows.ForEach
                (
                payFloatingCashflow =>
                    {
                        var payFloatingCashflowCombined = new CashflowCombined
                                                              {
                                                                  AccrualStartDate = payFloatingCashflow.AccrualStartDate,
                                                                  AccrualEndDate = payFloatingCashflow.AccrualEndDate,
                                                                  PaymentDate = payFloatingCashflow.PaymentDate,
                                                                  YearFraction = payFloatingCashflow.CouponYearFraction,
                                                                  FloatIntFV = payFloatingCashflow.AccruedInterest,
                                                                  FloatIntPV = payFloatingCashflow.AccruedInterestPV,
                                                                  SwapIntFV = -payFloatingCashflow.AccruedInterest,
                                                                  SwapIntPV = -payFloatingCashflow.AccruedInterestPV,
                                                                  ForwardRate = (decimal)payFloatingCashflow.Rate,
                                                                  DiscountFactor = payFloatingCashflow.PaymentDiscountFactor,
                                                                  Notional = payFloatingCashflow.Notional
                                                              };

// ReSharper disable AccessToModifiedClosure
                        result2.Add(payFloatingCashflowCombined);
// ReSharper restore AccessToModifiedClosure
                    }
                );
            recFixedCashflows.ForEach
                (
                recFixedCashflow =>
                    {
                        var recFixedCashflowCombined = new CashflowCombined
                                                           {
                                                               AccrualStartDate = recFixedCashflow.AccrualStartDate,
                                                               AccrualEndDate = recFixedCashflow.AccrualEndDate,
                                                               PaymentDate = recFixedCashflow.PaymentDate,
                                                               YearFraction = recFixedCashflow.CouponYearFraction,

                                                               FixedIntFV = recFixedCashflow.AccruedInterest,
                                                               FixedIntPV = recFixedCashflow.AccruedInterestPV,
                                                               SwapIntFV = +recFixedCashflow.AccruedInterest,
                                                               SwapIntPV = +recFixedCashflow.AccruedInterestPV,

                                                               FixedRate = (decimal)recFixedCashflow.Rate,
                                                               DiscountFactor = recFixedCashflow.PaymentDiscountFactor,
                                                               Notional = recFixedCashflow.Notional
                                                           };

// ReSharper disable AccessToModifiedClosure
                        result2.Add(recFixedCashflowCombined);
// ReSharper restore AccessToModifiedClosure
                    }
                );

            result2 = CombineByPaymentDates(result2);
            //
            //
            var result2Array = new object[2 + result2.Count, 12];
            int currentRow2 = 0;
            result2Array[currentRow2, 0] = "Pay and received cashflows:";
            for (int i = 1; i < 12; ++i)
            {
                result2Array[currentRow2, i] = "";
            }           
            ++currentRow2;
            result2Array[currentRow2, 0] = "Accrual start";
            result2Array[currentRow2, 1] = "Accrual end";
            result2Array[currentRow2, 2] = "Payment";
            result2Array[currentRow2, 3] = "Year fraction";

            result2Array[currentRow2, 4] = "Fixed Int FV";
            result2Array[currentRow2, 5] = "Float Int FV";
            result2Array[currentRow2, 6] = "Swap Int FV";
            result2Array[currentRow2, 7] = "Swap Int PV";

            result2Array[currentRow2, 8] = "Fixed rate";
            result2Array[currentRow2, 9] = "Forward rate";

            result2Array[currentRow2, 10] = "Discount factor";

            result2Array[currentRow2, 11] = "Notional";

            ++currentRow2;

            foreach(var cashflowCombined in result2)
            {
                result2Array[currentRow2, 0] = cashflowCombined.AccrualStartDate;
                result2Array[currentRow2, 1] = cashflowCombined.AccrualEndDate;
                result2Array[currentRow2, 2] = cashflowCombined.PaymentDate;
                result2Array[currentRow2, 3] = cashflowCombined.YearFraction;

                result2Array[currentRow2, 4] = cashflowCombined.FixedIntFV;
                result2Array[currentRow2, 5] = cashflowCombined.FloatIntFV;
                result2Array[currentRow2, 6] = cashflowCombined.SwapIntFV;
                result2Array[currentRow2, 7] = cashflowCombined.SwapIntPV;

                result2Array[currentRow2, 8] = cashflowCombined.FixedRate;
                result2Array[currentRow2, 9] = cashflowCombined.ForwardRate;

                result2Array[currentRow2, 10] = cashflowCombined.DiscountFactor;

                result2Array[currentRow2, 11] = cashflowCombined.Notional;

                ++currentRow2;
            }


            return result2Array;
        }

        private static List<CashflowCombined>   CombineByPaymentDates(IEnumerable<CashflowCombined> input)
        {
            var result = new List<CashflowCombined>(input);
            var comparer = new CashflowCombinedComparer();
            result.Sort(comparer);
            for (int i = 0; i < result.Count - 1; ++i)
            {
                CashflowCombined cf = result[i];
                CashflowCombined cfNext = result[i + 1];
                if (0 == comparer.Compare(cf, cfNext))
                {
                    // do combine
                    //
                    cf.SwapIntFV += cfNext.SwapIntFV;
                    cf.SwapIntPV += cfNext.SwapIntPV;
                    if (0 == cf.FloatIntFV)
                    {
                        cf.FloatIntFV = cfNext.FloatIntFV;
                    }
                    if (0 == cf.FloatIntPV)
                    {
                        cf.FloatIntPV = cfNext.FloatIntPV;
                    }                  
                    if (0 == cf.FixedIntFV)
                    {
                        cf.FixedIntFV = cfNext.FixedIntFV;
                    }
                    if (0 == cf.FixedIntPV)
                    {
                        cf.FixedIntPV = cfNext.FixedIntPV;
                    }
                    if (0 == cf.FixedRate)
                    {
                        cf.FixedRate = cfNext.FixedRate;
                    }
                    if (0 == cf.ForwardRate)
                    {
                        cf.ForwardRate = cfNext.ForwardRate;
                    }                    
                    // remove cf next
                    //
                    result.RemoveAt(i + 1);

                    return CombineByPaymentDates(result);
                }
            }
            
            return result;
        }

    }
}