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

#region Usings

using System;
using System.Collections.Generic;
using Highlander.Equities;
using Highlander.Reporting.Analytics.V5r3.Equities;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Spaces;
using Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities;
using Highlander.Reporting.ModelFramework.V5r3;
using SimpleStock = Highlander.Equities.SimpleStock;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Helpers
{
    public class ExtrapolationHelper
    {
        /// <summary>
        /// Does the extrapolation.
        /// </summary>
        /// <param name="asxParent">The ASX parent.</param>
        /// <param name="asxChild">The ASX child.</param>
        /// <param name="sdParent">The SD parent.</param>
        /// <param name="child">The child.</param>
        public static void DoExtrapolation(IStock asxParent, IStock asxChild, IStock sdParent, IVolatilitySurface child)
        {
           //foreach expiry in child, interpolate ratio at ASX parent, child and interpolate SD Parent and apply to SD Parent. 
            foreach (ForwardExpiry exp in child.NodalExpiry)
            {
                foreach (EquityStrike str in exp.Strikes)
                {
                    if (!str.VolatilityHasBeenSet)
                    {
                        double moneyness = str.StrikePrice / Convert.ToDouble(exp.FwdPrice);
                        double parentVol = GetVolAt(sdParent, exp.ExpiryDate, moneyness);
                        double scalingFactor = CalcExtrapolationFactor(asxParent, asxChild, exp.ExpiryDate);
                        IVolatilityPoint vp = new VolatilityPoint();
                        vp.SetVolatility(Convert.ToDecimal(parentVol * scalingFactor), VolatilityState.Default());
                        str.SetVolatility(vp);
                    }
                }
            }          
        }

        /// <summary>
        /// Calculates the extrapolation factor.
        /// </summary>
        /// <param name="asxParent">The ASX parent.</param>
        /// <param name="asxChild">The ASX child.</param>
        /// <param name="expiry">The expiry.</param>
        /// <returns></returns>
        public static double CalcExtrapolationFactor(IStock asxParent, 
                                       IStock asxChild,                                         
                                       DateTime expiry)                                                       
        {
            double parentVol = GetVolAt(asxParent, expiry, 1.0);
            double childVol = GetVolAt(asxChild, expiry, 1.0);         
            if (parentVol > 0)
                return childVol / parentVol;
            return 0.0;
        }

        /// <summary>
        /// Gets the vol at.
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="expiry">The expiry.</param>
        /// <param name="moneyness">The moneyness.</param>
        /// <returns></returns>
        public static double GetVolAt(IStock stock, DateTime expiry, double moneyness)
        {
            DateTime date0 = ((Stock)stock).Date;
            double fwd = ((Stock)stock).GetForward(date0,expiry);
            double y = fwd * moneyness;
            double x = expiry.Subtract(date0).Days / 365.0;
            IPoint point = new Point2D(x, y);
            stock.VolatilitySurface.SetInterpolatedCurve();
            ExtendedInterpolatedSurface interpolatedCurve = stock.VolatilitySurface.GetInterpolatedCurve();
            interpolatedCurve.Forward = fwd;
            interpolatedCurve.Spot = Convert.ToDouble(((Stock)stock).Spot);
            var vol = interpolatedCurve.Value(point);
            return vol;
        }

        /// <summary>
        /// Ares the no ET os.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <returns></returns>
        private static bool AreNoETOs(IStock stock)
        {
            foreach (ForwardExpiry expiry in stock.VolatilitySurface.Expiry)
            {
                var strikes = new List<EquityStrike>(expiry.Strikes);
                Strike match = strikes.Find(strikeItem => strikeItem.Volatility.Value > 0
                    );
                if (match != null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get historical scaling factor for lead stock and child stock
        /// </summary>
        /// <param name="leadStock">The lead stock.</param>
        /// <param name="childStock">The child stock.</param>
        /// <returns></returns>
        public static decimal GetHistoricalScaleFactor(IStock leadStock, IStock childStock)
        {
            decimal x = 0.0M;
            if (AreNoETOs(childStock))
            {
                decimal childVol = DoHistVolCalc(childStock);
                decimal parentVol = DoHistVolCalc(leadStock);
                if (parentVol != 0 & childVol != 0)
                    x = childVol / parentVol;                
            }
            return x;
        }

        /// <summary>
        /// Populates the historical volatility.
        /// </summary>
        /// <param name="leadStock">The lead stock.</param>
        /// <param name="childStock">The child stock.</param>
        /// <param name="child">The child.</param>
        public static void PopulateHistoricalVols(IStock leadStock, IStock childStock, IVolatilitySurface child)
        {
            decimal scalingFactor = GetHistoricalScaleFactor(leadStock, childStock);
            foreach (ForwardExpiry exp in child.NodalExpiry)
            {
                foreach (EquityStrike str in exp.Strikes)
                {
                    if (!str.VolatilityHasBeenSet)
                    {
                        double moneyness = str.StrikePrice / Convert.ToDouble(exp.FwdPrice);
                        double parentVol = GetVolAt(leadStock, exp.ExpiryDate, moneyness);
                        IVolatilityPoint vp = new VolatilityPoint();
                        vp.SetVolatility(Convert.ToDecimal(parentVol) * scalingFactor, VolatilityState.Default());
                        str.SetVolatility(vp);
                    }
                }
            }                      
        }

        /// <summary>
        /// Does the hist vol calc.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <returns></returns>
        public static decimal DoHistVolCalc(IStock stock)
        {
            double sumSq = 0.0;
            double sum = 0.0;
            UpdateDivsValuations(stock.Valuations, stock.Dividends);
            const double tradeDays = 250.0;
            double vol = 0.0;
            int n = 0;
            // Daily valuations
            for (int idx = 1; idx < stock.Valuations.Count; idx++)
            {
                if (!stock.Valuations[idx].ExDate)
                {
                    double p1 = Convert.ToDouble(stock.Valuations[idx - 1].Price);
                    double p2 = Convert.ToDouble(stock.Valuations[idx].Price);
                    double u = Math.Log(p2 / p1);
                    sumSq = sumSq + u * u;
                    sum = sum + u;
                    n += 1;
                }
            }
            if (stock.Valuations.Count > 2)
            {
                double xbar = sum / n;
                double sSq = 1.0 / (n - 1) * (sumSq - n * xbar * xbar);
                double s = Math.Sqrt(sSq);
                vol = s / (Math.Sqrt(1 / tradeDays));
            }
            return (new decimal(vol));            
        }

        /// <summary>
        /// Determines whether [is dividend ex date] [the specified valuation].
        /// </summary>
        /// <param name="valuations">The valuations.</param>
        /// <param name="dividends">The dividends.</param>
        /// <returns>
        /// 	<c>true</c> if [is dividend ex date] [the specified valuation]; otherwise, <c>false</c>.
        /// </returns>
        private static void UpdateDivsValuations(List<Valuation> valuations, IEnumerable<Dividend> dividends)
        {
            if (dividends != null)
            {
                foreach (Dividend dividend in dividends)
                {
                    Valuation match = valuations.Find(valuationItem => (valuationItem.Date == dividend.ExDate)
                        );
                    if (match != null)
                        match.ExDate = true;
                }
            }
        }
    }
}
