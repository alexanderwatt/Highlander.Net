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

using System;
using System.Collections.Generic;
using Highlander.Reporting.Analytics.V5r3.Interpolations;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Spaces;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Utilities.Exception;

namespace Highlander.Reporting.Analytics.V5r3.Rates
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
        /// <param name="baseDate">The base date.</param>
        /// <param name="dates"></param>
        /// <param name="rates">The rates.</param>
        public RateCurve(string currency, string rateType, DateTime baseDate, DateTime[] dates, double[] rates)
        {
            InterpolationType = "LinearInterpolation";
            RateType = rateType;
            Currency = currency;
            BaseDate = baseDate;
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
                DaysArray[idx] = dates[idx].Subtract(baseDate).Days;
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
        /// <param name="tDays1"></param>
        /// <param name="tDays2"></param>
        /// <returns></returns>
        public decimal ForwardRate(int tDays1, int tDays2)
        {
            const decimal dayBasis = 365.0M;
            decimal df1 = GetDf(tDays1);
            decimal df2 = GetDf(tDays2);
            decimal forwardRate = (df1 / df2 - 1.0M) / ((tDays2 - tDays1) / dayBasis);
            return forwardRate;
        }

        /// <summary>
        /// Get years arrays this instance.
        /// </summary>
        /// <returns></returns>
        public double[] GetYearsArray()
        {
            int n = DaysArray.Length;
            var yearFraction = new double[n];
            for (int i = 0; i < n; i++)
            {
                yearFraction[i] = DaysArray[i] / 365.0;
            }
            return yearFraction;
        }
    }
}
