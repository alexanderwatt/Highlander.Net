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

#region Using directives

using System;
using Highlander.Numerics.Dates;
using Highlander.Numerics.Distributions;
using Highlander.Numerics.Helpers;
using Highlander.Numerics.Integration;
using Highlander.Numerics.Maths;
using Highlander.Numerics.Maths.Collections;
using Highlander.Numerics.Rates;
using Highlander.Utilities.Helpers;

#endregion

namespace Highlander.Numerics.Options
{
    /// <summary>
    /// The back-scholes option class.
    /// </summary>
    public class OptionAnalytics
    {
        const double Eps = 1e-7;

        /// <summary>
        /// Simple Cox-Rubinstein pricing of an option for European exercise
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="spotPrice"></param>
        /// <param name="forwardPrice"></param>
        /// <param name="numberOfSteps"></param>
        /// <param name="strike"></param>
        /// <param name="volatility"></param>
        /// <param name="discountFactor"></param>
        /// <param name="time"></param>
        /// <returns>The value of the cox european option.</returns>
        public static double CoxEuroOption(Boolean callFlag, double spotPrice, double forwardPrice, short numberOfSteps,
                                           double strike, double volatility, double discountFactor, double time)
        {
            short i;
            var u = Math.Exp(volatility * Math.Sqrt(time / numberOfSteps));
            var d = 1 / u;
            var d2 = d * d;
            var p = (Math.Pow(forwardPrice / spotPrice, 1.0 / numberOfSteps) - d) / (u - d);
            var pd = (1 - p) / p;
            var pf = discountFactor * Math.Pow(p, numberOfSteps);
            var x = spotPrice * Math.Pow(u, numberOfSteps);
            var option = 0.0d;
            for (i = numberOfSteps; i >= 0; i--, x *= d2)
            {
                double v;
                if ((v = x - strike) <= 0) break;
                option += pf * v;
                pf *= pd * i / (numberOfSteps - i + 1);
            }
            if (!callFlag)
            {
                option = option + discountFactor * (strike - forwardPrice);
            }
            return option;
        }

        /// <summary>
        /// Solver.
        /// </summary>
        /// <param name="volatility"></param>
        /// <param name="time"></param>
        /// <param name="b"></param>
        /// <param name="numberOfSteps"></param>
        /// <returns></returns>
        public static double SolveU(double volatility, double time, double b, short numberOfSteps)
        {
            var u = Math.Exp(volatility * Math.Sqrt(time / numberOfSteps));
            double du;

            var k = volatility * volatility * time / (4 * numberOfSteps);
            do
            {
                var lnu = Math.Log(u);
                var lnu2 = lnu * lnu;
                var us1 = u * u - 1;
                var fu = (u * b - 1) * u * (u - b) * lnu2 - k * us1 * us1;
                var fpu = b * u * (u - b) * lnu2 + (u * b - 1) * (2 * u - b) * lnu2 +
                             2 * (u * b - 1) * (u - b) * lnu - 4 * k * us1 * u;
                du = -fu / fpu;
                u += du;

            } while (Math.Abs(du) > 1e-12);
            return u;
        }

