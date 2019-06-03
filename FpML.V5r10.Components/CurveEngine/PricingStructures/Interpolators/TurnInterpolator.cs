#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace Orion.CurveEngine.PricingStructures.Interpolators
{
    /// <summary>
    /// An interpolator for central bank date steps.
    /// </summary>
    public class TurnInterpolator : TermCurveInterpolator//GapStep is always on the zero curve. A time zero instantaneous rate may be required.
    {
        /// <summary>
        /// Gets the days for the turn effects.
        /// </summary>
        public DateTime[] TurnDates { get; set; }

        //TODO add EOM, EOQ and EOY perturbation: this transfers the step to the specific day. 
        //Also needs to add EOM swaps to the asset config file.

        /// <summary>
        /// The main ctor.
        /// </summary>
        /// <param name="termCurve"></param>
        /// <param name="baseDate"></param>
        /// <param name="turndates"></param>
        /// <param name="dayCounter"></param>
        public TurnInterpolator(TermCurve termCurve, DateTime baseDate, DateTime[] turndates, IDayCounter dayCounter)
            : base(ConvertTermCurve(termCurve, baseDate, turndates, dayCounter), baseDate, dayCounter)
        {
            TurnDates = turndates;
            TermCurve = InterpolateTurnPoints(termCurve, turndates, baseDate, dayCounter);
        }

        /// <summary>
        /// The term curve.
        /// </summary>
        public TermCurve TermCurve { get; }

        private static TermCurve ConvertTermCurve(TermCurve termCurve, DateTime baseDate, DateTime[] turnDates, IDayCounter dayCounter)
        {
            var interpolationMethod = new InterpolationMethod
                                          {Value = "PiecewiseConstantRateInterpolation"};
            termCurve.interpolationMethod = interpolationMethod;
            return InterpolateTurnPoints(termCurve, turnDates, baseDate, dayCounter);//Add the extra points..
        }

        private static TermCurve InterpolateTurnPoints(TermCurve termCurve, DateTime[] turnDates, DateTime baseDate, IDayCounter dayCounter)
        {
            //Step 1. Insert the extra points.
            //
            var resultTermCurve = Load(termCurve, turnDates, baseDate, dayCounter);
            //Step 2. 
            //
            //Step 3. SortPoints the termcurve if necessary.
            return SortPoints(resultTermCurve);//TODO add the new points...need to sort the termcurve points...
        }

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// </remarks>
        /// </summary>
        /// <param name="termCurve"></param>
        /// <param name="dates"></param>
        /// <returns></returns>
        public static int[] GetPreviousIndices(TermCurve termCurve, DateTime[] dates)
        {
            var termdates = termCurve.GetListTermDates();//GetDiscreteSpace().GetCoordinateArray(1);
            var results = new List<int>();
            var temp = new int[dates.Length];
            var counter = 0;
            foreach (var date in dates)  //This handles or is supposed to handle the case of multiple central bank dates between node points.
            {
                var index = Array.BinarySearch(termdates.ToArray(), date);

                if (index >= 0)
                {
                    temp[counter] = index;
                }
                else
                {
                    var nextIndex = ~index;
                    var prevIndex = nextIndex - 1;

                    temp[counter] = prevIndex;
                }
                counter++;
            }
            for(var i =1; i <= temp.Length-1;i++)
            {
                var j = 1;
                while(temp[i-1]==temp[i])
                {
                    j++;
                }
                results.Add(j);
            }
            return results.ToArray();
        }

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// </remarks>
        /// </summary>
        /// <param name="termCurve"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetPreviousIndex(TermCurve termCurve, DateTime date)
        {
            var dates = termCurve.GetListTermDates();//GetDiscreteSpace().GetCoordinateArray(1);
            var index = Array.BinarySearch(dates.ToArray(), date);
            if (index >= 0)
            {
                return index;
            }
            var nextIndex = ~index;
            var prevIndex = nextIndex - 1;
            return prevIndex;
        }

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// </remarks>
        /// </summary>
        /// <param name="termCurve"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static TermPoint GetPreviousPoint(TermCurve termCurve, DateTime date)
        {
            var dates = termCurve.GetListTermDates();//GetDiscreteSpace().GetCoordinateArray(1);
            var values = termCurve.GetListMidValues();
            var index = Array.BinarySearch(dates.ToArray(), date);
            if (index >= 0)
            {
                return termCurve.point[index];
            }
            var nextIndex = ~index;
            var prevIndex = nextIndex - 1;
            //TODO check for DateTime1D point and return the date.
            var prevPoint = TermPointFactory.Create(values[prevIndex], dates[prevIndex]);
            return prevPoint;
        }

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// </remarks>
        /// </summary>
        /// <param name="termCurve"></param>
        /// <param name="baseDate"></param>
        /// <param name="dayCounter"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static Decimal InterpolateRate(TermCurve termCurve, DateTime baseDate, IDayCounter dayCounter, DateTime date)
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
            var i1 = (time2-timei)/(time2-time1);
            var i2 = (timei - time1) / (time2 - time1);
            var r1 = (double)values[baseIndex];
            var r2 = (double)values[prevIndex];
            var rate = (i1*r1*time1 + i2*r2*time2)/timei;
            //TODO check for DateTime1D point and return the date.
            return (decimal)rate;
        }

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// </remarks>
        /// </summary>
        /// <param name="termCurve"></param>
        /// <param name="baseDate"></param>
        /// <param name="dayCounter"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static Decimal InterpolateRate2(TermCurve termCurve, DateTime baseDate, IDayCounter dayCounter, DateTime date)
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
            var time2 = dayCounter.YearFraction(baseDate, dates[nextIndex]);
            var timei = dayCounter.YearFraction(baseDate, date);
            var i1 = (time2 - timei) / (time2 - time1);
            var i2 = (timei - time1) / (time2 - time1);
            var r1 = (double)values[baseIndex];
            var r2 = (double)values[nextIndex];
            var rate = (i1 * r1 * time1 + i2 * r2 * time2) / timei;
            //TODO check for DateTime1D point and return the date.
            return (decimal)rate;
        }

        private static TermCurve  Load(TermCurve termCurve, DateTime[] rbaDates, DateTime baseDate, IDayCounter dayCounter)
        {
            var points = new List<TermPoint>(termCurve.point);
            var counter = 0;
            foreach(var point in termCurve.point)
            {
                var date = (DateTime) point.term.Items[0];
                var dates = rbaDates.Where(t => GetPreviousIndex(termCurve, t) == counter).ToList();
                foreach (var RBAdate in dates)
                {
                    if (dates.IndexOf(RBAdate) == 1)
                    {
                        var termPoint =
                            TermPointFactory.Create(InterpolateRate(termCurve, baseDate, dayCounter, dates[0]), dates[0]);
                        points.Insert(GetPreviousIndex(termCurve, date), termPoint);
                    }
                    else
                    {
                        var termPoint =
                            TermPointFactory.Create(InterpolateRate2(termCurve, baseDate, dayCounter, RBAdate), RBAdate);
                        points.Insert(GetPreviousIndex(termCurve, date), termPoint);
                    }
                }
                counter++;
            }
            return termCurve;
        }

        /// <summary>
        /// Sorts a termcurve.
        /// <remarks>
        /// No two dates are to be the same!
        /// </remarks>
        /// </summary>
        /// <param name="termCurve"></param>
        /// <returns></returns>
        private static TermCurve SortPoints(TermCurve termCurve)
        {
            var points = new SortedList<DateTime, TermPoint>();
            foreach (var point in termCurve.point)
            {
                points.Add((DateTime)point.term.Items[0], point);
            }
            points.Values.CopyTo(termCurve.point, 0);
            return termCurve;
        }
    }
}