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

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.Numerics.Distributions;

#endregion

namespace Highlander.Numerics.Counterparty
{
    public partial class EquityPCEAnalytics
    {

        private const int CSeed = 3151;
        private const int CPercentile = 95;

        /// <summary>
        /// Create merged grid of profile time slices and simulation points
        /// </summary>
        /// <param name="times"></param>
        /// <param name="stepSize"></param>
        /// <returns></returns>
        private static double[] CreateTimeGrid(double[] times, double stepSize)
        {
            int n = times.Length;
            double last = times[n - 1];
            List<double> mesh = new List<double>(times);        
            double time0 = stepSize;
            while (time0 < last)
            {
                time0 += stepSize;
                mesh.Add(time0);                
            }
            mesh.Sort();
            var nodups = mesh.Distinct().ToArray();
            return nodups;                                                          
        }

        /// <summary>
        /// Mean of an OU process
        /// </summary>
        /// <param name="theta"></param>
        /// <param name="kappa"></param>
        /// <param name="spot"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double OUMean(double kappa, double theta, double spot, double time)
        {
             double LNmean = theta + Math.Exp(-kappa * time) * (Math.Log(spot) - theta);
             return LNmean;
        }

        /// <summary>
        /// Variance of an OU process
        /// </summary>
        /// <param name="sigma"></param>
        /// <param name="kappa"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double OUVar(double sigma, double kappa, double time)
        {
            double vol = sigma* sigma/2.0/kappa * (1-Math.Exp(-2.0*kappa*time)) ;
            return vol;
        }

        /// <summary>
        /// Variance of a log-OU process
        /// </summary>
        /// <param name="spot"></param>
        /// <param name="sigma"></param>
        /// <param name="kappa"></param>
        /// <param name="theta"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double LNOUVar(double spot, double sigma, double kappa, double theta, double time)
        {
            double var = sigma * sigma / (2.0 * kappa) * (1 - Math.Exp(-2.0 * kappa * time));
            double term1 = Math.Exp(var) - 1;
            double term2 = Math.Exp(2 * (theta + Math.Exp(-kappa * time) * (Math.Log(spot) - theta)) + var);
            double lhs = term1 * term2;
            return lhs;
        }

        /// <summary>
        /// Log normal OU process upper bound
        /// </summary>
        /// <param name="spot"></param>
        /// <param name="confidence"></param>
        /// <param name="kappa"></param>
        /// <param name="theta"></param>
        /// <param name="sigma"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double LNOUUpperBound(double spot, double confidence, double kappa, double theta, double sigma, double time)
        {
            double mu = OUMean(kappa,theta, spot, time);            
            double z = InvCumulativeNormalDistribution.Function(confidence);
            double lhs = Math.Exp(mu + z * sigma * Math.Sqrt( (1 - Math.Exp(-2 * kappa * time)) / 2 / kappa ) );
            return lhs;
        }                                                                                             
    }
}