        /// <summary>
        ///  Simple Cox-Rubinstein pricing of an option for European exercise, with the correct "up" factor" (seems to make v. little difference) ...
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
        public static double CoxEuroOption1(Boolean callFlag, double spotPrice, double fwdPrice, short nSteps, 
          double strike, double vol, double df, double t)
            {
              short i;
              var b = Math.Pow(fwdPrice / spotPrice, 1.0 / nSteps);
                var u = SolveU(vol, t, b, nSteps);
                var d = 1 / u;
                var d2 = d * d;
                var p = (b - d) / (u - d);
                var pd = (1 - p) / p;
                var pf = df * Math.Pow(p, nSteps);
                var x = spotPrice * Math.Pow(u, nSteps);
              var option = 0.0d;
              for(i = nSteps; i >= 0; i--, x *= d2)
              {
                  double v;
                  if ((v = x - strike) <= 0) break;
                option += pf * v;
                pf *= pd * i / (nSteps - i + 1);
              }
              if(!callFlag)
              {
                  option = option + df * (strike - fwdPrice);
              }
              return option;
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
        public static double CoxEuroOption2(Boolean callFlag, double fwdPrice, short nSteps, double strike, 
                            double vol, double df, double t)
        {
          short i;
          var d2 = Math.Exp(-2 * vol * Math.Sqrt(t / nSteps));
          var pf = df * Math.Pow(0.5, nSteps);
          var x = fwdPrice * Math.Pow(2 / (1 + d2), nSteps);
          var option = 0.0d;
          for(i = nSteps; i >= 0; i--, x *= d2)
          {
              double v;
              if ((v = x - strike) <= 0) break; 
            option += pf * v;
            pf *= i / (nSteps - i + 1.0);
          }
          if(!callFlag)
          {
              option = option + df * (strike - fwdPrice);
          }
          return option;
        }

        /// <summary>
        /// Simple Cox-Rubinstein pricing of an option for European exercise, with probabilities of exactly 1/2, using Black-Scholes valuation for the final time step ... 
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="fwdPrice"></param>
        /// <param name="nSteps"></param>
        /// <param name="strike"></param>
        /// <param name="vol"></param>
        /// <param name="df"></param>
        /// <param name="t"></param>
        /// <returns>The value.</returns>
        public static double CoxEuroOption3(Boolean callFlag, double fwdPrice, short nSteps, 
          double strike, double vol, double df, double t)
        {
          int i;
          var d2 = Math.Exp(-2 * vol * Math.Sqrt(t / nSteps));
          var pf = df * Math.Pow(0.5, nSteps - 1);
          var x = fwdPrice * Math.Pow(2 / (1 + d2), nSteps - 1);
          var option = 0.0;
          for(i = nSteps - 1; i >= 0; i--, x *= d2)
          {
            option += pf * Opt(true, x, strike, vol, t / nSteps);
            pf *= (double)i / (nSteps - i);
          }
          if(!callFlag)
          {
              option = option + df * (strike - fwdPrice);
          }
          return option;
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
        public static double CoxEuroOption4(Boolean callFlag, double fwdPrice, short nSteps, 
        double strike, double vol, double df, double t)
        {
        short i;

        var dx = -2 / Math.Sqrt(nSteps);
        var srt = vol * Math.Sqrt(t);
        var d2 = Math.Exp(dx * srt);
        var f = fwdPrice * Math.Pow(2 / (1 + d2), nSteps);
        var x = nSteps * Math.Log(2 / (1 + d2)) / srt + .5 * srt;
        var option = 0.0;
        for(i = nSteps; i >= 0; i--, f *= d2, x += dx)
        {
            double v;
            if ((v = f - strike) <= 0) break;
        option += v * Math.Exp(-.5 * x * x);
        }
        option *= -Constants.InvSqrt2Pi * df * dx;
        if (callFlag)
        {
            option = option + df * (strike - fwdPrice);
        }
        return option;
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
        public static double CoxAmerOption(Boolean callFlag, double spotPrice, double fwdPrice, short nSteps,
          double strike, double vol, double df, double t) 
        {
            int i;
            double uMin;
            var call = new double[nSteps + 1];
            var put = new double[nSteps + 1];
            var r = Math.Pow(df, 1.0 / nSteps);
            var u = Math.Exp(vol * Math.Sqrt(t / nSteps));
            var u2 = u * u;
            var d = 1 / u;
            var c1 = r * (u - Math.Pow(fwdPrice / spotPrice, 1.0 / nSteps)) / (u - d);
            var c2 = r - c1;
            var x = uMin = spotPrice * Math.Pow(d, nSteps);
              for(i = 0; i <= nSteps; i++, x *= u2)  /* Set up final values ... */
              {
                call[i] = Math.Max(x - strike, 0);
                put[i] = Math.Max(strike - x, 0);
              }
              for(i = nSteps - 1; i >= 0; i--)
              {
                x = (uMin *= u);
                  int j;
                  for(j = 0; j <= i; j++, x *= u2)
                {
                  call[j] = Math.Max(c1 * call[j] + c2 * call[j + 1], x - strike);
                  put[j] = Math.Max(c1 * put[j] + c2 * put[j + 1], strike - x);
                }
              }
            var result = call[0];
            if(!callFlag)
            {
                result = put[0];
            }
            return result;
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
        public static double CoxFuturesAmerOption(Boolean callFlag, double fwdPrice, short nSteps,
  double strike, double vol, double df, double t)
        {
            int i;
            var call = new double[nSteps + 1];
          var put = new double[nSteps + 1];
          double uMin;
          var u = Math.Exp(vol * Math.Sqrt(t / nSteps));
          var u2 = u * u;
          var x = uMin = fwdPrice * Math.Pow(u, -nSteps);
          for(i = 0; i <= nSteps; i++, x *= u2)  // Values at expiry ...
          {
            call[i] = Math.Max(x - strike, 0);
            put[i] = Math.Max(strike - x, 0);
          }
          var r = Math.Pow(df, 1.0 / nSteps);
          var pu = r / (u + 1);
          var pd = r - pu;
          for(i = nSteps - 1; i >= 0;i--)
          {
            x = uMin *= u;
              int j;
              for(j = 0; j <= i; j++, x *= u2)
            {
              call[j] = Math.Max(pd * call[j] + pu * call[j + 1], x - strike);
              put[j] = Math.Max(pd * put[j] + pu * put[j + 1], strike - x);
            }
          }
          var result = call[0];
          if (!callFlag)
          {
              result = put[0];
          }
          return result;
        }

        /// <summary>
        /// Standard (Black-Scholes) option valuation
        /// </summary>
        /// <param name="callFlag">Flag for Put or Call.</param>
        /// <param name="fwdPrice">Price fixed today for purchase of asset at time t.</param>
        /// <param name="strike">Exercise price of option.</param>
        /// <param name="vol">Per cent volatility in units of (year)^(-1/2).</param>
        /// <param name="t">Time in years to the maturity of the option.</param>
        /// <returns></returns>
        public static double Opt(Boolean callFlag, double fwdPrice, double strike, double vol, double t)
        {
            //if (fwdPrice < 0)throw new Exception("Invalid parameters in option evaluation");
            //if (fwdPrice == 0 || t < 0 || vol < 0) return callFlag ? Math.Max(strike, 0) : Math.Max(-strike, 0); 
            if (fwdPrice <= 0 || t <= 0 || vol <= 0) return callFlag ? Math.Max((fwdPrice - strike), 0) : Math.Max(strike - fwdPrice, 0);
            if (strike <= 0)
            {
                /* Options should not have a negative strike: this is only here because this function is
                   called by the AvePriceOption functions, which if evaluated during their averaging period 
                   may have mandatory exercise even though they have not expired ... */
                if (callFlag) return (fwdPrice - strike);
                return 0;
            }
            //if (t == 0 || vol == 0)
            //{
            //    return callFlag ? Math.Max((fwdPrice - strike), 0) : Math.Max(-1*(fwdPrice - strike), 0);
            //}
            var srt = vol * Math.Sqrt(t);
            var h1 = Math.Log(fwdPrice / strike) / srt + srt / 2;
            var h2 = h1 - srt;
            if (callFlag) return (fwdPrice * new NormalDistribution().CumulativeDistribution(h1) - strike * new NormalDistribution().CumulativeDistribution(h2));
            return -(fwdPrice * new NormalDistribution().CumulativeDistribution(-h1) - strike * new NormalDistribution().CumulativeDistribution(-h2));
        }

        /// <summary>
        /// The integral I1 used above is given by I1(u, a) =
        /// \int_{-\infinity}^u dx {a exp(-x^2/2) \over a^2 + x^2}.
        /// We integrate from zero, since the integrand is even in x and
        /// \int_0^\infinity dx {a exp(-x^2/2) \over a^2 + x^2} = \pi exp(a^2/2) cn(-a)
        /// The integral \int _0 ^u dx {a exp(-x^2/2) \over a^2  + x^2}.
        /// (where u > 0), is done using Simpson's rule.   
        /// </summary>
        /// <param name="u"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static double I1(double u, double a)
        {
            double sm;
            double x = Math.Abs(u), aa = Math.Abs(a);
            const double c1 = 0.0804492816131978, /* sqrt(sqrt(2) - 1) / 8       */
                       c2 = -0.0441941738241592,  /* -1 / (8 * sqrt(8))          */
                       c3 = 1.2871885058111600,  /* 2 * sqrt(sqrt(2) - 1)       */
                       c4 = -0.3884434935075090,  /* -sqrt(sqrt(2) + 1) / 4      */
                       c5 = 0.0568862325702784,  /* sqrt(sqrt(2)-1)/(4*sqrt(8)) */
                       c6 = 3.1075479480600700,  /* 2 * sqrt(sqrt(2) + 1)       */
                       r8 = 2.8284271247461900;  /* sqrt(8)                     */
            /* outside -5 < u < 5, both the Simpson's rule evaluation & the analytic
               approximation start to get a bit kooky, so just assume infinity values */
            if (u < -5) return 0; /* Value will be less than 7.19e-8 anyway */
            var a2 = aa * aa;
            var q = Math.PI * Math.Exp(a2 / 2) * new NormalDistribution().CumulativeDistribution(-aa);
            if (u > 5) return 2 * (a > 0 ? q : -q);
            /* For small a, we approximate the function with a polynomial:
              a e^(-x^2/2)/(x^2 + a^2)  ~  a / ((x^2 + a^2)(1 + x^2/2 + x^4/8)),
              which we can do analytically, thereby bypassing the need for 10000000000
              Simpson's rule evaluations. Max error 1.5e-4 (for large |x|) */
            if (aa < 0.01)
            {
                var u2 = u * u;
                sm = (Math.Atan(x / aa) + aa * ((c1 + c2 * a2) * Math.Log((u2 + r8 - c3 * x) /
                       (u2 + r8 + c3 * x)) + (c4 + c5 * a2) * Math.Atan2(c6 * x, r8 - u2))) /
                       (1 - a2 / 8 * (4 - a2));
                if (sm > q) sm = q; /* The expression overestimates the integral:
                                   we prevent it from being greater than q here */
            }
            else
            {
                /*  FUNC(x) (aa * exp(-(x) * (x) / 2) / (a2 + (x) * (x))) */
                sm = 0;
                double h;
                var func0 = (aa * Math.Exp(-(0.0) * (0.0) / 2) / (a2 + (0.0) * (0.0)));
                var funcX = (aa * Math.Exp(-(x) * (x) / 2) / (a2 + (x) * (x)));
                var s = (h = x) * (func0 + funcX) / 2;
                double osm;
                do
                {
                    var os = s;
                    osm = sm;
                    /* We are calculating the sum of FUNC(z) for z from h/2 to x-h/2 in steps of
                       h, but have arranged it in this way to maximize speed ... */
                    double zi;
                    double zii;
                    var z = (zi = zii = h * h) / 8;
                    var f = Math.Exp(-z) * aa / 2;
                    z += a2 / 2;
                    var zMax = (x * x + a2) / 2;
                    double r;
                    var g = r = Math.Exp(-zii);
                    double sum;
                    for (sum = 0; z < zMax; z += zi, zi += zii, f *= g, g *= r)
                        sum += f / z;
                    s = s / 2 + (h /= 2) * sum;
                    sm = (4 * s - os) / 3;
                } while (Math.Abs(sm - osm) >= Eps * (1e-3 + Math.Abs(u > 0 ? q + osm : q - osm)));
            }
            q += u > 0 ? sm : -sm;
            q = q > 0 ? q : 0;
            return a > 0 ? q : -q;
        }

        /// <summary>
        /// Cumulative bivariate normal distribution,
        /// N_2 (x_1, x_2; \rho) =
        /// {1 \over 2\pi\sqrt{1-\rho^2}} \int_{-\infinity}^{x_1} dx\int_{-\infinity}^{x_2} dy
        /// exp(-{1\over 2}{(x^2 - 2\rho xy + y^2 \over 1-\rho^2)})
        /// where \rho is the correlation coefficient.
        /// This is needed to value options on options and complex choosers.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="correlation"></param>
        /// <returns></returns>
        public static double Norm2(double x1, double x2, double correlation)
        {
            if (correlation < -1 || correlation > 1) throw new Exception("Correlation must be between -1 and 1");
            if (correlation == -1) return x1 > -x2 ? new NormalDistribution().CumulativeDistribution(x1) - new NormalDistribution().CumulativeDistribution(-x2) : 0;
            if (correlation == 1) return new NormalDistribution().CumulativeDistribution(Math.Min(x1, x2));
            var s = 1 / Math.Sqrt(1 - correlation * correlation);
            if (x1 == 0)
            {
                if (x2 == 0) return 0.25 + Constants.Inv2PI * Math.Atan(correlation * s) * s;
                return (x2 > 0 ? 0.5 : 0) - Constants.Inv2PI * Math.Exp(-x2 * x2 / 2) * SimpsonsRuleIntegration.Value(-correlation * s * x2, x2);
            }
            if (x2 == 0)
                return (x1 > 0 ? 0.5 : 0) - Constants.Inv2PI * Math.Exp(-x1 * x1 / 2) * SimpsonsRuleIntegration.Value(-correlation * s * x1, x1);
            return (x1 < 0 || x2 < 0 ? 0 : 1) - Constants.Inv2PI * (
               Math.Exp(-x2 * x2 / 2) * SimpsonsRuleIntegration.Value(s * (x1 - correlation * x2), x2) +
               Math.Exp(-x1 * x1 / 2) * SimpsonsRuleIntegration.Value(s * (x2 - correlation * x1), x1));
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
        public static double[] OptWithGreeks(Boolean callFlag, double fwdPrice, double strike, double vol, double t)//TODO put back rate.
        {
            var result = new double[7];
            result[0] = 0.0;
            result[1] = 0.0;
            result[2] = 0.0;
            result[3] = 0.0;
            result[4] = 0.0;
            result[5] = 0.0;
            //if (fwdPrice < 0 || vol < 0 || t < 0) return result;
            if (fwdPrice <= 0)
            {
                var df = callFlag ? 1 : -1;
                //result[0] = callFlag ? Math.Max(-1 * strike, 0) : Math.Max(strike, 0);
                result[0] = callFlag ? Math.Max((fwdPrice - strike), 0) : Math.Max(-1 * (fwdPrice - strike), 0);
                result[1] = result[0] > 0 ? df : 0;
                result[5] = -t * result[0];
                return result;
            }
            if (strike <= 0)
            {
                if (!callFlag)
                {
                    return result;
                }
                result[1] = 1.0;
                result[0] = result[1] * (fwdPrice - strike);
                result[5] = -t * result[0];
                return result;
            }
            if (t <= 0 || vol <= 0)
            {
                var df = callFlag ? 1 : -1;
                result[0] = callFlag ? Math.Max((fwdPrice - strike), 0) : Math.Max(-1 * (fwdPrice - strike), 0);
                result[1] = result[0] > 0 ? df : 0;
                result[5] = -t * result[0];
                return result;
            }
            double sqrtT;
            var srt = vol * (sqrtT = Math.Sqrt(t));
            var h = Math.Log(fwdPrice / strike) / srt + srt / 2;
            var delta = callFlag ? new NormalDistribution().CumulativeDistribution(h) : -1 * new NormalDistribution().CumulativeDistribution(-1 * h);
            var d2 = new NormalDistribution().CumulativeDistribution((h - srt));
            var d3 = new NormalDistribution().CumulativeDistribution(-1 * (h - srt));
            var deltaStrike = callFlag ? -d2 : d3;
            var v = Constants.InvSqrt2Pi * Math.Exp(-h * h / 2);
            var gamma = v / (fwdPrice * srt);
            var vega = v * fwdPrice * sqrtT;
            var premium = callFlag ? fwdPrice * delta - strike * d2 : fwdPrice * delta + strike * d3;
            var rho = -t * premium;
            var theta = vega * vol / (2 * t);
            result[0] = premium;
            result[1] = delta;
            result[2] = gamma;
            result[3] = vega;
            result[4] = theta;
            result[5] = rho;
            result[6] = deltaStrike;
            return result;
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
        public static double[,] BlackScholesWithGreeks(Boolean callFlag, double fwdPrice, double strike, double vol, double t)//TODO put back rate.
        {
            var result = new double[6];
            result[0] = 0.0;
            result[1] = 0.0;
            result[2] = 0.0;
            result[3] = 0.0;
            result[4] = 0.0;
            result[5] = 0.0;
            //if (fwdPrice < 0 || vol < 0 || t < 0) return ArrayHelper.ArrayToHorizontalMatrix(result);
          if (fwdPrice <= 0)
          {
              var df = callFlag ? 1 : -1;
              result[0] = callFlag ? Math.Max((fwdPrice - strike), 0) : Math.Max(-1 * (fwdPrice - strike), 0);
              //result[0] = callFlag ? Math.Max(-1 * strike, 0) : Math.Max(strike, 0);
              //result[5] = -t * result[0];
              result[1] = result[0] > 0 ? df : 0;
              result[5] = -t * result[0];
              return ArrayHelper.ArrayToHorizontalMatrix(result);
          }
          if (strike <= 0)  
          { 
            if (!callFlag)
            {
                return ArrayHelper.ArrayToHorizontalMatrix(result);
            }
            result[1] = 1.0;
            result[0] = result[1] * (fwdPrice - strike);
            result[5] = -t * result[0];
            return ArrayHelper.ArrayToHorizontalMatrix(result);
          }
          if (t <= 0 || vol <= 0)
          {
              var df = callFlag ? 1 : -1;
              result[0] = callFlag ? Math.Max((fwdPrice - strike), 0) : Math.Max(-1 * (fwdPrice - strike), 0);
              result[1] = result[0] > 0 ? df : 0;
              result[5] = -t * result[0];
              return ArrayHelper.ArrayToHorizontalMatrix(result);
          }
          double sqrtT;
          var srt = vol*(sqrtT = Math.Sqrt(t));
          var h = Math.Log(fwdPrice/strike)/srt + srt/2;
          var delta = callFlag ? new NormalDistribution().CumulativeDistribution(h) : -1 * new NormalDistribution().CumulativeDistribution(-1 * h);
          var v = Constants.InvSqrt2Pi * Math.Exp(-h*h/2);
          var gamma = v/(fwdPrice*srt);
          var vega = v*fwdPrice*sqrtT;
          var premium = callFlag ? fwdPrice * delta - strike * new NormalDistribution().CumulativeDistribution((h - srt)) : fwdPrice * delta + strike * new NormalDistribution().CumulativeDistribution(-1 * (h - srt));
          var rho = -t*premium;
          var theta = vega*vol/(2*t) ;
          result[0] = premium;
          result[1] = delta;
          result[2] = gamma;
          result[3] = vega;
          result[4] = theta;
          result[5] = rho;
          return ArrayHelper.ArrayToHorizontalMatrix(result);
        }

        /// <summary>
        /// Solves for the B-S volatility.
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="fwdPrice"></param>
        /// <param name="strike"></param>
        /// <param name="premium"></param>
        /// <param name="r"></param>
        /// <param name="t"></param>
        /// <returns>The volatility for that price.</returns>
        public static double OptSolveVol(Boolean callFlag, double fwdPrice, double strike, double premium, double r, double t)
        {
            double vol = 0.20, dVol;
            var cp = callFlag ? 1 : -1;
            if (fwdPrice <= 0 || strike <= 0 || t <= 0 || premium <= 0) return 0;
            if (premium < Math.Max(cp * Math.Exp(-r * t) * (fwdPrice - strike), 0)) throw new Exception("No solution for volatility");
            do
            {
                var risks = OptWithGreeks(callFlag, fwdPrice, strike, vol, t);
                if (risks[3] == 0) throw new Exception("No volatility solution");
                dVol = (risks[0] - premium) / risks[3];
                vol -= dVol;
            } while (Math.Abs(dVol) > Eps);
            return vol;
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
        /// <param name="premium"></param>
        /// <returns>The forward value for that price and volatility.</returns>
        public static double OptSolveFwd(Boolean callFlag, double strike, double vol, double r, double t, double premium)
        {
            double fold;
            var rt = r * t;
            if (premium <= 0) throw new Exception("Invalid option parameters");
            var cp = callFlag ? 1:-1;
            var df = Math.Exp(-rt);
            var vrt = vol * Math.Sqrt(t);
            var fwdPrice = (strike * df + cp * premium) / df;
            do
            {
                var h = Math.Log(fwdPrice / strike) / vrt + vrt / 2;
                var delta = cp * df * new NormalDistribution().CumulativeDistribution(cp * h);
                var y = fwdPrice * delta - df * cp * strike * new NormalDistribution().CumulativeDistribution(cp * (h - vrt)) - premium;
                fold = fwdPrice;
                fwdPrice -= y / delta;     /* Employ Newton-Raphson technique */
            } while (Math.Abs(fwdPrice - fold) > Eps);
            return fwdPrice;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="fwd"></param>
        /// <param name="vol"></param>
        /// <param name="r"></param>
        /// <param name="t"></param>
        /// <param name="premium"></param>
        /// <returns></returns>
        public static double OptSolveStrike(Boolean callFlag, double fwd, double vol, double r, double t, double premium)
        {
            double fold;
            var rt = r * t;
            if (premium <= 0) throw new Exception("Invalid option parameters");
            var cp = callFlag ? 1 : -1;
            var df = Math.Exp(-rt);
            var vrt = vol * Math.Sqrt(t);
            var strike = (fwd * df - cp * premium) / df;
            do
            {
                var h = Math.Log(fwd / strike) / vrt + vrt / 2;
                var delta = -cp * df * new NormalDistribution().CumulativeDistribution(cp * (h - vrt));
                var y = BlackScholesWithGreeks(callFlag, fwd, strike, vol, t)[0, 0] - premium;
                fold = strike;
                strike -= y / delta;
            } while (Math.Abs(strike - fold) > Eps);
            return strike;

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
        public static double[] CompoundOpt(Boolean callOnOptionFlag, double strikeS, double rS, double tS, 
          Boolean callFlag, double strikeL, double rL, double tL, double fwdPrice, double vol)
        {
          double sqrttS, sqrttL;
          var result = new double[8];
          var cps = callOnOptionFlag ? 1 : -1;
          var cpl = callFlag ? 1 : -1;
          if (vol <= 0 || tS < 0 || tL < tS) throw new Exception("Invalid option parameters");
          var cpCP = cpl * cps;
          var vrtS = vol * (sqrttS = Math.Sqrt(tS));
          var vrtL = vol * (sqrttL = Math.Sqrt(tL));
          var rtS = rS * tS;
          var rtL = rL * tL;
          var tT = tL - tS;
          var x = (Math.Log(OptSolveFwd(callFlag, strikeL, vol, (rtL - rtS) / tT, tT, strikeS) / fwdPrice) - rtL) / vrtS + vrtS / 2;
          var y = (Math.Log(fwdPrice / strikeL) + rtL) / vrtL + vrtL / 2;
          var corr = cps * sqrttS / sqrttL;
          var xVrt = x - vrtS;
          var rtT = cpl / Math.Sqrt(tT);
          var fVol = fwdPrice * vol;
          var b = cpCP * strikeL  * Math.Exp(-rtL) * new NormalDistribution().BivariateNormal(-cpCP * x, cpl * (y - vrtL), corr);
          var cn1 = Constants.InvSqrt2Pi / sqrttS * Math.Exp(-xVrt * xVrt / 2) * new NormalDistribution().CumulativeDistribution(rtT * (xVrt * sqrttS + y * sqrttL));
          var cn2 = cps * Constants.InvSqrt2Pi / sqrttL * Math.Exp(-y * y / 2) * new NormalDistribution().CumulativeDistribution(-cps * rtT * (xVrt * sqrttL + y * sqrttS));
          var cn3 = cps * strikeS * Math.Exp(-rtS) * new NormalDistribution().CumulativeDistribution(-cpCP * x);
          result[1] = cpCP * new NormalDistribution().BivariateNormal(-cpCP * xVrt, cpl * y, corr);
          result[2] = (cn1 + cn2) / fVol;
          result[3] = fwdPrice * (tS * cn1 + tL * cn2);
          result[4] = fVol * cn1 / 2 + rS * cn3;
          result[5] = fVol * cn2 / 2 + rL * b;
          result[6] = tS * cn3;
          result[7] = tL * b;
          result[0] = fwdPrice * result[1] - b - cn3;
          return result;
        }

        ///<summary>
        /// The basic put/call payoff function.
        ///</summary>
        ///<param name="blackScholesPrams">The first element is the stock price, the second is the strike.</param>
        ///<returns>The stock price minus the strike.</returns>
        public static double FOpt(DoubleVector blackScholesPrams)
        {
            return blackScholesPrams[0] - blackScholesPrams[1];
        }

        /// <summary>
        /// Converts from price volatility to yield volatility.
        /// </summary>
        /// <param name="priceVol"></param>
        /// <param name="bpv"></param>
        /// <param name="price"></param>
        /// <param name="yield"></param>
        /// <returns/>
        public static double PriceToYieldVol(double priceVol, double bpv, double price, double yield)
        {
            if (priceVol < 0.0 || bpv < 0 || price <= 0.0 || yield <= 0.0) throw new Exception("Not valid inputs.");
            var convFactor = -Math.Log((price + bpv) / price) / Math.Log((yield - 0.0001) / yield);
            return priceVol / convFactor;
        }

        /// <summary>
        /// Converts from yield volatility to price volatility.
        /// </summary>
        /// <param name="yieldVol"></param>
        /// <param name="bpv"></param>
        /// <param name="price"></param>
        /// <param name="yield"></param>
        /// <returns></returns>
        public static double YieldToPriceVol(double yieldVol, double bpv, double price, double yield)
        {
            if (yieldVol < 0.0 || bpv < 0.0 || yield < 0.0) throw new Exception("Not valid inputs.");
            var convFactor = -Math.Log((price + bpv) / price) / Math.Log((yield - 0.0001) / yield);
            return yieldVol * convFactor;
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
        public static double[] ChooserOpt(double fwdPrice, double vol, double tS, double tL, double strike, double rL) 
        {
            var results = new double[8];
            double sqrttL, sqrttS;
            if (vol <= 0 || tS < 0 || tL < tS) throw new Exception("Invalid option parameters");
            /* call option (fwdPrice, strike, vol, rL, tL) */
            var srT = vol * (sqrttL = Math.Sqrt(tL));
            var kd = strike * Math.Exp(-rL * tL);
            var d = Math.Log(fwdPrice / kd);
            var h1 = d / srT + srT / 2;
            var s = kd * new NormalDistribution().CumulativeDistribution(h1 - srT);
            var delta = new NormalDistribution().CumulativeDistribution(h1);
            var v = Constants.InvSqrt2Pi * Math.Exp(-h1 * h1 / 2);
            var gamma = v / (fwdPrice * srT);
            var vega = v * fwdPrice * sqrttL;
            var rhoL = tL * s;
            var thetaL = s * rL + .5 * vega * vol / tL;
            var prem1 = fwdPrice * delta - s;
            /* ... plus put option (fwdPrice, strike * exp(rS * tS - rL * tL), vol, rS, tS) */
            var srt = vol * (sqrttS = Math.Sqrt(tS));
            var h2 = -d / srt - srt / 2;
            s = kd * new NormalDistribution().CumulativeDistribution(h2 + srt);
            delta += (d = -new NormalDistribution().CumulativeDistribution(h2));
            v = Constants.InvSqrt2Pi * Math.Exp(-h2 * h2 / 2);
            gamma += v / (fwdPrice * srt);
            vega += v * fwdPrice * sqrttS;
            rhoL -= tL * s;
            var thetaS = .5 * vol * fwdPrice * v / sqrttS;
            thetaL -= s * rL;
            results[0] = prem1 + fwdPrice * d + s;
            results[1] = delta;
            results[2] = gamma;
            results[3] = vega;
            results[4] = rhoL;
            results[5] = thetaL;
            results[6] = thetaS;
            return results;
        }

        ///<summary>
        /// Compound option parameters.
        ///</summary>
        public DoubleVector CompOptParam(double fwdPrice, Boolean cPs, Boolean cps, double ks, double Ks, double tTs, double vols, double rtTs)
        {
            var result = new DoubleVector(7)
            {
                [0] = fwdPrice,
                [1] = cPs ? 1 : -1,
                [2] = cps ? 1 : -1,
                [3] = ks,
                [4] = Ks,
                [5] = tTs,
                [6] = vols,
                [7] = rtTs
            };
            return result;
        }

        ///<summary>
        /// The payoff function for a compound option.
        ///</summary>
        ///<param name="parameters">A vector of parameters: fwdPrice, CPs, cps, ks, Ks, tTs, vols, rtTs</param>
        ///<returns></returns>
        public static double fComp(DoubleVector parameters)//TODO this needs to be PayOff function.
        {
            var x = parameters[2] == 1.0;
            var q = Opt(x, parameters[0], parameters[3], parameters[6], parameters[5]) - parameters[4];
            if (parameters[1] == 1) return q > 0 ? q : 0;
          return q < 0? -q: 0;
        }

        /// <summary>
        /// To value an option that pays
        /// max(notl1 * (S1 - strike1), notl2 * (S2 - strike2), 0) on the expiry date, where
        /// S1 is the spot price of asset 1 on the expiry date
        /// strike1 is the strike price of asset 1
        /// notl1 is the notional amount of asset 1
        /// (similarly for asset 2)
        /// </summary>
        public struct DsdnParam
        {
            /// <summary>
            /// 
            /// </summary>
          public double K1S;
            /// <summary>
            /// 
            /// </summary>
            public double N1S;
            /// <summary>
            /// 
            /// </summary>
            public double C1S;
            /// <summary>
            /// 
            /// </summary>
            public double C2S;
            /// <summary>
            /// 
            /// </summary>
            public double C3S;
            /// <summary>
            /// 
            /// </summary>
            public double C4S;
            /// <summary>
            /// 
            /// </summary>
            public double C5S;
            /// <summary>
            /// 
            /// </summary>
            public double C6S;
            /// <summary>
            /// 
            /// </summary>
            public double C7S;
            /// <summary>
            /// 
            /// </summary>
            public double C8S;
            /// <summary>
            /// 
            /// </summary>
            public double C9S;
        }

        /// <summary>
        /// To value an option that pays
        /// max(notl1 * (S1 - strike1), notl2 * (S2 - strike2), 0) on the expiry date, where
        /// S1 is the spot price of asset 1 on the expiry date
        /// strike1 is the strike price of asset 1
        /// notl1 is the notional amount of asset 1
        /// (similarly for asset 2)
        /// </summary>
        public static DoubleVector ConvertDsdnParam(DsdnParam dSdnParam)
        {
            var result = new DoubleVector(13)
            {
                [0] = 0.0d,
                [1] = dSdnParam.K1S,
                [2] = dSdnParam.N1S,
                [3] = dSdnParam.C1S,
                [4] = dSdnParam.C1S,
                [6] = dSdnParam.C3S,
                [7] = dSdnParam.C4S,
                [8] = dSdnParam.C5S,
                [9] = dSdnParam.C6S,
                [10] = dSdnParam.C7S,
                [11] = dSdnParam.C8S,
                [12] = dSdnParam.C9S
            };
            return result;
        }

        ///<summary>
        /// To value an option that pays
        /// max(notl1 * (S1 - strike1), notl2 * (S2 - strike2), 0) on the expiry date, where
        /// strike1 is the strike price of asset 1
        /// notl1 is the notional amount of asset 1
        /// (similarly for asset 2)
        ///</summary>
        ///<param name="parameters">The order for the vector is: S1 (0), K1s (1), N1s (2), c1s (3), 
        /// c2s (4), c3s (5), c4s (6), c5s (7), c6s (8), c7s (9), c8s (10), c9s (11)</param>
        ///<returns></returns>
        public static double F2S2N(DoubleVector parameters)// Vector parameters) : MultivariateRealFunction
        {
            var lnv = parameters[3] * Math.Log(parameters[0]) + parameters[4];
            if (parameters[0] <= parameters[1])
            {
                var h0 = parameters[6] * lnv;
                return parameters[5] * new NormalDistribution().CumulativeDistribution(h0 + parameters[7]) + parameters[8] * Math.Exp(lnv) * new NormalDistribution().CumulativeDistribution(h0 + parameters[9]);
            }
            var x = parameters[2] * (parameters[0] - parameters[1]);
            var y = x - parameters[5];
            var k0 = parameters[6] * (lnv - Math.Log(y));
            return x - y * new NormalDistribution().CumulativeDistribution(k0 + parameters[10]) + parameters[8] * Math.Exp(lnv) * new NormalDistribution().CumulativeDistribution(k0 + parameters[11]);
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
            DsdnParam p;
            p.K1S = strike1;
            p.N1S = notl1;
            var sqrtT = Math.Sqrt(t);
            var q1 = vol1 * sqrtT;
            var q2 = vol2 * sqrtT;
            p.C1S = corr * vol2 / vol1;
            p.C2S = -p.C1S * Math.Log(fwdPrice1) + 0.5 * corr * q2 * (q1 - corr * q2);
            p.C3S = -notl2 * strike2;
            var z = Math.Sqrt(1 - corr * corr) * q2;
            p.C4S = 1 / z;
            p.C5S = p.C4S * Math.Log(fwdPrice2 / strike2) - 0.5 * z;
            p.C6S = notl2 * fwdPrice2;
            p.C7S = p.C5S + z;
            p.C8S = p.C4S * Math.Log(p.C6S) - 0.5 * z;
            p.C9S = p.C8S + z;
            return Math.Exp(-r * t) * IntegrationHelpers.LogNormalIntegration(nSteps, F2S2N, 0, -1, q1, fwdPrice1, ConvertDsdnParam(p))[0];
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
        public static double AvePriceOption(Boolean callFlag, double strike, double premDF,
          short nPoints, double[] times, double[] fwdPrice, double[] vol)
        {
            double fSum = 0, vSum = 0;
            var lastT = double.MaxValue;
            int i, m = nPoints;
            var CP = callFlag ? 1 : -1;

            if (CP != -1 && CP != 1) throw new Exception("Option must be call or put");
            for (i = nPoints - 1; i >= 0; i--)
            {
                var t = times[0];
                if (t > lastT) throw new Exception("Averaging dates must be in sequence");
                lastT = t;
                if (t <= 0)
                {
                    var q = i + 1;
                    for (strike *= nPoints; i >= 0; i--) strike -= fwdPrice[i];
                    if ((m = nPoints - q) == 0) return Math.Max(-CP * premDF * strike / nPoints, 0);
                    strike /= m;
                    break;
                }
                var f = fwdPrice[i];
                vSum += f * (f + 2 * fSum) * Math.Exp(vol[i] * vol[i] * t);
                fSum += f;
            }
            var texp = times[nPoints - 1];
            return premDF * m / nPoints * Opt(callFlag, fSum / m, strike, Math.Sqrt(Math.Log(vSum / (fSum * fSum)) / texp), texp);
        }

        /* Spread option. Pays max(S1 - S2 - strike, 0) on the expiry date, 
   where S1 & S2 are the spot prices of the assets on the expiry date */

        ///<summary>
        /// Converts to a general vector.
        ///</summary>
        ///<param name="s1"></param>
        ///<param name="Kk"></param>
        ///<param name="c1k"></param>
        ///<param name="c2k"></param>
        ///<param name="c3k"></param>
        ///<param name="c4k"></param>
        ///<param name="c5k"></param>
        ///<param name="c6k"></param>
        ///<param name="c7k"></param>
        ///<returns></returns>
        public static DoubleVector SpreadOptParam(double s1, double Kk,double c1k,double c2k,double c3k,
            double c4k,double c5k,double c6k,double c7k)
        {
            var vector = new DoubleVector(9);
            vector[0] = s1;
            vector[1] = Kk;
            vector[2] = c1k;
            vector[3] = c2k;
            vector[4] = c3k;
            vector[5] = c4k;
            vector[6] = c5k;
            vector[7] = c6k;
            vector[8] = c7k;
            return vector;
        }

//public static double fSpread(double s1, SpreadOptParam &P)
//{
//  double sK = s1 - P.Kk;
//  double l1 = log(s1);
//  double l2 = log(sK);
//  double h = P.c1k * l1 + P.c2k * l2 + P.c3k;
//  return sK * cn(h) - P.c4k * exp(P.c6k * l1) * cn(h - P.c5k);
//}

        /// <summary>
        /// fSpread
        /// </summary>
        /// <param name="parameters">s1 (0), Kk (1), c1k (2), c2k (3), c3k (4), c4k (5), c5k (6), c6k (7), c7k (8);</param>
        /// <returns></returns>
        public static double fSpread(DoubleVector parameters) //double s1, SpreadOptParam &P
        {
          var sK = parameters[0] - parameters[1];
          var l1 = Math.Log(parameters[0]);
          var l2 = Math.Log(sK);
          var h = parameters[2] * l1 + parameters[3] * l2 + parameters[4];
          return sK * new NormalDistribution().CumulativeDistribution(h) - parameters[5] * Math.Exp(parameters[7] * l1) * new NormalDistribution().CumulativeDistribution(h - parameters[6]);
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
        public static double[] SpreadOption(
          long nSteps,
          double fwdPrice1,   
          double fwdPrice2,    
          double strike,      
          double vol1,
          double vol2,
          double corr,        
          double t)
        {
              var result = new[] {Math.Max(fwdPrice1 - fwdPrice2 - strike, 0), 0d, 0d, 0d};
              var p = new DoubleVector(6); //s1 (0), Kk (1), c1k (2), c2k (3), c3k (4), c4k (5), c5k (6), c6k (7), c7k (8)

              if (t < 0) throw new Exception("Bad option parameters");
              if (t == 0) return result;
              if (Math.Abs(corr) >= 1) throw new Exception("Correlation must be between -1 and 1");
              var sqrtT = Math.Sqrt(t);
              var q1 = vol1 * sqrtT;
              var q2 = vol2 * sqrtT;
              var x1 = fwdPrice1 * Math.Exp(-q1 * q1 / 2);
              var x2 = fwdPrice2 * Math.Exp(-q2 * q2 / 2);
              var s = 1 / Math.Sqrt(1 - corr * corr);
              p[1] = strike;
              p[2] = -corr * s / q1;
              p[3] = s / q2;
              p[4] = (corr * Math.Log(x1) / q1 - Math.Log(x2) / q2) * s;
              p[5] = fwdPrice2 * Math.Exp(-corr * corr * q2 * q2 / 2) * Math.Pow(x1, -corr * vol2 / vol1);
              p[6] = q2 / s;                               
              p[7] = corr * q2 / q1;
              return IntegrationHelpers.LogNormalIntegration(nSteps, fSpread, strike, -1, q1, fwdPrice1, p);
        }

        /// <summary>
        /// Real function.
        /// </summary>
        /// <param name="spreadOptionParameters"></param>
        /// <returns></returns>
        public static double FSpread1(DoubleVector spreadOptionParameters)
        {
            var sK = spreadOptionParameters[0] - spreadOptionParameters[1];
            if (sK <= 0) return 0;
            return sK * new NormalDistribution().CumulativeDistribution(spreadOptionParameters[2] * Math.Log(spreadOptionParameters[0]) + spreadOptionParameters[3] * Math.Log(sK) + spreadOptionParameters[4]);
        }

        /// <summary>
        /// fSpread2
        /// </summary>
        /// <param name="spreadOptionParameters"></param>
        /// <returns></returns>
        public static double FSpread2(DoubleVector spreadOptionParameters)
        {
            var sK = spreadOptionParameters[0] - spreadOptionParameters[1];
            if (sK <= 0) return 0;
            return Math.Pow(spreadOptionParameters[0], spreadOptionParameters[7]) * new NormalDistribution().CumulativeDistribution(spreadOptionParameters[2] * Math.Log(spreadOptionParameters[0]) + spreadOptionParameters[3] * Math.Log(sK) + spreadOptionParameters[8]);
        }

        /// <summary>
        /// fSpread3
        /// </summary>
        /// <param name="spreadOptionParameters">s1 (0), Kk (1), c1k (2), c2k (3), c3k (4), c4k (5), c5k (6), c6k (7), c7k (8)</param>
        /// <returns></returns>
        public static double FSpread3(DoubleVector spreadOptionParameters)
        {
            var sK = spreadOptionParameters[0] - spreadOptionParameters[1];
            if (sK <= 0) return 0;
            var h = spreadOptionParameters[2] * Math.Log(spreadOptionParameters[0]) + spreadOptionParameters[3] * Math.Log(sK) + spreadOptionParameters[8];
            return Math.Pow(spreadOptionParameters[0], spreadOptionParameters[7]) * Math.Exp(-.5 * h * h);
        }

        /// <summary>
        /// fSpread4
        /// </summary>
        /// <param name="spreadOptionParameters"></param>
        /// <returns></returns>
        public static double FSpread4(DoubleVector spreadOptionParameters)
        {
            var sK = spreadOptionParameters[0] - spreadOptionParameters[1];
            if (sK <= 0) return 0;
            var ls1 = Math.Log(spreadOptionParameters[0]);
            return ls1 * Math.Pow(spreadOptionParameters[0], spreadOptionParameters[7]) * new NormalDistribution().CumulativeDistribution(spreadOptionParameters[2] * ls1 + spreadOptionParameters[3] * Math.Log(sK) + spreadOptionParameters[8]);
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
        /// gamma12, vega1, vega2, corrSens, theta and the n steps</returns>
        public static double[] SpreadOptWithGreeks(long nSteps,   
          double fwdPrice1,   // price fixable now for purchase of asset 1 at time t
          double fwdPrice2,   // price fixable now for purchase of asset 2 at time t
          double strike,      // strike
          double vol1,        // volatility of asset 1
          double vol2,        // volatility of asset 2
          double corr,        // correlation coefficient
          double t)           // time to option expiry in years
        {
          long n2, n3, n4;
          var result = new double[11];
          result[0] = Math.Max(fwdPrice1 - fwdPrice2 - strike, 0);
          if (t < 0) throw new Exception("Negative time to expiry");
          if (t == 0) return result;
          if (Math.Abs(corr) >= 1) throw new Exception("Correlation must be between -1 and 1");
          var n1 = n2 = n3 = n4 = nSteps;
          var sqrtT = Math.Sqrt(t);
          var q1 = vol1 * sqrtT;
          var q2 = vol2 * sqrtT;
          var x1 = fwdPrice1 * Math.Exp(-q1 * q1 / 2);
          var x2 = fwdPrice2 * Math.Exp(-q2 * q2 / 2);
          var s = 1 / Math.Sqrt(1 - corr * corr);
          var Kk = strike;
          var c1k = -corr * s / q1;
          var c2k = s / q2;
          var c3k = (corr * Math.Log(x1) / q1 - Math.Log(x2) / q2) * s;
          var c4k = fwdPrice2 * Math.Exp(-corr * corr * q2 * q2 / 2) * Math.Pow(x1, -corr * vol2 / vol1);
          var c5k = q2 / s;
          var c6k = corr * q2 / q1;
          var c7k = c3k - c5k;
          var P = SpreadOptParam(s, Kk, c1k, c2k, c3k, c4k, c5k, c6k, c7k);
          var T1 = IntegrationHelpers.LogNormalIntegration(nSteps, FSpread1, strike, -1, q1, fwdPrice1, P);
          var d1 = T1[1];
          var g1 = T1[2];
          var v1 = T1[3];
          var T2 = IntegrationHelpers.LogNormalIntegration(nSteps, FSpread2, strike, -1, q1, fwdPrice1, P);
          var d2 = T2[1];
          var g2 = T2[2];
          var v2 = T2[3];
          var c12k = Math.Exp(.5 * corr * q2 * (q1 - corr * q2));
          var c8k = Math.Pow(fwdPrice1, -c6k) * c12k;
          var c10k = fwdPrice2 * c6k * c8k / fwdPrice1;
          result[1] = d1 - c4k * d2 + c10k * T2[0]; //delta1
          result[2] = -c8k * T2[0]; //delta2
          var c9k = -c1k;
          var t3 = Constants.InvSqrt2Pi * IntegrationHelpers.LogNormalIntegration(nSteps, FSpread3, strike, -1, q1, fwdPrice1, P)[0];
          var t4 = IntegrationHelpers.LogNormalIntegration(nSteps, FSpread4, strike, -1, q1, fwdPrice1, P)[0];
          result[3] = (g1 - c4k * g2) + c10k * (2 * d2 + ((-c6k - 1) * T2[0] + c9k * t3) / fwdPrice1); //gamma11
          result[4] = c8k * s * t3 / (q2 * fwdPrice2); //gamma22
          result[5] = c8k * (-d2 + (c6k * T2[0] - c9k * t3) / fwdPrice1);  //gamma12
          nSteps = n1 + n2 + n3 + n4;  //TODO there is something about these nuuumsteps that needs fixing.
          result[6] = (v1 - c4k * v2) * sqrtT + fwdPrice2 * c8k * (
            c6k / vol1 * t4 - (.5 * corr * vol2 * t + c6k / vol1 * Math.Log(fwdPrice1)) * T2[0]); //vega1
          result[7]= fwdPrice2 * c8k * (sqrtT / s * t3 - corr / vol1 * t4 + (corr / vol1 * Math.Log(fwdPrice1) +
            corr * t * (-.5 * vol1 + corr * vol2)) * T2[0]); //vega2
          result[8] = fwdPrice2 * c8k * (-corr * s * q2 * t3 - vol2 / vol1 * t4 + (vol2 / vol1 * Math.Log(fwdPrice1) + q2 * (
            -.5 * q1 + corr * q2)) * T2[0]); //corrSens
          result[9] = (v1 - c4k * v2) * q1 / (2 * t) + fwdPrice2 * c8k * .5 * vol2 * (1 / (s * sqrtT) * t3 - 
            corr * (vol1 - corr * vol2) * T2[0]);
          result[0] = T1[0] - c4k * T2[0]; //theta
          result[10] = nSteps;
            return result;
        }

        /// <summary>
        /// Basket option. Pays max(.5 * (S1 + S2) - strike, 0) on the expiry date, 
        /// where S1 and S2 are the spot prices of the assets on the expiry date
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="bc2"></param>
        /// <param name="bc3"></param>
        /// <param name="bc4"></param>
        /// <param name="bc5"></param>
        /// <param name="bc6"></param>
        /// <param name="bc7"></param>
        /// <param name="bc8"></param>
        /// <returns></returns>
        public static DoubleVector BasketOptParam(double s1, double bc2, double bc3, double bc4, double bc5, double bc6, double bc7, double bc8)
        {
            var vector = new DoubleVector(8)
            {
                [0] = s1,
                [1] = bc2,
                [2] = bc3,
                [3] = bc4,
                [4] = bc5,
                [5] = bc6,
                [6] = bc7,
                [7] = bc8
            };
            return vector;
        }

        /// <summary>
        /// Multivariate real function.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double FBasket1(DoubleVector p)
        {
          var sK = p[1] - p[0];
          var l1 = Math.Log(p[0]);
          var l2 = Math.Log(sK);
          var h = p[4] * l1 + p[5] * l2 + p[6];
          return -0.5 * sK * new NormalDistribution().CumulativeDistribution(h) + p[2] * Math.Exp(p[3] * l1) * new NormalDistribution().CumulativeDistribution(h + p[7]);
        }

        /// <summary>
        /// Multivariate real function.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double FBasket2(DoubleVector p)
        {
          var sK = p[1] - p[0];
          var l1 = Math.Log(p[0]);
          var l2 = Math.Log(sK);
          var h = p[4] * l1 + p[5] * l2 + p[6];//TODO check why this is not used.
          return -0.5 * sK + p[2] * Math.Exp(p[3] * l1);
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
        public static double[] BasketOption(
          long nSteps,
          double fwdPrice1,  
          double fwdPrice2,   
          double strike,      
          double vol1,      
          double vol2,     
          double corr,     
          double t)       
        {
          if (t < 0) throw new Exception("Negative time to expiry in option");
          if (t == 0) return new[] {Math.Max((fwdPrice1 + fwdPrice2) / 2 - strike, 0), 0.0, 0.0, 0.0};
          if (Math.Abs(corr) >= 1) throw new Exception("Correlation must be between -1 and 1");
          var sqrtT = Math.Sqrt(t);
          var q1 = vol1 * sqrtT;
          var q2 = vol2 * sqrtT;
          var x1 = fwdPrice1 * Math.Exp(-q1 * q1 / 2);
          var x2 = fwdPrice2 * Math.Exp(-q2 * q2 / 2);
          var ss = 1 - corr * corr;
          var s = 1 / Math.Sqrt(ss);
          var bc2 = 2 * strike;
          var bc4 = corr * q2 / q1;
          var bc3 = .5 * x2 * Math.Pow(x1, -bc4) * Math.Exp(.5 * q2 * q2 * ss);
          var bc5 = corr * s / q1;
          var bc6 = -s / q2;
          var bc7 = s * (Math.Log(x2) / q2 - corr / q1 * Math.Log(x1));
          var bc8 = -1 / bc6;
          var p = BasketOptParam(s, bc2, bc3, bc4, bc5, bc6, bc7, bc8);
          var nMax = nSteps;
          // For payoff 2.strike < S1 < Infinity (only contributes significantly when vol is high)
          var premium = IntegrationHelpers.LogNormalIntegration(nMax, FBasket2, bc2, -1, q1, fwdPrice1, p)[0];
          nSteps += nMax;
        // ... plus payoff when 0 < S1 < 2.strike  (main contribution)
          var result = IntegrationHelpers.LogNormalIntegration(nSteps, FBasket1, 0, bc2, q1, fwdPrice1, p);
          result[0] = premium + result[0];
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
        public static double[] QOptWithGreeks(Boolean callFlag, double fwdPrice, 
          double strike, double vol1, double vol2, double corr, double t)
        {
          double sqrtT;
          var cp = callFlag ? 1 : -1;
          if (fwdPrice <= 0 || vol1 < 0 || vol2 < 0 || t < 0) throw new Exception("Invalid option parameters");
          var result = new[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
          if (t == 0 || strike <= 0 || vol1 == 0)
          {
            result[0] = Math.Max(cp*(fwdPrice - strike), 0);
            result[1] = cp * (fwdPrice - strike) > 0 ? cp : 0;  
            return result;
          }
          var f = Math.Exp(corr * vol1 * vol2 * t);
          var f1 = fwdPrice * f;
          var srt = vol1 * (sqrtT = Math.Sqrt(t));
          var h1 = Math.Log(f1 / strike) / srt + srt / 2;
          var h2 = h1 - srt;
          var s = cp * strike * new NormalDistribution().CumulativeDistribution(cp * h2);
          var dDfwdPrice = f * cp * new NormalDistribution().CumulativeDistribution(cp * h1);
          var v = Constants.InvSqrt2Pi * Math.Exp(-h1 * h1 / 2);
            result[1] = dDfwdPrice;
            result[2] = f * v / (fwdPrice * srt);
            result[3] = f1 * v * sqrtT + dDfwdPrice * fwdPrice * corr * vol2 * t;
            result[4] = dDfwdPrice * fwdPrice * corr * vol1 * t;
            result[5] = dDfwdPrice * fwdPrice * vol1 * vol2 * t;
            result[6] = vol1 * (.5 * f1 * v / sqrtT + dDfwdPrice * fwdPrice * corr * vol2);
          result[0] = fwdPrice*dDfwdPrice - s;
          return result;
        }

        /// <summary>
        /// Values a digital option with greeks.
        /// </summary>
        /// <param name="callFlag"></param>
        /// <param name="fwdPrice"></param>
        /// <param name="strike"></param>
        /// <param name="vol"></param>
        /// <param name="t"></param>
        /// <returns>value, d_dfwdPrice, d2_dfwdPrice2, d_dvol, d_dstrike, d2_dstrike2, d_dt</returns>
        public static double[] DigitalWithGreeks(Boolean callFlag, double fwdPrice, double strike, double vol, double t) 
        {
            var cp = callFlag ? 1 : -1;
          if (fwdPrice <= 0 || vol < 0 || t < 0) throw new Exception("Invalid option parameters");
          var result = new[] {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0};
          if (t == 0 || strike <= 0 || vol == 0)
          {
            if (callFlag)
            {
                result[0] = (fwdPrice > strike) ?  1 : 0;
                return result;
            }
            result[0] = (fwdPrice > strike) ? 0 : 1;
              return result;
          }
          var srt = vol * Math.Sqrt(t);
          var fsrt = fwdPrice * srt;
          var ksrt = strike * srt;
          var h = Math.Log(fwdPrice / strike) / srt - srt / 2;
          var hsrt = h + srt;
          var q = -Constants.InvSqrt2Pi * cp * Math.Exp(-h * h / 2);
          var d = q / fsrt;
          var e = q * hsrt;
          var f = q / ksrt;
            result[0] = new NormalDistribution().CumulativeDistribution(cp*h);
          result[1] = -d;
          result[2] = d * hsrt / fsrt;
          result[3] = e / vol;
          result[4] = f;
          result[5] = f * (h - srt) / ksrt;
          result[6] = e / (2 * t);
          return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="k"></param>
        /// <param name="sigma"></param>
        /// <param name="T"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public static object[] BlackFwdPrice(double f, double k, double sigma, double T, int lambda)
        {
            var output = new object[4];
            double d1 = (Math.Log(f / k) + 0.5 * Math.Pow(sigma, 2) * T) / (sigma * Math.Sqrt(T));
            double d2 = d1 - sigma * Math.Sqrt(T);
            double nd1 = Misc.CummNormDist(d1 * lambda);
            double nd2 = Misc.CummNormDist(d2 * lambda);
            output[0] = (f * nd1 - k * nd2) * lambda;
            output[1] = nd1 * lambda;
            output[2] = (Math.Pow(2 * Math.PI, -0.5) * Math.Exp(-0.5 * Math.Pow(d1, 2))) / (f * sigma * Math.Sqrt(T));
            output[3] = (Math.Pow(2 * Math.PI, -0.5) * Math.Exp(-0.5 * Math.Pow(d1, 2))) * (f * Math.Sqrt(T));
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="k"></param>
        /// <param name="price"></param>
        /// <param name="T"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public static double BlackImpliedVol(double f, double k, double price, double T, int lambda)
        {
            double volatility = 0.1;
            double diff = 10;
            int c = 0;
            while (diff > Math.Pow(10, -10) | c < 1000)
            {
                object[] optPriceOut = BlackFwdPrice(f, k, volatility, T, lambda);
                diff = price - (double)optPriceOut[0];
                volatility += diff / (double)optPriceOut[3] * 0.01;
                c++;
            }
            return volatility;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="futuresPrice"></param>
        /// <param name="strikePrice"></param>
        /// <param name="volatility"></param>
        /// <param name="valueDate"></param>
        /// <param name="expiryMonthYear"></param>
        /// <param name="putCall"></param>
        /// <param name="holidays"></param>
        /// <returns></returns>
        public static object[] SFEBondOptionPrice(string contract, double futuresPrice, double strikePrice, double volatility, DateTime valueDate, DateTime expiryMonthYear, string putCall, object[,] holidays)
        {
            DateTime[] dates = DateHelper.SFEBondDates(expiryMonthYear, holidays);
            TimeSpan span = (dates[2].Date - valueDate.Date);
            double T = span.Days;
            T = T / 365;
            int lambda = 0;
            if (putCall == "C") lambda = 1;
            if (putCall == "P") lambda = -1;
            object[] output = BlackFwdPrice(FuturesAnalytics.SFEBondPrice(contract, futuresPrice), FuturesAnalytics.SFEBondPrice(contract, strikePrice), volatility, T, lambda);
            output[0] = (double)output[0] / FuturesAnalytics.SFEBondTickValue(contract, futuresPrice);
            output[1] = (double)output[1];
            output[2] = (double)output[2] / FuturesAnalytics.SFEBondTickValue(contract, futuresPrice) * 1000;
            output[3] = (double)output[3] / FuturesAnalytics.SFEBondTickValue(contract, futuresPrice) / 1000;
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="futuresPrice"></param>
        /// <param name="strikePrice"></param>
        /// <param name="priceBp"></param>
        /// <param name="valueDate"></param>
        /// <param name="expiryMonthYear"></param>
        /// <param name="putCall"></param>
        /// <param name="holidays"></param>
        /// <returns></returns>
        public static double SFEBondOptionImpliedVol(string contract, double futuresPrice, double strikePrice, double priceBp, DateTime valueDate, DateTime expiryMonthYear, string putCall, object[,] holidays)
        {
            double volatility = 0.05;
            double diff = 10;
            int c = 0;
            while (diff > Math.Pow(10, -10) | c < 1000)
            {
                object[] optPriceOut = SFEBondOptionPrice(contract, futuresPrice, strikePrice, volatility, valueDate, expiryMonthYear, putCall, holidays);
                diff = priceBp - (double)optPriceOut[0];
                volatility += diff / (double)optPriceOut[3] * 0.001;
                c++;
            }
            return volatility;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="futuresPrice"></param>
        /// <param name="strikePrice"></param>
        /// <param name="volatility"></param>
        /// <param name="valueDate"></param>
        /// <param name="expiryMonthYear"></param>
        /// <param name="putCall"></param>
        /// <param name="holidays"></param>
        /// <returns></returns>
        public static object[] SFEBillOptionPrice(double futuresPrice, double strikePrice, double volatility, DateTime valueDate, DateTime expiryMonthYear, string putCall, object[,] holidays)
        {
            double y = (100 - futuresPrice) / 100;
            double k = (100 - strikePrice) / 100;
            DateTime[] dates = DateHelper.SFEBillDates(expiryMonthYear, holidays);
            TimeSpan span = (dates[2].Date - valueDate.Date);
            double T = span.Days;
            T = T / 365;
            int lambda = 0;
            if (putCall == "P") lambda = 1;
            if (putCall == "C") lambda = -1;
            object[] output = BlackFwdPrice(y, k, volatility, T, lambda);
            output[0] = (double)output[0] * 10000;
            output[1] = (double)output[1] * -1;
            output[2] = (double)output[2] / 1000;
            output[3] = (double)output[3] * 100;
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="futuresPrice"></param>
        /// <param name="strikePrice"></param>
        /// <param name="priceBp"></param>
        /// <param name="valueDate"></param>
        /// <param name="expiryMonthYear"></param>
        /// <param name="putCall"></param>
        /// <param name="holidays"></param>
        /// <returns></returns>
        public static double SFEBillOptionImpliedVol(double futuresPrice, double strikePrice, double priceBp, DateTime valueDate, DateTime expiryMonthYear, string putCall, object[,] holidays)
        {
            double volatility = 0.1;
            double diff = 10;
            int c = 0;
            while (diff > Math.Pow(10, -10) | c < 1000)
            {
                object[] optPriceOut = SFEBillOptionPrice(futuresPrice, strikePrice, volatility, valueDate, expiryMonthYear, putCall, holidays);
                diff = priceBp - (double)optPriceOut[0];
                volatility += diff / (double)optPriceOut[3] * 0.01;
                c++;
            }
            return volatility;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="futuresPrice"></param>
        /// <param name="strikePrice"></param>
        /// <param name="volatility"></param>
        /// <param name="valueDate"></param>
        /// <param name="expiryMonthYear"></param>
        /// <param name="putCall"></param>
        /// <param name="holidays"></param>
        /// <returns></returns>
        public static object[] CMEEuroOptionPrice(double futuresPrice, double strikePrice, double volatility, DateTime valueDate, DateTime expiryMonthYear, string putCall, object[,] holidays)
        {
            double y = (100 - futuresPrice) / 100;
            double k = (100 - strikePrice) / 100;
            DateTime[] dates = DateHelper.CMEEuroDates(expiryMonthYear, holidays);
            TimeSpan span = (dates[2].Date - valueDate.Date);
            double T = span.Days;
            T = T / 365;
            int lambda = 0;
            if (putCall == "P") lambda = 1;
            if (putCall == "C") lambda = -1;
            object[] output = BlackFwdPrice(y, k, volatility, T, lambda);
            output[0] = (double)output[0] * 10000;
            output[1] = (double)output[1] * -1;
            output[2] = (double)output[2] / 1000;
            output[3] = (double)output[3] * 100;
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="futuresPrice"></param>
        /// <param name="strikePrice"></param>
        /// <param name="priceBp"></param>
        /// <param name="valueDate"></param>
        /// <param name="expiryMonthYear"></param>
        /// <param name="putCall"></param>
        /// <param name="holidays"></param>
        /// <returns></returns>
        public static double CMEEuroOptionImpliedVol(double futuresPrice, double strikePrice, double priceBp, DateTime valueDate, DateTime expiryMonthYear, string putCall, object[,] holidays)
        {
            double volatility = 0.2;
            double diff = 10;
            int c = 0;
            while (diff > Math.Pow(10, -10) | c < 1000)
            {
                object[] optPriceOut = CMEEuroOptionPrice(futuresPrice, strikePrice, volatility, valueDate, expiryMonthYear, putCall, holidays);
                diff = priceBp - (double)optPriceOut[0];
                volatility += diff / (double)optPriceOut[3] * 0.01;
                c++;
            }
            return volatility;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="k"></param>
        /// <param name="expiry"></param>
        /// <param name="beta"></param>
        /// <param name="alpha"></param>
        /// <param name="rho"></param>
        /// <param name="nu"></param>
        /// <returns></returns>
        public static double BlackVolSABR(double f, double k, double expiry, double beta, double alpha, double rho, double nu)
        {
            double z = nu / alpha * Math.Pow((f * k), 0.5 * (1 - beta)) * Math.Log(f / k);
            double x = Math.Log((Math.Sqrt(1 - 2 * rho * z + z * z) + z - rho) / (1 - rho));
            double n1 = (Math.Pow(alpha, 2) * Math.Pow(1 - beta, 2)) / (24 * Math.Pow((f * k), 1 - beta));
            double n2 = (rho * beta * nu * alpha) / (4 * Math.Pow(f * k, 0.5 * (1 - beta)));
            double n3 = Math.Pow(nu, 2) * (2 - 3 * Math.Pow(rho, 2)) / 24;
            double d1 = Math.Pow(1 - beta, 2) / 24 * Math.Pow(Math.Log(f / k), 2);
            double d2 = Math.Pow(1 - beta, 4) / 1920 * Math.Pow(Math.Log(f / k), 4);
            if (f != k && nu != 0)
            {
                return (alpha * (1 + (n1 + n2 + n3) * expiry)) / (Math.Pow(f * k, 0.5 * (1 - beta)) * (1 + d1 + d2)) * z / x;
            }
            return (alpha * (1 + (n1 + n2 + n3) * expiry)) / (Math.Pow(f * k, 0.5 * (1 - beta)) * (1 + d1 + d2));
        }
    }
}