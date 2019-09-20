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

#region Using Directives

using System;
using Highlander.Numerics.Dates;
using Highlander.Numerics.Solvers;

#endregion

namespace Highlander.Numerics.Rates
{
    /// <summary>
    /// A bond analytics function class.
    /// </summary>
    public static class BondAnalytics
    {
        /// <summary>
        /// Formulates the comp frequency type.
        /// </summary>
        /// <param name="compFreq"></param>
        /// <returns></returns>
        public static string FormatCompFreq(short compFreq)
        {
            switch (compFreq)
            {
                case 0:   return "Continuous";
                case 1:   return "Monthly";
                case 2:   return "Bi-monthly";
                case 3:   return "Qly";
                case 4:   return "Tri-annual";
                case 6:   return "Semi-ann";
                case 12:  return "Annual";
                case 360: return "Daily (360 day year)";
                case 365: return "Daily (365 day year)";
                default:  return "Unknown comp freq";
            }
        }

        /// <summary>
        /// Parses the com frequency token.
        /// </summary>
        /// <param name="compFreq"></param>
        /// <returns></returns>
        public static short ParseCompFreq(string compFreq)
        {
            switch (compFreq.Substring(0,1).ToUpper())
            {
                case "C": return 0; // continuous compounding
                case "M": return 1;
                case "B": return 2;
                case "Q": return 3;
                case "T": return 4;
                case "S": return 6;
                case "A": return 12;
                case "D360":return 360;
                case "D365":return 365;
                default: return 0;
            }
        }

        /// <summary>
        /// Yield calculation method
        /// </summary>
        public enum YieldCalcMethod
        {
            /// <summary>yield calculated by standard (ISMA) method</summary>
            YCM_ISMA = 2,
            /// <summary>US street convention</summary>
            YCM_US_street,
            /// <summary>Japanese simple yield"</summary>
            YCM_Japanese,
            /// <summary>"Moosmuller yield calculation. Same as US Treasury method."</summary>
            YCM_Moosmuller,
            /// <summary>'True' yield calculation</summary>
            YCM_true = 6,
            /// <summary>Consortium yield calculation</summary>
            YCM_consortium = 7,
            /// <summary>'True' yield calculation, based on nominal (unadjusted) dates</summary>
            YCM_true_unadj,
            /// <summary>Yield of discount bill</summary>
            YCM_discount
        }

        /// <summary>
        /// Parses a string into a yield calc method.
        /// </summary>
        /// <param name="ycm"></param>
        /// <returns></returns>
        public static YieldCalcMethod Parse(string ycm)
        {
            switch (ycm.ToLower())
            {
                case "isma":
                case "aibd":
                    return YieldCalcMethod.YCM_ISMA;
                case "jap":
                case "japanese":
                case "japan":
                case "simple":
                    return YieldCalcMethod.YCM_Japanese;
                case "us":
                case "treas":
                case "reasury":
                case "moos":
                case "moosmuller":
                    return YieldCalcMethod.YCM_Moosmuller;
                case "true":
                    return YieldCalcMethod.YCM_true;
                case "trueU":
                    return YieldCalcMethod.YCM_true_unadj;
                case "discount":
                    return YieldCalcMethod.YCM_discount;
                case "consortium":
                    return YieldCalcMethod.YCM_consortium;
                default:
                    return YieldCalcMethod.YCM_Moosmuller;
            }
        }

        /// <summary>
        /// Formats a yield calc method.
        /// </summary>
        /// <param name="ycm"></param>
        /// <returns></returns>
        public static string FormatYieldCalcMethod(YieldCalcMethod ycm)
        {
            switch (ycm)
            {
                case YieldCalcMethod.YCM_ISMA: return "ISMA";
                case YieldCalcMethod.YCM_US_street: return "US Street";
                case YieldCalcMethod.YCM_Japanese: return "Japanese";
                case YieldCalcMethod.YCM_Moosmuller: return "Moosmuller";
                case YieldCalcMethod.YCM_true: return "True";
                case YieldCalcMethod.YCM_consortium: return "Consortium";
                case YieldCalcMethod.YCM_true_unadj: return "True (Unadj)";
                case YieldCalcMethod.YCM_discount: return "Discount";
                default: return "";
            }
        }

