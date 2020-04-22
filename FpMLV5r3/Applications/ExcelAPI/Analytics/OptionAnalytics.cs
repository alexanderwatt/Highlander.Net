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

#region Using Directives

using System;
using System.Runtime.InteropServices;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.Options;
using HLV5r3.Helpers;
using Microsoft.Win32;

#endregion

namespace HLV5r3.Analytics
{
    /// <summary>
    /// The black-scholes option class.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("B0F8D6FE-CE26-41CF-9C70-DEB57A66A66A")]
    public class Options
    {
        #region Registration

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComRegisterFunction]
        public static void RegisterFunction(Type type)
        {
            Registry.ClassesRoot.CreateSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"));
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(ApplicationHelper.GetSubKeyName(type, "InprocServer32"), true);
            key.SetValue("", System.Environment.SystemDirectory + @"\mscoree.dll", RegistryValueKind.String);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComUnregisterFunction]
        public static void UnregisterFunction(Type type)
        {
            Registry.ClassesRoot.DeleteSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"), false);
        }

        #endregion
        
        #region Constructor

        #endregion

        #region Functions

        ///<summary>
        /// Gets the caplet value.
        ///</summary>
        ///<param name="floatRate">The floating rate.</param>
        ///<param name="strikeRate">The strike rate.</param>
        ///<param name="volatility">The lognormal volatility.</param>
        ///<param name="timeToExpiry">The time to expiry.</param>
        ///<returns>The caplet value using BSM.</returns>
        public static double GetCapletValue(double floatRate, double strikeRate, double volatility, double timeToExpiry)
        {
            return BlackModel.GetCapletValue(floatRate, strikeRate, volatility, timeToExpiry);
        }

        /// <summary>
        /// Gets the swaption value.
        /// </summary>
        ///<param name="rate">The floating rate.</param>
        ///<param name="strikeRate">The strike rate.</param>
        ///<param name="volatility">The lognormal volatility.</param>
        ///<param name="timeToExpiry">The time to expiry.</param>
        ///<returns>The swaption value using BSM.</returns>
        public static double GetSwaptionValue(double rate, double strikeRate, double volatility, double timeToExpiry)
        {
            var model = BlackScholesMertonModel.GetSwaptionValue(rate, strikeRate, volatility, timeToExpiry);
            return model;
        }

        /// <summary>
        /// Gets the call option value.
        /// </summary>
        ///<param name="floatRate">The floating rate.</param>
        ///<param name="strikeRate">The strike rate.</param>
        ///<param name="volatility">The lognormal volatility.</param>
        ///<param name="timeToExpiry">The time to expiry.</param>
        ///<returns>The call value using BSM.</returns>
        public static decimal GetCallOptionValue(decimal floatRate, decimal strikeRate, decimal volatility, decimal timeToExpiry)
        {
            var model = BlackScholesMertonModel.GetCallOptionValue(floatRate, strikeRate, volatility, timeToExpiry);
            return model;
        }

        /// <summary>
        /// Gets the put option value.
        /// </summary>
        ///<param name="floatRate">The floating rate.</param>
        ///<param name="strikeRate">The strike rate.</param>
        ///<param name="volatility">The lognormal volatility.</param>
        ///<param name="timeToExpiry">The time to expiry.</param>
        ///<returns>The put value using BSM.</returns>
        public static decimal GetPutOptionValue(decimal floatRate, decimal strikeRate, decimal volatility, decimal timeToExpiry)
        {
            var model = BlackScholesMertonModel.GetPutOptionValue(floatRate, strikeRate, volatility, timeToExpiry);
            return model;
        }

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// r = Continuously compounded interest rate between now and time t.
        /// Discount factor is exp(-r * t).
        /// </summary>
        /// <param name="callFlag">The call/put flag.</param>
        /// <param name="fwdPrice">Price fixed today for purchase of asset at time t</param>
        /// <param name="strike">Exercise price of option</param>
        /// <param name="vol">Per cent volatility in units of (year)^(-1/2)</param>
        /// <param name="t">Time in years to the maturity of the option.</param>
        /// <returns>An array of results for Black Scholes.</returns>
        public static object Greeks(bool callFlag, double fwdPrice, double strike, double vol, double t)
        {
            var model = BlackScholesMertonModel.Greeks(callFlag, fwdPrice, strike, vol, t);
            return model;
        }

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// r = Continuously compounded interest rate between now and time t.
        /// Discount factor is exp(-r * t).
        /// Different combinations in inputs to the generalized model instantiate different models:
        /// b=r (the cost of carry rate = the risk free rate). Black Scholes 1973 stock option model.
        /// b=r-q, where q is the continuous dividend yield. Merton 1973 stock option model.
        /// b=0. The Black 1976 futures option model.
        /// b=0 and r=0. Assay 1982 margined futures option model.
        /// b=r - rf, rf being the foreign rate. Garman Kohlhagen 1983 currency option model.
        /// </summary>
        /// <param name="callFlag">The call/put flag.</param>
        /// <param name="price">The stock price S. Price fixed today for purchase of asset at time t</param>
        /// <param name="strike">The strike price K. Exercise price of option</param>
        /// <param name="rate">The risk free rate.</param>
        /// <param name="costOfCarry">The cost of carry rate.</param>
        /// <param name="vol">Volatility of the relative price change of the underlying asset S. 
        /// Per cent volatility in units of (year)^(-1/2)</param>
        /// <param name="t">Time in years to the maturity of the option.</param>
        /// <returns>An array of results for Black Scholes.</returns>
        public static object BSMGeneralisedWithGreeks(bool callFlag, double price, double strike,
                                                      double rate, double costOfCarry, double vol, double t)
        {
            var model = BlackScholesMertonModel.BSMGeneralisedWithGreeks(callFlag, price, strike, rate, costOfCarry, vol, t);
            return model;
        }

