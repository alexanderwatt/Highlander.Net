#region Using directives

using System;
using Orion.Analytics.Helpers;
using Orion.Analytics.Interpolations;
using Orion.Numerics.Distributions.Continuous;
using Orion.Numerics.Integration;
using Orion.Numerics.Maths.Collections;
using Orion.Util.Helpers;
using Orion.Analytics.Rates;
using Orion.Analytics.Dates;

#endregion

namespace Orion.Analytics.Options
{
    /// <summary>
    /// The back-scholes option class.
    /// </summary>
    public class OptionAnalytics
    {
        const double EPS = 1e-7;
        const double OOR2PI = 0.398942280401433; // 1/sqrt(2.pi)   
        const double OO2PI = 0.159154943091895;  // 1/(2.pi}
        
        /// <summary>
        /// Simple Cox-Rubinstein pricing of an option for European exercise
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
        public static double CoxEuroOption(Boolean callFlag, double spotPrice, double fwdPrice, short nSteps,
                                           double strike, double vol, double df, double t)
        {
            short i;

            var u = Math.Exp(vol * Math.Sqrt(t / nSteps));
            var d = 1 / u;
            var d2 = d * d;
            var p = (Math.Pow(fwdPrice / spotPrice, 1.0 / nSteps) - d) / (u - d);
            var pd = (1 - p) / p;
            var pf = df * Math.Pow(p, nSteps);
            var x = spotPrice * Math.Pow(u, nSteps);
            var option = 0.0d;
            for (i = nSteps; i >= 0; i--, x *= d2)
            {
                double v;
                if ((v = x - strike) <= 0) break;
                option += pf * v;
                pf *= pd * i / (nSteps - i + 1);
            }
            if (!callFlag)
            {
                option = option + df * (strike - fwdPrice);
            }
            return option;
        }

