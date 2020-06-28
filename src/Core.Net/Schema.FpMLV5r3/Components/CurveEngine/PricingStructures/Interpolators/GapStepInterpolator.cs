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
using System.Linq;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.CalendarEngine.V5r3.Dates;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Serialisation;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Interpolators
{
    /// <summary>
    /// An interpolator for central bank date steps.
    /// </summary>
    public class GapStepInterpolator : TermCurveInterpolator
    {
        public CentralBanks CentralBank;
        
        /// <summary>
        /// Gets the days for that central bank.
        /// </summary>
        public DateTime[] CentralBankDays { get; set; }


        /// <summary>
        /// The main ctor.
        /// </summary>
        /// <param name="termCurve"></param>
        /// <param name="baseDate"></param>
        /// <param name="centralBankMonthRules"></param>
        /// <param name="centralBankDays"></param>
        /// <param name="centralBank"></param>
        /// <param name="dayCounter"></param>
        public GapStepInterpolator(TermCurve termCurve, DateTime baseDate, int centralBankMonthRules, DateTime[] centralBankDays,
                                   CentralBanks centralBank, IDayCounter dayCounter)
            : base(ConvertTermCurve(centralBank.ToString(), termCurve, baseDate, centralBankDays, 
                dayCounter), baseDate, dayCounter)
        {
            CentralBankDateRuleMonths = centralBankMonthRules;
            CentralBank = centralBank;
            CentralBankDays = centralBankDays;
        }

        /// <summary>
        /// Rules for central bank dates.
        /// </summary>
        public int CentralBankDateRuleMonths { get; }

        private static TermCurve ConvertTermCurve(string centralBankName, TermCurve termCurve, DateTime baseDate, DateTime[] centralBankDays, 
            IDayCounter dayCounter)
        {
            var interpolationMethod = new InterpolationMethod
                                          {Value = "PiecewiseConstantRateInterpolation"};
            termCurve.interpolationMethod = interpolationMethod;
            return InterpolateGapStepTermPoints(centralBankName, termCurve, centralBankDays, baseDate, dayCounter);
        }
        
        private static TermCurve InterpolateGapStepTermPoints(string centralBankName, TermCurve termCurve, IList<DateTime> centralBankDates, DateTime baseDate, IDayCounter dayCounter)
        {
            //Step 1. Insert the extra points.
            var resultTermCurve = LoadAndSort(centralBankName, termCurve, centralBankDates, baseDate, dayCounter);
            //Step 2. SortPoints the term curve if necessary.
            return resultTermCurve;
        }

        ///// <summary>
        ///// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        ///// <remarks>
        ///// If a GetValue method returns a exact match - this method should be returning null.
        ///// </remarks>
        ///// </summary>
        ///// <param name="termCurve"></param>
        ///// <param name="dates"></param>
        ///// <returns></returns>
        //public static int[] GetPreviousIndices(TermCurve termCurve, DateTime[] dates)
        //{
        //    var termDates = termCurve.GetListTermDates();
        //    var results = new List<int>();
        //    var temp = new int[dates.Length];
        //    var counter = 0;
        //    foreach (var date in dates)  //This handles or is supposed to handle the case of multiple central bank dates between node points.
        //    {
        //        var index = Array.BinarySearch(termDates.ToArray(), date);
        //        if (index >= 0)
        //        {
        //            temp[counter] = index;
        //        }
        //        else
        //        {
        //            var nextIndex = ~index;
        //            var prevIndex = nextIndex - 1;

        //            temp[counter] = prevIndex;
        //        }
        //        counter++;
        //    }
        //    for(var i =1; i <= temp.Length-1;i++)
        //    {
        //        var j = 1;
        //        while(temp[i-1]==temp[i])
        //        {
        //            j++;
        //        }
        //        results.Add(j);
        //    }
        //    return results.ToArray();
        //}

        ///// <summary>
        ///// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        ///// <remarks>
        ///// If a GetValue method returns a exact match - this method should be returning null.
        ///// </remarks>
        ///// </summary>
        ///// <param name="termCurve"></param>
        ///// <param name="date"></param>
        ///// <returns></returns>
        //public static int GetPreviousIndex(TermCurve termCurve, DateTime date)
        //{
        //    var dates = termCurve.GetListTermDates();
        //    var index = Array.BinarySearch(dates.ToArray(), date);
        //    if (index >= 0)
        //    {
        //        return index;
        //    }
        //    var nextIndex = ~index;
        //    var prevIndex = nextIndex - 1;
        //    return prevIndex;
        //}

        ///// <summary>
        ///// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        ///// <remarks>
        ///// If a GetValue method returns a exact match - this method should be returning null.
        ///// </remarks>
        ///// </summary>
        ///// <param name="termCurve"></param>
        ///// <param name="date"></param>
        ///// <returns></returns>
        //public static TermPoint GetPreviousPoint(TermCurve termCurve, DateTime date)
        //{
        //    var dates = termCurve.GetListTermDates();
        //    var values = termCurve.GetListMidValues();
        //    var index = Array.BinarySearch(dates.ToArray(), date);
        //    if (index >= 0)
        //    {
        //        return termCurve.point[index];
        //    }
        //    var nextIndex = ~index;
        //    var prevIndex = nextIndex - 1;
        //    //TODO check for DateTime1D point and return the date.
        //    var prevPoint = TermPointFactory.Create(values[prevIndex], dates[prevIndex]);
        //    return prevPoint;
        //}

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// This uses the Array.BinarySearch function which returns the index of the specified
        /// value in the specified array, if value is found; otherwise, a negative number.
        /// If value is not found and value is less than one or more elements in array, the
        /// negative number returned is the bitwise complement of the index of the first element
        /// that is larger than value. If value is not found and value is greater than all
        /// elements in array, the negative number returned is the bitwise complement of
        /// (the index of the last element plus 1). If this method is called with a non-sorted array,
        /// the return value can be incorrect and a negative number could be returned, even if value is present in array.
        /// </remarks>
        /// </summary>
        /// <param name="baseDate">The curve base date.</param>
        /// <param name="startDate">The interval start date.</param>
        /// <param name="endDate">The interval end date.</param>
        /// <param name="endRate">The start date continuous zero.</param>
        /// <param name="dayCounter">The day counter.</param>
        /// <param name="intermediateDate">The date must lie in the interval between start and end dates.</param>
        /// <param name="startRate">The start date continuous zero.</param>
        /// <returns></returns>
        public static decimal InterpolateRate(DateTime baseDate, DateTime startDate, decimal startRate, DateTime endDate, decimal endRate,
            IDayCounter dayCounter, DateTime intermediateDate)
        {
            var time1 = dayCounter.YearFraction(baseDate, startDate);
            var time2 = dayCounter.YearFraction(baseDate, endDate);
            var timei = dayCounter.YearFraction(baseDate, intermediateDate);
            var i1 = (time2 - timei) / (time2 - time1);
            var i2 = (timei - time1) / (time2 - time1);
            var r1 = (double)startRate;
            var r2 = (double)endRate;
            return (decimal)((i1 * r1 * time1 + i2 * r2 * time2) / timei);
        }

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// This uses the Array.BinarySearch function which returns the index of the specified
        /// value in the specified array, if value is found; otherwise, a negative number.
        /// If value is not found and value is less than one or more elements in array, the
        /// negative number returned is the bitwise complement of the index of the first element
        /// that is larger than value. If value is not found and value is greater than all
        /// elements in array, the negative number returned is the bitwise complement of
        /// (the index of the last element plus 1). If this method is called with a non-sorted array,
        /// the return value can be incorrect and a negative number could be returned, even if value is present in array.
        /// </remarks>
        /// </summary>
        /// <param name="termCurve"></param>
        /// <param name="baseDate"></param>
        /// <param name="dayCounter"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static decimal InterpolateRate(TermCurve termCurve, DateTime baseDate, IDayCounter dayCounter, DateTime date)
        {
            var dates = termCurve.GetListTermDates();
            var values = termCurve.GetListMidValues();
            var index = Array.BinarySearch(dates.ToArray(), date);
            if (index >= 0)
            {
                var result = termCurve.point[index].mid;
                return result;
            }
            var nextIndex = ~index;
            var prevIndex = nextIndex - 1;
            var baseIndex = prevIndex - 1;
            if (prevIndex == 0)
            {
                return values[prevIndex];//TODO check this.
            }
            var time1 = dayCounter.YearFraction(baseDate, dates[baseIndex]);
            var time2 = dayCounter.YearFraction(baseDate, dates[prevIndex]);
            var timei = dayCounter.YearFraction(baseDate, date);
            var i1 = (time2 - timei) / (time2 - time1);
            var i2 = (timei - time1) / (time2 - time1);
            var r1 = (double)values[baseIndex];
            var r2 = (double)values[prevIndex];
            var rate = (i1 * r1 * time1 + i2 * r2 * time2) / timei;
            //TODO check for DateTime1D point and return the date.
            return (decimal)rate;
        }

        ///// <summary>
        ///// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        ///// <remarks>
        ///// If a GetValue method returns a exact match - this method should be returning null.
        ///// </remarks>
        ///// </summary>
        ///// <param name="termCurve"></param>
        ///// <param name="baseDate"></param>
        ///// <param name="dayCounter"></param>
        ///// <param name="date"></param>
        ///// <returns></returns>
        //public static decimal InterpolateRate2(TermCurve termCurve, DateTime baseDate, IDayCounter dayCounter, DateTime date)
        //{
        //    var dates = termCurve.GetListTermDates();
        //    var values = termCurve.GetListMidValues();
        //    var index = Array.BinarySearch(dates.ToArray(), date);
        //    if (index >= 0)
        //    {
        //        var result = termCurve.point[index].mid;
        //        return result;
        //    }
        //    var nextIndex = ~index;
        //    var prevIndex = nextIndex - 1;
        //    var baseIndex = prevIndex - 1;
        //    if (prevIndex == 0)
        //    {
        //        return values[prevIndex];//TODO check this.
        //    }
        //    var time1 = dayCounter.YearFraction(baseDate, dates[baseIndex]);
        //    var time2 = dayCounter.YearFraction(baseDate, dates[nextIndex]);
        //    var timei = dayCounter.YearFraction(baseDate, date);
        //    var i1 = (time2 - timei) / (time2 - time1);
        //    var i2 = (timei - time1) / (time2 - time1);
        //    var r1 = (double)values[baseIndex];
        //    var r2 = (double)values[nextIndex];
        //    var rate = (i1 * r1 * time1 + i2 * r2 * time2) / timei;
        //    return (decimal)rate;
        //}

        private static List<Tuple<DateTime, decimal>> ProcessTermCurve(TermCurve termCurve)
        {
            var result = new List<Tuple<DateTime, decimal>>();
            var termDates = termCurve.GetListTermDates();
            var values = termCurve.GetListMidValues();
            int counter = 0;
            if (termDates.Count == values.Count)
            {
                foreach (var date in termDates)
                {
                    var tuple = new Tuple<DateTime, decimal>(date,  values[counter]);
                    result.Add(tuple);
                    counter++;
                }
            }
            return result;
        }

        private static TermCurve LoadAndSort(string centralBankName,  TermCurve termCurve, IList<DateTime> centralBankDates, DateTime baseDate, IDayCounter dayCounter)
        {
            var cloneTermCurve = XmlSerializerHelper.Clone(termCurve);
            var points = new List<TermPoint>();
            var tupleList = ProcessTermCurve(cloneTermCurve);
            //Gap step Logic
            //1. Is there a reserve bank date(s) between 2 consecutive term curve points?
            //2. If yes then how many?
            //3. Initially ignore all but the first one.
            //4. Insert the first date.
            //5. Loop past any other dates.
            if (centralBankDates != null)
            {
                var dates = centralBankDates.ToList();
                for (int i = 1; i < tupleList.Count; i++)
                {
                    var intervalStartDate = tupleList[i - 1].Item1;
                    var intervalEndDate = tupleList[i].Item1;
                    var intervalStartRate = tupleList[i - 1].Item2;
                    var intervalEndRate = tupleList[i].Item2;
                    var dateList = new List<DateTime>();
                    foreach (var centralDate in dates)
                    {
                        if (intervalStartDate < centralDate && centralDate < intervalEndDate)
                        {
                            dateList.Add(centralDate);
                        }
                    }
                    if (dateList.Count > 0)
                    {
                        var termPoint =
                            TermPointFactory.Create(centralBankName + ":" + dateList[0], InterpolateRate(baseDate, intervalStartDate, intervalStartRate,
                                    intervalEndDate, intervalEndRate, dayCounter, dateList[0]),
                                dates[0]);
                        points.Add(termPoint);
                    }
                }
            }
            var newPoints = cloneTermCurve.point.ToList();
            newPoints.AddRange(points);
            var sortedList = SortPoints(newPoints);
            cloneTermCurve.point = sortedList.ToArray();
            return cloneTermCurve;
        }

        /// <summary>
        /// Sorts a term curve.
        /// <remarks>
        /// No two dates are to be the same!
        /// </remarks>
        /// </summary>
        /// <param name="termPoints"></param>
        /// <returns></returns>
        private static List<TermPoint> SortPoints(List<TermPoint> termPoints)
        {
            termPoints.Sort((a, b) => ((DateTime) a.term.Items[0]).CompareTo((DateTime) b.term.Items[0]));
            return termPoints;
        }
    }
}