        /// <summary>
        /// A simple Cox-Rubinstein pricing of an option for European exercise
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="spotPrice"></param>
        /// <param name="fwdPrice"></param>
        /// <param name="nSteps"></param>
        /// <param name="strike"></param>
        /// <param name="vol"></param>
        /// <param name="df"></param>
        /// <param name="t"></param>
        /// <returns>The value of the cox european option.</returns>
        public static double CoxEuroOption(bool callFlag, double spotPrice, double fwdPrice, short nSteps,
                                           double strike, double vol, double df, double t)
        {
            return OptionAnalytics.CoxEuroOption(callFlag, spotPrice, fwdPrice,nSteps,
                                           strike, vol, df, t);
        }

        /// <summary>
        /// A simple solver.
        /// </summary>
        /// <param name="vol">The volatility.</param>
        /// <param name="t">The time to expiry.</param>
        /// <param name="b">The b parameter.</param>
        /// <param name="nSteps">The number of steps to use.</param>
        /// <returns>The calculated solution.</returns>
        public static double SolveU(double vol, double t, double b, short nSteps)
        {
            return OptionAnalytics.SolveU(vol, t, b, nSteps);
        }

        /// <summary>
        ///  A simple Cox-Rubinstein pricing of an option for European exercise, 
        /// with the correct "up" factor" (seems to make v. little difference) ...
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="spotPrice"></param>
        /// <param name="fwdPrice"></param>
        /// <param name="nSteps"></param>
        /// <param name="strike"></param>
        /// <param name="vol"></param>
        /// <param name="df"></param>
        /// <param name="t"></param>
        /// <returns>The value.</returns>
        public static double CoxEuroOption1(bool callFlag, double spotPrice, double fwdPrice, short nSteps, 
                                            double strike, double vol, double df, double t)
        {
            var result = OptionAnalytics.CoxEuroOption1(callFlag, spotPrice, fwdPrice, nSteps,
                                                       strike, vol, df, t);
            return result;
        }

        /// <summary>
        /// Simple Cox-Rubinstein pricing of an option for European exercise, with probabilities of exactly 1/2 ...
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="fwdPrice"></param>
        /// <param name="nSteps"></param>
        /// <param name="strike"></param>
        /// <param name="vol"></param>
        /// <param name="df"></param>
        /// <param name="t"></param>
        /// <returns>The value.</returns>
        public static double CoxEuroOption2(bool callFlag, double fwdPrice, short nSteps, double strike, 
                                            double vol, double df, double t)
        {
            var result = OptionAnalytics.CoxEuroOption2(callFlag, fwdPrice, nSteps,
                                                       strike, vol, df, t);
            return result;
        }

