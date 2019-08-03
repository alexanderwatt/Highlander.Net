/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using Directives

using System;
using System.Runtime.InteropServices;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using HLV5r3.Helpers;
using Microsoft.Win32;
using Orion.Analytics.DayCounters;
using Orion.Analytics.Rates;
using Orion.ModelFramework;
using Excel = Microsoft.Office.Interop.Excel;

#endregion

namespace HLV5r3.Analytics
{
    /// <summary>
    /// Swap Rate analytics functions.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("D851C762-AD8F-49A3-9B24-24D315B56AD2")]
    public class Swaps
    {
        #region Registration

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComRegisterFunction]
        public static void RegisterFunction(Type type)
        {
            Registry.ClassesRoot.CreateSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"));
            var key = Registry.ClassesRoot.OpenSubKey(ApplicationHelper.GetSubKeyName(type, "InprocServer32"), true);
            key?.SetValue("", System.Environment.SystemDirectory + @"\mscoree.dll", RegistryValueKind.String);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComUnregisterFunction]
        public static void UnregisterFunction(Type type)
        {
            Registry.ClassesRoot.DeleteSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"), false);
        }

        #endregion

        #region Functions

        // ---------------------------------------------------------------------
        // Business logic methods.
        // ---------------------------------------------------------------------
        /// <summary>
        /// Computes the BGS convexity correction. The magnitude of the
        /// correction, as described in the document "Pricing Analytics for a
        /// Balance Guaranteed Swap", prepared by George Scoufis,
        /// Reference Number GS-02042007/1 is:
        ///        F0*{1 - Phi(d1) - (L/B0)^(1 + 2*alpha/sigma^2)*phi(d2)}.
        /// Periods in the formula for F0, d1 and d2 are computed with
        /// ACT/365 day count.</summary>
        /// <param name="lastAmortisationDate">The date of the most recent
        /// realised amortisation.</param>
        /// <param name="amortisationDate">The date to which the convexity
        /// correction is required.</param>
        /// <param name="currentBondFactor">The current Bond Factor.</param>
        /// <param name="alpha">The parameter alpha in the Bond Factor curve
        /// model.</param>
        /// <param name="sigma">The parameter sigma in the Bond Factor curve
        /// model.</param>
        /// <param name="cleanUp">The clean up condition. If the current
        /// Bond Factor is less than or equal to the clean up, then the
        /// convexity correction is 0.0.</param>
        /// <returns>Convexity correction. Logic has been added to cater for
        /// the limiting cases: 1) sigma->0; 2) clean up ->0.</returns>
        public double ComputeBGSConvexityCorrection(DateTime lastAmortisationDate,
                                                    DateTime amortisationDate,
                                                    double currentBondFactor,
                                                    double alpha,
                                                    double sigma,
                                                    double cleanUp)
        {
            IDayCounter dayCountObj = Actual365.Instance;
            double tenor = dayCountObj.YearFraction(lastAmortisationDate,
                                                    amortisationDate);
            var bgs = new BGSConvexityCorrection();
            return bgs.ComputeBGSConvexityCorrection(tenor,
                                                     currentBondFactor,
                                                     alpha,
                                                     sigma,
                                                     cleanUp);
        }

        /// <summary>
        /// Gets the npv for a collection of coupon cash flows provided.
        /// </summary>
        /// <param name="paymentDiscountFactors">The discount factors.</param>
        /// <param name="yearFractions">The year fractions.</param>
        /// <param name="forwardRates">The forward rates.</param>
        /// <param name="notionals">The notionals</param>
        /// <returns>The break even rate.</returns>
        public double NPV(Excel.Range notionals, Excel.Range forwardRates, Excel.Range paymentDiscountFactors, Excel.Range yearFractions)
        {
            var unqNotionals = DataRangeHelper.StripDoubleRange(notionals);
            var unqForwardRates = DataRangeHelper.StripDoubleRange(forwardRates);
            var unqPaymentDiscountFactors = DataRangeHelper.StripDoubleRange(paymentDiscountFactors);
            var unqYearFractions = DataRangeHelper.StripDoubleRange(yearFractions);
            return SwapAnalytics.NPV(unqNotionals.ToArray(), unqForwardRates.ToArray(), unqPaymentDiscountFactors.ToArray(), unqYearFractions.ToArray());
        }

