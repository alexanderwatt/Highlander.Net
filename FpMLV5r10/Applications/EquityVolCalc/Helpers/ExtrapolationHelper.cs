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
using FpML.V5r10.Reporting.Analytics.Interpolations.Points;
using FpML.V5r10.Reporting.Analytics.Interpolations.Spaces;
using FpML.V5r10.Reporting.ModelFramework;

namespace FpML.V5r10.EquityVolatilityCalculator.Helpers
{
    public class ExtrapolationHelper
    {
        /// <summary>
        /// Does the extrap.
        /// </summary>
        /// <param name="asxParent">The ASX parent.</param>
        /// <param name="asxChild">The ASX child.</param>
        /// <param name="sdParent">The SD parent.</param>
        /// <param name="child">The child.</param>
        public void DoExtrap(IStock asxParent, IStock asxChild, IStock sdParent, IVolatilitySurface child)
        {
           //foreach expiry in child, interpolate ratio at ASX parent, child and interpolate SD Parent and apply to SD Parent. 
            foreach (ForwardExpiry exp in child.NodalExpiries)
            {
                foreach (Strike str in exp.Strikes)
                {
                    if (!str.VolatilityHasBeenSet)
                    {
                        double moneyness = str.StrikePrice / Convert.ToDouble(exp.FwdPrice);
                        double parentVol = GetVolAt(sdParent, exp.ExpiryDate, moneyness);
                        double scalingFactor = CalcExtrapFactor(asxParent, asxChild, exp.ExpiryDate);
                        IVolatilityPoint vp = new VolatilityPoint();
                        vp.SetVolatility(Convert.ToDecimal(parentVol * scalingFactor), VolatilityState.Default());
                        str.SetVolatility(vp);
                    }
                }
            }          
        }

        /// <summary>
        /// Calcs the extrap factor.
        /// </summary>
        /// <param name="asxParent">The ASX parent.</param>
        /// <param name="asxChild">The ASX child.</param>
        /// <param name="expiry">The expiry.</param>
        /// <returns></returns>
        public double CalcExtrapFactor(IStock asxParent, 
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
        public double GetVolAt(IStock stock, DateTime expiry, double moneyness)
        {
            DateTime date0 = ((Stock)stock).Date;
            double fwd = ((Stock)stock).GetForward(date0,expiry);
            double y = fwd * moneyness;
            double x = expiry.Subtract(date0).Days / 365.0;
            IPoint point = new Point2D(x, y);
            stock.VolatilitySurface.SetInterpolatedCurve();
            ExtendedInterpolatedSurface interpCurve = stock.VolatilitySurface.GetInterpolatedCurve();
            interpCurve.Forward = fwd;
            interpCurve.Spot = Convert.ToDouble(((Stock)stock).Spot);
            var vol = interpCurve.Value(point);
            return vol;
        }

        /// <summary>
        /// Ares the no ET os.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <returns></returns>
        private Boolean AreNoETOs(IStock stock)
        {
            foreach (ForwardExpiry expiry in stock.VolatilitySurface.Expiries)
            {
                var strikes = new List<Strike>(expiry.Strikes);
                Strike match = strikes.Find(strikeItem => strikeItem.Volatility.Value > 0
                    );
                if (match != null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get historical scaling factor for leadstock and child stock
        /// </summary>
        /// <param name="leadStock">The lead stock.</param>
        /// <param name="childStock">The child stock.</param>
        /// <returns></returns>
        public decimal GetHistoricalScalFactor(IStock leadStock, IStock childStock)
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
        /// Populates the historical vols.
        /// </summary>
        /// <param name="leadStock">The lead stock.</param>
        /// <param name="childStock">The child stock.</param>
        /// <param name="child">The child.</param>
        public void PopulateHistoricalVols(IStock leadStock, IStock childStock, IVolatilitySurface child)
        {
            decimal scalingFactor = GetHistoricalScalFactor(leadStock, childStock);
            foreach (ForwardExpiry exp in child.NodalExpiries)
            {
                foreach (Strike str in exp.Strikes)
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
        public decimal DoHistVolCalc(IStock stock)
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
            return (new Decimal(vol));            
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