        /// <summary>
        /// ConvertCompFreq
        /// </summary>
        /// <param name="toCompFreq"></param>
        /// <param name="fromCompFreq"></param>
        /// <param name="yield"></param>
        /// <returns></returns>
        public static double ConvertCompFreq(short toCompFreq, short fromCompFreq, double yield)
        {
            double fromH;
            if (toCompFreq == fromCompFreq) return yield;
            if (toCompFreq == 0) // convert to cc yield
            {
                fromH = fromCompFreq <= 12 ? 12 / fromCompFreq : fromCompFreq;
                return 100 * fromH * Math.Log(1 + yield / (100 * fromH));
            }
            double toH = toCompFreq <= 12 ? 12 / toCompFreq : toCompFreq;
            if (fromCompFreq == 0) // convert from CC yield
                return 100 * toH * (Math.Exp(yield / (100 * toH)) - 1);
            fromH = fromCompFreq <= 12 ? 12 / fromCompFreq : fromCompFreq;
            return 100 * toH * (Math.Pow(1 + yield / (100 * fromH), fromH / toH) - 1);
        }

        /// <summary>
        /// ConvertCompFreqSens
        /// </summary>
        /// <param name="toCompFreq"></param>
        /// <param name="fromCompFreq"></param>
        /// <param name="yield"></param>
        /// <returns></returns>
        public static double ConvertCompFreqSens(short toCompFreq, short fromCompFreq, double yield)
        {
            double fromH;
            if (toCompFreq == fromCompFreq) return 1;
            if (toCompFreq == 0) // convert to cc yield
            {
                fromH = fromCompFreq <= 12 ? 12 / fromCompFreq : fromCompFreq;
                return 1 / (1 + yield / (100 * fromH));
            }
            double toH = toCompFreq <= 12 ? 12 / toCompFreq : toCompFreq;
            if (fromCompFreq == 0) return Math.Exp(yield / (100 * toH)); // convert from CC yield    
            fromH = fromCompFreq <= 12 ? 12 / fromCompFreq : fromCompFreq;
            return Math.Pow(1 + yield / (100 * fromH), fromH / toH - 1);
        }

        /// <summary>
        /// CountCoupPdsEx
        /// </summary>
        /// <param name="ys"></param>
        /// <param name="ms"></param>
        /// <param name="rollDay"></param>
        /// <param name="ye"></param>
        /// <param name="me"></param>
        /// <param name="de"></param>
        /// <param name="coupFreq"></param>
        /// <returns></returns>
        public static int CountCoupPdsEx(int ys, int ms, int rollDay, int ye, int me, int de, int coupFreq)
        {
            int d1 = Math.Min(new DateTime(ye, me + 1, 0).Day, rollDay);
            int i = 12 * (ye - ys) + me - ms;
            if (i > 0 && de <= d1) i--;
            else if (i < 0 && de >= d1) i++;
            return i / coupFreq;
        }

        /// <summary>
        /// AnnDF
        /// </summary>
        /// <param name="yield"></param>
        /// <param name="compFreq"></param>
        /// <returns></returns>
        public static double AnnDF(double yield, int compFreq)
        {
            if (compFreq == 0) return Math.Exp(-yield / 100);
            int h = compFreq <= 12 ? 12 / compFreq : compFreq;
            return Math.Pow(1 + yield / (100 * h), -h);
        }

        /// <summary>
        /// PeriodDF
        /// </summary>
        /// <param name="coupFreq"></param>
        /// <param name="yield"></param>
        /// <param name="compFreq"></param>
        /// <returns></returns>
        public static double PeriodDF(int coupFreq, double yield, int compFreq)
        {
            if (compFreq == 0) return Math.Exp(-yield * coupFreq / 1200);
            var h = compFreq <= 12 ? 12 / compFreq : compFreq;
            return Math.Pow(1 + yield / (100 * h), -h * coupFreq / 12.0);
        }

        /// <summary>
        /// FloorDiv
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int FloorDiv(int x, int y) { return (x >= 0 ? x : x - y + 1) / y; }

        /// <summary>
        /// Sets the simple bond parameters for yield calculations.
        /// </summary>
        /// <param name="coupon">A coupon amount.</param>
        /// <param name="timeToPayment">The times betweeb settlement date and maturity.</param>
        /// <param name="a">An out paramter.</param>
        /// <param name="b">An out parameter.</param>
        public static void SimpleYieldParameters(Double coupon, Double timeToPayment, out Double a, out Double b)
        {
            var c = coupon;
            const double r = 1.0;
            a = -100 / timeToPayment;
            b = 100 * (c + r / timeToPayment);
        }

