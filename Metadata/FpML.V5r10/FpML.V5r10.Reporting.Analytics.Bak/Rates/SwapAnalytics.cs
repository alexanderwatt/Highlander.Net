using System;
using Orion.Analytics.Utilities;
using Math=System.Math;

namespace Orion.Analytics.Rates
{
    /// <summary>
    /// Rate analytics functions.
    /// </summary>
    public class SwapAnalytics
    {
        /// <summary>
        /// Gets the npv for a collection of coupon cashlows provided.
        /// </summary>
        /// <param name="paymentDiscountFactors">The discount factors.</param>
        /// <param name="yearFractions">The year fractions.</param>
        /// <param name="forwardRates">The forward rates.</param>
        /// <param name="notionals">The notionals</param>
        /// <returns>The break even rate.</returns>
        public static double NPV(double[] notionals, double[] forwardRates, double[] paymentDiscountFactors, double[] yearFractions)
        {
            return ArrayUtilities.SumProduct(notionals, forwardRates, paymentDiscountFactors, yearFractions);
        }

        /// <summary>
        /// Gets the npv for a collection of coupons and princiapal exchanges provided.
        /// </summary>
        /// <param name="couponPaymentDiscountFactors">The discount factors.</param>
        /// <param name="yearFractions">The year fractions.</param>
        /// <param name="forwardRates">The forward rates.</param>
        /// <param name="couponNotionals">The notionals of the coupons.</param>
        /// <param name="principalNotionals">The principal exchange notionals.</param>
        /// <param name="principalPaymentDiscountFactors">The payment discount factors for the principal exchanges.</param>
        /// <returns>The break even rate.</returns>
        public static double NPVWithExchanges(double[] couponNotionals, double[] forwardRates,
            double[] couponPaymentDiscountFactors, double[] yearFractions, double[] principalNotionals,
            double[] principalPaymentDiscountFactors)
        {
            var npv = ArrayUtilities.SumProduct(couponNotionals, forwardRates, couponPaymentDiscountFactors, yearFractions);
            var npv2 = ArrayUtilities.SumProduct(principalPaymentDiscountFactors, principalNotionals);
            return npv + npv2;
        }

        /// <summary>
        /// Gets the break even rate for a collection of coupon cashlows provided.
        /// </summary>
        /// <param name="paymentDiscountFactors">The discount factors.</param>
        /// <param name="yearFractions">The year fractions.</param>
        /// <param name="forwardRates">The forward rates.</param>
        /// <param name="notionals">The notionals</param>
        /// <returns>The break even rate.</returns>
        public static double BreakEvenRate(double[] notionals, double[] forwardRates, double[] paymentDiscountFactors, double[] yearFractions)
        {
            var npv = ArrayUtilities.SumProduct(notionals, forwardRates, paymentDiscountFactors, yearFractions);
            var delta0 = ArrayUtilities.SumProduct(notionals, paymentDiscountFactors, yearFractions);
            return npv / delta0;
        }

        /// <summary>
        /// Gets the break even rate for a collection of coupons and princiapal exchanges provided.
        /// </summary>
        /// <param name="couponPaymentDiscountFactors">The discount factors.</param>
        /// <param name="yearFractions">The year fractions.</param>
        /// <param name="forwardRates">The forward rates.</param>
        /// <param name="couponNotionals">The notionals of the coupons.</param>
        /// <param name="principalNotionals">The principal exchange notionals.</param>
        /// <param name="principalPaymentDiscountFactors">The payment discount factors for the principal exchanges.</param>
        /// <returns>The break even rate.</returns>
        public static double BreakEvenRateWithExchanges(double[] couponNotionals, double[] forwardRates,
            double[] couponPaymentDiscountFactors, double[] yearFractions, double[] principalNotionals, 
            double[] principalPaymentDiscountFactors)
        {
            var result = 0.0;
            var delta0 = ArrayUtilities.SumProduct(couponNotionals, couponPaymentDiscountFactors, yearFractions);
            var npv = ArrayUtilities.SumProduct(couponNotionals, forwardRates, couponPaymentDiscountFactors, yearFractions);
            var npv2 = ArrayUtilities.SumProduct(principalPaymentDiscountFactors, principalNotionals);
            if (delta0 != 0)
            {
                result = (npv + npv2) / delta0;
            }
            return result;
        }