        /// <summary>
        /// Solver.
        /// </summary>
        /// <param name="vol"></param>
        /// <param name="t"></param>
        /// <param name="b"></param>
        /// <param name="nSteps"></param>
        /// <returns></returns>
        public static double SolveU(double vol, double t, double b, short nSteps)
        {
            var u = Math.Exp(vol * Math.Sqrt(t / nSteps));
            double du;

            var k = vol * vol * t / (4 * nSteps);
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
        var F = fwdPrice * Math.Pow(2 / (1 + d2), nSteps);
        var x = nSteps * Math.Log(2 / (1 + d2)) / srt + .5 * srt;
        var option = 0.0;
        for(i = nSteps; i >= 0; i--, F *= d2, x += dx)
        {
            double v;
            if ((v = F - strike) <= 0) break;
        option += v * Math.Exp(-.5 * x * x);
        }
        option *= -OOR2PI * df * dx;
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
            double umin;
            var _call = new double[nSteps + 1];
            var _put = new double[nSteps + 1];

            var r = Math.Pow(df, 1.0 / nSteps);
            var u = Math.Exp(vol * Math.Sqrt(t / nSteps));
            var u2 = u * u;
            var d = 1 / u;
            var c1 = r * (u - Math.Pow(fwdPrice / spotPrice, 1.0 / nSteps)) / (u - d);
            var c2 = r - c1;
            var x = umin = spotPrice * Math.Pow(d, nSteps);
              for(i = 0; i <= nSteps; i++, x *= u2)  /* Set up final values ... */
              {
                _call[i] = Math.Max(x - strike, 0);
                _put[i] = Math.Max(strike - x, 0);
              }
              for(i = nSteps - 1; i >= 0; i--)
              {
                x = (umin *= u);
                  int j;
                  for(j = 0; j <= i; j++, x *= u2)
                {
                  _call[j] = Math.Max(c1 * _call[j] + c2 * _call[j + 1], x - strike);
                  _put[j] = Math.Max(c1 * _put[j] + c2 * _put[j + 1], strike - x);
                }
              }
            var result = _call[0];
            if(!callFlag)
            {
                result = _put[0];
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
            var _call = new double[nSteps + 1];
          var _put = new double[nSteps + 1];
          double umin;

          var u = Math.Exp(vol * Math.Sqrt(t / nSteps));
          var u2 = u * u;
          var x = umin = fwdPrice * Math.Pow(u, -nSteps);
          for(i = 0; i <= nSteps; i++, x *= u2)  // Values at expiry ...
          {
            _call[i] = Math.Max(x - strike, 0);
            _put[i] = Math.Max(strike - x, 0);
          }
          var r = Math.Pow(df, 1.0 / nSteps);
          var pu = r / (u + 1);
          var pd = r - pu;
          for(i = nSteps - 1; i >= 0;i--)
          {
            x = (umin *= u);
              int j;
              for(j = 0; j <= i; j++, x *= u2)
            {
              _call[j] = Math.Max(pd * _call[j] + pu * _call[j + 1], x - strike);
              _put[j] = Math.Max(pd * _put[j] + pu * _put[j + 1], strike - x);
            }
          }
          var result = _call[0];
          if (!callFlag)
          {
              result = _put[0];
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
        /// \int_{-\infty}^u dx {a exp(-x^2/2) \over a^2 + x^2}.
        /// We integrate from zero, since the integrand is even in x and
        /// \int_0^\infty dx {a exp(-x^2/2) \over a^2 + x^2} = \pi exp(a^2/2) cn(-a)
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
                var funcx = (aa * Math.Exp(-(x) * (x) / 2) / (a2 + (x) * (x)));
                var s = (h = x) * (func0 + funcx) / 2;
                double osm;
                do
                {
                    var os = s;
                    osm = sm;
                    /* We are calculating the sum of FUNC(z) for z from h/2 to x-h/2 in steps of
                       h, but have arranged it in this way to maximise speed ... */
                    double zi;
                    double zii;
                    var z = (zi = zii = h * h) / 8;
                    var f = Math.Exp(-z) * aa / 2;
                    z += a2 / 2;
                    var zmax = (x * x + a2) / 2;
                    double r;
                    var g = r = Math.Exp(-zii);
                    double sum;
                    for (sum = 0; z < zmax; z += zi, zi += zii, f *= g, g *= r)
                        sum += f / z;
                    s = s / 2 + (h /= 2) * sum;
                    sm = (4 * s - os) / 3;
                } while (Math.Abs(sm - osm) >= EPS * (1e-3 + Math.Abs(u > 0 ? q + osm : q - osm)));
            }
            q += u > 0 ? sm : -sm;
            q = q > 0 ? q : 0;
            return a > 0 ? q : -q;
        }

        /// <summary>
        /// Cumulative bivariate normal distribution,
        /// N_2 (x_1, x_2; \rho) =
        /// {1 \over 2\pi\sqrt{1-\rho^2}} \int_{-\infty}^{x_1} dx\int_{-\infty}^{x_2} dy
        /// exp(-{1\over 2}{(x^2 - 2\rho xy + y^2 \over 1-\rho^2)})
        /// where \rho is the correlation coefficient.
        /// This is needed to value options on options and complex choosers.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="corr"></param>
        /// <returns></returns>
        public static double Norm2(double x1, double x2, double corr)
        {
            if (corr < -1 || corr > 1) throw new Exception("Correlation must be between -1 and 1");
            if (corr == -1) return x1 > -x2 ? new NormalDistribution().CumulativeDistribution(x1) - new NormalDistribution().CumulativeDistribution(-x2) : 0;
            if (corr == 1) return new NormalDistribution().CumulativeDistribution(Math.Min(x1, x2));
            var s = 1 / Math.Sqrt(1 - corr * corr);
            if (x1 == 0)
            {
                if (x2 == 0) return 0.25 + OO2PI * Math.Atan(corr * s) * s;
                return (x2 > 0 ? 0.5 : 0) - OO2PI * Math.Exp(-x2 * x2 / 2) * SimpsonsRuleIntegration.Value(-corr * s * x2, x2);
            }
            if (x2 == 0)
                return (x1 > 0 ? 0.5 : 0) - OO2PI * Math.Exp(-x1 * x1 / 2) * SimpsonsRuleIntegration.Value(-corr * s * x1, x1);
            return (x1 < 0 || x2 < 0 ? 0 : 1) - OO2PI * (
               Math.Exp(-x2 * x2 / 2) * SimpsonsRuleIntegration.Value(s * (x1 - corr * x2), x2) +
               Math.Exp(-x1 * x1 / 2) * SimpsonsRuleIntegration.Value(s * (x2 - corr * x1), x1));
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
        /// <returns>An array of reuslts for Black Scholes.</returns>
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
            var v = OOR2PI * Math.Exp(-h * h / 2);
            var gamma = v / (fwdPrice * srt);
            var vega = v * fwdPrice * sqrtT;
            var prem = callFlag ? fwdPrice * delta - strike * d2 : fwdPrice * delta + strike * d3;
            var rho = -t * prem;
            var theta = vega * vol / (2 * t);
            result[0] = prem;
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
        /// <returns>An array of reuslts for Black Scholes.</returns>
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
            var v = OOR2PI*Math.Exp(-h*h/2);
            var gamma = v/(fwdPrice*srt);
            var vega = v*fwdPrice*sqrtT;
            var prem = callFlag ? fwdPrice * delta - strike * new NormalDistribution().CumulativeDistribution((h - srt)) : fwdPrice * delta + strike * new NormalDistribution().CumulativeDistribution(-1 * (h - srt));
            var rho = -t*prem;
            var theta = vega*vol/(2*t) ;
            result[0] = prem;
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
        /// <param name="prem"></param>
        /// <param name="r"></param>
        /// <param name="t"></param>
        /// <returns>The volatiltiy for that price.</returns>
        public static double OptSolveVol(Boolean callFlag, double fwdPrice, double strike, double prem, double r, double t)
        {
            double vol = 0.20, dvol;
            var CP = callFlag ? 1 : -1;
            if (fwdPrice <= 0 || strike <= 0 || t <= 0 || prem <= 0) return 0;
            if (prem < Math.Max(CP * Math.Exp(-r * t) * (fwdPrice - strike), 0)) throw new Exception("No solution for volatility");
            do
            {
                var risks = OptWithGreeks(callFlag, fwdPrice, strike, vol, t);
                if (risks[3] == 0) throw new Exception("No volatility solution");
                dvol = (risks[0] - prem) / risks[3];
                vol -= dvol;
            } while (Math.Abs(dvol) > EPS);
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
        /// <param name="prem"></param>
        /// <returns>The forward value for that price and volatility.</returns>
        public static double OptSolveFwd(Boolean callFlag, double strike, double vol, double r, double t, double prem)
        {
            double fold;
            var rt = r * t;

            if (prem <= 0) throw new Exception("Invalid option parameters");
            var CP = callFlag ? 1:-1;
            var df = Math.Exp(-rt);
            var vrt = vol * Math.Sqrt(t);
            var fwdPrice = (strike * df + CP * prem) / df;
            do
            {
                var h = Math.Log(fwdPrice / strike) / vrt + vrt / 2;
                var delta = CP * df * new NormalDistribution().CumulativeDistribution(CP * h);
                var y = fwdPrice * delta - df * CP * strike * new NormalDistribution().CumulativeDistribution(CP * (h - vrt)) - prem;
                fold = fwdPrice;
                fwdPrice -= y / delta;     /* Employ Newton-Rapheson technique */
            } while (Math.Abs(fwdPrice - fold) > EPS);
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
        /// <param name="prem"></param>
        /// <returns></returns>
        public static double OptSolveStrike(Boolean callFlag, double fwd, double vol, double r, double t, double prem)
        {
            double Fold;
            var rt = r * t;
            if (prem <= 0) throw new Exception("Invalid option parameters");
            var CP = callFlag ? 1 : -1;
            var df = Math.Exp(-rt);
            var vrt = vol * Math.Sqrt(t);
            var strike = (fwd * df - CP * prem) / df;
            do
            {
                var h = Math.Log(fwd / strike) / vrt + vrt / 2;
                var delta = -CP * df * new NormalDistribution().CumulativeDistribution(CP * (h - vrt));

                var y = BlackScholesWithGreeks(callFlag, fwd, strike, vol, t)[0, 0] - prem;
                Fold = strike;
                strike -= y / delta;


            } while (Math.Abs(strike - Fold) > EPS);
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
        /// <returns>The order of the return types is: Prem, Delta, Gamma, Vega, ThetaS, ThetaL, RhoS, RhoL</returns>
        public static double[] CompoundOpt(Boolean callOnOptionFlag, double strikeS, double rS, double tS, 
          Boolean callFlag, double strikeL, double rL, double tL, double fwdPrice, double vol)
        {
          double sqrttS, sqrttL;
          var result = new double[8];
          var CPS = callOnOptionFlag ? 1 : -1;
          var CPL = callFlag ? 1 : -1;
          if (vol <= 0 || tS < 0 || tL < tS) throw new Exception("Invalid option parameters");
          var cpCP = CPL * CPS;
          var vrtS = vol * (sqrttS = Math.Sqrt(tS));
          var vrtL = vol * (sqrttL = Math.Sqrt(tL));
          var rtS = rS * tS;
          var rtL = rL * tL;
          var tT = tL - tS;
          var x = (Math.Log(OptSolveFwd(callFlag, strikeL, vol, (rtL - rtS) / tT, tT, strikeS) / fwdPrice) - rtL) / vrtS + vrtS / 2;
          var y = (Math.Log(fwdPrice / strikeL) + rtL) / vrtL + vrtL / 2;
          var corr = CPS * sqrttS / sqrttL;
          var xVrt = x - vrtS;
          var rtT = CPL / Math.Sqrt(tT);
          var fvol = fwdPrice * vol;
          var B = cpCP * strikeL  * Math.Exp(-rtL) * new NormalDistribution().BivariateNormal(-cpCP * x, CPL * (y - vrtL), corr);
          var cn1 = OOR2PI / sqrttS * Math.Exp(-xVrt * xVrt / 2) * new NormalDistribution().CumulativeDistribution(rtT * (xVrt * sqrttS + y * sqrttL));
          var cn2 = CPS * OOR2PI / sqrttL * Math.Exp(-y * y / 2) * new NormalDistribution().CumulativeDistribution(-CPS * rtT * (xVrt * sqrttL + y * sqrttS));
          var cn3 = CPS * strikeS * Math.Exp(-rtS) * new NormalDistribution().CumulativeDistribution(-cpCP * x);
          result[1] = cpCP * new NormalDistribution().BivariateNormal(-cpCP * xVrt, CPL * y, corr);
          result[2] = (cn1 + cn2) / fvol;
          result[3] = fwdPrice * (tS * cn1 + tL * cn2);
          result[4] = fvol * cn1 / 2 + rS * cn3;
          result[5] = fvol * cn2 / 2 + rL * B;
          result[6] = tS * cn3;
          result[7] = tL * B;
          result[0] = fwdPrice * result[1] - B - cn3;
          return result;
        }

        ///<summary>
        /// The basic put/call payoff function.
        ///</summary>
        ///<param name="blackScholesPrams">The first element is the stock price, the second is the strike.</param>
        ///<returns>The stock preice minus the strike.</returns>
        public static double fOpt(DoubleVector blackScholesPrams)
        {
            return blackScholesPrams[0] - blackScholesPrams[1];
        }

        /// <summary>
        /// Converts from price volatility to yield volatility.
        /// </summary>
        /// <param name="pricevol"></param>
        /// <param name="bpv"></param>
        /// <param name="price"></param>
        /// <param name="yield"></param>
        /// <returns
        public static double PricetoYieldVol(double pricevol, double bpv, double price, double yield)
        {
            if (pricevol < 0.0 || bpv < 0 || price <= 0.0 || yield <= 0.0) throw new Exception("Not valid inputs.");

            var convFactor = -Math.Log((price + bpv) / price) / Math.Log((yield - 0.0001) / yield);

            return pricevol / convFactor;

        }

        /// <summary>
        /// Converts from yield volatility to price volatility.
        /// </summary>
        /// <param name="yieldvol"></param>
        /// <param name="bpv"></param>
        /// <param name="price"></param>
        /// <param name="yield"></param>
        /// <returns></returns>
        public static double YieldtoPriceVol(double yieldvol, double bpv, double price, double yield)
        {
            if (yieldvol < 0.0 || bpv < 0.0 || yield < 0.0) throw new Exception("Not valid inputs.");

            var convFactor = -Math.Log((price + bpv) / price) / Math.Log((yield - 0.0001) / yield);

            return yieldvol * convFactor;
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
            var v = OOR2PI * Math.Exp(-h1 * h1 / 2);
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
            v = OOR2PI * Math.Exp(-h2 * h2 / 2);
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
        public DoubleVector CompOptParam(double fwdPrice, Boolean CPs, Boolean cps, double ks, double Ks, double tTs, double vols, double rtTs)
        {
            var result = new DoubleVector(7);
            result[0] = fwdPrice;
            result[1] = CPs ? 1 : -1;
            result[2] = cps ? 1 : -1;
            result[3] = ks;
            result[4] = Ks;
            result[5] = tTs;
            result[6] = vols;
            result[7] = rtTs;
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
        public struct DSDNParam
        {
            /// <summary>
            /// 
            /// </summary>
          public double K1s;
            /// <summary>
            /// 
            /// </summary>
            public double N1s;
            /// <summary>
            /// 
            /// </summary>
            public double c1s;
            /// <summary>
            /// 
            /// </summary>
            public double c2s;
            /// <summary>
            /// 
            /// </summary>
            public double c3s;
            /// <summary>
            /// 
            /// </summary>
            public double c4s;
            /// <summary>
            /// 
            /// </summary>
            public double c5s;
            /// <summary>
            /// 
            /// </summary>
            public double c6s;
            /// <summary>
            /// 
            /// </summary>
            public double c7s;
            /// <summary>
            /// 
            /// </summary>
            public double c8s;
            /// <summary>
            /// 
            /// </summary>
            public double c9s;
        }

        /// <summary>
        /// To value an option that pays
        /// max(notl1 * (S1 - strike1), notl2 * (S2 - strike2), 0) on the expiry date, where
        /// S1 is the spot price of asset 1 on the expiry date
        /// strike1 is the strike price of asset 1
        /// notl1 is the notional amount of asset 1
        /// (similarly for asset 2)
        /// </summary>
        public static DoubleVector ConvertDSDNParam(DSDNParam dSDNParam)
        {
            var result = new DoubleVector(13);
            result[0] = 0.0d;
            result[1] = dSDNParam.K1s;
            result[2] = dSDNParam.N1s;
            result[3] = dSDNParam.c1s;
            result[4] = dSDNParam.c1s;
            result[6] = dSDNParam.c3s;
            result[7] = dSDNParam.c4s;
            result[8] = dSDNParam.c5s;
            result[9] = dSDNParam.c6s;
            result[10] = dSDNParam.c7s;
            result[11] = dSDNParam.c8s;
            result[12] = dSDNParam.c9s;
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
        public static double f2S2N(DoubleVector parameters)// Vector parameters) : MultivariateRealFunction
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
            DSDNParam P;

            P.K1s = strike1;
            P.N1s = notl1;
            var sqrt_t = Math.Sqrt(t);
            var q1 = vol1 * sqrt_t;
            var q2 = vol2 * sqrt_t;
            P.c1s = corr * vol2 / vol1;
            P.c2s = -P.c1s * Math.Log(fwdPrice1) + 0.5 * corr * q2 * (q1 - corr * q2);
            P.c3s = -notl2 * strike2;
            var z = Math.Sqrt(1 - corr * corr) * q2;
            P.c4s = 1 / z;
            P.c5s = P.c4s * Math.Log(fwdPrice2 / strike2) - 0.5 * z;
            P.c6s = notl2 * fwdPrice2;
            P.c7s = P.c5s + z;
            P.c8s = P.c4s * Math.Log(P.c6s) - 0.5 * z;
            P.c9s = P.c8s + z;
            return Math.Exp(-r * t) * IntegrationHelpers.LogNormalIntegration(nSteps, f2S2N, 0, -1, q1, fwdPrice1, ConvertDSDNParam(P))[0];
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
        /// gamma12, vega1, vega2, corrSens, theta and the nsteps</returns>
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
          var t3 = OOR2PI * IntegrationHelpers.LogNormalIntegration(nSteps, FSpread3, strike, -1, q1, fwdPrice1, P)[0];
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
            var vector = new DoubleVector(8);
            vector[0] = s1;
            vector[1] = bc2;
            vector[2] = bc3;
            vector[3] = bc4;
            vector[4] = bc5;
            vector[5] = bc6;
            vector[6] = bc7;
            vector[7] = bc8;
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
          var
              sqrtT = Math.Sqrt(t);
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
          var prem = IntegrationHelpers.LogNormalIntegration(nMax, FBasket2, bc2, -1, q1, fwdPrice1, p)[0];
          nSteps += nMax;
        // ... plus payoff when 0 < S1 < 2.strike  (main contribution)
          var result = IntegrationHelpers.LogNormalIntegration(nSteps, FBasket1, 0, bc2, q1, fwdPrice1, p);
            result[0] = prem + result[0];
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
          var v = OOR2PI * Math.Exp(-h1 * h1 / 2);
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
          var q = -OOR2PI * cp * Math.Exp(-h * h / 2);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="k"></param>
        /// <param name="expiry"></param>
        /// <param name="Params"></param>
        /// <param name="interpMethod"></param>
        /// <returns></returns>
        public static double InterpBlackVolSABR(double f, double k, double expiry, object[,] Params, string interpMethod)
        {
            int size = Params.GetUpperBound(0);
            var sabrVolAtStrike = new object[size + 1, 2];

            for (int i = 0; i <= size; i++)
            {
                sabrVolAtStrike[i, 0] = (double)Params[i, 0];
                sabrVolAtStrike[i, 1] = BlackVolSABR(f, k, (double)Params[i, 0], (double)Params[i, 1], (double)Params[i, 2], (double)Params[i, 3], (double)Params[i, 4]);
            }

            if (interpMethod == "L")
            {
                return InterpolationFunctions.LinearInterp(sabrVolAtStrike, expiry);
            }

            if (interpMethod == "H")
            {
                return InterpolationFunctions.HSplineInterp(sabrVolAtStrike, expiry);
            }

            if (interpMethod == "V")
            {
                for (int i = 0; i <= size; i++) { sabrVolAtStrike[i, 1] = Math.Pow((double)sabrVolAtStrike[i, 1], 2); }
                return Math.Pow(InterpolationFunctions.LinearInterp(sabrVolAtStrike, expiry), 0.5);
            }

            return InterpolationFunctions.LinearInterp(sabrVolAtStrike, expiry);
        }
    }
}