        /// <summary>
        /// Gets the npv for a collection of coupons and principal exchanges provided.
        /// </summary>
        /// <param name="couponPaymentDiscountFactors">The discount factors.</param>
        /// <param name="yearFractions">The year fractions.</param>
        /// <param name="forwardRates">The forward rates.</param>
        /// <param name="couponNotionals">The notionals of the coupons.</param>
        /// <param name="principalNotionals">The principal exchange notionals.</param>
        /// <param name="principalPaymentDiscountFactors">The payment discount factors for the principal exchanges.</param>
        /// <returns>The break even rate.</returns>
        public double NPVWithExchanges(Excel.Range couponNotionals, Excel.Range forwardRates, Excel.Range couponPaymentDiscountFactors,
                                               Excel.Range yearFractions, Excel.Range principalNotionals, Excel.Range principalPaymentDiscountFactors)
        {
            var unqCouponNotionals = DataRangeHelper.StripDoubleRange(couponNotionals);
            var unqForwardRates = DataRangeHelper.StripDoubleRange(forwardRates);
            var unqCouponPaymentDiscountFactors = DataRangeHelper.StripDoubleRange(couponPaymentDiscountFactors);
            var unqYearFractions = DataRangeHelper.StripDoubleRange(yearFractions);
            var unqPrincipalNotionals = DataRangeHelper.StripDoubleRange(principalNotionals);
            var unqPrincipalPaymentDiscountFactors = DataRangeHelper.StripDoubleRange(principalPaymentDiscountFactors);
            return SwapAnalytics.NPVWithExchanges(unqCouponNotionals.ToArray(), unqForwardRates.ToArray(), unqCouponPaymentDiscountFactors.ToArray(), unqYearFractions.ToArray(),
                unqPrincipalNotionals.ToArray(), unqPrincipalPaymentDiscountFactors.ToArray());
        }

        /// <summary>
        /// Gets the break even rate for a collection of coupon cashflows provided.
        /// </summary>
        /// <param name="paymentDiscountFactors">The discount factors.</param>
        /// <param name="yearFractions">The year fractions.</param>
        /// <param name="forwardRates">The forward rates.</param>
        /// <param name="notionals">The notionals</param>
        /// <returns>The break even rate.</returns>
        public double BreakEvenRate(Excel.Range notionals, Excel.Range forwardRates, Excel.Range paymentDiscountFactors, Excel.Range yearFractions)
        {
            var unqNotionals = DataRangeHelper.StripDoubleRange(notionals);
            var unqForwardRates = DataRangeHelper.StripDoubleRange(forwardRates);
            var unqPaymentDiscountFactors = DataRangeHelper.StripDoubleRange(paymentDiscountFactors);
            var unqYearFractions = DataRangeHelper.StripDoubleRange(yearFractions);
            return SwapAnalytics.BreakEvenRate(unqNotionals.ToArray(), unqForwardRates.ToArray(), unqPaymentDiscountFactors.ToArray(), unqYearFractions.ToArray());
        }

        /// <summary>
        /// Gets the break even rate for a collection of coupons and principal exchanges provided.
        /// </summary>
        /// <param name="couponPaymentDiscountFactors">The discount factors.</param>
        /// <param name="yearFractions">The year fractions.</param>
        /// <param name="forwardRates">The forward rates.</param>
        /// <param name="couponNotionals">The notionals of the coupons.</param>
        /// <param name="principalNotionals">The principal exchange notionals.</param>
        /// <param name="principalPaymentDiscountFactors">The payment discount factors for the principal exchanges.</param>
        /// <returns>The break even rate.</returns>
        public double BreakEvenRateWithExchanges(Excel.Range couponNotionals, Excel.Range forwardRates,
                                                         Excel.Range couponPaymentDiscountFactors, Excel.Range yearFractions, Excel.Range principalNotionals,
                                                         Excel.Range principalPaymentDiscountFactors)
        {
            var unqCouponNotionals = DataRangeHelper.StripDoubleRange(couponNotionals);
            var unqForwardRates = DataRangeHelper.StripDoubleRange(forwardRates);
            var unqCouponPaymentDiscountFactors = DataRangeHelper.StripDoubleRange(couponPaymentDiscountFactors);
            var unqYearFractions = DataRangeHelper.StripDoubleRange(yearFractions);
            var unqPrincipalNotionals = DataRangeHelper.StripDoubleRange(principalNotionals);
            var unqPrincipalPaymentDiscountFactors = DataRangeHelper.StripDoubleRange(principalPaymentDiscountFactors);
            return SwapAnalytics.BreakEvenRateWithExchanges(unqCouponNotionals.ToArray(), unqForwardRates.ToArray(), unqCouponPaymentDiscountFactors.ToArray(), unqYearFractions.ToArray(),
                unqPrincipalNotionals.ToArray(), unqPrincipalPaymentDiscountFactors.ToArray());
        }