        /// <summary>
        /// Gets the delta0 for a collection of coupon cashlows provided.
        /// </summary>
        /// <param name="paymentDiscountFactors">The discount factors.</param>
        /// <param name="yearFractions">The year fractions.</param>
        /// <param name="notionals">The notionals</param>
        /// <param name="fixedFlag">Delta0 is zero for fixed coupons.</param>
        /// <returns>The break even rate.</returns>
        public static double Delta0(double[] notionals, double[] paymentDiscountFactors, double[] yearFractions, Boolean fixedFlag)
        {
            if (fixedFlag) return 0.0;
            var delta0 = ArrayUtilities.SumProduct(notionals, paymentDiscountFactors, yearFractions);
            return -delta0 / 10000.0;
        }

        /// <summary>
        /// Gets the delta0 for a collection of coupon cashlows provided.
        /// </summary>
        /// <param name="paymentDiscountFactors">The discount factors.</param>
        /// <param name="yearFractions">The year fractions.</param>
        /// <param name="notionals">The notionals</param>
        /// <param name="principalPaymentDiscountFactors">The dfs for the principal exchanges.</param>
        /// <param name="fixedFlag">Delta0 is zero for fixed coupons.</param>
        /// <param name="principalNotionals">The principal Exchanges.</param>
        /// <returns>The break even rate.</returns>
        public static double Delta0WithExchanges(double[] notionals, double[] paymentDiscountFactors, double[] yearFractions, 
            double[] principalNotionals, double[] principalPaymentDiscountFactors, Boolean fixedFlag)
        {
            if (fixedFlag) return 0.0;
            var delta0 = ArrayUtilities.SumProduct(notionals, paymentDiscountFactors, yearFractions);
            return -delta0 / 10000.0;
        }

        /// <summary>
        /// Gets the delta0 for a collection of coupon cashlows provided.
        /// </summary>
        /// <param name="paymentDiscountFactors">The discount factors.</param>
        /// <param name="amounts">The amounts</param>
        /// <param name="curveYearFractions">The curve year fractions.</param>
        /// <param name="periodAsTimesPerYears">Delta1 compounding Frequency.</param>
        /// <returns>The break even rate.</returns>
        public static double Delta1(double[] amounts, double[] paymentDiscountFactors,
            double[] curveYearFractions, double periodAsTimesPerYears)
        {
            var delta1 = Delta1Arrays(amounts, paymentDiscountFactors, curveYearFractions, periodAsTimesPerYears);
            return delta1;
        }

        /// <summary>
        /// Gets the delta0 for a collection of coupon cashlows provided.
        /// </summary>
        /// <param name="paymentDiscountFactors">The discount factors.</param>
        /// <param name="yearFractions">The year fractions.</param>
        /// <param name="notionals">The notionals</param>
        /// <param name="principalPaymentDiscountFactors">The dfs for the principal exchanges.</param>
        /// <param name="principalCurveYearFractionss">The principal exchange curve year fractions.</param>
        /// <param name="compoundingFrequency">Delta1 compounding Frequency.</param>
        /// <param name="couponCurveYearsFractions">The coupon time to payments.</param>
        /// <param name="principalNotionals">The principal Exchanges.</param>
        /// <returns>The break even rate.</returns>
        public static double Delta1WithExchanges(double[] notionals, double[] paymentDiscountFactors,
            double[] yearFractions, double[] couponCurveYearsFractions, double[] principalNotionals,
            double[] principalPaymentDiscountFactors, double[] principalCurveYearFractionss, double compoundingFrequency)
        {
            var amts = ArrayUtilities.Product(notionals, paymentDiscountFactors, yearFractions);
            var delta1 = Delta1Arrays(amts, paymentDiscountFactors, couponCurveYearsFractions, compoundingFrequency)
                + Delta1Arrays(principalNotionals, principalPaymentDiscountFactors, principalCurveYearFractionss, compoundingFrequency);
            return delta1;
        }

        /// <summary>
        /// Evaluates the delta wrt the discount rate R.
        /// </summary>
        /// <param name="amounts">The amounts.</param>
        /// <param name="paymentDiscountFactors">The payment discount factors.</param>
        /// <param name="periodAsTimesPerYears">The compounding year fractions.</param>
        /// <param name="curveYearFractions">The time to payment year fractions.</param>
        /// <returns></returns>
        public static double Delta1Arrays(double[] amounts, double[] paymentDiscountFactors, 
            double[] curveYearFractions, double periodAsTimesPerYears)
        {
            var delta = ArrayUtilities.DeltaHelper(curveYearFractions, periodAsTimesPerYears, paymentDiscountFactors);
            return ArrayUtilities.SumProduct(amounts, delta);
        }

