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
    public class EquityPCEAnalytics
    {

        //private const int cSeed = 3151;
        private const int cPercentile = 95;

        /// <summary>
        /// Gets the collar PCE.
        /// </summary>
        /// <param name="ratedays">The ratedays.</param>
        /// <param name="rateamts">The rateamts.</param>
        /// <param name="divdays">The divdays.</param>
        /// <param name="divamts">The divamts.</param>
        /// <param name="volSurface">The vol surface.</param>
        /// <param name="spot">The spot.</param>
        /// <param name="callstrike">The callstrike.</param>
        /// <param name="putstrike">The putstrike.</param>
        /// <param name="maturity">The maturity of the collar</param>
        /// <param name="meanrev">The meanrev parameter used in the OU dynamics.</param>
        /// <param name="histvol">The historical vol estimate used in the OU dynamics.</param>
        /// <param name="timeSlice">The time slice.</param>
        /// <returns></returns>
        public static double[,] GetCollarPCE(string payoff,
                                    int[] ratedays,
                                    double[] rateamts,
                                    int[] divdays,
                                    double[] divamts,
                                    List<OrcWingParameters> volSurface,
                                    double spot,
                                    double callstrike,
                                    double putstrike,
                                    double maturity,
                                    double kappa,
                                    double theta,
                                    double sigma,
                                    double[] profiletimes, 
                                    double confidence,                                
                                    double tstepSize,
                                    int simulations,
                                    int seed)	
         
        {
            int nprofile = profiletimes.Length;        
            double[,] results = new double[nprofile,3];
            List<double> profileList = new List<double>(profiletimes);
            List<double> _profiletimes = profileList.FindAll(delegate(double item) { return (item <= maturity); });       
            List<RiskStatistics> res = Euler( payoff,
                                             ratedays,
                                             rateamts,
                                             divdays,
                                             divamts,
                                             volSurface,
                                             spot,
                                             callstrike,
                                             putstrike,
                                             maturity,
                                             _profiletimes.ToArray(),
                                             kappa,
                                             theta,
                                             sigma,
                                             confidence,
                                             tstepSize,
                                             simulations,
                                             seed);

            for (int idx = 0; idx < _profiletimes.Count; idx++)
            {
                Statistics.Statistics stats = new Statistics.Statistics();
                double[] sample = res[idx].Sample();
                double mean = res[idx].Mean;
                double stErrorEstimate = res[idx].ErrorEstimate;               
                results[idx, 0] = mean;
                results[idx, 1] = stats.Percentile(ref sample, cPercentile);       
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
        /// <param name="ratedays"></param>
        /// <param name="rateamts"></param>
        /// <param name="divdays"></param>
        /// <param name="divamts"></param>
        /// <param name="volSurface"></param>
        /// <param name="spot"></param>
        /// <param name="callstrike"></param>
        /// <param name="putstrike"></param>
        /// <param name="maturity"></param>
        /// <param name="profiletimes"></param>
        /// <param name="kappa"></param>
        /// <param name="theta"></param>
        /// <param name="sigma"></param>
        /// <param name="tStepSize"></param>
        /// <param name="sims"></param>
        /// <returns></returns>
        private static List<RiskStatistics> Euler(  string payoff,
                                                    int[] ratedays,
                                                    double[] rateamts,
                                                    int[] divdays,
                                                    double[] divamts,
                                                    List<OrcWingParameters> volSurface,
                                                    double spot,
                                                    double callstrike,
                                                    double putstrike,
                                                    double maturity,
                                                    double[] profiletimes,
                                                    double kappa,
                                                    double theta,
                                                    double sigma,
                                                    double confidence,
                                                    double tStepSize,
                                                    int sims,
                                                    int seed                        
                                                    )
        {           
            double[] times = CreateTimeGrid(profiletimes, tStepSize);
            int n = times.Length;
            int profilepoints = profiletimes.Length;
            double lns0 = Math.Log(spot);
            IBasicRng basRng = new MCG31vsl(seed);
            //IContinousRng unifRng = new UniformRng(basRng,0,1);
            BoxMullerGaussianRng gen = new BoxMullerGaussianRng(basRng, 0, 1);            
            double[] lns = new double[n];                   
            double[] results = new double[profilepoints];
            List<double> _profileTimeList = new List<double>(profiletimes);
            List<RiskStatistics> samples = new List<RiskStatistics>(profilepoints);
            for (int idx = 0; idx < profilepoints; idx++) {samples.Add(new RiskStatistics()); }                     
         
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

                    if (_profileTimeList.Contains(times[idx]))
                    {
                        double y = GetYValue(payoff ,ratedays, rateamts, divdays, divamts, volSurface, spot, Math.Exp(lns[idx]), callstrike, putstrike, times[idx], maturity, confidence);                               
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
        /// <param name="ratedays"></param>
        /// <param name="rateamts"></param>
        /// <param name="divdays"></param>
        /// <param name="divamts"></param>
        /// <param name="volSurface"></param>
        /// <param name="spot"></param>
        /// <param name="s"></param>
        /// <param name="callstrike"></param>
        /// <param name="putstrike"></param>
        /// <param name="t0"></param>
        /// <param name="maturity"></param>
        /// <returns></returns>
        private static double GetYValue(string function,
                                         int[] ratedays,
                                         double[] rateamts,
                                         int[] divdays,
                                         double[] divamts,
                                         List<OrcWingParameters> volSurface,
                                         double spot,
                                         double s,
                                         double callstrike,
                                         double putstrike,                                      
                                         double t0,
                                         double maturity,
                                         double confidence)
        {
            //double df = EquityAnalytics.GetDFCCLin365(0, t0, ratedays, rateamts);

            if (function == "CollarPCE")
                return CollarPCEFunction(ratedays, rateamts, divdays, divamts, volSurface, spot, s, callstrike, putstrike, t0, maturity);
            if (function == "Asset")
                return s;
            return 0.0;
        }

        /// <summary>
        /// Evaluate the PCE for a collar given the parameters passed below
        /// </summary>
        /// <param name="ratedays"></param>
        /// <param name="rateamts"></param>
        /// <param name="divdays"></param>
        /// <param name="divamts"></param>
        /// <param name="volSurface"></param>
        /// <param name="spot"></param>
        /// <param name="s"></param>
        /// <param name="callstrike"></param>
        /// <param name="putstrike"></param>
        /// <param name="t0"></param>
        /// <param name="maturity"></param>
        /// <returns></returns>
        private static double CollarPCEFunction( int[] ratedays,
                                    double[] rateamts,
                                    int[] divdays,
                                    double[] divamts,
                                    List<OrcWingParameters> volSurface,
                                    double spot,
                                    double s,
                                    double callstrike,
                                    double putstrike,                          
                                    double t0,
                                    double maturity)
        {
            t0 = Math.Min(t0, maturity); // cap profile point to maturity;
            double tau = maturity - t0;
            double r = EquityAnalytics.GetRateCCLin365(t0, maturity, ratedays, rateamts);
            double q = EquityAnalytics.GetYieldCCLin365(spot, t0, maturity, divdays, divamts, ratedays, rateamts);            
            double callvol = EquityAnalytics.GetWingValue(volSurface, new LinearInterpolation(), tau, callstrike);
            double putvol = EquityAnalytics.GetWingValue(volSurface, new LinearInterpolation(), tau, putstrike);          
            double fwd = s * Math.Exp((r - q) * tau);
            BlackScholes bs = new BlackScholes();
            double lhs = Math.Max(bs.BSprice(fwd, tau, callstrike, r, callvol, true) - bs.BSprice(fwd, tau, putstrike, r, putvol, false),0) ;
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