        /// <summary>
        /// Gets the delta0 for a collection of coupon cashflows provided.
        /// </summary>
        /// <param name="paymentDiscountFactors">The discount factors.</param>
        /// <param name="yearFractions">The year fractions.</param>
        /// <param name="notionals">The notionals</param>
        /// <param name="fixedFlag">Delta0 is zero for fixed coupons.</param>
        /// <returns>The break even rate.</returns>
        public double Delta0(Excel.Range notionals, Excel.Range paymentDiscountFactors, Excel.Range yearFractions, bool fixedFlag)
        {
            var unqNotionals = DataRangeHelper.StripDoubleRange(notionals);
            var unqPaymentDiscountFactors = DataRangeHelper.StripDoubleRange(paymentDiscountFactors);
            var unqYearFractions = DataRangeHelper.StripDoubleRange(yearFractions);
            return SwapAnalytics.Delta0(unqNotionals.ToArray(), unqPaymentDiscountFactors.ToArray(), unqYearFractions.ToArray(), fixedFlag);
        }

        /// <summary>
        /// Gets the delta0 for a collection of coupon cashflows provided.
        /// </summary>
        /// <param name="paymentDiscountFactors">The discount factors.</param>
        /// <param name="yearFractions">The year fractions.</param>
        /// <param name="notionals">The notionals</param>
        /// <param name="principalPaymentDiscountFactors">The dfs for the principal exchanges.</param>
        /// <param name="fixedFlag">Delta0 is zero for fixed coupons.</param>
        /// <param name="principalNotionals">The principal Exchanges.</param>
        /// <returns>The break even rate.</returns>
        public double Delta0WithExchanges(Excel.Range notionals, Excel.Range paymentDiscountFactors, Excel.Range yearFractions,
                                                  Excel.Range principalNotionals, Excel.Range principalPaymentDiscountFactors, bool fixedFlag)
        {
            var unqNotionals = DataRangeHelper.StripDoubleRange(notionals);
            var unqCouponPaymentDiscountFactors = DataRangeHelper.StripDoubleRange(paymentDiscountFactors);
            var unqYearFractions = DataRangeHelper.StripDoubleRange(yearFractions);
            var unqPrincipalNotionals = DataRangeHelper.StripDoubleRange(principalNotionals);
            var unqPrincipalPaymentDiscountFactors = DataRangeHelper.StripDoubleRange(principalPaymentDiscountFactors);
            return SwapAnalytics.Delta0WithExchanges(unqNotionals.ToArray(), unqCouponPaymentDiscountFactors.ToArray(), unqYearFractions.ToArray(),
                unqPrincipalNotionals.ToArray(), unqPrincipalPaymentDiscountFactors.ToArray(), fixedFlag);
        }

        /// <summary>
        /// Gets the delta0 for a collection of coupon cashflows provided.
        /// </summary>
        /// <param name="paymentDiscountFactors">The discount factors.</param>
        /// <param name="amounts">The amounts</param>
        /// <param name="curveYearFractions">The curve year fractions.</param>
        /// <param name="periodAsTimesPerYears">Delta1 compounding Frequency.</param>
        /// <returns>The break even rate.</returns>
        public double Delta1(Excel.Range amounts, Excel.Range paymentDiscountFactors,
                                     Excel.Range curveYearFractions, double periodAsTimesPerYears)
        {
            var unqNotionals = DataRangeHelper.StripDoubleRange(amounts);
            var unqCouponPaymentDiscountFactors = DataRangeHelper.StripDoubleRange(paymentDiscountFactors);
            var unqYearFractions = DataRangeHelper.StripDoubleRange(curveYearFractions);
            return SwapAnalytics.Delta1(unqNotionals.ToArray(), unqCouponPaymentDiscountFactors.ToArray(), unqYearFractions.ToArray(), periodAsTimesPerYears);
        }

