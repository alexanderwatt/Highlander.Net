/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using Directives

using System;
using System.Runtime.InteropServices;
using HLV5r3.Helpers;
using Microsoft.Win32;
using Orion.Analytics.Rates;

#endregion

namespace HLV5r3.Analytics
{
    /// <summary>
    /// A bond analytics function class.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("AD086B62-0C2E-49DD-83F3-465531FAB126")]
    public class Bonds
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
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(ApplicationHelper.GetSubKeyName(type, "InprocServer32"), true);
            key.SetValue("", System.Environment.SystemDirectory + @"\mscoree.dll", RegistryValueKind.String);
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
                
        #region Constructor

        #endregion

        #region Functions

        /// <summary>
        /// Converting yield compounding frequencies. Use coupon frequency of zero to mean continuously compounded yield..
        /// </summary>
        /// <param name="toCompoundingFrequency">The frequency to convert to.</param>
        /// <param name="fromCompoundingFrequency">The frequency to convert from.</param>
        /// <param name="yield">The yield to use.</param>
        /// <returns>The new yield for that frequency.</returns>
        public double ConvertCompoundingFrequency(short toCompoundingFrequency, short fromCompoundingFrequency, double yield)
        {
            return BondAnalytics.ConvertCompFreq(toCompoundingFrequency, fromCompoundingFrequency, yield);
        }

