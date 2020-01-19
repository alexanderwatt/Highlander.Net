/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using Highlander.Numerics.Solvers;
using MathNet.Numerics.Differentiation;

namespace Highlander.Numerics.Rates
{
    /// <summary>
    /// Rate analytics functions.
    /// </summary>
    public class CashflowAnalytics
    {
        #region Solver Properties

        ///<summary>
        ///</summary>
        public decimal CurveYearFraction { get; set; }

        ///<summary>
        ///</summary>
        public decimal PeriodAsTimesPerYear { get; set; }

        ///<summary>
        ///</summary>
        public decimal ExpectedValue { get; set; }

        ///<summary>
        ///</summary>
        public List<decimal> BucketedDiscountFactors { get; set; }

        #endregion

        #region Cashflow Analytics

        ///<summary>
        ///</summary>
        ///<param name="cashflow">The expected value of the cashflow.</param>
        ///<param name="discountFactor">The payment discount factor.</param>
        ///<returns></returns>
        public static Decimal CashflowNPV(decimal cashflow, decimal discountFactor)
        {
            return cashflow * discountFactor;
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        public static Decimal CashflowDelta1CCR(decimal cashflow, decimal curveYearFraction, decimal discountFactor)
        {
            return CashflowNPV(cashflow, discountFactor) * -curveYearFraction / 10000;
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        public static Decimal CashflowGamma1CCR(decimal cashflow, decimal curveYearFraction, decimal discountFactor)
        {
            return CashflowDelta1CCR(cashflow, curveYearFraction, discountFactor) * -curveYearFraction / 10000;
        }

        ///<summary>
        ///</summary>
        ///<param name="cashflow">The expected value of the cashflow.</param>
        ///<param name="periodAsTimesPerYear">Thenumber of times per year to compound.</param>
        ///<param name="curveYearFraction">The year fraction to the payment date.</param>
        ///<param name="bucketingRate">Tee discounting rate to use.</param>
        ///<param name="discountFactor">The payment discount factor.</param>
        ///<returns></returns>
        public static Decimal CashflowDelta1(decimal cashflow, decimal periodAsTimesPerYear, decimal curveYearFraction, decimal bucketingRate, decimal discountFactor)
        {
            var temp = periodAsTimesPerYear * bucketingRate;
            return CashflowNPV(cashflow, discountFactor) * -curveYearFraction / (1 + temp) / 10000;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        public static Decimal[] CashflowBucketDelta12(decimal cashflow, decimal periodAsTimesPerYear, decimal curveYearFraction, decimal bucketingRate, decimal discountFactor)
        {
            var npv = cashflow*discountFactor;
            var temp = periodAsTimesPerYear * bucketingRate;
            var time = (curveYearFraction / periodAsTimesPerYear);
            var cycles = Convert.ToInt32(Math.Floor(time));
            var remainder = curveYearFraction - cycles * periodAsTimesPerYear;
            var result = new decimal[cycles + 1];
            for (var i = 0; i < cycles; i++)
            {
                result[i] = -npv * periodAsTimesPerYear / (1 + temp) / 10000;
            }
            var tailValue = remainder * bucketingRate;
            result[result.Length - 1] = -npv * remainder / (1 + tailValue) / 10000;
            return result;
        }

        #endregion

        #region Constructor

        public CashflowAnalytics(decimal curveYearFraction, decimal periodAsTimesPerYear, List<decimal> bucketedDiscountFactors, decimal expectedValue)
        {
            CurveYearFraction = curveYearFraction;
            PeriodAsTimesPerYear = periodAsTimesPerYear;
            ExpectedValue = expectedValue;
            BucketedDiscountFactors = bucketedDiscountFactors;
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <param name="startDiscountFactor">The start discount factor.</param>
        /// <param name="endDiscountFactor">The end discount factor.</param>
        /// <param name="yearFaction">The year faction.</param>
        /// <returns></returns>
        public static Decimal GetRate(Decimal startDiscountFactor, Decimal endDiscountFactor, Decimal yearFaction)
        {
            return ((startDiscountFactor / endDiscountFactor) - 1.0m) / yearFaction;
        }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="dfStart">The discount factor.</param>
        /// <param name="dfEnd"></param>
        /// <returns></returns>
        public static Decimal GetRate(DateTime startDate, DateTime endDate, Decimal dfStart, Decimal dfEnd)
        {
            var yearFraction = (endDate - startDate).Days / 365.0m;
            return (dfStart / dfEnd - 1) / yearFraction;
        }

        /// <summary>
        /// Find the index of this coupon year fraction in Array of bucketed year fractions
        /// </summary>
        /// <returns></returns>
        public static int FindYearFractionIndex(decimal curveYearFraction, decimal periodAsTimesPerYear)
        {
            var time = (double)(curveYearFraction / periodAsTimesPerYear);
            var cycles = Convert.ToInt32(time);
            return cycles;
        }

        public static Decimal[] GetBucketedRates(decimal curveYearFraction, decimal periodAsTimesPerYear, List<decimal> bucketedDiscountFactors)
        {
            var len = FindYearFractionIndex(curveYearFraction, periodAsTimesPerYear);
            var bucketedRates = new decimal[len];

            if (len == 0)
            {
                bucketedRates = new[] { 0.0m };
            }
            else
            {
                for (var i = len; i > 0; --i)
                {
                    bucketedRates[i - 1] = GetRate(bucketedDiscountFactors[i - 1],
                                                   bucketedDiscountFactors[i],
                                                   periodAsTimesPerYear);
                }
            }
            return bucketedRates;
        }

        /// <summary>
        /// RealFunction used by extreme optimizer to calculate the derivative
        /// of price wrt floating rate
        /// </summary>
        /// <param name="x">The given rate</param>
        /// <returns></returns>
        public Decimal BucketedDeltaTargetFunction(Decimal x)
        {
            const decimal multiplier = 1.0m;
            var bucketedRates = GetBucketedRates(CurveYearFraction, PeriodAsTimesPerYear, BucketedDiscountFactors);
            var len = bucketedRates.Length;
            var poly = new Polynomial(1, 0);
            var index = len;
            while (index > 0)
            {
                var coeffs = new Decimal[2];
                coeffs[0] = 1.0m;
                coeffs[1] = PeriodAsTimesPerYear;
                var thePoly = new Polynomial(coeffs, 1);
                poly = poly * thePoly;
                --index;
            }
            return ExpectedValue / (multiplier * poly.Value(x));
        }

        /// <summary>
        /// Evaluating Bucketed Delta
        /// </summary>
        /// <returns>The bucketed delta</returns>
        public Decimal GetBucketedDelta1(decimal forwardRate)
        {
            //var realFunction = new RealFunction(BucketedDeltaTargetFunction);
            var rate = forwardRate;
            var dRate = Decimal.ToDouble(rate);
            var numerical = new NumericalDerivative();
            var delta = (Decimal)numerical.EvaluateDerivative(BucketedDeltaTargetFunction, dRate, 1);
            return delta / 10000;
        }

        /// <summary>
        /// Implementation of business logic used by extreme optimization
        /// </summary>
        /// <param name="x">current value</param>
        /// <returns></returns>
        public Double BucketedDeltaTargetFunction(Double x)
        {
            var dx = (Decimal)x;
            return Decimal.ToDouble(BucketedDeltaTargetFunction(dx));
        }

        /// <summary>
        /// Evaluating the vector of Bucketed Delta
        /// </summary>
        /// <returns>The vector of bucketed delta</returns>
        public Decimal[] EvaluateBucketedDeltaVector(decimal curveYearFraction, decimal periodAsTimesPerYear, List<decimal> bucketedDiscountFactors, decimal forwardRate)
        {
            var bucketedRates = GetBucketedRates(curveYearFraction, periodAsTimesPerYear, bucketedDiscountFactors);
            var len = bucketedRates.Length;
            var bucketedDeltaVector = new Decimal[len];
            const Decimal cDefault = 0.0m;
            Decimal result;
            if (len == 1 && bucketedRates[0] == cDefault)
            {
                result = cDefault;
            }
            else
            {
                result = GetBucketedDelta1(forwardRate);
            }
            for (var i = 0; i < len; ++i)
            {
                bucketedDeltaVector[i] = result;
            }
            return bucketedDeltaVector;
        }

        #endregion

        #region Simple Coupon Analytics

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forwardRate"></param>
        ///<param name="discountingForwardRate"></param>
        ///<returns></returns>
        public static decimal CouponExpectedValue(decimal notional, decimal yearFraction, decimal forwardRate, decimal discountingForwardRate)
        {
            return notional * yearFraction * forwardRate / (1 + yearFraction * discountingForwardRate);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forwardRate"></param>
        ///<param name="discountingForwardRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static decimal CouponNPV(decimal notional, decimal yearFraction, decimal forwardRate, decimal discountingForwardRate, decimal discountFactor)
        {
            return CouponExpectedValue(notional, yearFraction, forwardRate, discountingForwardRate) * discountFactor;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static decimal CouponDeltaR(decimal notional, decimal yearFraction, decimal discountFactor)
        {
            return notional * yearFraction / 10000 * discountFactor;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static decimal CouponDelta0(decimal notional, decimal yearFraction, decimal discountFactor)
        {
            return InArrearsCouponDeltaR(notional, yearFraction, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static decimal CouponGamma0(decimal notional, decimal yearFraction, decimal discountFactor)
        {
            return 0.0m;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static decimal CouponGammaX(decimal notional, decimal yearFraction, decimal discountFactor)//TODO
        {
            return 0.0m;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static decimal CouponAccrualFactor(decimal notional, decimal yearFraction, decimal discountFactor)
        {
            return InArrearsCouponDeltaR(notional, yearFraction, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="forwardRate">The forecast rate.</param>
        ///<param name="periodAsTimesPerYear">Thenumber of times per year to compound.</param>
        ///<param name="curveYearFraction">The year fraction to the payment date.</param>
        ///<param name="bucketingRate">The discounting rate to use.</param>
        ///<param name="discountFactor">The payment discount factor.</param>
        ///<param name="notional">The notional of the coupon.</param>
        ///<param name="yearFraction">The coupon year fraction.</param>
        ///<returns></returns>
        public static decimal CouponDelta1(decimal notional, decimal yearFraction, decimal forwardRate, decimal periodAsTimesPerYear, decimal curveYearFraction, decimal bucketingRate, decimal discountFactor)
        {
            var cashflow = InArrearsCouponNPV(notional, yearFraction, forwardRate, discountFactor);
            var temp = periodAsTimesPerYear * bucketingRate;
            return cashflow * -curveYearFraction / (1 + temp) / 10000;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>//TODO replace the bucket rate by the discount factor derived ratefor that compounding period.
        public static decimal[] CouponBucketDelta12(decimal notional, decimal yearFraction, decimal forwardRate, decimal periodAsTimesPerYear, decimal curveYearFraction, decimal bucketingRate, decimal discountFactor)
        {
            var npv = InArrearsCouponNPV(notional, yearFraction, forwardRate, discountFactor);
            var temp = periodAsTimesPerYear * bucketingRate;
            var time = (curveYearFraction / periodAsTimesPerYear);
            var cycles = Convert.ToInt32(Math.Floor(time));
            var remainder = curveYearFraction - cycles * periodAsTimesPerYear;
            var result = new decimal[cycles + 1];
            for (var i = 0; i < cycles; i++)
            {
                result[i] = -npv * periodAsTimesPerYear / (1 + temp) / 10000;
            }
            var tailValue = remainder * bucketingRate;
            result[result.Length - 1] = -npv * remainder / (1 + tailValue) / 10000;
            return result;
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        public static decimal CouponDelta1CCR(decimal notional, decimal yearFraction, decimal forwardRate, decimal curveYearFraction, decimal discountFactor)
        {
            return InArrearsCouponNPV(notional, yearFraction, forwardRate, discountFactor) * -curveYearFraction / 10000;
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        public static decimal CouponGamma1CCR(decimal notional, decimal yearFraction, decimal forwardRate, decimal curveYearFraction, decimal discountFactor)
        {
            return InArrearsCouponDelta1CCR(notional, yearFraction, forwardRate, curveYearFraction, discountFactor) * curveYearFraction / 10000;
        }

        #endregion

        #region InArrears Coupon Analytics

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="rate"></param>
        ///<returns></returns>
        public static Decimal InArrearsCouponExpectedValue(decimal notional, decimal yearFraction, decimal rate)
        {
            return notional * yearFraction * rate;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forwardRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal InArrearsCouponNPV(decimal notional, decimal yearFraction, decimal forwardRate, decimal discountFactor)
        {
            return InArrearsCouponExpectedValue(notional, yearFraction, forwardRate) * discountFactor;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal InArrearsCouponDeltaR(decimal notional, decimal yearFraction, decimal discountFactor)
        {
            return notional * yearFraction / 10000 * discountFactor;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal InArrearsCouponDelta0(decimal notional, decimal yearFraction, decimal discountFactor)
        {
            return InArrearsCouponDeltaR(notional, yearFraction, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal InArrearsCouponGamma0(decimal notional, decimal yearFraction, decimal discountFactor)
        {
            return 0.0m;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal InArrearsCouponGammaX(decimal notional, decimal yearFraction, decimal discountFactor)//TODO
        {
            return 0.0m;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal InArrearsCouponAccrualFactor(decimal notional, decimal yearFraction, decimal discountFactor)
        {
            return InArrearsCouponDeltaR(notional, yearFraction, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="forwardRate">The forecast rate.</param>
        ///<param name="periodAsTimesPerYear">Thenumber of times per year to compound.</param>
        ///<param name="curveYearFraction">The year fraction to the payment date.</param>
        ///<param name="bucketingRate">The discounting rate to use.</param>
        ///<param name="discountFactor">The payment discount factor.</param>
        ///<param name="notional">The notional of the coupon.</param>
        ///<param name="yearFraction">The coupon year fraction.</param>
        ///<returns></returns>
        public static Decimal InArrearsCouponDelta1(decimal notional, decimal yearFraction, decimal forwardRate, decimal periodAsTimesPerYear, decimal curveYearFraction, decimal bucketingRate, decimal discountFactor)
        {
            var cashflow = InArrearsCouponNPV(notional, yearFraction, forwardRate, discountFactor);
            var temp = periodAsTimesPerYear * bucketingRate;
            return cashflow * -curveYearFraction / (1 + temp) / 10000;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>//TODO replace the bucket rate by the discount factor derived ratefor that compounding period.
        public static Decimal[] InArrearsCouponBucketDelta12(decimal notional, decimal yearFraction, decimal forwardRate, decimal periodAsTimesPerYear, decimal curveYearFraction, decimal bucketingRate, decimal discountFactor)
        {
            var npv = InArrearsCouponNPV(notional, yearFraction, forwardRate, discountFactor);
            var temp = periodAsTimesPerYear * bucketingRate;
            var time = (curveYearFraction / periodAsTimesPerYear);
            var cycles = Convert.ToInt32(Math.Floor(time));
            var remainder = curveYearFraction - cycles * periodAsTimesPerYear;
            var result = new decimal[cycles + 1];
            for (var i = 0; i < cycles; i++)
            {
                result[i] = -npv * periodAsTimesPerYear / (1 + temp) / 10000;
            }
            var tailValue = remainder * bucketingRate;
            result[result.Length - 1] = -npv * remainder / (1 + tailValue) / 10000;
            return result;
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        public static Decimal InArrearsCouponDelta1CCR(decimal notional, decimal yearFraction, decimal forwardRate, decimal curveYearFraction, decimal discountFactor)
        {
            return InArrearsCouponNPV(notional, yearFraction, forwardRate, discountFactor) * -curveYearFraction / 10000;
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        public static Decimal InArrearsCouponGamma1CCR(decimal notional, decimal yearFraction, decimal forwardRate, decimal curveYearFraction, decimal discountFactor)
        {
            return InArrearsCouponDelta1CCR(notional, yearFraction, forwardRate, curveYearFraction, discountFactor) * curveYearFraction / 10000;
        }

        #endregion

        #region ISDA Discount Floating Coupon Analytics

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate">The forecastRate.</param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFloatingCouponNPV(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)
        {
            return ISDADiscountedFloatingCouponExpectedValue(notional, yearFraction, forecastRate) * discountFactor;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate">The forecastRate.</param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFloatingCouponExpectedValue(decimal notional, decimal yearFraction, decimal forecastRate)
        {
            return notional * yearFraction * forecastRate / (1 + yearFraction * forecastRate);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFloatingCouponDeltaR(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)//TODO
        {
            return ISDADiscountedFloatingCouponDelta0(notional, yearFraction, forecastRate, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFloatingCouponDelta0(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)
        {
            var result = (notional * discountFactor - ISDADiscountedFloatingCouponNPV(notional, yearFraction, forecastRate, discountFactor) / (1 + yearFraction * forecastRate)) * yearFraction / 10000;
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFloatingCouponGamma0(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)
        {
            const decimal result = 0.0m;
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFloatingCouponGammaX(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)//TODO
        {
            const decimal result = 0.0m;
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFloatingCouponAccrualFactor(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)
        {
            return ISDADiscountedFloatingCouponDelta0(notional, yearFraction, forecastRate, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="curveYearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFloatingCouponDelta1(decimal notional, decimal yearFraction, decimal forecastRate, decimal curveYearFraction, decimal discountFactor)
        {
            return ISDADiscountedFloatingCouponDelta1CCR(notional, yearFraction, forecastRate, curveYearFraction, discountFactor);//TODO Not correct
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        public static Decimal ISDADiscountedFloatingCouponDelta1CCR(decimal notional, decimal yearFraction, decimal forecastRate, decimal curveYearFraction, decimal discountFactor)
        {
            return ISDADiscountedFloatingCouponNPV(notional, yearFraction, forecastRate, discountFactor) * -curveYearFraction / 10000;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="curveYearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFloatingCouponGamma1(decimal notional, decimal yearFraction, decimal forecastRate, decimal curveYearFraction, decimal discountFactor)
        {
            var result = -curveYearFraction * ISDADiscountedFloatingCouponDelta1CCR(notional, yearFraction, forecastRate, curveYearFraction, discountFactor) / 10000;
            return result;
        }

        #endregion

        #region ISDA Discount Fixed Coupon Anlaytics

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="fixedRate"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFixedCouponNPV(decimal notional, decimal yearFraction, decimal fixedRate, decimal forecastRate, decimal discountFactor)
        {
            return ISDADiscountedFixedCouponExpectedValue(notional, yearFraction, fixedRate, forecastRate) * discountFactor;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="fixedRate"></param>
        ///<param name="forecastRate"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFixedCouponExpectedValue(decimal notional, decimal yearFraction, decimal fixedRate, decimal forecastRate)
        {
            return notional * yearFraction * fixedRate / (1 + yearFraction * forecastRate);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFixedCouponDeltaR(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)//TODO
        {
            return ISDADiscountedFloatingCouponDelta0(notional, yearFraction, forecastRate, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate">The R in the derivative.</param>
        ///<param name="fixedRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFixedCouponDelta0(decimal notional, decimal yearFraction, decimal fixedRate, decimal forecastRate, decimal discountFactor)
        {
            return -ISDADiscountedFixedCouponNPV(notional, yearFraction, fixedRate, forecastRate, discountFactor) * yearFraction / (1 + yearFraction * forecastRate) / 10000;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFixedCouponAccrualFactor(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)
        {
            return ISDADiscountedFixedCouponDeltaR(notional, yearFraction, forecastRate, discountFactor);//TODO check this.
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="rate"></param>
        ///<param name="forecastRate"></param>
        ///<param name="curveYearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFixedCouponDelta1(decimal notional, decimal yearFraction, decimal rate, decimal forecastRate, decimal curveYearFraction, decimal discountFactor)
        {
            return ISDADiscountedFixedCouponNPV(notional, yearFraction, rate, forecastRate, discountFactor) * -curveYearFraction / 10000;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="rate"></param>
        ///<param name="forecastRate"></param>
        ///<param name="curveYearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal ISDADiscountedFixedCouponGamma1(decimal notional, decimal yearFraction, decimal rate, decimal forecastRate, decimal curveYearFraction, decimal discountFactor)
        {
            var result = curveYearFraction * ISDADiscountedFixedCouponDelta1(notional, yearFraction, rate, forecastRate, curveYearFraction, discountFactor) / 10000;
            return result;
        }

        #endregion

        #region AFMA Discounted Coupon Analytic

               ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="rate"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedCouponExpectedValue(decimal notional, decimal yearFraction, decimal rate)
        {
            return notional * (1 - 1 / (1 + yearFraction * rate));
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="rate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedCouponNPV(decimal notional, decimal yearFraction, decimal rate, decimal discountFactor)
        {
            return AFMADiscountedCouponExpectedValue(notional, yearFraction, rate) * discountFactor;
        }

        #endregion

        #region AFMA Discounted Floating Coupon Analytic

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedFloatingCouponDeltaR(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)//TODO
        {
            return ISDADiscountedFloatingCouponDeltaR(notional, yearFraction, forecastRate, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedFloatingCouponDelta0(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)
        {
            var result = ISDADiscountedFloatingCouponDelta0(notional, yearFraction, forecastRate, discountFactor);
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedFloatingCouponGamma0(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)
        {
            return ISDADiscountedFloatingCouponGamma0(notional, yearFraction, forecastRate, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedFloatingCouponGammaX(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)//TODO
        {
            return ISDADiscountedFloatingCouponGammaX(notional, yearFraction, forecastRate, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedFloatingCouponAccrualFactor(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)
        {
            return ISDADiscountedFloatingCouponAccrualFactor(notional, yearFraction, forecastRate, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="curveYearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedFloatingCouponDelta1(decimal notional, decimal yearFraction, decimal forecastRate, decimal curveYearFraction, decimal discountFactor)
        {
            return ISDADiscountedFloatingCouponDelta1(notional, yearFraction, forecastRate, curveYearFraction, discountFactor);//TODO Not correct
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        public static Decimal AFMADiscountedFloatingCouponDelta1CCR(decimal notional, decimal yearFraction, decimal forecastRate, decimal curveYearFraction, decimal discountFactor)
        {
            return ISDADiscountedFloatingCouponDelta1CCR(notional, yearFraction, forecastRate, curveYearFraction, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="curveYearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedFloatingCouponGamma1(decimal notional, decimal yearFraction, decimal forecastRate, decimal curveYearFraction, decimal discountFactor)
        {
            var result = ISDADiscountedFloatingCouponGamma1(notional, yearFraction, forecastRate, curveYearFraction, discountFactor);
            return result;
        }

        #endregion
 
        #region AFMA Discounted Fixed Coupon Analytic

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedFixedCouponDeltaR(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)
        {
            return AFMADiscountedFloatingCouponDeltaR(notional, yearFraction, forecastRate, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedFixedCouponDelta0(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)
        {
            const decimal result = 0.0m;
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedFixedCouponGamma0(decimal notional, decimal yearFraction, decimal discountFactor)
        {
            return 0.0m;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedFixedCouponGammaX(decimal notional, decimal yearFraction, decimal discountFactor)//TODO
        {
            return 0.0m;
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedFixedCouponAccrualFactor(decimal notional, decimal yearFraction, decimal forecastRate, decimal discountFactor)
        {
            return AFMADiscountedFixedCouponDeltaR(notional, yearFraction, forecastRate, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="curveYearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedFixedCouponDelta1(decimal notional, decimal yearFraction, decimal forecastRate, decimal curveYearFraction, decimal discountFactor)
        {
            return ISDADiscountedFloatingCouponDelta1(notional, yearFraction, forecastRate, curveYearFraction, discountFactor);//TODO Not correct
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        public static Decimal AFMADiscountedFixedCouponDelta1CCR(decimal notional, decimal yearFraction, decimal forecastRate, decimal curveYearFraction, decimal discountFactor)
        {
            return ISDADiscountedFloatingCouponDelta1CCR(notional, yearFraction, forecastRate, curveYearFraction, discountFactor);
        }

        ///<summary>
        ///</summary>
        ///<param name="notional"></param>
        ///<param name="yearFraction"></param>
        ///<param name="forecastRate"></param>
        ///<param name="curveYearFraction"></param>
        ///<param name="discountFactor"></param>
        ///<returns></returns>
        public static Decimal AFMADiscountedFixedCouponGamma1(decimal notional, decimal yearFraction, decimal forecastRate, decimal curveYearFraction, decimal discountFactor)
        {
            var result = ISDADiscountedFloatingCouponGamma1(notional, yearFraction, forecastRate, curveYearFraction, discountFactor);
            return result;
        }

        #endregion  
    }
}