        /// <summary>
        /// Gets the delta0 for a collection of coupon cashflows provided.
        /// </summary>
        /// <param name="paymentDiscountFactors">The discount factors.</param>
        /// <param name="yearFractions">The year fractions.</param>
        /// <param name="notionals">The notionals</param>
        /// <param name="principalPaymentDiscountFactors">The dfs for the principal exchanges.</param>
        /// <param name="principalCurveYearFractions">The principal exchange curve year fractions.</param>
        /// <param name="compoundingFrequency">Delta1 compounding Frequency.</param>
        /// <param name="couponCurveYearsFractions">The coupon time to payments.</param>
        /// <param name="principalNotionals">The principal Exchanges.</param>
        /// <returns>The break even rate.</returns>
        public double Delta1WithExchanges(Excel.Range notionals, Excel.Range paymentDiscountFactors,
                                                  Excel.Range yearFractions, Excel.Range couponCurveYearsFractions, Excel.Range principalNotionals,
                                                  Excel.Range principalPaymentDiscountFactors, Excel.Range principalCurveYearFractions, double compoundingFrequency)
        {
            var unqNotionals = DataRangeHelper.StripDoubleRange(notionals);
            var unqCouponPaymentDiscountFactors = DataRangeHelper.StripDoubleRange(paymentDiscountFactors);
            var unqYearFractions = DataRangeHelper.StripDoubleRange(yearFractions);
            var unqCouponCurveYearsFractions = DataRangeHelper.StripDoubleRange(couponCurveYearsFractions);
            var unqPrincipalNotionals = DataRangeHelper.StripDoubleRange(principalNotionals);
            var unqPrincipalPaymentDiscountFactors = DataRangeHelper.StripDoubleRange(principalPaymentDiscountFactors);
            var unqPrincipalCurveYearFractions = DataRangeHelper.StripDoubleRange(principalCurveYearFractions);
            return SwapAnalytics.Delta1WithExchanges(unqNotionals.ToArray(), unqCouponPaymentDiscountFactors.ToArray(),
                                                     unqYearFractions.ToArray(), unqCouponCurveYearsFractions.ToArray(), unqPrincipalNotionals.ToArray(),
                                                     unqPrincipalPaymentDiscountFactors.ToArray(), unqPrincipalCurveYearFractions.ToArray(), compoundingFrequency);
        }

        /// <summary>
        /// Evaluates the delta wrt the discount rate R.
        /// </summary>
        /// <param name="amounts">The amounts.</param>
        /// <param name="paymentDiscountFactors">The payment discount factors.</param>
        /// <param name="periodAsTimesPerYears">The compounding year fractions.</param>
        /// <param name="curveYearFractions">The time to payment year fractions.</param>
        /// <returns></returns>
        public double Delta1Arrays(Excel.Range amounts, Excel.Range paymentDiscountFactors,
                                           Excel.Range curveYearFractions, double periodAsTimesPerYears)
        {
            var unqNotionals = DataRangeHelper.StripDoubleRange(amounts);
            var unqCouponPaymentDiscountFactors = DataRangeHelper.StripDoubleRange(paymentDiscountFactors);
            var unqYearFractions = DataRangeHelper.StripDoubleRange(curveYearFractions);
            return SwapAnalytics.Delta1Arrays(unqNotionals.ToArray(), unqCouponPaymentDiscountFactors.ToArray(),
                                              unqYearFractions.ToArray(), periodAsTimesPerYears);
        }

        /// <summary>
        /// Evaluates the delta wrt the discount rate R.
        /// </summary>
        /// <param name="notionals">The notionals.</param>
        /// <param name="yearFractions">The day count fractions.</param>
        /// <param name="rates">The rates.</param>
        /// <param name="paymentDiscountFactors">The payment discount factors.</param>
        /// <param name="periodAsTimesPerYears">The compounding year fractions.</param>
        /// <param name="curveYearFractions">The time to payment year fractions.</param>
        /// <returns></returns>
        public double Delta1Arrays2(Excel.Range notionals, Excel.Range yearFractions, Excel.Range rates,
                                           Excel.Range paymentDiscountFactors, Excel.Range curveYearFractions, double periodAsTimesPerYears)
        {
            var unqNotionals = DataRangeHelper.StripDoubleRange(notionals);
            var unqRates = DataRangeHelper.StripDoubleRange(rates);
            var unqCouponPaymentDiscountFactors = DataRangeHelper.StripDoubleRange(paymentDiscountFactors);
            var unqYearFractions = DataRangeHelper.StripDoubleRange(yearFractions);
            var unqCurveYearFractions = DataRangeHelper.StripDoubleRange(curveYearFractions);
            return SwapAnalytics.Delta1Arrays(unqNotionals.ToArray(), unqYearFractions.ToArray(), unqRates.ToArray(),
                                              unqCouponPaymentDiscountFactors.ToArray(), unqCurveYearFractions.ToArray(), periodAsTimesPerYears);
        }