        /// <summary>
        /// Compounding frequency to convert to Compounding frequency to convert from Yield to convert
        /// </summary>
        /// <param name="toCompoundingFrequency">The result compounding frequency.</param>
        /// <param name="fromCompoundingFrequency">The base compounding frequency.</param>
        /// <param name="yield">The current yield.</param>
        /// <returns></returns>
        public double ConvertCompoundingFrequencySensitivity(short toCompoundingFrequency, short fromCompoundingFrequency, double yield)
        {
            return BondAnalytics.ConvertCompFreqSens(toCompoundingFrequency, fromCompoundingFrequency, yield);
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
        /// <param name="couponFrequency"></param>
        /// <returns></returns>
        public int CountCoupPdsEx(int ys, int ms, int rollDay, int ye, int me, int de, int couponFrequency)
        {
            return BondAnalytics.CountCoupPdsEx(ys, ms, rollDay, ye, me, de, couponFrequency);
        }

        /// <summary>
        /// AnnDF
        /// </summary>
        /// <param name="yield"></param>
        /// <param name="compoundingFrequency"></param>
        /// <returns></returns>
        public double AnnDF(double yield, int compoundingFrequency)
        {
            return BondAnalytics.AnnDF(yield, compoundingFrequency);
        }

        /// <summary>
        /// PeriodDF
        /// </summary>
        /// <param name="couponFrequency"></param>
        /// <param name="yield"></param>
        /// <param name="compoundingFrequency"></param>
        /// <returns></returns>
        public double PeriodDF(int couponFrequency, double yield, int compoundingFrequency)
        {
            return BondAnalytics.PeriodDF(couponFrequency, yield, compoundingFrequency);
        }

        /// <summary>
        /// IsmaNPV. Calc of bond dirty price. If last coup is regular then set k = 0 and cf = 0.
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c"></param>
        /// <param name="cf"></param>
        /// <param name="r"></param>
        /// <param name="q"></param>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <param name="v"></param>
        /// <returns>v^q * (c1 + c * (v + v^2 + ... + v^n) + (R + cf) * v^(n + k)), where v is the discount factor for a single coupon period</returns>
        public double IsmaNPV(double c1, double c, double cf, double r, double q, short n,
                                     double k, double v)
        {
            return BondAnalytics.IsmaNPV( c1,  c,  cf,  r,  q,  n,
                                          k,  v);
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
        /// <returns>(c1 + c * (v + v^2 + ... + v^n) + (R + cf) * v^(n + k)) / (1 + q * y / (100.H)), where v is the discount factor for a single coupon period</returns>
        public double BondMoosNPV(double c1, double c, double cf, double r, double q, short n,
                                         double k, short h, double y)
        {
            return BondAnalytics.BondMoosNPV( c1,  c,  cf,  r,  q,  n,
                                              k,  h,  y);
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
        public double IsmaPVBP(double c1, double c, double cf, double r, double q, short n,
                                      double k, double v)
        {
            return BondAnalytics.IsmaPVBP( c1,  c,  cf,  r,  q,  n,
                                           k,  v);
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
        public double BondMoosPVBP(double c1, double c, double cf, double r, double q, short n,
                                          double k, short h, double y)
        {
            return BondAnalytics.BondMoosPVBP( c1,  c,  cf,  r,  q,  n,
                                               k,  h,  y);
        }

        /// <summary>
        /// d2Pdv2
        /// </summary>
        /// <param name="q"></param>
        /// <param name="c1"></param>
        /// <param name="periodDiscountFactor"></param>
        /// <param name="c"></param>
        /// <param name="n"></param>
        /// <param name="r"></param>
        /// <param name="cf"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public double D2Pdv2(double q, double c1, double periodDiscountFactor, double c, int n, double r, double cf,
                                    double k)
        {
            return BondAnalytics.d2Pdv2(q, c1, periodDiscountFactor, c, n, r, cf,
                                         k);
        }

        /// <summary>
        /// Calculates the ISMA dirty bond price.
        /// </summary>
        /// <param name="accrualYearFraction">The first accrual coupon year fraction.</param>
        /// <param name="finalAccrualYearFraction">The final coupon year fraction. If regular this is zero.</param>
        /// <param name="numberOfFullCoupons">The number of full coupon periods remaining until redemption. 
        /// The numer of remaining coupon periods is therefore n+1;</param>
        /// <param name="nextCoupon">The next coupon payment. This may be partial or zero if the bond is trading ex div.</param>
        /// <param name="next2Coupon">The next 2 coupon.</param>
        /// <param name="annualCoupon">The annual coupon payment for 100 unit of face value.</param>
        /// <param name="finalCoupon">The final coupon with an odd final period. This is zero for all other bonds.</param>
        /// <param name="couponsPerYear">The number of coupon periods in a year.</param>
        /// <param name="periodDiscountFactor">The discount factor for one period, v = 1/(1+y/h)</param>
        /// <param name="annualYield">The required annual nominal redemption yield expressed as a decimal.</param>
        /// <returns>Te dirty price according to ISMA convention.</returns>
        public double ISMADirtyPrice(double accrualYearFraction, double finalAccrualYearFraction, int numberOfFullCoupons, 
                                            double nextCoupon, double next2Coupon, double annualCoupon, double finalCoupon, int couponsPerYear, 
            double periodDiscountFactor, double annualYield)
        {
            return BondAnalytics.ISMADirtyPrice(accrualYearFraction, finalAccrualYearFraction, numberOfFullCoupons,
                                                 nextCoupon, next2Coupon, annualCoupon, finalCoupon, couponsPerYear, periodDiscountFactor, annualYield);
        }
        
        ///<summary>
        /// Calculated the bond yield to maturity.
        ///</summary>
        ///<param name="maturityDate">redemtion date</param>
        ///<param name="lastCouponDate">last coupon date</param>
        ///<param name="couponRate">coupon rate</param>
        ///<param name="couponFrequency">coupons per year,1 for annual, 2 for semi, 4 for quoterly</param>
        ///<param name="faceValue">The bond face value.</param>
        ///<param name="dirtyPrice">dirty price</param>
        ///<returns>The yield to maturity.</returns>
        public double CalculateBondYTM(DateTime maturityDate, DateTime lastCouponDate, double couponRate, int couponFrequency, double faceValue, double dirtyPrice)
        {

            return BondAnalytics.CalculateBondYTM( maturityDate,  lastCouponDate,  couponRate,  couponFrequency,  faceValue,  dirtyPrice);
        }

        /// <summary>
        /// Calculates the ISMA dirty price.
        /// </summary>
        /// <param name="accrualYearFraction">The first accrual coupon year fraction.</param>
        /// <param name="finalAccrualYearFraction">The final coupon year fraction. If regular this is zero.</param>
        /// <param name="numberOfFullCoupons">The number of full coupon periods remaining until redemption. 
        /// The numer of remaining coupon periods is therefore n+1;</param>
        /// <param name="nextCoupon">The next coupon payment. This may be partial or zero if the bond is trading ex div.</param>
        /// <param name="next2Coupon">The next 2 coupon.</param>
        /// <param name="annualCoupon">The annual coupon payment for 100 unit of face value.</param>
        /// <param name="finalCoupon">The final coupon with an odd final period. This is zero for all other bonds.</param>
        /// <param name="couponsPerYear">The number of coupon periods in a year.</param>
        /// <param name="periodDiscountFactor">The disoount factor for one period, 1/(1+y/h)</param>
        /// <param name="annualYield">The required annual nominal redemption yield expressed as a decimal.</param>
        /// <returns>The ISMA dirty price.</returns>
        public double ISMADP(double accrualYearFraction, double finalAccrualYearFraction, int numberOfFullCoupons,
                                    double nextCoupon, double next2Coupon, double annualCoupon, double finalCoupon, int couponsPerYear, double periodDiscountFactor, double annualYield)
        {
            return BondAnalytics.ISMADP(accrualYearFraction, finalAccrualYearFraction, numberOfFullCoupons,
                                         nextCoupon, next2Coupon, annualCoupon, finalCoupon, couponsPerYear, periodDiscountFactor, annualYield);
        }

        #endregion
    }
}