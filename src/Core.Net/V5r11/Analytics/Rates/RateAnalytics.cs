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

#region Usings

using System;
using System.Collections.Generic;
using Highlander.Codes.V5r3;
using Math=System.Math;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Rates
{
    /// <summary>
    /// Rate analytics functions.
    /// </summary>
    public static class RateAnalytics
    {
        private const double Tolerance = 0.000001;

        private static double GetCompoundingPeriod(CompoundingFrequencyEnum compoundingFrequencyEnum)
        {
            double frequency;
            switch (compoundingFrequencyEnum)
            {
                case CompoundingFrequencyEnum.Continuous:
                    frequency = 0;
                    break;
                case CompoundingFrequencyEnum.Daily:
                    frequency = 1 / 365d;
                    break;
                case CompoundingFrequencyEnum.Weekly:
                    frequency = 1 / 52d;
                    break;
                case CompoundingFrequencyEnum.Monthly:
                    frequency = 1 / 12d;
                    break;
                case CompoundingFrequencyEnum.Quarterly:
                    frequency = 0.25;
                    break;
                case CompoundingFrequencyEnum.SemiAnnual:
                    frequency = 0.5;
                    break;
                case CompoundingFrequencyEnum.Annual:
                    frequency = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(compoundingFrequencyEnum), "CompoundingFrequency is of an invalid type " + compoundingFrequencyEnum);
            }
            return frequency;
        }

        /// <summary>
        /// Discount rate derivative for analytical bucketing.
        /// </summary>
        /// <param name="discountFactorBucketStartDate">The discount factor to the bucket start date</param>
        /// <param name="discountFactorFlowDate">The flow date should be after the bucket start date.</param>
        /// <param name="flowYearFraction">The year fraction between the bucket start date and the flow date</param>
        /// <param name="discountFactorBucketEndDate">The discount factor to the bucket end date</param>
        /// <param name="bucketYearFraction">The bucket year fraction.</param>
        /// <returns></returns>
        public static decimal DeltaDiscFacWrtR(decimal flowYearFraction, decimal discountFactorFlowDate,
            decimal discountFactorBucketStartDate, decimal discountFactorBucketEndDate, decimal bucketYearFraction)
        {
            if (flowYearFraction < 0)
                return 0;
            if (flowYearFraction > bucketYearFraction)
            {
                var result = -bucketYearFraction *
                              discountFactorBucketEndDate /      // D(j,A,r) 
                              discountFactorBucketStartDate *    // D(i,A,r) 
                              discountFactorFlowDate;           // D(t,A,r) 
                return result;
            }
            return -bucketYearFraction * discountFactorFlowDate * discountFactorFlowDate /
                       discountFactorBucketEndDate *              // D(i,A,r)
                    flowYearFraction / bucketYearFraction;      //(t-i)/(j-i)
        }

        /// <summary>
        /// A single index accruing over a coupon period.
        /// </summary>
        /// <param name="discountFactorIndexDates">This must include the index start date and end date. </param>
        /// <param name="indexCouponYearFractions">The year fractions between coupon dates. Te first number is between the index start date and the first coupon flow.</param>
        /// <param name="discountFactorBucketStartDate">The discount factor to the bucket start date.</param>
        /// <param name="discountFactorBucketEndDate">The discount factor to the bucket end date.</param>
        /// <param name="bucketYearFraction">The bucket year fraction.</param>
        /// <param name="indexRate">The underlying rate of the index.</param>
        /// <returns></returns>
        public static decimal DeltaCouponIndexWrtR(List<decimal> discountFactorIndexDates,
            List<decimal> indexCouponYearFractions, decimal discountFactorBucketStartDate,
            decimal discountFactorBucketEndDate, decimal bucketYearFraction, decimal indexRate)
        {
            decimal sumDIdr = 0;
            var arraySize = indexCouponYearFractions.Count; // indexDateList.Count - 1 ;  
            //	Define multipliers
            //	1.	D_StartMinusEnd = (D(1,B,r)-D(N+1,B,r))
            var dStartMinusEnd = discountFactorIndexDates[0] - discountFactorIndexDates[discountFactorIndexDates.Count - 1];
            //	2.	I=I/(D(1,B,r)-D(N+1,B,r)) 
            var I = indexRate / dStartMinusEnd;
            //	3.	I=I*I/(D(1,B,r)-D(N+1,B,r))
            var squared = I * indexRate;
            for (var i = 0; i < arraySize; i++)
            {
                var flowYearFraction = 0.0m;
                flowYearFraction = indexCouponYearFractions[i] + flowYearFraction;
                if (i == 0)
                {
                    sumDIdr = sumDIdr + DeltaDiscFacWrtR(flowYearFraction, discountFactorIndexDates[i], discountFactorBucketStartDate, discountFactorBucketEndDate, bucketYearFraction) * I;
                }
                else if (i == arraySize)
                {
                    sumDIdr = sumDIdr - DeltaDiscFacWrtR(flowYearFraction, discountFactorIndexDates[i], discountFactorBucketStartDate, discountFactorBucketEndDate, bucketYearFraction) *
                             (indexCouponYearFractions[i - 1] * squared + I);
                }
                else
                {
                    sumDIdr = sumDIdr - DeltaDiscFacWrtR(flowYearFraction, discountFactorIndexDates[i], discountFactorBucketStartDate, discountFactorBucketEndDate, bucketYearFraction) *
                             (indexCouponYearFractions[i - 1] * squared);
                }
            }
            return sumDIdr;
        }

        /// <summary>
        /// The derivative with respect to the rate.
        /// </summary>
        /// <param name="discountFactorFlowDate">The discount factor to the flow rate.</param>
        /// <param name="flowYearFraction">The year fraction to the flow date.</param>
        /// <param name="discountFactorIndexStartDate">The discount factor to the index start date. The index might be LIBOR or a Swap.</param>
        /// <param name="discountFactorIndexEndDate">The discount factor to the index end date. </param>
        /// <param name="indexYearFraction">The index year fraction. For example, this could be 0.35 or 3.0</param>
        /// <param name="bucketYearFraction">The bucket year fraction.</param>
        /// <param name="discountFactorBucketStartDate">The discount factor to the bucket start date.</param>
        /// <param name="discountFactorBucketEndDate">The discount factor to the bucket end date.</param>
        /// <returns></returns>
        public static decimal DeltaIndexWrtR(decimal discountFactorFlowDate, decimal flowYearFraction, decimal discountFactorIndexStartDate, decimal discountFactorIndexEndDate,
            decimal indexYearFraction, decimal bucketYearFraction, decimal discountFactorBucketStartDate, decimal discountFactorBucketEndDate)
        {
            // dD(x,B,r)/dr
            var derivativeOfX = DeltaDiscFacWrtR(flowYearFraction, discountFactorFlowDate, discountFactorBucketStartDate, discountFactorBucketEndDate, bucketYearFraction);
            // dD(y,B,r)/dr  
            var derivativeOfY = DeltaDiscFacWrtR(flowYearFraction, discountFactorFlowDate, discountFactorBucketStartDate, discountFactorBucketEndDate, bucketYearFraction);
            // Daycount(indexStart, indexEnd, indexDaycount, indexCoupon);
            var days = indexYearFraction;//TODO Is this days or year fraction?
            // D(x,B,r)
            var dfx = discountFactorIndexStartDate;
            // D(y,B,r) 
            var dfy = discountFactorIndexEndDate;
            return 1 / days * 1 / dfy * derivativeOfX - 1 / days * 1 / dfy * dfx / dfy * derivativeOfY;
        }

        /// <summary>
        /// Gets the derivative of the discount factor wrt the zero rate..
        /// </summary>
        /// <param name="discountFactor">The relevant df.</param>
        /// <param name="yearFraction">The year fraction.</param>
        /// <param name="rate">the rate.</param>
        /// <returns></returns>
        public static decimal GetDeltaZeroRate(decimal discountFactor, decimal yearFraction, decimal rate)
        {
            return discountFactor / (1.0m + yearFraction * rate);
        }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <param name="startDiscountFactor">The start df.</param>
        /// <param name="yearFraction">The year fraction.</param>
        /// <param name="rate">the rate.</param>
        /// <returns></returns>
        public static decimal GetDiscountFactorAtMaturity(decimal startDiscountFactor, decimal yearFraction, decimal rate)
        {
            return startDiscountFactor / (1.0m + yearFraction * rate); 
        }

        /// <summary>
        /// Gets the rate.
        /// </summary>
        /// <param name="startDiscountFactor">The start discount factor.</param>
        /// <param name="endDiscountFactor">The end discount factor.</param>
        /// <param name="yearFraction">The year fraction.</param>
        /// <returns></returns>
        public static decimal GetRate(decimal startDiscountFactor, decimal endDiscountFactor, decimal yearFraction)
        {
            if (yearFraction != 0)
            {
                return (startDiscountFactor/endDiscountFactor - 1.0m)/yearFraction;
            }
            return 0.0m;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        public static decimal GetDelta1(decimal npv, decimal curveYearFraction, decimal rate, decimal yearFraction)
        {
            return npv * curveYearFraction / (1 + yearFraction * rate) / 10000;
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        public static decimal GetDeltaCcr(decimal npv, decimal curveYearFraction, decimal rate, decimal yearFraction)
        {
            return npv * curveYearFraction / 10000;
        }

        ///<summary>
        ///</summary>
        ///<param name="zeroRate"></param>
        ///<param name="yearFraction"></param>
        ///<param name="compoundingPeriod"></param>
        ///<returns></returns>
        public static double ZeroRateToDiscountFactor(double zeroRate, double yearFraction,
                                                      double compoundingPeriod)
        {
            double result;
            if (compoundingPeriod == 0)
            {
                result = Math.Exp(zeroRate * -yearFraction);
            }
            else
            {
                double baseNumber = 1 + zeroRate * compoundingPeriod;
                double power = -yearFraction / compoundingPeriod;
                result = Math.Pow(baseNumber, power);
            }
            return result;
        }

        /// <summary>
        /// Converts from the zero rate to a terminal wealth.
        /// </summary>
        /// <param name="zeroRate"></param>
        /// <param name="yearFraction"></param>
        /// <param name="compoundingPeriod"></param>
        /// <returns></returns>
        public static decimal ZeroRateToDiscountFactor(decimal zeroRate, decimal yearFraction, decimal compoundingPeriod)
        {
            double result = ZeroRateToDiscountFactor((double)zeroRate, (double)yearFraction, (double)compoundingPeriod);
            return Convert.ToDecimal(result);
        }

        ///<summary>
        ///</summary>
        ///<param name="zeroRate"></param>
        ///<param name="yearFraction"></param>
        ///<param name="compoundingFrequency"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public static double ZeroRateToDiscountFactor(double zeroRate, double yearFraction, string compoundingFrequency)
        {
            return ZeroRateToDiscountFactor(zeroRate, yearFraction, EnumParse.ToCompoundingFrequencyEnum(compoundingFrequency));
        }
        ///<summary>
        ///</summary>
        ///<param name="zeroRate"></param>
        ///<param name="yearFraction"></param>
        ///<param name="compoundingFrequencyEnum"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public static double ZeroRateToDiscountFactor(double zeroRate, double yearFraction,
                                                      CompoundingFrequencyEnum compoundingFrequencyEnum)
        {
            double compoundingPeriod = GetCompoundingPeriod(compoundingFrequencyEnum);
            return ZeroRateToDiscountFactor(zeroRate, yearFraction, compoundingPeriod);
        }

        ///<summary>
        /// Converts a discount factor to a compounding zero rate.
        ///</summary>
        ///<param name="yearFraction">The year Fraction.</param>
        ///<param name="discountFactor">The discount factor.</param>
        ///<param name="frequency">The compounding frequency. Can take: Continuous, Daily, Quarterly,
        /// Semi-Annual,SemiAnnual,Semi and Annual</param>
        ///<returns>The compounded zero rate requested.</returns>
        public static double DiscountFactorToZeroRate(double discountFactor, double yearFraction, string frequency)
        {
            var compounding = EnumParse.ToCompoundingFrequencyEnum(frequency);
            double result = DiscountFactorToZeroRate(discountFactor, yearFraction, compounding);
            return result;
        }

        /// <summary>
        /// Converts a discount factor to a compounding zero rate.
        /// </summary>
        /// <param name="startDiscountFactor"></param>
        /// <param name="endDiscountFactor"></param>
        /// <param name="yearFraction"></param>
        /// <param name="compoundingPeriod"></param>
        /// <returns></returns>
        public static decimal DiscountFactorToZeroRate(decimal startDiscountFactor, decimal endDiscountFactor,
            decimal yearFraction, decimal compoundingPeriod)
        {
            double discountFactor = Convert.ToDouble(endDiscountFactor / startDiscountFactor);
            double result = DiscountFactorToZeroRate(discountFactor, (double)yearFraction, (double)compoundingPeriod);
            return Convert.ToDecimal(result);
        }

        /// <summary>
        /// Converts a discount factor to a compounding zero rate.
        /// </summary>
        ///<param name="discountFactor"></param>
        ///<param name="yearFraction"></param>
        ///<param name="compoundingFrequencyEnum"></param>
        ///<returns></returns>
        public static double DiscountFactorToZeroRate(double discountFactor, double yearFraction,
                                                      CompoundingFrequencyEnum compoundingFrequencyEnum)
        {
            double compoundingPeriod = GetCompoundingPeriod(compoundingFrequencyEnum);
            return DiscountFactorToZeroRate(discountFactor, yearFraction, compoundingPeriod);
        }

        /// <summary>
        /// Converts a discount factor to a compounding zero rate.
        /// </summary>
        ///<param name="discountFactor"></param>
        ///<param name="yearFraction"></param>
        ///<param name="compoundingPeriod"></param>
        ///<returns></returns>
        public static double DiscountFactorToZeroRate(double discountFactor, double yearFraction,
                                                      double compoundingPeriod)
        {
            double result;
            if (compoundingPeriod == 0)
            {
                result = -Math.Log(discountFactor) / yearFraction;
            }
            else
            {
                double power = -compoundingPeriod / yearFraction;
                result = (Math.Pow(discountFactor, power) - 1.0) / compoundingPeriod;
            }
            return result;
        }

        /// <summary>
        /// Converts a discount factor to a compounding zero rate.
        /// </summary>
        ///<param name="startDiscountFactor"></param>
        ///<param name="yearFraction"></param>
        ///<param name="endDiscountFactor"></param>
        ///<returns></returns>
        public static double DiscountFactorsToForwardRate(double startDiscountFactor, double endDiscountFactor,
            double yearFraction)
        {
            double result = 0;
            if (Math.Abs(yearFraction) > Tolerance)
            {
                result = (startDiscountFactor / endDiscountFactor - 1) / yearFraction;
            }
            return result;
        }

        /// <summary>
        /// Converts from the zero rate to a terminal wealth.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="yearFraction"></param>
        /// <param name="compoundingFrequency"></param>
        /// <returns></returns>
        public static decimal TerminalWealthFromZeroRate(decimal rate, decimal yearFraction,
            string compoundingFrequency)
        {
            return TerminalWealthFromZeroRate(rate, yearFraction,
                                              EnumParse.ToCompoundingFrequencyEnum(compoundingFrequency));
        }

        /// <summary>
        /// Converts from the zero rate to a terminal wealth.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="yearFraction"></param>
        /// <param name="compoundingFrequency"></param>
        /// <returns></returns>
        public static decimal TerminalWealthFromZeroRate(decimal rate, decimal yearFraction,
            CompoundingFrequencyEnum compoundingFrequency)
        {
            double compoundingPeriod = GetCompoundingPeriod(compoundingFrequency);
            decimal result;
            if (compoundingPeriod == 0)
            {
                result = (decimal)Math.Exp(Convert.ToDouble(-rate * yearFraction));
            }
            else
            {
                decimal df;

                if ((double)yearFraction > compoundingPeriod)
                {
                    df = (decimal)Math.Pow(1 + compoundingPeriod * (double)rate, (double)yearFraction);
                }
                else
                {
                    df = 1 + yearFraction * rate;
                }
                result = 1 / df;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentDate">Contract payment time, in quarters.</param>
        /// <param name="settleDate">Delivery time, in quarters.</param>
        /// <param name="discount">Discount factors used.</param>
        /// <returns></returns>
        public static double ForwardContract(int paymentDate, int settleDate, QuarterlyDiscounts discount)
        {
            return discount.Get(settleDate) / discount.Get(paymentDate);
        }

        /// <summary>
        /// Computes the quarterly cash forward rate for a specific future time.
        /// </summary>
        /// <param name="forwardDate"></param>
        /// <param name="discount">Discount factors used.</param>
        /// <returns></returns>
        public static double CashForwardRate(int forwardDate, QuarterlyDiscounts discount)
        {
            double rate = 4 * (discount.Get(forwardDate) / discount.Get(forwardDate + 1) - 1);
            return rate;
        }

        /// <summary>
        /// Computes the shifted quarterly cash forward rate for a specific future time.
        /// </summary>
        /// <param name="forwardDate"></param>
        /// <param name="discount">Discount factors used.</param>
        /// <param name="shift">Shifts used.</param>
        /// <returns></returns>
        public static double ShiftedCashForwardRate(int forwardDate, QuarterlyDiscounts discount, QuarterlyShifts shift)
        {
            double rate = 4 * (discount.Get(forwardDate) / discount.Get(forwardDate + 1) - 1);
            rate += shift.Get(forwardDate);
            return rate;
        }

        /// <summary>
        /// Computes the swap rate of a quarterly swap.
        /// </summary>
        /// <param name="expiry">Expiry time in quarters.</param>
        /// <param name="tenor">Length of the swap in quarters.</param>
        /// <param name="discount">Discount factors used.</param>
        /// <returns></returns>
        public static double SwapRate(int expiry, int tenor, QuarterlyDiscounts discount)
        {
            double numerator = 0;
            double denominator = 0;
            for (int i = 0; i < tenor; i++)
            {
                double forward = ForwardContract(expiry, expiry + i + 1, discount);
                numerator += forward * CashForwardRate(expiry + i, discount);
                denominator += forward;
            }
            return numerator / denominator;
        }

        /// <summary>
        /// Computes the shifted swap rate of a quarterly swap.
        /// </summary>
        /// <param name="expiry">Expiry time in quarters.</param>
        /// <param name="tenor">Length of the swap in quarters.</param>
        /// <param name="discount">Discount factors used.</param>
        /// <param name="shift">Shifts used.</param>
        /// <returns></returns>
        public static double ShiftedSwapRate(int expiry, int tenor, QuarterlyDiscounts discount, QuarterlyShifts shift)
        {
            double numerator = 0;
            double denominator = 0;
            for (int i = 0; i < tenor; i++)
            {
                double forward = ForwardContract(expiry, expiry + i + 1, discount);
                numerator += forward * ShiftedCashForwardRate(expiry + i, discount, shift);
                denominator += forward;
            }
            return numerator / denominator;
        }

        /// <summary>
        /// The "shift" of the swap rate, or shifted swap rate - actual swap rate.
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="tenor"></param>
        /// <param name="discount"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static double SwapShift(int expiry, int tenor, QuarterlyDiscounts discount, QuarterlyShifts shift)
        {
            double numerator = 0;
            double denominator = 0;
            for (int i = 0; i < tenor; i++)
            {
                double forward = ForwardContract(expiry, expiry + i + 1, discount);
                numerator += forward * shift.Get(expiry + i);
                denominator += forward;
            }
            return numerator / denominator;
        }
    }
}
