/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using Orion.Analytics.Distributions;

#endregion

namespace Orion.Analytics.Statistics
{
    /// <summary>
    /// Risk statistic tool.
    /// </summary>
    public class RiskStatistics : Accumulator
    {
        #region RiskStatistics / RiskMeasures

        /// <summary>
        /// Potential-Upside at a given percentile.
        /// </summary>
        /// <param name="percentile"></param>
        /// <returns></returns>
        public double PotentialUpside(double percentile)
        {
            return PotentialUpside( percentile,
                                    Mean, StandardDeviation);
        }

        /// <summary>
        /// Potential upside.
        /// </summary>
        /// <param name="percentile">Percentile must be in range 90%-100%.</param>
        /// <param name="mean"></param>
        /// <param name="std"></param>
        /// <returns></returns>
        public static double PotentialUpside(double percentile, double mean, double std)
        {
            if (percentile>=1.0 || percentile<0.9)
                throw new ArgumentOutOfRangeException(nameof(percentile));
            // Percentile (={0}) out of range 90%<=p<100%.//
            var gInverse =
                new InvCumulativeNormalDistribution(mean, std);
            // PotenzialUpSide must be a gain
            // this means that it has to be MAX(dist(percentile), 0.0)
            return Math.Max(gInverse.Value(percentile), 0.0);
        }

        /// <summary>
        /// Value-At-Risk at a given percentile.
        /// </summary>
        /// <param name="percentile"></param>
        /// <returns></returns>
        public double ValueAtRisk(double percentile) 
        {
            return ValueAtRisk( percentile,
                                Mean, StandardDeviation);
        }

        /// <summary>
        /// Value at Risk (VaR).
        /// </summary>
        /// <param name="percentile">Percentile must be in range 90%-100%.</param>
        /// <param name="mean"></param>
        /// <param name="std"></param>
        /// <returns></returns>
        public static double ValueAtRisk(double percentile, double mean, double std) 
        {
            if (percentile>=1.0 || percentile<0.9)
                throw new ArgumentOutOfRangeException(nameof(percentile));
            // Percentile (={0}) out of range 90%<=p<100%.
            var gInverse =
                new InvCumulativeNormalDistribution(mean, std);
            // VAR must be a loss
            // this means that it has to be MIN(dist(1.0-percentile), 0.0)
            // VAR must also be a positive quantity, so -MIN(*)
            return -Math.Min( gInverse.Value(1.0-percentile), 0.0 );
        }

        /// <summary>
        /// Expected Shortfall at a given percentile.
        /// </summary>
        /// <param name="percentile"></param>
        /// <returns></returns>
        public double ExpectedShortfall(double percentile) 
        {
            return ExpectedShortfall( percentile,
                                      Mean, StandardDeviation);
        }

        /// <summary>
        /// Expected shortfall.
        /// </summary>
        /// <param name="percentile">Percentile must be in range 90%-100%.</param>
        /// <param name="mean"></param>
        /// <param name="std"></param>
        /// <returns></returns>
        public static double ExpectedShortfall(double percentile, double mean, double std)  
        {

            if (percentile>=1.0 || percentile<0.9) 
                throw new ArgumentOutOfRangeException( nameof(percentile) );
            // Percentile (={0}) out of range 90%<=p<100%.
            var gInverse =
                new InvCumulativeNormalDistribution(mean, std);
            double var = gInverse.Value(1.0-percentile);
            var g = new NormalDistribution(mean, std);
            double result = mean - std*std*g.Value(var)/(1.0-percentile);
            // expectedShortfall must be a loss
            // this means that it has to be MIN(result, 0.0)
            // expectedShortfall must also be a positive quantity, so -MIN(*)
            return -Math.Min(result, 0.0);
        }

        /// <summary>
        /// Shortfall (observations below target).
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public double Shortfall(double target) 
        {
            return Shortfall( target,
                              Mean, StandardDeviation);
        }

        /// <summary>
        /// Shortfall.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="mean"></param>
        /// <param name="std"></param>
        /// <returns></returns>
        public static double Shortfall(double target, double mean, double std)  
        {
            var gIntegral =
                new CumulativeNormalDistribution(mean, std);
            return gIntegral.Value(target);
        }

        /// <summary>
        /// Average Shortfall (averaged shortfall).
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public double AverageShortfall(double target)  
        {
            return AverageShortfall( target,
                                     Mean, StandardDeviation);
        }

        /// <summary>
        /// Average shortfall.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="mean"></param>
        /// <param name="std"></param>
        /// <returns></returns>
        public static double AverageShortfall(double target, double mean, double std)  
        {
            var gIntegral =
                new CumulativeNormalDistribution(mean, std);
            var g = new NormalDistribution(mean, std);
            return ( (target-mean)*gIntegral.Value(target) + 
                     std*std*g.Value(target) );
        }

        #endregion
    }
}