        /// <summary>
        /// Simple Cox-Rubinstein pricing of an option for European exercise, 
        /// with probabilities of exactly 1/2, using Black-Scholes valuation for the final time step ... 
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="fwdPrice"></param>
        /// <param name="nSteps"></param>
        /// <param name="strike"></param>
        /// <param name="vol"></param>
        /// <param name="df"></param>
        /// <param name="t"></param>
        /// <returns>The value.</returns>
        public static double CoxEuroOption3(bool callFlag, double fwdPrice, short nSteps, 
                                            double strike, double vol, double df, double t)
        {
            var result = OptionAnalytics.CoxEuroOption3(callFlag, fwdPrice, nSteps,
                                                       strike, vol, df, t);
            return result;
        }

        /// <summary>
        /// European option obtained by approximating log normal integral with a sum ... 
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="fwdPrice"></param>
        /// <param name="nSteps"></param>
        /// <param name="strike"></param>
        /// <param name="vol"></param>
        /// <param name="df"></param>
        /// <param name="t"></param>
        /// <returns>The value</returns>
        public static double CoxEuroOption4(bool callFlag, double fwdPrice, short nSteps, 
                                            double strike, double vol, double df, double t)
        {
            var result = OptionAnalytics.CoxEuroOption4(callFlag, fwdPrice, nSteps,
                                                        strike, vol, df, t);         
            return result;
        }

        /// <summary>
        /// Simple Cox-Ross-Rubinstein binomial tree model for American exercise
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="spotPrice"></param>
        /// <param name="fwdPrice"></param>
        /// <param name="nSteps"></param>
        /// <param name="strike"></param>
        /// <param name="vol"></param>
        /// <param name="df"></param>
        /// <param name="t"></param>
        /// <returns>The value.</returns>
        public static double CoxAmerOption(bool callFlag, double spotPrice, double fwdPrice, short nSteps,
                                           double strike, double vol, double df, double t) 
        {
            return OptionAnalytics.CoxAmerOption( callFlag,  spotPrice,  fwdPrice,  nSteps,
                                            strike,  vol,  df,  t);
        }

