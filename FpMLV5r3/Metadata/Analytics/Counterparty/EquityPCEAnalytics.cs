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
using System.Linq;
using Highlander.Reporting.Analytics.V5r3.Distributions;
using Highlander.Reporting.Analytics.V5r3.Equities;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Interpolations;
using Highlander.Reporting.Analytics.V5r3.Statistics;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Counterparty
{
    /// <summary>
    /// 
    /// </summary>
    public class EquityPCEAnalytics
    {

        //private const int cSeed = 3151;
        private const int CPercentile = 95;

        /// <summary>
        /// Gets the collar PCE.
        /// </summary>
        /// <param name="payoff"></param>
        /// <param name="rateDays">The rate days.</param>
        /// <param name="rateAmounts">The rate amounts.</param>
        /// <param name="dividendDays">The dividend days.</param>
        /// <param name="dividendAmounts">The dividend amounts.</param>
        /// <param name="volSurface">The vol surface.</param>
        /// <param name="spot">The spot.</param>
        /// <param name="callStrike">The call strike.</param>
        /// <param name="putStrike">The put strike.</param>
        /// <param name="maturity">The maturity of the collar</param>
        /// <param name="sigma"></param>
        /// <param name="profileTimes"></param>
        /// <param name="confidence"></param>
        /// <param name="tStepSize"></param>
        /// <param name="simulations"></param>
        /// <param name="seed"></param>
        /// <param name="kappa"></param>
        /// <param name="theta"></param>
        /// <returns></returns>
        public static double[,] GetCollarPCE(string payoff,
                                    int[] rateDays,
                                    double[] rateAmounts,
                                    int[] dividendDays,
                                    double[] dividendAmounts,
                                    List<OrcWingParameters> volSurface,
                                    double spot,
                                    double callStrike,
                                    double putStrike,
                                    double maturity,
                                    double kappa,
                                    double theta,
                                    double sigma,
                                    double[] profileTimes, 
                                    double confidence,                                
                                    double tStepSize,
                                    int simulations,
                                    int seed)	
         
        {
            int nProfile = profileTimes.Length;        
            double[,] results = new double[nProfile,3];
            List<double> profileList = new List<double>(profileTimes);
            List<double> times = profileList.FindAll(item => (item <= maturity));       
            List<RiskStatistics> res = Euler( payoff,
                                             rateDays,
                                             rateAmounts,
                                             dividendDays,
                                             dividendAmounts,
                                             volSurface,
                                             spot,
                                             callStrike,
                                             putStrike,
                                             maturity,
                                             times.ToArray(),
                                             kappa,
                                             theta,
                                             sigma,
                                             confidence,
                                             tStepSize,
                                             simulations,
                                             seed);

            for (int idx = 0; idx < times.Count; idx++)
            {
                Statistics.Statistics stats = new Statistics.Statistics();
                double[] sample = res[idx].Sample();
                double mean = res[idx].Mean;
                double stErrorEstimate = res[idx].ErrorEstimate;               
                results[idx, 0] = mean;
                results[idx, 1] = stats.Percentile(ref sample, CPercentile);       
                results[idx, 2] = stErrorEstimate; 
            }
            return results;
        }

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
        /// Run an Euler-like simulation to generate paths of the OU-process
        /// Evaluate function points using GetYValue()
        /// </summary>
        /// <param name="payoff"></param>
        /// <param name="rateDays"></param>
        /// <param name="rateAmounts"></param>
        /// <param name="dividendDays"></param>
        /// <param name="dividendAmounts"></param>
        /// <param name="volSurface"></param>
        /// <param name="spot"></param>
        /// <param name="callStrike"></param>
        /// <param name="putStrike"></param>
        /// <param name="maturity"></param>
        /// <param name="profileTimes"></param>
        /// <param name="kappa"></param>
        /// <param name="theta"></param>
        /// <param name="sigma"></param>
        /// <param name="confidence"></param>
        /// <param name="tStepSize"></param>
        /// <param name="sims"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        private static List<RiskStatistics> Euler(  string payoff,
                                                    int[] rateDays,
                                                    double[] rateAmounts,
                                                    int[] dividendDays,
                                                    double[] dividendAmounts,
                                                    List<OrcWingParameters> volSurface,
                                                    double spot,
                                                    double callStrike,
                                                    double putStrike,
                                                    double maturity,
                                                    double[] profileTimes,
                                                    double kappa,
                                                    double theta,
                                                    double sigma,
                                                    double confidence,
                                                    double tStepSize,
                                                    int sims,
                                                    int seed                        
                                                    )
        {           
            double[] times = CreateTimeGrid(profileTimes, tStepSize);
            int n = times.Length;
            int profilePoints = profileTimes.Length;
            double lns0 = Math.Log(spot);
            IBasicRng basRng = new MCG31vsl(seed);
            //IContinousRng unifRng = new UniformRng(basRng,0,1);
            BoxMullerGaussianRng gen = new BoxMullerGaussianRng(basRng, 0, 1);            
            double[] lns = new double[n];                   
            double[] results = new double[profilePoints];
            List<double> profileTimeList = new List<double>(profileTimes);
            List<RiskStatistics> samples = new List<RiskStatistics>(profilePoints);
            for (int idx = 0; idx < profilePoints; idx++) {samples.Add(new RiskStatistics()); }
            for (int kdx = 0;kdx<sims;kdx++)
            {
                int jdx = 0;               
                double wdt = gen.NextDouble() * Math.Sqrt(times[0]);
                lns[0] = theta + Math.Exp(-kappa * times[0]) * (lns0 - theta) + sigma * wdt ;         
                for (int idx = 1; idx < n; idx++)
                {
                    double dt = times[idx] - times[idx - 1];
                    wdt = gen.NextDouble() * Math.Sqrt(dt);
                    lns[idx] = theta + Math.Exp(-kappa * dt) * (lns[idx-1] - theta) + sigma * wdt;
                    if (profileTimeList.Contains(times[idx]))
                    {
                        double y = GetYValue(payoff ,rateDays, rateAmounts, dividendDays, dividendAmounts, volSurface, spot, Math.Exp(lns[idx]), callStrike, putStrike, times[idx], maturity, confidence);                               
                        samples[jdx++].Add(y);
                    }      
                }                               
            }
            return samples;
        }


        /// <summary>
        /// Return the y-value or function evaluation at our simulated point
        /// </summary>
        /// <param name="function"></param>
        /// <param name="ratedDays"></param>
        /// <param name="rateAmounts"></param>
        /// <param name="dividendDays"></param>
        /// <param name="dividendAmounts"></param>
        /// <param name="volSurface"></param>
        /// <param name="spot"></param>
        /// <param name="s"></param>
        /// <param name="callStrike"></param>
        /// <param name="putStrike"></param>
        /// <param name="t0"></param>
        /// <param name="maturity"></param>
        /// <param name="confidence"></param>
        /// <returns></returns>
        private static double GetYValue(string function,
                                         int[] ratedDays,
                                         double[] rateAmounts,
                                         int[] dividendDays,
                                         double[] dividendAmounts,
                                         List<OrcWingParameters> volSurface,
                                         double spot,
                                         double s,
                                         double callStrike,
                                         double putStrike,                                      
                                         double t0,
                                         double maturity,
                                         double confidence)
        {
            if (function == "CollarPCE")
                return CollarPCEFunction(ratedDays, rateAmounts, dividendDays, dividendAmounts, volSurface, spot, s, callStrike, putStrike, t0, maturity);
            if (function == "Asset")
                return s;
            return 0.0;
        }

        /// <summary>
        /// Evaluate the PCE for a collar given the parameters passed below
        /// </summary>
        /// <param name="rateDays"></param>
        /// <param name="rateAmounts"></param>
        /// <param name="dividendDays"></param>
        /// <param name="dividendAmounts"></param>
        /// <param name="volSurface"></param>
        /// <param name="spot"></param>
        /// <param name="s"></param>
        /// <param name="callStrike"></param>
        /// <param name="putStrike"></param>
        /// <param name="t0"></param>
        /// <param name="maturity"></param>
        /// <returns></returns>
        private static double CollarPCEFunction( int[] rateDays,
                                    double[] rateAmounts,
                                    int[] dividendDays,
                                    double[] dividendAmounts,
                                    List<OrcWingParameters> volSurface,
                                    double spot,
                                    double s,
                                    double callStrike,
                                    double putStrike,                          
                                    double t0,
                                    double maturity)
        {
            t0 = Math.Min(t0, maturity); // cap profile point to maturity;
            double tau = maturity - t0;
            double r = EquityAnalytics.GetRateCCLin365(t0, maturity, rateDays, rateAmounts);
            double q = EquityAnalytics.GetYieldCCLin365(spot, t0, maturity, dividendDays, dividendAmounts, rateDays, rateAmounts);            
            double callVolatility = EquityAnalytics.GetWingValue(volSurface, new LinearInterpolation(), tau, callStrike);
            double putVolatility = EquityAnalytics.GetWingValue(volSurface, new LinearInterpolation(), tau, putStrike);          
            double fwd = s * Math.Exp((r - q) * tau);
            BlackScholes bs = new BlackScholes();
            double lhs = Math.Max(bs.BSprice(fwd, tau, callStrike, r, callVolatility, true) - bs.BSprice(fwd, tau, putStrike, r, putVolatility, false),0) ;
            return lhs;
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
