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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.Analytics.V5r3.Rates;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Utilities.Helpers;
using HLV5r3.Helpers;
using Microsoft.Win32;

#endregion

namespace HLV5r3.Analytics
{
    /// <summary>
    /// Rate analytics functions.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("F1B4D562-E097-40C9-8426-48CB59B8E3FC")]
    public class Rates
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
        /// Gets the derivative of the discount factor wrt the zero rate..
        /// </summary>
        /// <param name="discountFactor">The relevant df.</param>
        /// <param name="yearFraction">The year fraction.</param>
        /// <param name="rate">the rate.</param>
        /// <returns></returns>
        public static decimal GetDeltaZeroRate(decimal discountFactor, decimal yearFraction, decimal rate)
        {
            return RateAnalytics.GetDeltaZeroRate(discountFactor, yearFraction, rate);
        }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <param name="startDiscountFactor">The start df.</param>
        /// <param name="yearFraction">The year fraction.</param>
        /// <param name="rate">the rate.</param>
        /// <returns></returns>
        public static decimal GetDFAtMaturity(decimal startDiscountFactor, decimal yearFraction, decimal rate)
        {
            return RateAnalytics.GetDiscountFactorAtMaturity(startDiscountFactor, yearFraction, rate); 
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
            return RateAnalytics.GetRate(startDiscountFactor, endDiscountFactor, yearFraction);
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        public static decimal GetDelta1(decimal npv, decimal curveYearFraction, decimal rate, decimal yearFraction)
        {
            return RateAnalytics.GetDelta1(npv, curveYearFraction, rate,yearFraction);
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        public static decimal GetDeltaCCR(decimal npv, decimal curveYearFraction, decimal rate, decimal yearFraction)
        {
            return RateAnalytics.GetDeltaCcr(npv, curveYearFraction, rate, yearFraction);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="pt">The pt.</param>
        /// <param name="interpolationType">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="times">A vertical array of times.</param>
        /// <param name="values">A vertical array of values.</param>
        /// <returns>The value at that point.</returns>
        public static double GetPointValue(double pt, string interpolationType, bool extrapolation, object times, object values)
        {
            List<double> unqtimes = DataRangeHelper.StripDoubleRange(times);
            List<double> unqvalues = DataRangeHelper.StripDoubleRange(values);
            return CurveAnalytics.GetValue(pt, interpolationType, extrapolation, unqtimes.ToArray(), unqvalues.ToArray());
        }

        ///<summary>
        /// Gets the interpolated value from the curve.
        ///</summary>
        ///<param name="baseDate">The based date.</param>
        ///<param name="targetDate">The target date.</param>
        /// <param name="interpolationType">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="times">A vertical array of times.</param>
        /// <param name="values">A vertical array of values.</param>
        /// <returns>The value at that point.</returns>
        public static double GetDateValue(DateTime baseDate, DateTime targetDate, string interpolationType, bool extrapolation, object times, object values)
        {
            List<double> unqtimes = DataRangeHelper.StripDoubleRange(times);
            List<double> unqvalues = DataRangeHelper.StripDoubleRange(values);
            IPoint point = new DateTimePoint1D(baseDate, targetDate);
            return CurveAnalytics.GetDateValue(point, interpolationType, extrapolation, unqtimes.ToArray(), unqvalues.ToArray());
        }

        ///<summary>
        /// Converts a discount factor to a compounding zero rate.
        ///</summary>
        ///<param name="targetDate">The target date.</param>
        ///<param name="discountFactor">The discount factor.</param>
        ///<param name="frequency">The compounding frequency. Can take: Continuous, Daily, Quarterly,
        /// Semi-Annual,SemiAnnual,Semi and Annual.</param>
        ///<param name="baseDate">The base date.</param>
        ///<returns>The compounded zero rate requested.</returns>
        public static double DiscountFactorToZeroRate1(DateTime baseDate, DateTime targetDate, double discountFactor, string frequency)
        {
            double yearFraction = Actual365.Instance.YearFraction(baseDate, targetDate);
            return RateAnalytics.DiscountFactorToZeroRate(discountFactor, yearFraction, frequency);
        }

        /// <summary>
        /// Converts from the zero rate to a terminal wealth.
        /// </summary>
        /// <param name="startDiscountFactor"></param>
        /// <param name="endDiscountFactor"></param>
        /// <param name="yearFraction"></param>
        /// <param name="compoundingPeriod"></param>
        /// <returns></returns>
        public static decimal DiscountFactorToZeroRate2(decimal startDiscountFactor, decimal endDiscountFactor,
                                               decimal yearFraction, decimal compoundingPeriod)
        {
            return RateAnalytics.DiscountFactorToZeroRate(startDiscountFactor, endDiscountFactor,
                                                  yearFraction, compoundingPeriod);
        }

        ///<summary>
        /// Converts a discount factor to a compounding zero rate.
        ///</summary>
        ///<param name="yearFraction">The year Fraction.</param>
        ///<param name="discountFactor">The discount factor.</param>
        ///<param name="frequency">The compounding frequency. Can take: Continuous, Daily, Quarterly,
        /// Semi-Annual,SemiAnnual,Semi and Annual</param>
        ///<returns>The compounded zero rate requested.</returns>
        public static double DiscountFactorToZeroRate3(double yearFraction, double discountFactor, string frequency)
        {
            return RateAnalytics.DiscountFactorToZeroRate(discountFactor, yearFraction, frequency);
        }

        /// <summary>
        /// Converts from the zero rate to a terminal wealth.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="yearFraction"></param>
        /// <param name="compoundingPeriod"></param>
        /// <returns></returns>
        public static decimal ZeroRateToDiscountFactor(decimal rate, decimal yearFraction,//TODO yearFraction <> 0.
                                             decimal compoundingPeriod)
        {
            return RateAnalytics.ZeroRateToDiscountFactor(rate, yearFraction,
                                                compoundingPeriod);
        }

        #endregion
    }
}