        /// <summary>
        /// THe Cox futures american model.
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="fwdPrice"></param>
        /// <param name="nSteps"></param>
        /// <param name="strike"></param>
        /// <param name="vol"></param>
        /// <param name="df"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double CoxFuturesAmerOption(bool callFlag, double fwdPrice, short nSteps,
                                                  double strike, double vol, double df, double t)
        {
            return OptionAnalytics.CoxFuturesAmerOption(callFlag, fwdPrice, nSteps,
                                            strike, vol, df, t);
        }

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// </summary>
        /// <param name="callFlag">Flag for Put or Call.</param>
        /// <param name="fwdPrice">Price fixed today for purchase of asset at time t.</param>
        /// <param name="strike">Exercise price of option.</param>
        /// <param name="vol">Per cent volatility in units of (year)^(-1/2).</param>
        /// <param name="t">Time in years to the maturity of the option.</param>
        /// <returns>The Black-Scholes value.</returns>
        public static double Opt(bool callFlag, double fwdPrice, double strike, double vol, double t)
        {
            return OptionAnalytics.Opt(callFlag, fwdPrice, 
                                            strike, vol, t);
        }

        ///// <summary>
        ///// Cumulative normal distribution,
        ///// i.e. {1 \over \sqrt{2\pi}} \int_{-\infty}^x du exp(-u^2/2).
        ///// Max. error 7.5e-8 
        ///// </summary>
        ///// <param name="x"></param>
        ///// <returns></returns>
        //[ExcelName("CumulativeNormalDistribution")]
        //public static double cn(double x)
        //{
        //    if (x == 0) return .5;
        //    var t = 1 / (1 + .2316419 * Math.Abs(x));
        //    t *= OOR2PI * Math.Exp(-.5 * x * x) * (.31938153 + t * (-.356563782 +
        //                                                            t * (1.781477937 + t * (-1.821255978 + t * 1.330274429))));
        //    return (x >= 0 ? 1 - t : t);
        //}

        ///// <summary>
        ///// Calculate exp(x^2/2) * cn(x)
        ///// </summary>
        ///// <param name="x"></param>
        ///// <returns></returns>
        //[ExcelName("CumulativeNormalExp")]
        //public static double cnbar(double x)
        //{
        //    var t = 2.828427125 / (2.828427125 + Math.Abs(x));
        //    t = t * Math.Exp(-1.958659411 + t * (1.00002368 + t * (.37409196 +
        //                                                           t * (.09678418 + t * (-.18628806 + t * (.27886807 + t * (-1.13520398 +
        //                                                                                                                    t * (1.48851587 + t * (-.82215223 + t * .17087277)))))))));
        //    return (x >= 0 ? Math.Exp(.5 * x * x) - t : t);
        //}

        ///// <summary>
        ///// The integral I1 used above is given by I1(u, a) =
        ///// \int_{-\infty}^u dx {a exp(-x^2/2) \over a^2 + x^2}.
        ///// We integrate from zero, since the integrand is even in x and
        ///// \int_0^\infty dx {a exp(-x^2/2) \over a^2 + x^2} = \pi exp(a^2/2) cn(-a)
        ///// The integral \int _0 ^u dx {a exp(-x^2/2) \over a^2  + x^2}.
        ///// (where u > 0), is done using Simpson's rule.   
        ///// </summary>
        ///// <param name="u"></param>
        ///// <param name="a"></param>
        ///// <returns></returns>
        //[ExcelName("SimpsonsRuleIntegration")]
        //public static double SimpsonsRuleIntegration(double u, double a)
        //{
        //    var result = new SimpsonsRuleIntegration();
        //    return 0.0;
        //}

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// r = Continuously compounded interest rate between now and time t.
        /// Discount factor is exp(-r * t).
        /// </summary>
        /// <param name="callFlag">The call/put flag.</param>
        /// <param name="fwdPrice">Price fixed today for purchase of asset at time t</param>
        /// <param name="strike">Exercise price of option</param>
        /// <param name="vol">Per cent volatility in units of (year)^(-1/2)</param>
        /// <param name="t">Time in years to the maturity of the option.</param>
        /// <returns>An array of reuslts for Black Scholes.</returns>
        public static double[,] BlackScholesWithGreeks(bool callFlag, double fwdPrice, double strike, double vol, double t)//TODO put back rate.
        {
            var result = OptionAnalytics.BlackScholesWithGreeks(callFlag, fwdPrice, strike, vol, t);
            return result;
        }

        /// <summary>
        /// Solves for the B-S volatility.
        /// </summary>
        /// <param name="callFlag">The call/put flag.</param>
        /// <param name="fwdPrice">The forward price of the asset.</param>
        /// <param name="strike">The strike of the asset.</param>
        /// <param name="prem">The premium value for the option to solve for.</param>
        /// <param name="r">The continuously compounding zero rate.</param>
        /// <param name="t">The time to expiry.</param>
        /// <returns>The volatility for that price.</returns>
        public static double OptSolveVol(bool callFlag, double fwdPrice, double strike, double prem, double r, double t)
        {
            var result = OptionAnalytics.OptSolveVol(callFlag, fwdPrice, strike, prem, r, t);
            return result;
        }

        /// <summary>
        /// To find a value for fwdPrice (the price to fix now for delivery of asset at time
        /// t), which gives a premium prem for an option of strike strike expiring at time t
        /// with volatility vol and continuously compounded interest rate 0->t of r.
        /// </summary>
        /// <param name="callFlag">Call or put flag.</param>
        /// <param name="strike">the strike.</param>
        /// <param name="vol">The volatility</param>
        /// <param name="r">The continuously compounded interest rate.</param>
        /// <param name="t">The time to expiry.</param>
        /// <param name="premium">The premium of the option.</param>
        /// <returns>The forward value for that price and volatility.</returns>
        public static double OptSolveFwd(bool callFlag, double strike, double vol, double r, double t, double premium)
        {
            var result = OptionAnalytics.OptSolveFwd(callFlag, strike, vol, r, t, premium);
            return result;
        }

        /// <summary>
        /// Valuation of an option on an option:      
        /// </summary>
        /// <param name="callOnOptionFlag">Boolean for option on option being call/put</param>
        /// <param name="strikeS">strike of option on option</param>
        /// <param name="rS">exp(-rS.tS) is DF from now to time tS</param>
        /// <param name="tS">Time to expiry of option on option (years)</param>
        /// <param name="callFlag">Boolean for underlying option being call/put</param>
        /// <param name="strikeL">strike of underlying option</param>
        /// <param name="rL">exp(-rL.tL) is DF from now to time tL</param>
        /// <param name="tL">Time to expiry of underlying option (years; must be greater than tS)</param>
        /// <param name="fwdPrice">Outright: to pay now for assured delivery of asset at tL</param>
        /// <param name="vol">The volatility.</param>
        /// <returns>The order of the return types is: Premium, Delta, Gamma, Vega, ThetaS, ThetaL, RhoS, RhoL</returns>
        public static double[] CompoundOpt(bool callOnOptionFlag, double strikeS, double rS, double tS,
            bool callFlag, double strikeL, double rL, double tL, double fwdPrice, double vol)
        {
            var result = OptionAnalytics.CompoundOpt(callOnOptionFlag,  strikeS,  rS,  tS, 
                                            callFlag,  strikeL,  rL,  tL,  fwdPrice,  vol);
            return result;
        }

        ///// <summary>
        ///// Numerical integration of the lognormal, i.e.
        ///// I = \int_L^U pd(S) dS func(S)
        ///// where pd(S) is the measure exp(-(ln(S/S0)+vol^2*t/2)^2/(2*vol^2*t))/(S*vol*sqrt(2*pi*t))
        ///// which applies to a random walk of volatility vol after time t such that
        ///// S = S0.
        ///// Uses Simpson's rule evaluation, after a strategic change of variables.       
        ///// </summary>
        ///// <param name="nSteps">Max no. of ordinates; if zero or negative no limit;</param>
        ///// <param name="fn">The function of S to be evaluated</param>
        ///// <param name="L">The lower limit (minimum 0)</param>
        ///// <param name="U">The upper limit: -ve value means infinity</param>
        ///// <param name="q">volatility * sqrt(t)</param>
        ///// <param name="S0">The expectation value of the underlying at time t</param>
        ///// <param name="parameters">The parameter Vector of the multivariate function:  the first parameter elements is the asset price S.</param>
        ///// <returns>A vector array of results: value, 
        ///// delta = {\partial I \over \partial S_0}, 
        ///// gamma = {\partial^2 I \over \partial S_0^2}, 
        ///// vega = {\partial I \over \partial q}, 
        ///// nSteps = The actual number used is returned here.</returns>
        //[ExcelName("LogNormalIntegration2")]
        //public static double[] LogNormInt2(long nSteps, MultivariateRealFunction fn,
        //                                   double L, double U, double q, double S0, Vector parameters)
        //{
        //    var result = OptionAnalytics.OptSolveFwd(callFlag, strike, vol, r, t, prem);

        //    return result;
        //}

        ///// <summary>
        ///// Numerical integration of the lognormal, i.e.
        ///// I = \int_L^U pd(S) dS func(S)
        ///// where pd(S) is the measure exp(-(ln(S/S0)+vol^2*t/2)^2/(2*vol^2*t))/(S*vol*sqrt(2*pi*t))
        ///// which applies to a random walk of volatility vol after time t such that
        ///// S = S0.
        ///// Uses Simpson's rule evaluation, after a strategic change of variables.
        ///// </summary>
        ///// <param name="nSteps">Max no. of ordinates; if zero or negative no limit;</param>
        ///// <param name="fn">The function of S to be evaluated</param>
        ///// <param name="L">The lower limit (minimum 0)</param>
        ///// <param name="U">The upper limit: -ve value means infinity</param>
        ///// <param name="q">volatility * sqrt(t)</param>
        ///// <param name="S0">The expectation value of the underlying at time t</param>
        ///// <param name="parameters">The parameter Vector of the multivariate function:  the first parameter elements is the asset price S.</param>
        ///// <returns>A vector array of results: value, 
        ///// delta = {\partial I \over \partial S_0}, 
        ///// gamma = {\partial^2 I \over \partial S_0^2}, 
        ///// vega = {\partial I \over \partial q}, 
        ///// nSteps = The actual number used is returned here.</returns>
        //[ExcelName("LogNormalIntegration")]
        //public static double[] LogNormInt(long nSteps, MultivariateRealFunction fn,
        //                                  double L, double U, double q, double S0, Vector parameters)
        //{
        //    double lb, ub, s, s1, s2, sm1 = 0d, sm2 = 0d;
        //    double v, w, y;
        //    double yy;
        //    long n_max = nSteps, n_used;
        //    var result = new double[5];

        //    if (S0 <= 0 || q <= 0) throw new Exception("Invalid LogNormInt parameters");
        //    var q2 = q * q;
        //    var lnS1 = Math.Log(S0) - .5 * q2;

        //    /* Change limits to new variables ...  */

        //    if (L > 0)
        //    {
        //        v = (Math.Log(L) - lnS1) / q;
        //        lb = (Math.Sqrt(.25 + v * v) - .5) / v;
        //    }
        //    else lb = -1;
        //    if (U < 0) ub = 1;
        //    else if (U == 0) ub = -1;
        //    else
        //    {
        //        v = (Math.Log(U) - lnS1) / q;
        //        ub = (Math.Sqrt(.25 + v * v) - .5) / v;
        //    }
        //    if (ub < lb) { v = ub; ub = lb; lb = v; }
        //    else if (ub == lb)
        //    {
        //        return result;
        //    }
        //    if (lb > -LNI_MAX)
        //    {
        //        v = q * lb / (1 - lb * lb) + lnS1;
        //        var xx = Math.Exp(v);
        //        parameters[0] = xx;
        //        yy = fn(parameters);
        //        s = Math.Exp(-.5 * Math.Pow(lb / (1 - lb * lb), 2.0)) * (1 + lb * lb) /
        //            Math.Pow(1 - lb * lb, 2.0) * yy;
        //        s1 = s * v;
        //        s2 = s1 * v;
        //    }
        //    else s = s1 = s2 = 0;
        //    if (ub < LNI_MAX)
        //    {
        //        v = q * ub / (1 - ub * ub) + lnS1;
        //        var xx = Math.Exp(v);
        //        parameters[0] = xx;
        //        yy = fn(parameters);
        //        w = Math.Exp(-.5 * Math.Pow(ub / (1 - ub * ub), 2.0)) * (1 + ub * ub) / Math.Pow(1 - ub * ub, 2.0) * yy;
        //        s += w;
        //        w *= v;
        //        s1 += w;
        //        s2 += w * v;
        //    }
        //    var h = ub - lb;
        //    var h2 = h / 2;
        //    s *= h2;
        //    s1 *= h2;
        //    s2 *= h2;
        //    double sm = 0;
        //    for (n_used = 2; 2 * n_used - 1 <= n_max || n_max <= 0; n_used += n_used - 1)
        //    {
        //        var os = s;
        //        var os1 = s1;
        //        var os2 = s2;
        //        var osm = sm;
        //        double sum;
        //        double sum1;
        //        double sum2;
        //        double z;
        //        for (z = lb + h / 2, sum = sum1 = sum2 = 0; z < ub; z += h)
        //        {
        //            if (z < -LNI_MAX || z > LNI_MAX) continue;
        //            y = 1 / (1 - z * z);
        //            v = z * y;
        //            w = v * v;
        //            var p = q * v + lnS1;
        //            var xx = Math.Exp(p);
        //            parameters[0] = xx;
        //            yy = fn(parameters);
        //            v = Math.Exp(-.5 * w) * (y * y + w) * yy;
        //            sum += v;
        //            v *= p;
        //            sum1 += v;
        //            sum2 += v * p;
        //        }
        //        h /= 2;
        //        s = s / 2 + h * sum;  /* include midpoints under trapezoid rule */
        //        s1 = s1 / 2 + h * sum1;
        //        s2 = s2 / 2 + h * sum2;
        //        sm = (4 * s - os) / 3;       /* convert to Simpson's rule */
        //        sm1 = (4 * s1 - os1) / 3;
        //        sm2 = (4 * s2 - os2) / 3;
        //        if (Math.Abs(sm - osm) < 1e-9 + EPS * Math.Abs(osm) && n_used >= 33) break;
        //    }
        //    sm *= OOR2PI;
        //    sm1 *= OOR2PI;
        //    sm2 *= OOR2PI;
        //    w = lnS1 / q2;
        //    v = ((w + 1) * lnS1 - 1) * sm - (1 + 2 * w) * sm1 + sm2 / q2;
        //    y = S0 * q2;
        //    result[0] = sm;
        //    result[1] = (sm1 - lnS1 * sm) / y;
        //    result[2] = v / (S0 * y);
        //    result[3] = v / q;
        //    result[4] = nSteps;
        //    return result;
        //}

        /// <summary>
        /// Converts from price volatility to yield volatility.
        /// </summary>
        /// <param name="pricevol">The price volatility.</param>
        /// <param name="bpv">The basis point value of the asset.</param>
        /// <param name="price">The price of the asset.</param>
        /// <param name="yield">The yield of the asset.</param>
        /// <returns>The yield volatility measure.</returns>
        public static double PricetoYieldVol(double pricevol, double bpv, double price, double yield)
        {
            return OptionAnalytics.PriceToYieldVol(pricevol,  bpv,  price,  yield);

        }

        /// <summary>
        /// Converts from yield volatility to price volatility.
        /// </summary>
        /// <param name="yieldvol">The yield volatility of the asset.</param>
        /// <param name="bpv">The basis point value of the asset.</param>
        /// <param name="price">The price of the asset.</param>
        /// <param name="yield">The yield of the asset.</param>
        /// <returns>The price volatility measure.</returns>
        public static double YieldtoPriceVol(double yieldvol, double bpv, double price, double yield)
        {
            return OptionAnalytics.YieldToPriceVol(yieldvol,  bpv,  price,  yield);
        }

        /// <summary>
        /// Valuation of a regular chooser option:
        /// The pricing model values this as the sum of a call option of expiry tL
        ///  and strike strike, plus a put option of expiry tS and strike strike * df(tS,tL)
        ///  where df(tS,tL) is the discount function between dates tS and tL.
        /// </summary>
        /// <param name="fwdPrice">Outright: to pay now for assured delivery of asset at tL</param>
        /// <param name="vol">Volatility</param>
        /// <param name="tS">Time to date where choice has to be made (years)</param>
        /// <param name="tL">Time to expiry of underlying option (years; must be greater than tS)</param>
        /// <param name="strike">strike of option</param>
        /// <param name="rL">exp(-rL.tL) is DF from now to time tL</param>
        /// <returns>An array of calculated doubles: Premium, delta, gamma, vega, rhoL, thetal, thetaS.</returns>
        public static object[,] ChooserOpt(double fwdPrice, double vol, double tS, double tL, double strike, double rL) 
        {
            var unqVals = OptionAnalytics.ChooserOpt(fwdPrice, vol, tS, tL, strike, rL);
            var result = RangeHelper.ConvertArrayToRange(unqVals);
            return result;
        }

        ///<summary>
        /// A dual strike dual notional call option.
        ///</summary>
        ///<param name="nSteps"></param>
        ///<param name="notl1"></param>
        ///<param name="fwdPrice1">price fixable now for purchase of asset 1 at time t</param>
        ///<param name="strike1"></param>
        ///<param name="notl2"></param>
        ///<param name="fwdPrice2">price fixable now for purchase of asset 2 at time t</param>
        ///<param name="strike2"></param>
        ///<param name="vol1"></param>
        ///<param name="vol2"></param>
        ///<param name="corr">correlation coefficient</param>
        ///<param name="r">riskless CC interest rate to option expiry</param>
        ///<param name="t">time to option expiry (years) </param>
        ///<returns></returns>
        public static double DualStrikeDualNotionalCall(
            long nSteps,
            double notl1,
            double fwdPrice1,   
            double strike1,
            double notl2,
            double fwdPrice2,  
            double strike2,
            double vol1,
            double vol2,
            double corr,  
            double r,   
            double t)      
        {
            return OptionAnalytics.DualStrikeDualNotionalCall(nSteps,
             notl1,
             fwdPrice1,   
             strike1,
             notl2,
             fwdPrice2,  
             strike2,
             vol1,
             vol2,
             corr,  
             r,   
             t);
        }

        /// <summary>
        /// Average rate options ...
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="strike"></param>
        /// <param name="premDF"></param>
        /// <param name="nPoints"></param>
        /// <param name="times"></param>
        /// <param name="fwdPrice"></param>
        /// <param name="vol"></param>
        /// <returns></returns>
        public static double AvePriceOption(bool callFlag, double strike, double premDF,
                                            short nPoints, double[] times, double[] fwdPrice, double[] vol)
        {
            return OptionAnalytics.AvePriceOption(callFlag,  strike,  premDF,
                                             nPoints,  times,  fwdPrice,  vol);
        }

        /// <summary>
        /// A simple spread option.
        /// </summary>
        /// <param name="nSteps"></param>
        /// <param name="fwdPrice1">price fixable now for purchase of asset 1 at time t</param>
        /// <param name="fwdPrice2">price fixable now for purchase of asset 2 at time t</param>
        /// <param name="strike"></param>
        /// <param name="vol1"></param>
        /// <param name="vol2"></param>
        /// <param name="corr">correlation coefficient</param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static object[,] SpreadOption(
            long nSteps,
            double fwdPrice1,   
            double fwdPrice2,    
            double strike,      
            double vol1,
            double vol2,
            double corr,        
            double t)
        {
            var unqVals = OptionAnalytics.SpreadOption(
            nSteps,
            fwdPrice1,   
            fwdPrice2,    
            strike,      
            vol1,
            vol2,
            corr,        
            t);
            var result = RangeHelper.ConvertArrayToRange(unqVals);
            return result;
        }

        /// <summary>
        /// SpreadOptWithGreeks
        /// </summary>
        /// <param name="nSteps"></param>
        /// <param name="fwdPrice1">price fixable now for purchase of asset 1 at time t</param>
        /// <param name="fwdPrice2">price fixable now for purchase of asset 2 at time t</param>
        /// <param name="strike">strike</param>
        /// <param name="vol1">volatility of asset 1</param>
        /// <param name="vol2">volatility of asset 2</param>
        /// <param name="corr">correlation coefficient</param>
        /// <param name="t">time to option expiry in years</param>
        /// <returns>delta1, delta2, gamma11, gamma22, 
        /// gamma12, vega1, vega2, corrSens, theta and the nsteps</returns>
        public static object[,] SpreadOptWithGreeks(long nSteps,   
                                                   double fwdPrice1,   // price fixable now for purchase of asset 1 at time t
                                                   double fwdPrice2,   // price fixable now for purchase of asset 2 at time t
                                                   double strike,      // strike
                                                   double vol1,        // volatility of asset 1
                                                   double vol2,        // volatility of asset 2
                                                   double corr,        // correlation coefficient
                                                   double t)           // time to option expiry in years
        {
            var unqVals = OptionAnalytics.SpreadOptWithGreeks(nSteps,   
                                                   fwdPrice1,   // price fixable now for purchase of asset 1 at time t
                                                   fwdPrice2,   // price fixable now for purchase of asset 2 at time t
                                                   strike,      // strike
                                                   vol1,        // volatility of asset 1
                                                   vol2,        // volatility of asset 2
                                                   corr,        // correlation coefficient
                                                   t);
            var result = RangeHelper.ConvertArrayToRange(unqVals);
            return result;
        }

        /// <summary>
        /// A simple basket option model.
        /// </summary>
        /// <param name="nSteps"></param>
        /// <param name="fwdPrice1">Price fixable now for purchase of asset 1 at time t</param>
        /// <param name="fwdPrice2">Price fixable now for purchase of asset 2 at time t</param>
        /// <param name="strike">Strike</param>
        /// <param name="vol1">Volatility of asset 1</param>
        /// <param name="vol2">Volatility of asset 2</param>
        /// <param name="corr">Correlation coefficient</param>
        /// <param name="t">Time to option expiry in years</param>
        /// <returns>A vector of value and greeks.</returns>
        public static object[,] BasketOption(
            long nSteps,
            double fwdPrice1,  
            double fwdPrice2,   
            double strike,      
            double vol1,      
            double vol2,     
            double corr,     
            double t)       
        {
            var unqVals = OptionAnalytics.BasketOption(
            nSteps,
            fwdPrice1,  
            fwdPrice2,   
            strike,      
            vol1,      
            vol2,     
            corr,     
            t) ;
            var result = RangeHelper.ConvertArrayToRange(unqVals);
            return result;
        }
   
        /// <summary>
        /// To value and work out sensitivities for an option where the forward value of the asset
        /// is fwdPrice.exp(corr.vol1.vol2.t)
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="fwdPrice"></param>
        /// <param name="strike"></param>
        /// <param name="vol1"></param>
        /// <param name="vol2"></param>
        /// <param name="corr"></param>
        /// <param name="t"></param>
        /// <returns>value, d_dfwdPrice, d2_dfwdPrice2, d_dvol1, d_dvol2, d_dcorr, d_dt, </returns>
        public static object[,] QOptWithGreeks(bool callFlag, double fwdPrice, 
                                       double strike, double vol1, double vol2, double corr, double t)
        {
            var unqVals = OptionAnalytics.QOptWithGreeks(callFlag, fwdPrice, 
                                       strike, vol1, vol2, corr, t);
            var result = RangeHelper.ConvertArrayToRange(unqVals);
            return result;
        }

        /// <summary>
        /// Values a digital option with greeks.
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="fwdPrice"></param>.
        /// <param name="strike"></param>
        /// <param name="vol"></param>
        /// <param name="t"></param>
        /// <returns>value, d_dfwdPrice, d2_dfwdPrice2, d_dvol, d_dstrike, d2_dstrike2, d_dt</returns>
        public static object[,] DigitalWithGreeks(bool callFlag, double fwdPrice, double strike, double vol, double t) 
        {
            var unqVals = OptionAnalytics.DigitalWithGreeks(callFlag, fwdPrice, strike, vol, t) ;
            var result = RangeHelper.ConvertArrayToRange(unqVals);
            return result;
        }

        #endregion
    }
}