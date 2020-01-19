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

using System;
using System.Collections.Generic;
using FpML.V5r10.EquityVolatilityCalculator.Exception;
using FpML.V5r10.Reporting.Analytics.Interpolations;
using FpML.V5r10.Reporting.Analytics.Interpolations.Points;
using FpML.V5r10.Reporting.Analytics.Interpolations.Spaces;
using FpML.V5r10.Reporting.Analytics.Rates;
using FpML.V5r10.Reporting.ModelFramework;

namespace FpML.V5r10.EquityVolatilityCalculator
{

    public class RateCurve
    {
        /// <summary>
        /// Gets or sets the type of the rate.
        /// </summary>
        /// <value>The type of the rate.</value>
        public string RateType { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public int RatePoints { get; set; }

        /// <summary>
        /// Base date
        /// </summary>
        public DateTime BaseDate { get; set; }

        /// <summary>
        /// Gets or sets the rate array.
        /// </summary>
        /// <value>The rate array.</value>
        public double[] RateArray { get; set; }

        /// <summary>
        /// Array of times in years
        /// </summary>
        /// <value>The time array.</value>
        public DateTime[] DateArray { get; set; }

        /// <summary>
        /// Array of times in years
        /// </summary>
        /// <value>The time array.</value>
        public int[] DaysArray { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        /// <value>The currency.</value>
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the type of the interpolation.
        /// </summary>
        /// <value>The type of the interpolation.</value>
        public string InterpolationType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="rateType"></param>
        /// <param name="basedate">The basedate.</param>
        /// <param name="dates"></param>
        /// <param name="rates">The rates.</param>
        public RateCurve(string currency, String rateType, DateTime basedate, DateTime[] dates, double[] rates)
        {
            InterpolationType = "LinearInterpolation";
            RateType = rateType;
            Currency = currency;
            BaseDate = basedate;
            int n1 = dates.Length;
            int n2 = rates.Length;
            RatePoints = n1;
            if (n1 != n2) throw new InvalidValueException("Rate ranges must be of the same length");
            RateArray = new double[RatePoints];
            DaysArray = new int[RatePoints];
            DateArray = new DateTime[RatePoints];
            dates.CopyTo(DateArray, 0);
            rates.CopyTo(RateArray, 0);
            for (int idx = 0; idx < RatePoints; idx++)
            {
                DaysArray[idx] = dates[idx].Subtract(basedate).Days;
            }            
        }

        /// <summary>
        /// Gets the df.
        /// </summary>
        /// <param name="days">The days.</param>
        /// <returns></returns>
        public decimal GetDf(int days)
        {           

            var rateCurve = new InterpolatedCurve(new DiscreteCurve( GetYearsArray(),
                                                                RateArray), InterpolationFactory.Create(InterpolationType), false);
            double maturityYrs = days / 365.0;
            IPoint point = new Point1D(maturityYrs);         
            double rate = rateCurve.Value(point);
            double df = RateAnalytics.ZeroRateToDiscountFactor(rate, maturityYrs, RateType);
            decimal df0 = Convert.ToDecimal(df);
            return df0;
        }

        /// <summary>
        /// Creates the curve.
        /// </summary>
        /// <returns></returns>
        internal IDictionary<int, decimal> CreateCurve(int[] times, double[] amounts)
        {
            IDictionary<int, decimal> curve = new Dictionary<int, decimal>();
            for (int idx = 0; idx < times.Length; idx++)
            {
                curve.Add(times[idx], Convert.ToDecimal(amounts[idx]));
            }
            return curve;
        }


        /// <summary>
        /// Returns simple interest ACT/365 forward rate
        /// </summary>
        /// <param name="tdays1"></param>
        /// <param name="tdays2"></param>
        /// <returns></returns>
        public decimal ForwardRate(int tdays1, int tdays2)
        {
            //IDictionary<int, decimal> curve = CreateCurve(_DaysArray,_RateArray);
            //return CurveAnalytics.ForwardRate(tdays1, tdays2, curve);
            const decimal dayBasis = 365.0M;
            decimal df1 = GetDf(tdays1);
            decimal df2 = GetDf(tdays2);
            decimal forwardRate = (df1 / df2 - 1.0M) / ((tdays2 - tdays1) / dayBasis);
            return forwardRate;
        }

        /// <summary>
        /// Get years arrays this instance.
        /// </summary>
        /// <returns></returns>
        public double[] GetYearsArray()
        {
            int n = DaysArray.Length;
            var yearfrac = new double[n];
            for (int i = 0; i < n; i++)
            {
                yearfrac[i] = DaysArray[i] / 365.0;
            }
            return yearfrac;
        }
    }
}
