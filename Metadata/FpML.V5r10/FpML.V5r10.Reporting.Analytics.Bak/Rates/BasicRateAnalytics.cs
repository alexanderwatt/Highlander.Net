using System;

namespace Orion.Analytics.Rates
{
    ///<summary>
    ///</summary>
    public class BasicRateAnalytics
    {
        ///<summary>
        ///</summary>
        ///<param name="notionalAmount"></param>
        ///<param name="yearFraction"></param>
        ///<param name="rate"></param>
        ///<param name="startDiscountFactor"></param>
        ///<param name="endDiscountFactor"></param>
        ///<returns></returns>
        public static Decimal EvaluateNPV(decimal notionalAmount, decimal yearFraction, decimal rate, decimal startDiscountFactor, decimal endDiscountFactor)
        {
            return notionalAmount * yearFraction *
                   (rate - EvaluateImpliedQuote(startDiscountFactor, endDiscountFactor, yearFraction)) * endDiscountFactor;
        }

        ///<summary>
        ///</summary>
        ///<param name="futureValue"></param>
        ///<param name="startDiscountFactor"></param>
        ///<param name="endDiscountFactor"></param>
        ///<returns></returns>
        public static Decimal EvaluateNPV(decimal futureValue, decimal startDiscountFactor, decimal endDiscountFactor)
        {
            return futureValue * endDiscountFactor / startDiscountFactor;
        }

        ///<summary>
        ///</summary>
        ///<param name="notionalAmount"></param>
        ///<param name="rate"></param>
        ///<param name="yearFraction"></param>
        ///<returns></returns>
        public static Decimal EvaluateFutureValue(decimal notionalAmount, decimal rate, decimal yearFraction)
        {
            return notionalAmount*yearFraction*rate;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        public static Decimal EvaluateDeltaR(decimal notionalAmount, decimal yearFraction, decimal endDiscountFactor)
        {
            return notionalAmount * yearFraction * endDiscountFactor / 10000;
        }

        /// <summary>
        /// Evaluates the accrual factor.
        /// </summary>
        /// <returns></returns>
        public static Decimal EvaluateAccrualFactor(decimal yearFraction, decimal endDiscountFactor)
        {
            return yearFraction * endDiscountFactor;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        public static Decimal EvaluateImpliedQuote(decimal startDiscountFactor, decimal endDiscountFactor, decimal yearFraction)
        {
            return ((startDiscountFactor / endDiscountFactor) - 1) / yearFraction;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        public static Decimal EvaluateDiscountFactorAtMaturity(decimal startDiscountFactor, decimal yearFraction, decimal rate)
        {
            return startDiscountFactor / (1 + yearFraction * rate);
        }
    }
}