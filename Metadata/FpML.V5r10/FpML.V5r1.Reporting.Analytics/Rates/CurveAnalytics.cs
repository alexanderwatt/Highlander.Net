#region Using directives


using System;
using System.Collections.Generic;
using System.Linq;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolations.Spaces;
using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.Rates
{
    /// <summary>
    /// Creates a simple curve in 2-dimensional space.
    /// </summary>
    public class CurveAnalytics
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="pt">The pt.</param>
        /// <param name="interpolationType">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="times">A vetical array of times.</param>
        /// <param name="values">A vertical array of values.</param>
        /// <returns>The value at that point.</returns>
        public static Double GetValue(Double pt, string interpolationType, bool extrapolation, Double[] times, Double[] values)
        {
            var curve = new InterpolatedCurve(new DiscreteCurve(times, values), InterpolationFactory.Create(interpolationType), extrapolation);
            IPoint point = new Point1D(pt);
            return curve.Value(point);
        }

        ///<summary>
        /// Gets the interpolated value from the curve.
        ///</summary>
        ///<param name="point">A 1D point</param>
        /// <param name="interpolationType">The interpolation.</param>
        /// <param name="extrapolation">if set to <c>true</c> [extrapolation].</param>
        /// <param name="times">A vetical array of times.</param>
        /// <param name="values">A vertical array of values.</param>
        /// <returns>The value at that point.</returns>
        public static Double GetDateValue(IPoint point, string interpolationType, bool extrapolation, double[] times, double[] values)
        {
            var curve = new InterpolatedCurve(new DiscreteCurve(times, values), InterpolationFactory.Create(interpolationType), extrapolation);
            return curve.Value(point);
        }



        /// <summary>
        /// Generates discount factors from a curve.
        /// Time bucket baseDate can be different to curveBaseDate.
        /// </summary>
        /// <param name="timeBuckets"></param>
        /// <param name="timeBucketBaseDate"></param>
        /// <param name="curveBaseDate"></param>
        /// <param name="curveIn"></param>
        /// <returns></returns>
        public static decimal[] GenerateDiscountFactorsFromCurve(int[] timeBuckets, DateTime timeBucketBaseDate, DateTime curveBaseDate, IDictionary<DateTime, decimal> curveIn)
        {
            IDictionary<int, decimal> curve = ConvertCurve(curveBaseDate, curveIn, true);
            return GenerateDiscountFactorsFromCurve(timeBuckets, timeBucketBaseDate, curveBaseDate, curve);
        }

        /// <summary>
        /// Generates discount factors from a curve.
        /// Assumes time bucket zero is at curveBaseDate.
        /// </summary>
        /// <param name="timeBuckets"></param>
        /// <param name="curveBaseDate"></param>
        /// <param name="curveIn"></param>
        /// <returns></returns>
        public static decimal[] GenerateDiscountFactorsFromCurve(int[] timeBuckets, DateTime curveBaseDate, IDictionary<DateTime, decimal> curveIn)
        {
            IDictionary<int, decimal> curve = ConvertCurve(curveBaseDate, curveIn, true);
            return GenerateDiscountFactorsFromCurve(timeBuckets, curve);
        }

        /// <summary>
        /// Generates discount factors from a curve.
        /// </summary>
        /// <param name="timeBuckets"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static decimal[] GenerateDiscountFactorsFromCurve(int[] timeBuckets, DateTime timeBucketBaseDate, DateTime curveBaseDate, IDictionary<int, decimal> curve)
        {
            decimal[] dfValues = new decimal[timeBuckets.Length];
            //work out lag between curve base date and time bucket base date
            TimeSpan ts = curveBaseDate - timeBucketBaseDate;
            int lag = ts.Days;

            for (int i = 0; i < timeBuckets.Length; i++)
            {
                if (i < lag)
                {
                    dfValues[i] = 1.0M; //timebucket is before curve start
                }
                else dfValues[i] = DF(timeBuckets[i] - lag, curve);
            }
            return dfValues;
        }
        /// <summary>
        /// Generates discount factors from a curve.
        /// Assumes time bucket zero is at curveBaseDate.
        /// </summary>
        /// <param name="timeBuckets"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static decimal[] GenerateDiscountFactorsFromCurve(int[] timeBuckets, IDictionary<int, decimal> curve)
        {
            decimal[] dfValues = new decimal[timeBuckets.Length];

            for (int i = 0; i < timeBuckets.Length; i++)
            {
                dfValues[i] = DF(timeBuckets[i], curve);
            }
            return dfValues;
        }

        /// <summary>
        /// Function returns simple ACT/365 forward rate.
        /// 
        /// Discrete forward rate between two time points.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="curve"></param>
        /// <returns>simple ACT/365 forward rate</returns>
        public static decimal ForwardRate(int t1, int t2, IDictionary<int, decimal> curve)
        {

            decimal dayBasis = 365.0M;
            decimal df1 = DF(t1, curve);
            decimal df2 = DF(t2, curve);
            decimal forwardRate = (df1 / df2 - 1.0M) / ((t2 - t1) / dayBasis);

            return forwardRate;

        }

        /// <summary>
        /// Function calculates forward discount factor
        /// (Implied forward discount factor)
        /// 
        /// 
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        /// 
        public static decimal ForwardDF(int t1, int t2, IDictionary<int, decimal> curve)
        {

            decimal df1 = DF(t1, curve);
            decimal df2 = DF(t2, curve);
            decimal forwardDF = df2 / df1;

            return forwardDF;

        }

        public static decimal ForwardDF(DateTime t1, DateTime t2, IDictionary<DateTime, decimal> curve, DateTime curveBaseDate)
        {

            decimal df1 = DF(t1, curve, curveBaseDate);
            decimal df2 = DF(t2, curve, curveBaseDate);
            decimal forwardDF = df2 / df1;

            return forwardDF;

        }

        /// <summary>
        /// Function returns discount factor. 
        /// 
        /// This is the only function which depends on the curve quotation.
        /// Assumes continuously compounded ACT/365 curve quotation.
        /// </summary>
        /// <param name="t">days from reference</param>
        /// <param name="curve">cc ACT/365 curve</param>
        /// <returns></returns>
        public static decimal DF(int t, IDictionary<int, decimal> curve)
        {
            decimal df;

            decimal rate = LinearInterpolation(t, curve);

            df = (decimal)Math.Exp(-(double)(rate * (t / 365.0M)));

            return df;

        }

        /// <summary>
        /// Function returns discount factor. 
        /// 
        /// This is the only function which depends on the curve quotation.
        /// Assumes continuously compounded ACT/365 curve quotation.
        /// </summary>
        /// <param name="t">date</param>
        /// <param name="curve">cc ACT/365 curve</param>
        /// <param name="curveBaseDate">curve base date</param>
        /// <returns></returns>
        public static decimal DF(DateTime t, IDictionary<DateTime, decimal> curve, DateTime curveBaseDate)
        {
            decimal df;

            decimal rate = LinearInterpolation(t, curve);
            TimeSpan ts = t - curveBaseDate;

            df = (decimal)Math.Exp(-(double)(rate * (ts.Days / 365.0M)));

            return df;

        }

        /// <summary>
        /// Generates a date set from integer offsets (days) from curve base date.
        /// </summary>
        /// <param name="timeBuckets"></param>
        /// <param name="baseDate"></param>
        /// <returns></returns>
        public static DateTime[] GenerateDateSet(int[] timeBuckets, DateTime baseDate)
        {
            //setup date set
            DateTime[] dateSet = new DateTime[timeBuckets.Length];
            for (int i = 0; i < timeBuckets.Length; i++)
            {
                DateTime date = new DateTime(baseDate.Year, baseDate.Month, baseDate.Day);
                dateSet[i] = date.AddDays(timeBuckets[i]);
            }
            return dateSet;
        }

        /// <summary>
        /// Converts a curve.
        /// </summary>
        /// <param name="curveBaseDate"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static IDictionary<int, decimal> ConvertCurve(DateTime curveBaseDate, IDictionary<DateTime, decimal> curve, bool startAtBaseDate)
        {
            //setup date set
            IDictionary<int, decimal> result = new SortedDictionary<int, decimal>();
            TimeSpan ts;
            foreach (KeyValuePair<DateTime, decimal> point in curve)
            {
                ts = point.Key - curveBaseDate;
                if (ts.Days >= 0)
                    result.Add(ts.Days, point.Value);
                else if (!startAtBaseDate) result.Add(ts.Days, point.Value);
            }

            return result;
        }

        /// <summary>
        /// Converts a curve.
        /// </summary>
        /// <param name="curveBaseDate"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static IDictionary<DateTime, decimal> ConvertCurve(DateTime curveBaseDate, IDictionary<int, decimal> curve)
        {
            IDictionary<DateTime, decimal> result = new SortedDictionary<DateTime, decimal>();

            foreach (KeyValuePair<int, decimal> point in curve)
            {
                DateTime newDate = new DateTime(curveBaseDate.Year,
                                                curveBaseDate.Month,
                                                curveBaseDate.Day);
                newDate = newDate.AddDays(point.Key);
                result.Add(newDate, point.Value);
            }
            return result;
        }

        /// <summary>
        /// Function interpolates curve linearly assuming that the collection is already sorted
        /// </summary>
        /// <param name="t1"> Point to evaluate</param>
        /// <param name="curve"> curve data dictionary</param>
        /// <returns></returns>
        public static decimal LinearInterpolation(int t1, IDictionary<int, decimal> curve)
        {
            if (t1 <= GetCurveElement(0, curve).Key)
            {
                //Console.WriteLine("[LinearInterpolation][WARN]: Lower curve bound exceeded.");

                return GetCurveElement(0, curve).Value; //lower curve bound
            }
            if (t1 >= GetCurveElement(curve.Count - 1, curve).Key)
            {
                //Console.WriteLine("[LinearInterpolation][WARN]: Upper curve bound exceeded.");

                return GetCurveElement(curve.Count - 1, curve).Value; //upper curve bound
            }
            //check if t1 exactly matches a curve point (ie no interpolation necessary)

            decimal x1;
            decimal y1;

            decimal x2;
            decimal y2;

            for (int j = 1; j < curve.Count; j++)
            {
                if (t1 <= GetCurveElement(j, curve).Key)
                {
                    x2 = GetCurveElement(j, curve).Key / 365.0M;
                    y2 = GetCurveElement(j, curve).Value;

                    x1 = GetCurveElement(j - 1, curve).Key / 365.0M;
                    y1 = GetCurveElement(j - 1, curve).Value;

                    decimal k = (y2 - y1) / (x2 - x1);
                    decimal a = y1 - k * x1;

                    decimal val = k * t1 / 365.0M + a; //changed by damien (divide by 365)

                    return val;
                }
            }

            return 0;

        }

        /// <summary>
        /// Function interpolates curve linearly assuming that the collection is already sorted
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static decimal LinearInterpolation(DateTime t1, IDictionary<DateTime, decimal> curve)
        {

            DateTime[] indexList = curve.Keys.ToArray();
            if (indexList.Length == 0) throw new IndexOutOfRangeException("Error: no curve data");

            if (t1.CompareTo(indexList[0]) <= 0)
            {
                //Console.WriteLine("[LinearInterpolation][WARN]: Lower curve bound exceeded.");

                return curve[indexList[0]]; //lower curve bound
            }

            if (t1.CompareTo(indexList[indexList.Length - 1]) >= 0)
            {
                //Console.WriteLine("[LinearInterpolation][WARN]: Upper curve bound exceeded.");

                return curve[indexList[indexList.Length - 1]]; //upper curve bound
            }

            decimal x1;
            decimal y1;
            decimal x2;
            decimal y2;

            for (int j = 1; j < indexList.Length; j++)
            {
                if (t1.CompareTo(indexList[j]) <= 0)
                {
                    TimeSpan ts2 = indexList[j] - indexList[0];
                    x2 = ts2.Days / 365.0M;
                    y2 = curve[indexList[j]];

                    TimeSpan ts1 = indexList[j - 1] - indexList[0];
                    x1 = ts1.Days / 365.0M;
                    y1 = curve[indexList[j - 1]];

                    decimal k = (y2 - y1) / (x2 - x1);
                    decimal a = y1 - k * x1;

                    TimeSpan tst1 = t1 - indexList[0];
                    decimal t1xVal = Convert.ToDecimal(tst1.Days);
                    decimal val = k * t1xVal / 365.0M + a; //changed by damien (divide by 365)

                    return val;
                }
            }

            return 0;
        }

        /// <summary>
        /// 
        /// converts to date from string in DD/MM/YYYY format
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static DateTime ConvertToDateFromString(string s)
        {
            return DateTime.Parse(s);
        }

        /// <summary>
        /// Gets a curve element.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static KeyValuePair<int, decimal> GetCurveElement(int i, IDictionary<int, decimal> curve)
        {
            int j = 0;

            foreach (KeyValuePair<int, decimal> temp in curve)
            {
                if (j == i)
                {
                    return new KeyValuePair<int, decimal>(temp.Key, temp.Value);
                }

                ++j;
            }

            throw new IndexOutOfRangeException();
        }
    }
}