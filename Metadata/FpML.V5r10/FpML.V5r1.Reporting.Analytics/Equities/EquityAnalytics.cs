using System;
using System.Collections.Generic;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Interpolations.Spaces;
using Orion.Analytics.Rates;
using FpML.V5r3.Codes;
using Orion.ModelFramework;
using Orion.Analytics.Interpolations.Points;
using Orion.Numerics.DataStructures;
using Orion.Numerics.Functions;


namespace Orion.Analytics.Equities
{
    /// <summary>
    /// Dividend cruve
    /// </summary>
    public static class EquityAnalytics 
    {

        /// <summary>
        /// Creates the curve.
        /// </summary>
        /// <param name="ratedays">The ratedays.</param>
        /// <param name="rateamts">The rateamts.</param>
        /// <param name="daybasis">The daybasis.</param>
        /// <returns></returns>
        public static DiscreteCurve CreateCurve(int[] ratedays, double[] rateamts, int daybasis)
        {
            int n=ratedays.Length;
            double[] rateyears = new double[n];
            for (int idx=0; idx<n;idx++)
            {
                rateyears[idx] = System.Convert.ToDouble(ratedays[idx])/daybasis;
            }
            return new DiscreteCurve(rateyears, rateamts);
        }        


        /// <summary>
        /// Gets the dividend discount factor.
        /// </summary>
        public static decimal GetDiscountFactor(decimal years, decimal rate, CompoundingFrequencyEnum cf)
        {
            double years0 = Convert.ToDouble(years);
            double rate0 = Convert.ToDouble(rate);
            double df = RateAnalytics.ZeroRateToDiscountFactor(rate0, years0, cf);
            decimal df0 = Convert.ToDecimal(df);
            return df0;         
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="pt">The pt.</param>
        /// <param name="divCurve">The div curve.</param>
        /// <returns></returns>
        public static double Value(IPoint pt, DiscreteCurve divCurve) 
        {          
            List<IPoint> pts = divCurve.GetPointList();
            foreach (IPoint item in pts)
            {
                if (item == pt)
                    return pt.FunctionValue;
            }
            return 0;           
        }

        /// <summary>
        /// Anns the yield.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="divCurve">The div curve.</param>
        /// <param name="ratetimes">The rate times.</param>
        /// <param name="rateamts">The rate amounts.</param>
        /// <param name="im">the im?</param>
        /// <param name="cf">The cf?</param>
        /// <returns></returns>
        public static double GetPVDivs(DateTime baseDate, DateTime targetDate, DiscreteCurve divCurve, double[] ratetimes, double[] rateamts, string im, string cf)
        {
            return GetPVDivs(baseDate, targetDate, divCurve, ratetimes, rateamts, im,
                             EnumParse.ToCompoundingFrequencyEnum(cf));
        }

        /// <summary>
        /// Anns the yield.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="divCurve">The div curve.</param>
        /// <param name="ratetimes">The rate times.</param>
        /// <param name="rateamts">The rate amounts.</param>
        /// <param name="im">the im?</param>
        /// <param name="cf">The cf?</param>
        /// <returns></returns>
        public static double GetPVDivs(DateTime baseDate, DateTime targetDate, DiscreteCurve divCurve, double[] ratetimes, double[] rateamts, string im, CompoundingFrequencyEnum cf)
        {
            List<IPoint> points = divCurve.GetPointList();
            double sum=0;

            var rateCurve = new InterpolatedCurve(new DiscreteCurve(ratetimes, rateamts), InterpolationFactory.Create(im), false);

            int t0 = (targetDate - baseDate).Days;
            double maturity = t0 / 365.0;           

            foreach (IPoint pt in points)
            {
                decimal t = Convert.ToDecimal(pt.GetX());
                decimal rate = Convert.ToDecimal(rateCurve.Value(pt));
                double df = Convert.ToDouble(GetDiscountFactor(t,rate, cf));
                if ((pt.GetX() <= maturity) & (pt.GetX() >0))
                   sum += pt.FunctionValue * df;
            }
            return sum;            
        }


        /// <summary>
        /// Anns the yield.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="divCurve">The div curve.</param>
        /// <param name="ratetimes">The rate times.</param>
        /// <param name="rateamts">The rate amounts.</param>
        /// <param name="im">the im?</param>
        /// <param name="cf">The cf?</param>
        /// <returns></returns>
        public static double GetPVDivs(double yearFraction1, double yearFraction2, int[] divdays, double[] divamts, int[] ratedays, double[] rateamts, string im, CompoundingFrequencyEnum cf, int daybasis)
        {
            DiscreteCurve divCurve = CreateCurve(divdays, divamts, daybasis);
            DiscreteCurve rc = CreateCurve(ratedays, rateamts, daybasis);

            List<IPoint> points = divCurve.GetPointList();
            double sum = 0;            
            var rateCurve = new InterpolatedCurve(rc, InterpolationFactory.Create(im), false);

            foreach (IPoint pt in points)
            {
                decimal t = Convert.ToDecimal(pt.GetX());
                decimal rate = Convert.ToDecimal(rateCurve.Value(pt));
                double df = Convert.ToDouble(GetDiscountFactor(t, rate, cf));
                if ((pt.GetX() <= yearFraction2) & (pt.GetX() > yearFraction1))
                    sum += pt.FunctionValue * df;
            }
            return sum;
        }


        /// <summary>
        /// Gets the PV divs lin365.
        /// </summary>
        /// <param name="yearFraction1">The year fraction1.</param>
        /// <param name="yearFraction2">The year fraction2.</param>
        /// <param name="divdays">The divdays.</param>
        /// <param name="divamts">The divamts.</param>
        /// <param name="ratedays">The ratedays.</param>
        /// <param name="rateamts">The rateamts.</param>
        /// <returns></returns>
        public static double GetPVDivsCCLin365(double yearFraction1, double yearFraction2, int[] divdays, double[] divamts, int[] ratedays, double[] rateamts)
        {
            return GetPVDivs(yearFraction1, yearFraction2, divdays, divamts, ratedays, rateamts, "LinearInterpolation", CompoundingFrequencyEnum.Continuous, 365);
        }


        /// <summary>
        /// Gets the yield lin365.
        /// </summary>
        /// <param name="spot">The spot.</param>
        /// <param name="yearFraction1">The year fraction1.</param>
        /// <param name="yearFraction2">The year fraction2.</param>
        /// <param name="divdays">The divdays.</param>
        /// <param name="divamts">The divamts.</param>
        /// <param name="ratedays">The ratedays.</param>
        /// <param name="rateamts">The rateamts.</param>
        /// <returns></returns>
        public static double GetYieldCCLin365(double spot, double yearFraction1, double yearFraction2, int[] divdays, double[] divamts, int[] ratedays, double[] rateamts)
        {
            double PVDivs1 = GetPVDivsCCLin365(0, yearFraction1, divdays, divamts, ratedays, rateamts);
            double PVDivs2 = GetPVDivsCCLin365(0, yearFraction2, divdays, divamts, ratedays, rateamts);
            double q1 = 0;
            double q2 = 0;
            if (yearFraction1 > 0)
                q1 = -1 / yearFraction1 * System.Math.Log(1 - PVDivs1 / spot);
            if (yearFraction2 >0)
                q2 = -1 / yearFraction2 * System.Math.Log(1 - PVDivs2 / spot);
            if (yearFraction1 != yearFraction2)
                return (q2 * yearFraction2 - q1 * yearFraction1) / (yearFraction2 - yearFraction1);
            else return 0;
        }


        /// <summary>
        /// Gets the rate lin365.
        /// </summary>
        /// <param name="yearFraction1">The year fraction1.</param>
        /// <param name="yearFraction2">The year fraction2.</param>
        /// <param name="ratedays">The ratedays.</param>
        /// <param name="rateamts">The rateamts.</param>
        /// <param name="daybasis">The daybasis.</param>
        /// <returns></returns>
        public static double GetRateCCLin365(double yearFraction1, double yearFraction2, int[] ratedays, double[] rateamts )
        {
            DiscreteCurve rc = CreateCurve(ratedays, rateamts, 365);
            var rateCurve = new InterpolatedCurve(rc, InterpolationFactory.Create("LinearInterpolation"), false);
            IPoint pt1 = new Point1D(yearFraction1);
            IPoint pt2 = new Point1D(yearFraction2);
            double rt1 = rateCurve.Value(pt1);
            double rt2 = rateCurve.Value(pt2);
            if (yearFraction1 != yearFraction2)
                return (rt2 * yearFraction2 - rt1 * yearFraction1) / (yearFraction2 - yearFraction1);
            else
                return 0;
        }

        /// <summary>
        /// Gets the DFCC lin365.
        /// </summary>
        /// <param name="yearFraction1">The year fraction1.</param>
        /// <param name="yearFraction2">The year fraction2.</param>
        /// <param name="ratedays">The ratedays.</param>
        /// <param name="rateamts">The rateamts.</param>
        /// <returns></returns>
        public static double GetDFCCLin365(double yearFraction1, double yearFraction2, int[] ratedays, double[] rateamts)
        {
            double r = GetRateCCLin365(yearFraction1, yearFraction2, ratedays, rateamts);
            decimal df2 = GetDiscountFactor(System.Convert.ToDecimal(yearFraction2), System.Convert.ToDecimal(r), CompoundingFrequencyEnum.Continuous);
            decimal df1 = GetDiscountFactor(System.Convert.ToDecimal(yearFraction1), System.Convert.ToDecimal(r), CompoundingFrequencyEnum.Continuous);
            return System.Convert.ToDouble(df2) / System.Convert.ToDouble(df1);
        }


        /// <summary>
        /// Gets the forward lin365.
        /// </summary>
        /// <param name="yearFraction">The year fraction.</param>
        /// <param name="divdays">The divdays.</param>
        /// <param name="divamts">The divamts.</param>
        /// <param name="ratedays">The ratedays.</param>
        /// <param name="rateamts">The rateamts.</param>
        /// <returns></returns>
        public static double GetForwardCCLin365(double spot, double yearFraction, int[] divdays, double[] divamts, int[] ratedays, double[] rateamts)
        {
            double pvdivs = GetPVDivsCCLin365(0,yearFraction,divdays,divamts,ratedays,rateamts);
            double r0 = GetRateCCLin365(0,yearFraction, ratedays, rateamts);
            double df = RateAnalytics.ZeroRateToDiscountFactor(r0, yearFraction, CompoundingFrequencyEnum.Continuous);
            return (spot-pvdivs)/df;
        }


        public static double GetForwardCCLin365(double spot,
                                                double yearFraction1,
                                                double yearFraction2,
                                                int[] divdays,
                                                double[] divamts,
                                                int[] ratedays,
                                                double[] rateamts)
        {           
            double f1 = GetForwardCCLin365(spot, yearFraction1, divdays, divamts, ratedays, rateamts);
            double f2 = GetForwardCCLin365(spot, yearFraction2, divdays, divamts, ratedays, rateamts);

            return f2 / f1*spot;
        }


        /// <summary>
        /// Gets the Wing volatlity
        /// </summary>
        /// <param name="volSurface">The vol surface.</param>
        /// <param name="time">The time.</param>
        /// <param name="strike">The strike.</param>
        /// <returns></returns>
        public static double GetWingValue(List<OrcWingParameters> wingParameterList, IInterpolation xInterp, double time, double strike)
        {
            double eps = 0.0001;
            IPoint pt1 = new Point2D(time, strike * (1 + eps));
            IPoint pt2 = new Point2D(time, strike * (1 - eps));

            double[] _years = new double[wingParameterList.Count];

            for (int idx = 0; idx < wingParameterList.Count; idx++)
            {
                _years[idx] = wingParameterList[idx].TimeToMaturity;
            }
            var volArray = new double[wingParameterList.Count];           
            for (int idx = 0; idx < wingParameterList.Count; idx++)
            {
                volArray[idx] = OrcWingVol.Value(strike, wingParameterList[idx]);
            }
            double res=0;
            if (volArray.Length > 1)
            {
                xInterp.Initialize(_years, volArray);
                res = xInterp.ValueAt(time, false);
            }
            else
                res = volArray[0];
            return res;
        }

        /// <summary>
        /// Gets the skew.
        /// </summary>
        /// <param name="wingParameterList">The wing parameter list.</param>
        /// <param name="xInterp">The x interp.</param>
        /// <param name="time">The time.</param>
        /// <param name="strike">The strike.</param>
        /// <returns></returns>
        public static double GetWingSkew(List<OrcWingParameters> wingParameterList, IInterpolation xInterp, double time, double strike)
        {
            const double eps = 0.0001;
            double f1 = GetWingValue(wingParameterList,xInterp,time,strike*(1+eps));
            double f0 = GetWingValue(wingParameterList,xInterp,time,strike*(1-eps));
            return (f1-f0) / (2*eps*strike);           
        }

    }
}