        /// <summary>
        /// Evaluates the delta wrt the discount rate R.
        /// </summary>
        /// <param name="notionals">The notionals.</param>
        /// <param name="yearfractions">The daycount fractions.</param>
        /// <param name="rates">The rates.</param>
        /// <param name="paymentDiscountFactors">The payment discount factors.</param>
        /// <param name="periodAsTimesPerYears">The compounding year fractions.</param>
        /// <param name="curveYearFractions">The time to payment year fractions.</param>
        /// <returns></returns>
        public static double Delta1Arrays(double[] notionals, double[] yearfractions, double[] rates, 
            double[] paymentDiscountFactors, double[] curveYearFractions, double periodAsTimesPerYears)
        {
            var amount = ArrayUtilities.Product(notionals, yearfractions, rates, paymentDiscountFactors);
            var delta = ArrayUtilities.DeltaHelper(curveYearFractions, periodAsTimesPerYears, rates);
            return ArrayUtilities.SumProduct(amount, delta);
        }

        /// <summary>
        /// Evaluates the delta wrt the discount rate R.
        /// </summary>
        /// <param name="notional">The notional for that period.</param>
        /// <param name="yearfraction">the daycount fraction for that coupon.</param>
        /// <param name="rate">The rate for that period.</param>
        /// <param name="paymentDiscountFactor">The payment discount factor.</param>
        /// <param name="periodAsTimesPerYear">the compounding year fraction.</param>
        /// <param name="curveYearFraction">The time to payment year fraction.</param>
        /// <returns></returns>
        public static double Delta1ForNotional(double notional, double yearfraction, double rate, double paymentDiscountFactor, 
            double periodAsTimesPerYear, double curveYearFraction)
        {
            if (curveYearFraction == 0.0)
            {
                return 0.0;
            }
            var amount = notional*yearfraction*rate;
            return Delta1ForAnAmount(amount, paymentDiscountFactor, periodAsTimesPerYear, curveYearFraction);
        }

        /// <summary>
        /// Evaluates the delta wrt the discount rate R.
        /// </summary>
        /// <param name="amount">The amount for that period.</param>
        /// <param name="paymentDiscountFactor">The payment discount factor.</param>
        /// <param name="periodAsTimesPerYear">the compounding year fraction.</param>
        /// <param name="curveYearFraction">The time to payment year fraction.</param>
        /// <returns></returns>
        public static double Delta1ForAnAmount(double amount, double paymentDiscountFactor,
            double periodAsTimesPerYear, double curveYearFraction)
        {
            if(curveYearFraction==0.0)
            {
                return 0.0; 
            }
            var rate = (double)Math.Log((double)paymentDiscountFactor) / curveYearFraction;
            var temp = periodAsTimesPerYear * -rate;
            var result = amount * paymentDiscountFactor * curveYearFraction / (1 + temp) / 10000.0;
            return result;
        }

        /// <summary>
        /// Evaluates the delta wrt the forward rate R.
        /// </summary>
        /// <param name="notional">The notional for that period.</param>
        /// <param name="paymentDiscountFactor">The payment discount factor.</param>
        /// <param name="dayFraction">The year fraction.</param>
        /// <param name="rate">The rate.</param>
        /// <returns></returns>
        public static double Delta0Coupon(double notional, double dayFraction, double rate, double paymentDiscountFactor)
        {
            var result = -notional * dayFraction * paymentDiscountFactor / 10000.0;
            return result;
        }

        /// <summary>
        /// Evaluates the delta wrt the forward rate R for discount coupon.
        /// </summary>
        /// <param name="notional">The notional for that period.</param>
        /// <param name="paymentDiscountFactor">The payment discount factor.</param>
        /// <param name="dayFraction">The year fraction.</param>
        /// <param name="rate">The rate.</param>
        /// <returns></returns>
        public static double Delta0DiscountCoupon(double notional, double dayFraction, double rate, double paymentDiscountFactor)
        {
            var result = notional / (1 + dayFraction * rate) * paymentDiscountFactor;
            return -result * dayFraction / (1 + dayFraction * rate) / 10000.0;
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
        public static double ComputeAnnuityFactor(double forwardSwapRate,
                                                  double dfSwapStart,
                                                  double dfSwapEnd)
        {
            return (dfSwapStart - dfSwapEnd) / forwardSwapRate;
        }
    }
}
