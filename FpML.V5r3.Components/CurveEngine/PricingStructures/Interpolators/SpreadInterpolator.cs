#region Using Directives

using System;
using Orion.Analytics.DayCounters;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.CurveEngine.PricingStructures.Interpolators
{
    /// <summary>
    /// A special interpolator for spreads.
    /// </summary>
    public class SpreadInterpolator : TermCurveInterpolator
    {
        /// <summary>
        /// The main ctor.
        /// </summary>
        /// <param name="termCurve">a term curve containing the spread values at each provided node.</param>
        /// <param name="baseDate">The base date.</param>
        public SpreadInterpolator(TermCurve termCurve, DateTime baseDate)
            : base(termCurve, baseDate, Actual365.Instance)
        {
        }

//        public static void AddDiscountFactor(DateTime dt, double discountFactor, TermCurve termCurve)
//        {
//            TermPoint termPoint = TermPointFactory.Create((decimal)discountFactor, dt);
//
//            TermCurveHelper.Add(termCurve, termPoint);
//        }
//
//        public static void AddPoint(DateTime dt, decimal value, TermCurve termCurve)
//        {
//            TermPoint termPoint = TermPointFactory.Create(value, dt);
//
//            TermCurveHelper.Add(termCurve, termPoint);
//        }

//        public static void UpdatePoint(DateTime dt, decimal value, TermCurve termCurve)
//        {
//            //            TermPoint termPoint = TermPointFactory.Create((decimal)discountFactor, dt);
//            //            
//            //            TermCurveHelper.Add(termCurve, termPoint);
//
//            //tc[dt] = df;
//
//            TermPoint tp = Find(termCurve, dt);
//
//            if (tp == null)
//            {
//                AddPoint(dt, value, termCurve);
//            }
//            else
//            {
//                tp.mid = value;
//            }
//        }

//        public static void UpdateDiscountFactor(DateTime dt, double discountFactor, TermCurve termCurve)
//        {
//            //            TermPoint termPoint = TermPointFactory.Create((decimal)discountFactor, dt);
//            //            
//            //            TermCurveHelper.Add(termCurve, termPoint);
//
//            //tc[dt] = df;
//
//            TermPoint termPoint = Find(termCurve, dt);
//
//            if (termPoint == null)
//            {
//                AddDiscountFactor(dt, discountFactor, termCurve);
//            }
//            else
//            {
//                termPoint.mid = (decimal)discountFactor;
//            }
//        }

//        private static TermPoint Find(TermCurve tc, DateTime dt)
//        {
//            foreach (TermPoint termPoint in tc.point)
//            {
//                if (TermPointHelper.GetDate(termPoint) == dt)
//                {
//                    return termPoint;
//                }
//
//            }
//
//            return null;
//        }
//
//        private static int FindLastIndex(TermCurve termCurve, DateTime dt)
//        {
//            int index = Array.FindIndex(termCurve.point,
//                    delegate(TermPoint termPoint)
//                    {
//                        return TermPointHelper.GetDate(termPoint) > dt;
//                    }
//                );
//
//
////            if (-1 == index)
////            {
////                return -1;
////            }
//
//
//            //            foreach (TermPoint tp in termCurve.point)
//            //            {
//            //                if (TermPointFactory.GetDate(tp) > dt)//don't have to care about exact match because we have Find method.
//            //                {
//            //                    return tp;
//            //                }
//            //
//            //            }
//
//            return index;
//        }
    }
}