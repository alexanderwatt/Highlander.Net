#region Using Directives

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HLV5r3.Helpers;
using Microsoft.Win32;
using Orion.Analytics.DayCounters;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Rates;
using Excel = Microsoft.Office.Interop.Excel;
using IPoint = Orion.ModelFramework.IPoint;

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

        public Rates()
        {
        }

        #endregion

        #region Functions

        /// <summary>
        /// Gets the derivative of the discount factor wrt the zero rate..
        /// </summary>
        /// <param name="discountFactor">The relevant df.</param>
        /// <param name="yearFraction">The year fraction.</param>
        /// <param name="rate">the rate.</param>
        /// <returns></returns>
        public Decimal GetDeltaZeroRate(decimal discountFactor, decimal yearFraction, decimal rate)
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
        public Decimal GetDFAtMaturity(decimal startDiscountFactor, decimal yearFraction, decimal rate)
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
        public Decimal GetRate(Decimal startDiscountFactor, Decimal endDiscountFactor, Decimal yearFraction)
        {
            return RateAnalytics.GetRate(startDiscountFactor, endDiscountFactor, yearFraction);
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        public static Decimal GetDelta1(Decimal npv, Decimal curveYearFraction, Decimal rate, Decimal yearFraction)
        {
            return RateAnalytics.GetDelta1(npv, curveYearFraction, rate,yearFraction);
        }

        /// <summary>
        /// Evaluates the delta wrt the continuously compounding rate R.
        /// </summary>
        /// <returns></returns>
        public Decimal GetDeltaCCR(Decimal npv, Decimal curveYearFraction, Decimal rate, Decimal yearFraction)
        {
            return RateAnalytics.GetDeltaCcr(npv, curveYearFraction, rate, yearFraction);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="pt">The pt.</param>
        /// <param name="interpolationType">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="times">A vetical array of times.</param>
        /// <param name="values">A vertical array of values.</param>
        /// <returns>The value at that point.</returns>
        public Double GetPointValue(Double pt, string interpolationType, bool extrapolation, Excel.Range times, Excel.Range values)
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
        /// <param name="times">A vetical array of times.</param>
        /// <param name="values">A vertical array of values.</param>
        /// <returns>The value at that point.</returns>
        public Double GetDateValue(DateTime baseDate, DateTime targetDate, string interpolationType, bool extrapolation, Excel.Range times, Excel.Range values)
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
        public double DiscountFactorToZeroRate1(DateTime baseDate, DateTime targetDate, double discountFactor, String frequency)
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
        public decimal DiscountFactorToZeroRate2(decimal startDiscountFactor, decimal endDiscountFactor,
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
        public double DiscountFactorToZeroRate3(double yearFraction, double discountFactor, String frequency)
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
        public decimal ZeroRateToDiscountFactor(decimal rate, decimal yearFraction,//TODO yearFraction <> 0.
                                             decimal compoundingPeriod)
        {
            return RateAnalytics.ZeroRateToDiscountFactor(rate, yearFraction,
                                                compoundingPeriod);
        }

        #endregion
    }
}