        /// <summary>
        /// Evaluates the delta wrt the discount rate R.
        /// </summary>
        /// <param name="notional">The notional for that period.</param>
        /// <param name="yearFraction">the day count fraction for that coupon.</param>
        /// <param name="rate">The rate for that period.</param>
        /// <param name="paymentDiscountFactor">The payment discount factor.</param>
        /// <param name="periodAsTimesPerYear">the compounding year fraction.</param>
        /// <param name="curveYearFraction">The time to payment year fraction.</param>
        /// <returns></returns>
        public double Delta1ForNotional(double notional, double yearFraction, double rate, double paymentDiscountFactor, 
                                                double periodAsTimesPerYear, double curveYearFraction)
        {
            return SwapAnalytics.Delta1ForNotional(notional, yearFraction, rate, paymentDiscountFactor, 
                                                   periodAsTimesPerYear, curveYearFraction);
        }

        /// <summary>
        /// Evaluates the delta wrt the discount rate R.
        /// </summary>
        /// <param name="amount">The amount for that period.</param>
        /// <param name="paymentDiscountFactor">The payment discount factor.</param>
        /// <param name="periodAsTimesPerYear">the compounding year fraction.</param>
        /// <param name="curveYearFraction">The time to payment year fraction.</param>
        /// <returns></returns>
        public double Delta1ForAnAmount(double amount, double paymentDiscountFactor,
                                                double periodAsTimesPerYear, double curveYearFraction)
        {
            return SwapAnalytics.Delta1ForAnAmount(amount, paymentDiscountFactor,
                                                   periodAsTimesPerYear, curveYearFraction);
        }

        /// <summary>
        /// Evaluates the delta wrt the forward rate R.
        /// </summary>
        /// <param name="notional">The notional for that period.</param>
        /// <param name="paymentDiscountFactor">The payment discount factor.</param>
        /// <param name="dayFraction">The year fraction.</param>
        /// <param name="rate">The rate.</param>
        /// <returns></returns>
        public double Delta0Coupon(double notional, double dayFraction, double rate, double paymentDiscountFactor)
        {
            return SwapAnalytics.Delta0Coupon(notional, dayFraction, rate, paymentDiscountFactor);
        }

        /// <summary>
        /// Evaluates the delta wrt the forward rate R for discount coupon.
        /// </summary>
        /// <param name="notional">The notional for that period.</param>
        /// <param name="paymentDiscountFactor">The payment discount factor.</param>
        /// <param name="dayFraction">The year fraction.</param>
        /// <param name="rate">The rate.</param>
        /// <returns></returns>
        public double Delta0DiscountCoupon(double notional, double dayFraction, double rate, double paymentDiscountFactor)
        {
            return SwapAnalytics.Delta0DiscountCoupon(notional, dayFraction, rate, paymentDiscountFactor);
        }

        /// <summary>
        /// Converts a sting period to a time interval.
        /// </summary>
        /// <param name="interval">The string interval. Valid intervals are: </param>
        /// <returns>The double interval.</returns>
        public double TimeFromInterval(string interval)
        {
            return PeriodHelper.Parse(interval).ToYearFraction();
        }

        /// <summary>
        /// Computes today's value of an annuity factor for a vanilla
        /// interest rate swap.
        /// </summary>
        /// <param name="forwardSwapRate">Forward swap rate, expressed as a
        /// decimal.
        /// Example: if the forward swap rate is 6.820177%, call the function
        /// with the value 0.006820177.</param>
        /// <param name="dfSwapStart">Discount factor from today to the start
        /// of the swap.
        /// Example: 6M option into a 2YR quarterly swap.
        /// The appropriate discount factor is for the tenor 6M.</param>
        /// <param name="dfSwapEnd">Discount factor from today to the end of
        /// the swap.
        /// Example: 6M option into a 2YR quarterly swap.
        /// The appropriate discount factor is for the tenor (6M + 2YR).</param>
        /// <returns>Annuity factor for a vanilla interest rate swap.</returns>
        public double ComputeAnnuityFactor(double forwardSwapRate,
                                                  double dfSwapStart,
                                                  double dfSwapEnd)
        {
            return SwapAnalytics.ComputeAnnuityFactor(forwardSwapRate,
                                                      dfSwapStart,
                                                      dfSwapEnd);
        }

        #endregion
    }
}