        /// <summary>
        /// IsmaNPV. Calc of bond dirty price. If last coup is regular then set k = 0 & cf = 0.
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c"></param>
        /// <param name="cf"></param>
        /// <param name="r"></param>
        /// <param name="q"></param>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double IsmaNPV(double c1, double c, double cf, double r, double q, short n,
                                     double k, double v)
        {
            /* returns: v^q * (c1 + c * (v + v^2 + ... + v^n) + (R + cf) * v^(n + k))
               where v is the discount factor for a single coupon period */
            if (n < 0) return Math.Pow(v, q) * (r + cf); // in final irregular coupon pd
            if (v == 1) return c1 + c * n + r + cf;
            var vn = Math.Pow(v, n);
            var vnk = k == 0 ? vn : Math.Pow(v, n + k);
            return Math.Pow(v, q) * (c1 + c * v * (1 - vn) / (1 - v) + (r + cf) * vnk);
        }

        /// <summary>
        /// Isma dirty price calculation.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="numFlows">The number of flows.</param>
        /// <param name="numCoupons">The number of coupons.</param>
        /// <param name="lastRegCouponDate">The last regular ccoupon date if provided.</param>
        /// <param name="flowAmounts">An array of flow amount.</param>
        /// <param name="finalCoupTimeFrac">The final coupon time fraction.</param>
        /// <param name="q"></param>
        /// <param name="dpDV"></param>
        /// <param name="d2PDV2"></param>
        /// <returns>The dirty price.</returns>
        public static double IsmaNPV1(double v, int numFlows, int numCoupons, DateTime? lastRegCouponDate, double[] flowAmounts, double finalCoupTimeFrac, double q, out double dpDV, out double d2PDV2)
        {
            var vq = Math.Pow(v, q);
            var p = NextCoupDP(v, numFlows, numCoupons, lastRegCouponDate, flowAmounts, finalCoupTimeFrac, out dpDV, out d2PDV2) * vq;
            double t = q * p / v, u = vq * dpDV;
            dpDV = u + t;
            d2PDV2 = ((q - 1) * t + 2 * q * u) / v + vq * d2PDV2;
            return p;
        }

        /// <summary>
        /// The NextCoupDP.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="numFlows"></param>
        /// <param name="numCoupons"></param>
        /// <param name="lastRegCouponDate"></param>
        /// <param name="flowAmounts"></param>
        /// <param name="finalCoupTimeFrac"></param>
        /// <param name="dpDV"></param>
        /// <param name="d2PDV2"></param>
        /// <returns></returns>
        public static double NextCoupDP(double v, int numFlows, int numCoupons, DateTime? lastRegCouponDate, double[] flowAmounts, double finalCoupTimeFrac, out double dpDV, out double d2PDV2)
        {
            int i = numFlows - 1;
            double dirtyPrice = flowAmounts[i];
            if (lastRegCouponDate !=null && numCoupons < i)
            {
                double k = finalCoupTimeFrac;
                dirtyPrice *= Math.Pow(v, k);
                dpDV = dirtyPrice * k / v;
                d2PDV2 = (k - 1) * dpDV / v;
                dirtyPrice += flowAmounts[--i];
            }
            else dpDV = d2PDV2 = 0;
            while (i > numCoupons)
            {
                d2PDV2 = v * d2PDV2 + 2 * dpDV;
                dpDV = v * dpDV + dirtyPrice;
                dirtyPrice = v * dirtyPrice + flowAmounts[--i];
            }
            return dirtyPrice;
        }

        /// <summary>
        /// BondMoosNPV
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c"></param>
        /// <param name="cf"></param>
        /// <param name="r"></param>
        /// <param name="q"></param>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <param name="h"></param>
        /// <param name="y"></param>
        /// <returns></returns>]
        public static double BondMoosNPV(double c1, double c, double cf, double r, double q, short n,
                                         double k, short h, double y)
        {
            /* returns: (c1 + c * (v + v^2 + ... + v^n) + (R + cf) * v^(n + k)) / (1 + q * y / (100.H))
               where v is the discount factor for a single coupon period */
            var v = 1 / (1 + y / (100 * h));
            var mmf = 1 / (1 + q * y / (100 * h));
            if (n < 0) return mmf * (r + cf); // in final irregular coupon pd
            if (y == 0) return c1 + c * n + r + cf;
            var vn = Math.Pow(v, n);
            var vnk = k == 0 ? vn : Math.Pow(v, n + k);
            return mmf * (c1 + c * v * (1 - vn) / (1 - v) + (r + cf) * vnk);
        }

