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
using FpML.V5r10.EquityVolatilityCalculator.Pricing;

namespace FpML.V5r10.EquityVolatilityCalculator.Helpers
{
    /// <summary>
    /// Helper class for options
    /// </summary>
    public static class OptionHelper
    {
        /// <summary>
        /// Gets the implied vol.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <param name="exp">The exp.</param>
        /// <param name="volGuess">The vol guess.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="call">if set to <c>true</c> [call].</param>
        /// <param name="style">The style.</param>
        /// <param name="prem">The premium.</param>
        /// <param name="gridSteps">The grid steps.</param>
        /// <returns></returns>
        public static double GetImpliedVol(Stock stock, DateTime exp, double volGuess, double strike, bool call, string style, double prem, int gridSteps)
        {
            DateTime date0 = stock.Date;
            DateTime exp0 = exp;
            double spot0 = Convert.ToDouble(stock.Spot);
            double strike0 = strike;
            string paystyle0 = call ? "C" : "P";
            string exercise0 = style;

            //Override vol guess to test if it effects performance
         
            double fwd = stock.GetForward(date0, exp);
            var option = new AmOptionAnalytics(date0, exp0, spot0, strike0, volGuess, exercise0, paystyle0, stock.RateCurve, stock.Dividends, gridSteps);                        
            double vol0 = option.OptSolveVol(prem, fwd);
            return vol0;

        }

        /// <summary>
        /// Gets the implied vol.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <param name="exp">The exp.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="call">if set to <c>true</c> [call].</param>
        /// <param name="style">The style.</param>
        /// <param name="prem">The prem.</param>
        /// <param name="gridSteps">The grid steps.</param>
        /// <returns></returns>
        public static double GetImpliedVol(Stock stock, DateTime exp, double strike, bool call, string style, double prem, int gridSteps)
        {
            DateTime date0 = stock.Date;
            DateTime exp0 = exp;
            double spot0 = Convert.ToDouble(stock.Spot);
            double strike0 = strike;
            string paystyle0 = call ? "C" : "P";
            string exercise0 = style;

            //Override vol guess to test if it effects performance

            double fwd = stock.GetForward(date0, exp);
            double t = exp.Subtract(stock.Date).Days / 365.0;        
            double volGuess = Math.Sqrt(Math.Abs(Math.Log((fwd / strike) * 2 / t)));
            var option = new AmOptionAnalytics(date0, exp0, spot0, strike0, volGuess, exercise0, paystyle0, stock.RateCurve, stock.Dividends, gridSteps);
            double vol0 = option.OptSolveVol(prem, fwd);
            return vol0;

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
            VolatilitySurfaceHelper.UpdateDivsValuations(stock.Valuations, stock.Dividends);
            const double tradeDays = 250.0;
            double vol = 0.0;
            int n = 0;

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





    }
}