        /// <summary>
        /// IsmaPVBP
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c"></param>
        /// <param name="cf"></param>
        /// <param name="r"></param>
        /// <param name="q"></param>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double IsmaPVBP(double c1, double c, double cf, double r, double q, short n,
                                      double k, double v)
        {
            // return d(IsmaNPV)/dv
            if (n < 0) return q * Math.Pow(v, q - 1) * (r + cf); // in final irregular coupon pd
            if (v == 1) return q * c1 + n * c * (q + .5 * (n + 1)) + (r + cf) * (q + n + k);
            double vn = Math.Pow(v, n), vq = Math.Pow(v, q), v1 = 1 - v;
            return q * c1 * vq / v + c * vq / v1 * ((q + 1) * (1 - vn) - n * vn) + c * vq * v * (1 - vn) / (v1 * v1) +
                   (r + cf) * (q + n + k) * Math.Pow(v, q + n + k - 1);
        }

        /// <summary>
        /// BondMoosPVBP
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c"></param>
        /// <param name="cf"></param>
        /// <param name="r"></param>
        /// <param name="q"></param>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <param name="h"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double BondMoosPVBP(double c1, double c, double cf, double r, double q, short n,
                                          double k, short h, double y)
        {
            // returns dP/dY for the Moosmuller yield calc method 
            double v = 1 / (1 + y / (100 * h)), mmf = 1 / (1 + q * y / (100 * h));
            if (n < 0) return -mmf * mmf * q * (r + cf) / (100 * h); // in final irregular coupon pd
            return -mmf * (v * v * IsmaPVBP(c1, c, cf, r, 0, n, k, v) +
                           q * mmf * IsmaNPV(c1, c, cf, r, 0, n, k, v)) / (100 * h);
        }

        /// <summary>
        /// d2Pdv2
        /// </summary>
        /// <param name="q"></param>
        /// <param name="c1"></param>
        /// <param name="v"></param>
        /// <param name="c"></param>
        /// <param name="n"></param>
        /// <param name="R"></param>
        /// <param name="cf"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static double d2Pdv2(double q, double c1, double v, double c, int n, double R, double cf,
                                    double k)
        {
            if (n == -1) return q * (q - 1) * Math.Pow(v, q - 2) * (R + cf); // final irreg coup pd
            if (v == 1) return q * (q - 1) * c1 + (R + cf) * (q + n + k) * (q + n + k - 1) +
                               c * (n * (n * n + 3 * q * (q + n) - 4) + 1) / 3;
            double v1 = 1 - v, vn = Math.Pow(v, n), vn1 = 1 - vn;
            return q * (q - 1) * c1 * Math.Pow(v, q - 2) + c * Math.Pow(v, q - 1) / v1 * (q * (q + 1) * vn1 -
                                                                                          n * (2 * q + n + 1) * vn + 2 * v / v1 * ((q + 1) * vn1 - n * vn + v * vn1 / v1)) +
                   (R + cf) * (q + n + k) * (q + n + k - 1) * Math.Pow(v, q + n + k - 2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accrualYearFraction">The first accrual coupon year fraction.</param>
        /// <param name="finalAccrualYearFraction">The final coupon year fraction. If regular this is zero.</param>
        /// <param name="n">The number of full coupon periods remaining until redemption. 
        /// The numer of remaining coupon periods is therefore n+1;</param>
        /// <param name="nextCoupon">The next coupon payment. This may be partial or zero if the bond is trading ex div.</param>
        /// <param name="next2Coupon">The next 2 coupon.</param>
        /// <param name="annualCoupon">The annual coupon payment for 100 unit of face value.</param>
        /// <param name="finalCoupon">The final coupon with an odd final period. This is zero for all other bonds.</param>
        /// <param name="h">The number of coupon periods in a year.</param>
        /// <param name="v">The disoount factor for one period, 1/(1+y/h)</param>
        /// <param name="y">The required annual nominal redemption yield expressed as a decimal.</param>
        /// <returns></returns>
        public static double ISMADirtyPrice(double accrualYearFraction, double finalAccrualYearFraction, int n, 
                                            double nextCoupon, double next2Coupon, double annualCoupon, double finalCoupon, int h, double v, double y)
        {
            var a = Math.Pow(v, accrualYearFraction);
            var b = Math.Pow(v, n + finalAccrualYearFraction);
            var final = (100 + finalCoupon)*b;
            var c = nextCoupon + next2Coupon*v;
            var d = annualCoupon/h*Math.Pow(v, 2)*(1 - Math.Pow(v, n - 1))/(1 - v);
            var dirtyPrice = a * (c + d + final);
            return dirtyPrice;
        }
        
        ///<summary>
        ///</summary>
        ///<param name="maturityDate">redemption date</param>
        ///<param name="lastCouponDate">last coupon date</param>
        ///<param name="couponRate">coupon rate</param>
        ///<param name="couponFrequency">coupons per year,1 for annual, 2 for semi, 4 for quarterly</param>
        ///<param name="faceValue">The face value of the bond.</param>
        ///<param name="dirtyPrice">dirty price</param>
        ///<returns></returns>
        public static double CalculateBondYTM(DateTime maturityDate, DateTime lastCouponDate, double couponRate, int couponFrequency, double faceValue, double dirtyPrice)
        {
            var c = new CalculateBondYTMObjectiveFunctionClass(maturityDate, lastCouponDate, couponRate, couponFrequency, faceValue, dirtyPrice);
            // Instantiate and initialise the equation solver.
            var solver = new Brent();

            return solver.Solve(c, .0000001, couponRate, 0.01);
        }

        ///<summary>
        ///</summary>
        public class CalculateBondYTMObjectiveFunctionClass : IObjectiveFunction
        {
            private readonly DateTime _maturityDate;
            private readonly DateTime _lastCouponDate;
            private readonly double _couponRate;
            private readonly int _couponFrequency;
            private readonly double _faceValue;
            private readonly double _dirtyPrice;

            ///<summary>
            ///</summary>
            ///<param name="maturityDate"></param>
            ///<param name="lastCouponDate"></param>
            ///<param name="couponRate"></param>
            ///<param name="couponFrequency"></param>
            ///<param name="faceValue"></param>
            ///<param name="dirtyPrice"></param>
            public CalculateBondYTMObjectiveFunctionClass(DateTime maturityDate, DateTime lastCouponDate, double couponRate, int couponFrequency, double faceValue, double dirtyPrice)
            {
                _maturityDate = maturityDate;
                _lastCouponDate = lastCouponDate;
                _couponRate = couponRate;
                _couponFrequency = couponFrequency;
                _faceValue = faceValue;
                _dirtyPrice = dirtyPrice;
            }

            ///<summary>
            ///</summary>
            ///<param name="ytm"></param>
            ///<returns></returns>
            public double Value(double ytm)
            {
                double accrualYearFraction = (DateTime.Today - _lastCouponDate).TotalDays / 365;
                //int finalAccrualYearFraction = 0;
                double finalAccrualYearFraction = (1.0 / _couponFrequency) - accrualYearFraction;
                int numberOfCouponPeriodsInAYear = _couponFrequency;
                //int i = (int)((matDate - lastCouponDate).TotalDays / 365) * numberOfCouponPeriodsInAYear;
                double i = (((_maturityDate - _lastCouponDate).TotalDays / 365.0) * numberOfCouponPeriodsInAYear);
                double nextCoupon = _couponRate * _faceValue / numberOfCouponPeriodsInAYear;
                //double nextCoupon = 0;
                double next2Coupon = _couponRate * _faceValue / numberOfCouponPeriodsInAYear;
                //double annualCoupon = couponRate * 100 / numberOfCouponPeriodsInAYear;
                double annualCoupon = _couponRate * _faceValue;
                //double finalCoupon = 0.0;
                double finalCoupon = _couponRate * _faceValue * finalAccrualYearFraction;
                double discountFactorForOnePeriod = 1 / (1 + ytm / numberOfCouponPeriodsInAYear);
                var calculatedDirtyPrice = ISMADirtyPrice(accrualYearFraction,
                                                          finalAccrualYearFraction,
                                                          (int)i,
                                                          nextCoupon,
                                                          next2Coupon,
                                                          annualCoupon, finalCoupon, numberOfCouponPeriodsInAYear, discountFactorForOnePeriod, ytm);
                return calculatedDirtyPrice - _dirtyPrice;
            }

            public double Derivative(double x)
            {
                throw new NotImplementedException();
            }

            ///<summary>
            ///</summary>
            ///<param name="frequency"></param>
            ///<returns></returns>
            public static int FrequencyToMonths(int frequency)
            {
                return 12 / frequency;
            }
        }  

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accrualYearFraction">The first accrual coupon year fraction.</param>
        /// <param name="finalAccrualYearFraction">The final coupon year fraction. If regular this is zero.</param>
        /// <param name="n">The number of full coupon periods remaining until redemption. 
        /// The numer of remaining coupon periods is therefore n+1;</param>
        /// <param name="nextCoupon">The next coupon payment. This may be partial or zero if the bond is trading ex div.</param>
        /// <param name="next2Coupon">The next 2 coupon.</param>
        /// <param name="annualCoupon">The annual coupon payment for 100 unit of face value.</param>
        /// <param name="finalCoupon">The final coupon with an odd final period. This is zero for all other bonds.</param>
        /// <param name="h">The number of coupon periods in a year.</param>
        /// <param name="v">The disoount factor for one period, 1/(1+y/h)</param>
        /// <param name="y">The required annual nominal redemption yield expressed as a decimal.</param>
        /// <returns></returns>
        public static double ISMADP(double accrualYearFraction, double finalAccrualYearFraction, int n,
                                    double nextCoupon, double next2Coupon, double annualCoupon, double finalCoupon, int h, double v, double y)
        {
            var a = Math.Pow(v, accrualYearFraction);
            var b = Math.Pow(v, n + finalAccrualYearFraction);
            var final = (100 + finalCoupon * 100) * b;
            var c = nextCoupon * 100 + next2Coupon * v * 100;
            var d = annualCoupon / h * Math.Pow(v, 2) * (1 - Math.Pow(v, n - 1)) / (1 - v) * 100;
            var dirtyPrice = a * (c + d + final);
            return dirtyPrice / 100;
        }

        public static double BondPrice(DateTime settlement, DateTime maturity, double couponRate, double yield, int couponFreq, int exIntPeriod)
        {
            DateTime nextCouponDate = DateHelper.NextCouponDate(settlement, maturity, couponFreq);
            DateTime lastCouponDate = DateHelper.LastCouponDate(settlement, maturity, couponFreq);
            TimeSpan fSpan = nextCouponDate - settlement;
            TimeSpan dSpan = nextCouponDate - lastCouponDate;
            TimeSpan nSpan = maturity - nextCouponDate;
            int d = dSpan.Days;
            int f = fSpan.Days;
            var n = (int)Math.Round((double)nSpan.Days / 365 * couponFreq, 0);
            int exIntFlag = 1;
            if (f <= exIntPeriod) exIntFlag = 0;
            double v = Math.Pow(1 + yield / couponFreq, -1);
            double an = (1 - Math.Pow(v, n)) / (yield / couponFreq);
            double price = Math.Pow(v, f / (double)d) * (Math.Pow(v, n) + couponRate / couponFreq * (exIntFlag + an)) * 100;
            return price;
        }

        public static double BillPrice(DateTime settlement, DateTime maturity, double yield, double faceValue)
        {
            TimeSpan nSpan = maturity - settlement;
            int n = nSpan.Days;
            double price = faceValue / (1 + yield * n / 365);
            return price;
        }

        public static double FRNPrice(DateTime settlement, DateTime maturity, double initialMarginBp, double tradingMarginBp, int paymentFreq, double currentRateSet, 
            double rateToNextCouponDate, double currentSwapRate, int dayCount, int exIntPeriod, object[,] holidays)
        {
            DateTime nextCouponDate = DateHelper.NextCouponDate(settlement, maturity, paymentFreq);
            DateTime lastCouponDate = DateHelper.LastCouponDate(settlement, maturity, paymentFreq);
            nextCouponDate = DateHelper.DateRoll(nextCouponDate, 0, "D", "F", holidays);
            lastCouponDate = DateHelper.DateRoll(lastCouponDate, 0, "D", "F", holidays);
            TimeSpan fSpan = nextCouponDate - settlement;
            TimeSpan dSpan = nextCouponDate - lastCouponDate;
            TimeSpan nSpan = maturity - nextCouponDate;
            int d = dSpan.Days;
            int f = fSpan.Days;
            var n = (int)Math.Round((double)nSpan.Days / 365 * paymentFreq, 0);
            int exIntFlag = 1;
            if (f <= exIntPeriod) exIntFlag = 0;
            double annuityYield = currentSwapRate + tradingMarginBp / 10000;
            double v = Math.Pow(1 + annuityYield / paymentFreq, -1);
            double an = (1 - Math.Pow(v, n)) / (annuityYield / paymentFreq);
            double price = (exIntFlag * (currentRateSet + initialMarginBp / 10000) * d / dayCount
                            + ((initialMarginBp - tradingMarginBp) / 10000 / paymentFreq) * an + 1)
                            / (1 + (rateToNextCouponDate + tradingMarginBp / 10000) * f / dayCount) * 100;
            return price;
        }

        public static double CIBPrice(DateTime settlement, DateTime maturity, double couponRate, double yield, int couponFreq, double kt, double p, int exIntPeriod)
        {
            DateTime nextCouponDate = DateHelper.NextCouponDate(settlement, maturity, couponFreq);
            DateTime lastCouponDate = DateHelper.LastCouponDate(settlement, maturity, couponFreq);
            TimeSpan fSpan = nextCouponDate - settlement;
            TimeSpan dSpan = nextCouponDate - lastCouponDate;
            TimeSpan nSpan = maturity - nextCouponDate;
            int d = dSpan.Days;
            int f = fSpan.Days;
            var n = (int)Math.Round((double)nSpan.Days / 365 * couponFreq, 0);
            int exIntFlag = 1;
            if (f <= exIntPeriod) exIntFlag = 0;
            double v = Math.Pow(1 + yield / couponFreq, -1);
            double an = (1 - Math.Pow(v, n)) / (yield / couponFreq);
            double price = Math.Pow(v, f / (double)d) * (Math.Pow(v, n) + couponRate / couponFreq * (exIntFlag + an))
                                    * kt * Math.Pow(1 + p, -(f / (double)d));
            return price;
        }

        public static double IABPrice(DateTime settlement, DateTime maturity, double baseAnnuity, double yield, int pmtFreq, double cpiBase, double cpiLatest, double cpiPrevious, bool nextPmtKnownFlag, int exIntPeriod)
        {
            DateTime nextCouponDate = DateHelper.NextCouponDate(settlement, maturity, pmtFreq);
            DateTime lastCouponDate = DateHelper.LastCouponDate(settlement, maturity, pmtFreq);
            TimeSpan fSpan = nextCouponDate - settlement;
            TimeSpan dSpan = nextCouponDate - lastCouponDate;
            TimeSpan nSpan = maturity - nextCouponDate;
            int d = dSpan.Days;
            int f = fSpan.Days;
            var n = (int)Math.Round((double)nSpan.Days / 365 * pmtFreq, 0);
            int exIntFlag = 1;
            if (f <= exIntPeriod) exIntFlag = 0;
            double v = Math.Pow(1 + yield / pmtFreq, -1);
            double an = (1 - Math.Pow(v, n)) / (yield / pmtFreq);
            double q = cpiLatest / cpiPrevious;
            double bt = baseAnnuity * cpiLatest / cpiBase;
            double price = Math.Pow(v / q, f / (double)d) * bt * (exIntFlag + an);
            if (nextPmtKnownFlag == false) price = price * q;
            return price;
        }

        public static double RBABondPrice(DateTime settlement, DateTime maturity, double couponRate, double yield)
        {
            const int extIntPeriod = 7;
            const int coupFreq = 2;
            double price;
            DateTime discountSwitchDate = maturity.AddMonths(-12 / coupFreq).AddDays(-extIntPeriod);
            if (settlement < discountSwitchDate)
            {
                price = Math.Round(BondPrice(settlement, maturity, couponRate, yield, coupFreq, extIntPeriod), 3);
            }
            else
            {
                TimeSpan finalExPeriodDaysSpan = maturity - settlement;
                int finalExPeriodDays = finalExPeriodDaysSpan.Days;
                int finalExFlag = 1;
                if (finalExPeriodDays < extIntPeriod) finalExFlag = 0;
                double faceValue = (1 + couponRate / coupFreq * finalExFlag) * 100;
                price = BillPrice(settlement, maturity, yield, faceValue);
            }
            return price;
        }
    }
}