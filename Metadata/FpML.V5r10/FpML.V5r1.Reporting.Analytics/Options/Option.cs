#region Using directives

using System;
using Extreme.Mathematics;
using Extreme.Mathematics.LinearAlgebra;
using nabCap.QR.ExcelApi.Interface;
using National.QRSC.Utility.Helpers;

//using nabCap.QR.ExcelApi.Interface;

#endregion

namespace National.QRSC.Analytics.Options
{
    /////<summary>
    ///// The exercise type.
    /////</summary>
    //public enum ExerciseType { ET_European , ET_American };

    ///// <summary>
    ///// The metrics available.
    ///// </summary>
    //public enum OptionRiskMetrics{Premium, Delta, Gamma, Vega, Theta, Rho,}

    /// <summary>
    /// The back-scholes option class.
    /// </summary>
    [ExcelName("OptionFunctions")]
    public class Option
    {
        const double EPS = 1e-7;

        const double pi = 3.14159265358979;

        const double OO2PI = 0.159154943091895;  // 1/(2.pi}

        const double OOR2PI = 0.398942280401433; // 1/sqrt(2.pi)   
        
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
        [ExcelName("CoxEuropean")]
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
        [ExcelName("SimpleSolver")]
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
        [ExcelName("CoxEuropean1")]
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
        [ExcelName("CoxEuropean2")]
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
        [ExcelName("CoxEuropean3")]
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
        [ExcelName("CoxEuropean4")]
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
        [ExcelName("CoxAmerican")]
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
        [ExcelName("CoxFuturesAmerican")]
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
        [ExcelName("BlackScholesPrice")]
        public static double Opt(Boolean callFlag, double fwdPrice, double strike, double vol, double t)
        {
            if (fwdPrice < 0 || vol < 0 || t < 0)throw new Exception("Invalid parameters in option evaluation");
            if (fwdPrice == 0) return callFlag ? Math.Max(strike, 0) : Math.Max(-strike, 0); 
            if (strike <= 0)
            {
                /* Options should not have a negative strike: this is only here because this function is
                   called by the AvePriceOption functions, which if evaluated during their averaging period 
                   may have mandatory exercise even though they have not expired ... */
                if (callFlag) return (fwdPrice - strike);
                return 0;
            }
            if (t == 0 || vol == 0)
            {
                return callFlag ? Math.Max((fwdPrice - strike), 0) : Math.Max(-1*(fwdPrice - strike), 0);
            }
            var srt = vol * Math.Sqrt(t);
            var h1 = Math.Log(fwdPrice / strike) / srt + srt / 2;
            var h2 = h1 - srt;
            if (callFlag) return (fwdPrice * cn(h1) - strike * cn(h2));
            return -(fwdPrice * cn(-h1) - strike * cn(-h2));
        }

        /// <summary>
        /// Cumulative normal distribution,
        /// i.e. {1 \over \sqrt{2\pi}} \int_{-\infty}^x du exp(-u^2/2).
        /// Max. error 7.5e-8 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        [ExcelName("CumulativeNormalDistribution")]
        public static double cn(double x)
        {
            if (x == 0) return .5;
            var t = 1 / (1 + .2316419 * Math.Abs(x));
            t *= OOR2PI * Math.Exp(-.5 * x * x) * (.31938153 + t * (-.356563782 +
              t * (1.781477937 + t * (-1.821255978 + t * 1.330274429))));
            return (x >= 0 ? 1 - t : t);
        }

        /// <summary>
        /// Calculate exp(x^2/2) * cn(x)
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        [ExcelName("CumulativeNormalExp")]
        public static double cnbar(double x)
        {
            var t = 2.828427125 / (2.828427125 + Math.Abs(x));
            t = t * Math.Exp(-1.958659411 + t * (1.00002368 + t * (.37409196 +
            t * (.09678418 + t * (-.18628806 + t * (.27886807 + t * (-1.13520398 +
            t * (1.48851587 + t * (-.82215223 + t * .17087277)))))))));
            return (x >= 0 ? Math.Exp(.5 * x * x) - t : t);
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
        [ExcelName("SimpsonsRuleIntegration")]
        public static double I1(double u, double a)
        {
            double sm;
            double x = Math.Abs(u), aa = Math.Abs(a);
            const double c1 =  0.0804492816131978, /* sqrt(sqrt(2) - 1) / 8       */
                       c2 = -0.0441941738241592,  /* -1 / (8 * sqrt(8))          */
                       c3 =  1.2871885058111600,  /* 2 * sqrt(sqrt(2) - 1)       */
                       c4 = -0.3884434935075090,  /* -sqrt(sqrt(2) + 1) / 4      */
                       c5 =  0.0568862325702784,  /* sqrt(sqrt(2)-1)/(4*sqrt(8)) */
                       c6 =  3.1075479480600700,  /* 2 * sqrt(sqrt(2) + 1)       */
                       r8 =  2.8284271247461900;  /* sqrt(8)                     */

        /* outside -5 < u < 5, both the Simpson's rule evaluation & the analytic
           approximation start to get a bit kooky, so just assume infinity values */

          if (u < -5) return 0; /* Value will be less than 7.19e-8 anyway */
          var a2 = aa * aa;
          var q = pi * Math.Exp(a2 / 2) * cn(-aa);
          if (u > 5) return 2 * (a > 0? q: -q);

        /* For small a, we approximate the function with a polynomial:

          a e^(-x^2/2)/(x^2 + a^2)  ~  a / ((x^2 + a^2)(1 + x^2/2 + x^4/8)),

          which we can do analytically, thereby bypassing the need for 10000000000
          Simpson's rule evaluations. Max error 1.5e-4 (for large |x|) */

          if (aa < 0.01)
          {
            var u2 = u * u;
            sm = (Math.Atan(x / aa) + aa * ((c1 + c2 * a2) * Math.Log((u2 + r8 - c3 * x) /
                   (u2 + r8 + c3 * x)) + (c4 + c5 * a2) * Math.Atan2(c6 * x, r8 - u2))) /
                   (1 - a2 / 8  * (4 - a2));
            if (sm > q) sm = q; /* The expression overestimates the integral:
                                   we prevent it from being greater than q here */
          }
          else
          {
            /*  FUNC(x) (aa * exp(-(x) * (x) / 2) / (a2 + (x) * (x))) */
            sm = 0;
              double h;
              var func0 = (aa*Math.Exp(-(0.0)*(0.0)/2)/(a2 + (0.0)*(0.0)));
              var funcx = (aa*Math.Exp(-(x)*(x)/2)/(a2 + (x)*(x)));
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
            } while (Math.Abs(sm - osm) >= EPS * (1e-3 + Math.Abs(u > 0? q + osm: q - osm)));
          }
          q += u > 0? sm: -sm;
          q = q > 0? q: 0;
          return a > 0? q: -q;
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
        [ExcelName("CumulativeBivariateNormalDistribution")]
        public static double Norm2(double x1, double x2, double corr)
        {
            if (corr < -1 || corr > 1) throw new Exception("Correlation must be between -1 and 1");
            if (corr == -1) return x1 > -x2 ? cn(x1) - cn(-x2) : 0;
            if (corr == 1) return cn(Math.Min(x1, x2));
            var s = 1 / Math.Sqrt(1 - corr * corr);
            if (x1 == 0)
            {
                if (x2 == 0) return 0.25 + OO2PI * Math.Atan(corr * s)* s;
                return (x2 > 0 ? 0.5 : 0) - OO2PI * Math.Exp(-x2 * x2 / 2) * I1(-corr * s * x2, x2);
            }
            if (x2 == 0)
                return (x1 > 0 ? 0.5 : 0) - OO2PI * Math.Exp(-x1 * x1 / 2) * I1(-corr * s * x1, x1);
            return (x1 < 0 || x2 < 0 ? 0 : 1) - OO2PI * (
               Math.Exp(-x2 * x2 / 2) * I1(s * (x1 - corr * x2), x2) +
               Math.Exp(-x1 * x1 / 2) * I1(s * (x2 - corr * x1), x1));
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
            var result = new double[6];
            result[0] = 0.0;
            result[1] = 0.0;
            result[2] = 0.0;
            result[3] = 0.0;
            result[4] = 0.0;
            result[5] = 0.0;
            if (fwdPrice < 0 || vol < 0 || t < 0) return result;
            if (fwdPrice == 0)
            {
                result[0] = callFlag ? Math.Max(-1 * strike, 0) : Math.Max(strike, 0);
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
            if (t == 0 || vol == 0)
            {
                var df = callFlag ? 1 : -1;
                result[0] = callFlag ? Math.Max((fwdPrice - strike), 0) : Math.Max(-1 * (fwdPrice - strike), 0);
                result[1] = result[0] > 0 ? df : 0;
                result[5] = -t * result[0];
                return result;
            }
            double sqrt_t;
            var srt = vol * (sqrt_t = Math.Sqrt(t));
            var h = Math.Log(fwdPrice / strike) / srt + srt / 2;
            var delta = callFlag ? cn(h) : -1 * cn(-1 * h);
            var v = OOR2PI * Math.Exp(-h * h / 2);
            var gamma = v / (fwdPrice * srt);
            var vega = v * fwdPrice * sqrt_t;
            var prem = callFlag ? fwdPrice * delta - strike * cn((h - srt)) : fwdPrice * delta + strike * cn(-1 * (h - srt));
            var rho = -t * prem;
            var theta = vega * vol / (2 * t);
            result[0] = prem;
            result[1] = delta;
            result[2] = gamma;
            result[3] = vega;
            result[4] = theta;
            result[5] = rho;
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
        [ExcelName("BlackScholesWithGreeks")]
        public static double[,] BlackScholesWithGreeks(Boolean callFlag, double fwdPrice, double strike, double vol, double t)//TODO put back rate.
        {
            var result = new double[6];
            result[0] = 0.0;
            result[1] = 0.0;
            result[2] = 0.0;
            result[3] = 0.0;
            result[4] = 0.0;
            result[5] = 0.0;
          if (fwdPrice < 0 || vol < 0 || t < 0) throw new Exception("Invalid parameter in option evaluation");
          if (fwdPrice == 0)
          {
              result[0] = callFlag ? Math.Max(-1 * strike, 0) : Math.Max(strike, 0);
              result[5] = -t * result[0];
              return ArrayHelper.DoubleArrayToHorizontalMatrix(result);
          }
          if (strike <= 0)  
          { 
            if (!callFlag)
            {
                return ArrayHelper.DoubleArrayToHorizontalMatrix(result);
            }
            result[1] = 1.0;
            result[0] = result[1] * (fwdPrice - strike);
            result[5] = -t * result[0];
            return ArrayHelper.DoubleArrayToHorizontalMatrix(result);
          }
          if (t == 0 || vol == 0)
          {
              var df = callFlag ? 1 : -1;
              result[0] = callFlag ? Math.Max((fwdPrice - strike), 0) : Math.Max(-1 * (fwdPrice - strike), 0);
              result[1] = result[0] > 0 ? df : 0;
              result[5] = -t * result[0];
              return ArrayHelper.DoubleArrayToHorizontalMatrix(result);
          }
            double sqrt_t;
            var srt = vol*(sqrt_t = Math.Sqrt(t));
            var h = Math.Log(fwdPrice/strike)/srt + srt/2;
            var delta = callFlag ? cn(h) : -1 * cn(-1 * h);
            var v = OOR2PI*Math.Exp(-h*h/2);
            var gamma = v/(fwdPrice*srt);
            var vega = v*fwdPrice*sqrt_t;
            var prem = callFlag ? fwdPrice * delta - strike * cn((h - srt)) : fwdPrice * delta + strike * cn(-1 * (h - srt));
            var rho = -t*prem;
            var theta = vega*vol/(2*t);
            result[0] = prem;
            result[1] = delta;
            result[2] = gamma;
            result[3] = vega;
            result[4] = theta;
            result[5] = rho;
            return ArrayHelper.DoubleArrayToHorizontalMatrix(result);
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
        [ExcelName("BlackScholesVolatilitySolver")]
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
        [ExcelName("BlackScholesForwardSolver")]
        public static double OptSolveFwd(Boolean callFlag, double strike, double vol, double r, double t, double prem)
        {
            double Fold;
            var rt = r * t;

            if (prem <= 0) throw new Exception("Invalid option parameters");
            var CP = callFlag ? 1:-1;
            var df = Math.Exp(-rt);
            var vrt = vol * Math.Sqrt(t);
            var fwdPrice = (strike * df + CP * prem) / df;
            do
            {
                var h = Math.Log(fwdPrice / strike) / vrt + vrt / 2;
                var delta = CP * df * cn(CP * h);
                var y = fwdPrice * delta - df * CP * strike * cn(CP * (h - vrt)) - prem;
                Fold = fwdPrice;
                fwdPrice -= y / delta;     /* Employ Newton-Rapheson technique */
            } while (Math.Abs(fwdPrice - Fold) > EPS);
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
                var delta = -CP * df * cn(CP * (h - vrt));

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
        [ExcelName("CompoundOptionGreeks")]
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
          var x_vrt = x - vrtS;
          var rtT = CPL / Math.Sqrt(tT);
          var Fvol = fwdPrice * vol;
          var B = cpCP * strikeL  * Math.Exp(-rtL) * Norm2(-cpCP * x, CPL * (y - vrtL), corr);
          var cn1 = OOR2PI / sqrttS * Math.Exp(-x_vrt * x_vrt / 2) * cn(rtT * (x_vrt * sqrttS + y * sqrttL));
          var cn2 = CPS * OOR2PI /sqrttL * Math.Exp(-y * y / 2) * cn(-CPS * rtT * (x_vrt * sqrttL + y * sqrttS));
          var cn3 = CPS * strikeS * Math.Exp(-rtS) * cn(-cpCP * x);
          result[1] = cpCP * Norm2(-cpCP * x_vrt, CPL * y, corr);
          result[2] = (cn1 + cn2) / Fvol;
          result[3] = fwdPrice * (tS * cn1 + tL * cn2);
          result[4] = Fvol * cn1 / 2 + rS * cn3;
          result[5] = Fvol * cn2 / 2 + rL * B;
          result[6] = tS * cn3;
          result[7] = tL * B;
          result[0] = fwdPrice * result[1] - B - cn3;
          return result;
        }

        const double LNI_MAX = 0.9868;

        ///<summary>
        /// The basic put/call payoff function.
        ///</summary>
        ///<param name="blackScholesPrams">The first element is the stock price, the second is the strike.</param>
        ///<returns>The stock preice minus the strike.</returns>
        [ExcelName("EuropeanPutCallPayOff")]
        public static double fOpt(Vector blackScholesPrams)
        {
            return blackScholesPrams[0] - blackScholesPrams[1];
        }

        /// <summary>
        /// Numerical integration of the lognormal, i.e.
        /// I = \int_L^U pd(S) dS func(S)
        /// where pd(S) is the measure exp(-(ln(S/S0)+vol^2*t/2)^2/(2*vol^2*t))/(S*vol*sqrt(2*pi*t))
        /// which applies to a random walk of volatility vol after time t such that
        /// S = S0.
        /// Uses Simpson's rule evaluation, after a strategic change of variables.       
        /// </summary>
        /// <param name="nSteps">Max no. of ordinates; if zero or negative no limit;</param>
        /// <param name="fn">The function of S to be evaluated</param>
        /// <param name="L">The lower limit (minimum 0)</param>
        /// <param name="U">The upper limit: -ve value means infinity</param>
        /// <param name="q">volatility * sqrt(t)</param>
        /// <param name="S0">The expectation value of the underlying at time t</param>
        /// <param name="parameters">The parameter Vector of the multivariate function:  the first parameter elements is the asset price S.</param>
        /// <returns>A vector array of results: value, 
        /// delta = {\partial I \over \partial S_0}, 
        /// gamma = {\partial^2 I \over \partial S_0^2}, 
        /// vega = {\partial I \over \partial q}, 
        /// nSteps = The actual number used is returned here.</returns>
        [ExcelName("LogNormalIntegration2")]
        public static double[] LogNormInt2(long nSteps, MultivariateRealFunction fn,
          double L, double U, double q, double S0, Vector parameters)
        {
            double lb, ub, s, s1, s2, sm1 = 0d, sm2 = 0d;
            double v, w, y;
            double yy;
            long n_max = nSteps, n_used;
            var result = new double[5];

            if (S0 <= 0 || q <= 0) throw new Exception("Invalid LogNormInt parameters");
            var q2 = q * q;
            var lnS1 = Math.Log(S0) - .5 * q2;

            /* Change limits to new variables ...  */

            if (L > 0)
            {
                v = (Math.Log(L) - lnS1) / q;
                lb = (Math.Sqrt(.25 + v * v) - .5) / v;
            }
            else lb = -1;
            if (U < 0) ub = 1;
            else if (U == 0) ub = -1;
            else
            {
                v = (Math.Log(U) - lnS1) / q;
                ub = (Math.Sqrt(.25 + v * v) - .5) / v;
            }
            if (ub < lb) { v = ub; ub = lb; lb = v; }
            else if (ub == lb)
            {
                return result;
            }
            if (lb > -LNI_MAX)
            {
                v = q * lb / (1 - lb * lb) + lnS1;
                var xx = Math.Exp(v);
                parameters[0] = xx;
                yy = fn(parameters);
                s = Math.Exp(-.5 * Math.Pow(lb / (1 - lb * lb), 2.0)) * (1 + lb * lb) /
                  Math.Pow(1 - lb * lb, 2.0) * yy;
                s1 = s * v;
                s2 = s1 * v;
            }
            else s = s1 = s2 = 0;
            if (ub < LNI_MAX)
            {
                v = q * ub / (1 - ub * ub) + lnS1;
                var xx = Math.Exp(v);
                parameters[0] = xx;
                yy = fn(parameters);
                w = Math.Exp(-.5 * Math.Pow(ub / (1 - ub * ub), 2.0)) * (1 + ub * ub) / Math.Pow(1 - ub * ub, 2.0) * yy;
                s += w;
                w *= v;
                s1 += w;
                s2 += w * v;
            }
            var h = ub - lb;
            var h2 = h / 2;
            s *= h2;
            s1 *= h2;
            s2 *= h2;
            double sm = 0;
            for (n_used = 2; 2 * n_used - 1 <= n_max || n_max <= 0; n_used += n_used - 1)
            {
                var os = s;
                var os1 = s1;
                var os2 = s2;
                var osm = sm;
                double sum;
                double sum1;
                double sum2;
                double z;
                for (z = lb + h / 2, sum = sum1 = sum2 = 0; z < ub; z += h)
                {
                    if (z < -LNI_MAX || z > LNI_MAX) continue;
                    y = 1 / (1 - z * z);
                    v = z * y;
                    w = v * v;
                    var p = q * v + lnS1;
                    var xx = Math.Exp(v);
                    parameters[0] = xx;
                    yy = fn(parameters);
                    v = Math.Exp(-.5 * w) * (y * y + w) * yy;
                    sum += v;
                    v *= p;
                    sum1 += v;
                    sum2 += v * p;
                }
                h /= 2;
                s = s / 2 + h * sum;  /* include midpoints under trapezoid rule */
                s1 = s1 / 2 + h * sum1;
                s2 = s2 / 2 + h * sum2;
                sm = (4 * s - os) / 3;       /* convert to Simpson's rule */
                sm1 = (4 * s1 - os1) / 3;
                sm2 = (4 * s2 - os2) / 3;
                if (Math.Abs(sm - osm) < 1e-9 + EPS * Math.Abs(osm) && n_used >= 33) break;
            }
            sm *= OOR2PI;
            sm1 *= OOR2PI;
            sm2 *= OOR2PI;
            w = lnS1 / q2;
            v = ((w + 1) * lnS1 - 1) * sm - (1 + 2 * w) * sm1 + sm2 / q2;
            y = S0 * q2;
            result[0] = sm;
            result[1] = (sm1 - lnS1 * sm) / y;
            result[2] = v / (S0 * y);
            result[3] = v / q;
            result[4] = nSteps;
            return result;
        }

        /// <summary>
        /// Numerical integration of the lognormal, i.e.
        /// I = \int_L^U pd(S) dS func(S)
        /// where pd(S) is the measure exp(-(ln(S/S0)+vol^2*t/2)^2/(2*vol^2*t))/(S*vol*sqrt(2*pi*t))
        /// which applies to a random walk of volatility vol after time t such that
        /// S = S0.
        /// Uses Simpson's rule evaluation, after a strategic change of variables.
        /// </summary>
        /// <param name="nSteps">Max no. of ordinates; if zero or negative no limit;</param>
        /// <param name="fn">The function of S to be evaluated</param>
        /// <param name="L">The lower limit (minimum 0)</param>
        /// <param name="U">The upper limit: -ve value means infinity</param>
        /// <param name="q">volatility * sqrt(t)</param>
        /// <param name="S0">The expectation value of the underlying at time t</param>
        /// <param name="parameters">The parameter Vector of the multivariate function:  the first parameter elements is the asset price S.</param>
        /// <returns>A vector array of results: value, 
        /// delta = {\partial I \over \partial S_0}, 
        /// gamma = {\partial^2 I \over \partial S_0^2}, 
        /// vega = {\partial I \over \partial q}, 
        /// nSteps = The actual number used is returned here.</returns>
        [ExcelName("LogNormalIntegration")]
        public static double[] LogNormInt(long nSteps, MultivariateRealFunction fn,
          double L, double U, double q, double S0, Vector parameters)
        {
            double lb, ub, s, s1, s2, sm1 = 0d, sm2 = 0d;
            double v, w, y;
            double yy;
            long n_max = nSteps, n_used;
            var result = new double[5];

            if (S0 <= 0 || q <= 0) throw new Exception("Invalid LogNormInt parameters");
            var q2 = q * q;
            var lnS1 = Math.Log(S0) - .5 * q2;

            /* Change limits to new variables ...  */

            if (L > 0)
            {
                v = (Math.Log(L) - lnS1) / q;
                lb = (Math.Sqrt(.25 + v * v) - .5) / v;
            }
            else lb = -1;
            if (U < 0) ub = 1;
            else if (U == 0) ub = -1;
            else
            {
                v = (Math.Log(U) - lnS1) / q;
                ub = (Math.Sqrt(.25 + v * v) - .5) / v;
            }
            if (ub < lb) { v = ub; ub = lb; lb = v; }
            else if (ub == lb)
            {
                return result;
            }
            if (lb > -LNI_MAX)
            {
                v = q * lb / (1 - lb * lb) + lnS1;
                var xx = Math.Exp(v);
                parameters[0] = xx;
                yy = fn(parameters);
                s = Math.Exp(-.5 * Math.Pow(lb / (1 - lb * lb), 2.0)) * (1 + lb * lb) /
                  Math.Pow(1 - lb * lb, 2.0) * yy;
                s1 = s * v;
                s2 = s1 * v;
            }
            else s = s1 = s2 = 0;
            if (ub < LNI_MAX)
            {
                v = q * ub / (1 - ub * ub) + lnS1;
                var xx = Math.Exp(v);
                parameters[0] = xx;
                yy = fn(parameters);
                w = Math.Exp(-.5 * Math.Pow(ub / (1 - ub * ub), 2.0)) * (1 + ub * ub) / Math.Pow(1 - ub * ub, 2.0) * yy;
                s += w;
                w *= v;
                s1 += w;
                s2 += w * v;
            }
            var h = ub - lb;
            var h2 = h / 2;
            s *= h2;
            s1 *= h2;
            s2 *= h2;
            double sm = 0;
            for (n_used = 2; 2 * n_used - 1 <= n_max || n_max <= 0; n_used += n_used - 1)
            {
                var os = s;
                var os1 = s1;
                var os2 = s2;
                var osm = sm;
                double sum;
                double sum1;
                double sum2;
                double z;
                for (z = lb + h / 2, sum = sum1 = sum2 = 0; z < ub; z += h)
                {
                    if (z < -LNI_MAX || z > LNI_MAX) continue;
                    y = 1 / (1 - z * z);
                    v = z * y;
                    w = v * v;
                    var p = q * v + lnS1;
                    var xx = Math.Exp(p);
                    parameters[0] = xx;
                    yy = fn(parameters);
                    v = Math.Exp(-.5 * w) * (y * y + w) * yy;
                    sum += v;
                    v *= p;
                    sum1 += v;
                    sum2 += v * p;
                }
                h /= 2;
                s = s / 2 + h * sum;  /* include midpoints under trapezoid rule */
                s1 = s1 / 2 + h * sum1;
                s2 = s2 / 2 + h * sum2;
                sm = (4 * s - os) / 3;       /* convert to Simpson's rule */
                sm1 = (4 * s1 - os1) / 3;
                sm2 = (4 * s2 - os2) / 3;
                if (Math.Abs(sm - osm) < 1e-9 + EPS * Math.Abs(osm) && n_used >= 33) break;
            }
            sm *= OOR2PI;
            sm1 *= OOR2PI;
            sm2 *= OOR2PI;
            w = lnS1 / q2;
            v = ((w + 1) * lnS1 - 1) * sm - (1 + 2 * w) * sm1 + sm2 / q2;
            y = S0 * q2;
            result[0] = sm;
            result[1] = (sm1 - lnS1 * sm) / y;
            result[2] = v / (S0 * y);
            result[3] = v / q;
            result[4] = nSteps;
            return result;
        }

        /// <summary>
        /// Converts from price volatility to yield volatility.
        /// </summary>
        /// <param name="pricevol"></param>
        /// <param name="bpv"></param>
        /// <param name="price"></param>
        /// <param name="yield"></param>
        /// <returns></returns>
        [ExcelName("PriceYieldVolatilityConversion")]
        public double PricetoYieldVol(double pricevol, double bpv, double price, double yield)
        {
            if (pricevol < 0.0 || bpv < 0 || price <= 0.0 || yield <= 0.0) throw new Exception("Not valid inputs.");

            var conv_factor = -Math.Log((price + bpv) / price) / Math.Log((yield - 0.0001) / yield);

            return pricevol / conv_factor;

        }

        /// <summary>
        /// Converts from yield volatility to price volatility.
        /// </summary>
        /// <param name="yieldvol"></param>
        /// <param name="bpv"></param>
        /// <param name="price"></param>
        /// <param name="yield"></param>
        /// <returns></returns>
        [ExcelName("YieldPriceVolatiltiyConversion")]
        public double YieldtoPriceVol(double yieldvol, double bpv, double price, double yield)
        {
            if (yieldvol < 0.0 || bpv < 0.0 || yield < 0.0) throw new Exception("Not valid inputs.");

            var conv_factor = -Math.Log((price + bpv) / price) / Math.Log((yield - 0.0001) / yield);

            return yieldvol * conv_factor;
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
        [ExcelName("ChooserOptionWithGreeks")]
        public double[] ChooserOpt(double fwdPrice, double vol, double tS, double tL, double strike, double rL) 
        {
            var results = new double[8];
            double sqrttL, sqrttS;

            if (vol <= 0 || tS < 0 || tL < tS) throw new Exception("Invalid option parameters");

            /* call option (fwdPrice, strike, vol, rL, tL) */

            var srT = vol * (sqrttL = Math.Sqrt(tL));
            var kd = strike * Math.Exp(-rL * tL);
            var d = Math.Log(fwdPrice / kd);
            var h1 = d / srT + srT / 2;
            var s = kd * cn(h1 - srT);
            var delta = cn(h1);
            var v = OOR2PI * Math.Exp(-h1 * h1 / 2);
            var gamma = v / (fwdPrice * srT);
            var vega = v * fwdPrice * sqrttL;
            var rhoL = tL * s;
            var thetaL = s * rL + .5 * vega * vol / tL;
            var prem1 = fwdPrice * delta - s;

            /* ... plus put option (fwdPrice, strike * exp(rS * tS - rL * tL), vol, rS, tS) */

            var srt = vol * (sqrttS = Math.Sqrt(tS));
            var h2 = -d / srt - srt / 2;
            s = kd * cn(h2 + srt); 
            delta += (d = -cn(h2));
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
        public Vector CompOptParam(double fwdPrice, Boolean CPs, Boolean cps, double ks, double Ks, double tTs, double vols, double rtTs)
        {
            var result = new GeneralVector(7);
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
        public static double fComp(Vector parameters)//TODO this needs to be PayOff function.
        {
            var x = parameters[2] == 1.0 ? true : false;
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
          public double K1s;
            public double N1s;
            public double c1s;
            public double c2s;
            public double c3s;
            public double c4s;
            public double c5s;
            public double c6s;
            public double c7s;
            public double c8s;
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
        public static Vector ConvertDSDNParam(DSDNParam dSDNParam)
        {
            var result = new GeneralVector(13);
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

        /////<summary>
        ///// To value an option that pays
        ///// max(notl1 * (S1 - strike1), notl2 * (S2 - strike2), 0) on the expiry date, where
        ///// strike1 is the strike price of asset 1
        ///// notl1 is the notional amount of asset 1
        ///// (similarly for asset 2)
        /////</summary>
        /////<param name="S1">S1 is the spot price of asset 1 on the expiry date</param>
        /////<param name="P">The order for the vector is: S1 (0), K1s (1), N1s (2), c1s (3), 
        ///// c2s (4), c3s (5), c4s (6), c5s (7), c6s (8), c7s (9), c8s (10), c9s</param>
        /////<returns></returns>
        //public static double f2S2N(double S1, DSDNParam P)// Vector parameters) : MultivariateRealFunction
        //{
        //    var lnv = P.c1s * Math.Log(S1) + P.c2s;
        //  if (S1 <= P.K1s)
        //  {
        //    var h0 = P.c4s * lnv;
        //    return P.c3s * cn(h0 + P.c5s) + P.c6s * Math.Exp(lnv) * cn(h0 + P.c7s);
        //  }  
        //  var x = P.N1s * (S1 - P.K1s);    
        //  var y = x - P.c3s;
        //  var k0 = P.c4s * (lnv - Math.Log(y));
        //  return x - y * cn(k0 + P.c8s) + P.c6s * Math.Exp(lnv) * cn(k0 + P.c9s);
        //}

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
        public static double f2S2N(Vector parameters)// Vector parameters) : MultivariateRealFunction
        {
            var lnv = parameters[3] * Math.Log(parameters[0]) + parameters[4];
            if (parameters[0] <= parameters[1])
            {
                var h0 = parameters[6] * lnv;
                return parameters[5] * cn(h0 + parameters[7]) + parameters[8] * Math.Exp(lnv) * cn(h0 + parameters[9]);
            }
            var x = parameters[2] * (parameters[0] - parameters[1]);
            var y = x - parameters[5];
            var k0 = parameters[6] * (lnv - Math.Log(y));
            return x - y * cn(k0 + parameters[10]) + parameters[8] * Math.Exp(lnv) * cn(k0 + parameters[11]);
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
        [ExcelName("DualStrikeDualNotionalCall")]
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
            return Math.Exp(-r * t) * LogNormInt(nSteps, f2S2N, 0, -1, q1, fwdPrice1, ConvertDSDNParam(P))[0];
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
        [ExcelName("AveragePriceOption")]
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
        public static Vector SpreadOptParam(double s1, double Kk,double c1k,double c2k,double c3k,
            double c4k,double c5k,double c6k,double c7k)
        {
            var vector = new GeneralVector(9);
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
        public static double fSpread(Vector parameters) //double s1, SpreadOptParam &P
        {
          var sK = parameters[0] - parameters[1];
          var l1 = Math.Log(parameters[0]);
          var l2 = Math.Log(sK);
          var h = parameters[2] * l1 + parameters[3] * l2 + parameters[4];
          return sK * cn(h) - parameters[5] * Math.Exp(parameters[7] * l1) * cn(h - parameters[6]);
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
       [ExcelName("SpreadOption")]
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
              var P = new GeneralVector(6); //s1 (0), Kk (1), c1k (2), c2k (3), c3k (4), c4k (5), c5k (6), c6k (7), c7k (8)

              if (t < 0) throw new Exception("Bad option parameters");
              if (t == 0) return result;
              if (Math.Abs(corr) >= 1) throw new Exception("Correlation must be between -1 and 1");
              var sqrt_t = Math.Sqrt(t);
              var q1 = vol1 * sqrt_t;
              var q2 = vol2 * sqrt_t;
              var X1 = fwdPrice1 * Math.Exp(-q1 * q1 / 2);
              var X2 = fwdPrice2 * Math.Exp(-q2 * q2 / 2);
              var s = 1 / Math.Sqrt(1 - corr * corr);
              P[1] = strike;
              P[2] = -corr * s / q1;
              P[3] = s / q2;
              P[4] = (corr * Math.Log(X1) / q1 - Math.Log(X2) / q2) * s;
              P[5] = fwdPrice2 * Math.Exp(-corr * corr * q2 * q2 / 2) * Math.Pow(X1, -corr * vol2 / vol1);
              P[6] = q2 / s;                               
              P[7] = corr * q2 / q1;
              return LogNormInt(nSteps, fSpread, strike, -1, q1, fwdPrice1, P);
        }

        /// <summary>
        /// Real function.
        /// </summary>
        /// <param name="spreadOptionParameters"></param>
        /// <returns></returns>
        public static double fSpread1(Vector spreadOptionParameters)
        {
            var sK = spreadOptionParameters[0] - spreadOptionParameters[1];
            if (sK <= 0) return 0;
            return sK * cn(spreadOptionParameters[2] * Math.Log(spreadOptionParameters[0]) + spreadOptionParameters[3] * Math.Log(sK) + spreadOptionParameters[4]);
        }

        /// <summary>
        /// fSpread2
        /// </summary>
        /// <param name="spreadOptionParameters"></param>
        /// <returns></returns>
        public static double fSpread2(Vector spreadOptionParameters)
        {
            var sK = spreadOptionParameters[0] - spreadOptionParameters[1];
            if (sK <= 0) return 0;
            return Math.Pow(spreadOptionParameters[0], spreadOptionParameters[7]) * cn(spreadOptionParameters[2] * Math.Log(spreadOptionParameters[0]) + spreadOptionParameters[3] * Math.Log(sK) + spreadOptionParameters[8]);
        }

        /// <summary>
        /// fSpread3
        /// </summary>
        /// <param name="spreadOptionParameters">s1 (0), Kk (1), c1k (2), c2k (3), c3k (4), c4k (5), c5k (6), c6k (7), c7k (8)</param>
        /// <returns></returns>
        public static double fSpread3(Vector spreadOptionParameters)
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
        public static double fSpread4(Vector spreadOptionParameters)
        {
            var sK = spreadOptionParameters[0] - spreadOptionParameters[1];
            if (sK <= 0) return 0;
            var ls1 = Math.Log(spreadOptionParameters[0]);
            return ls1 * Math.Pow(spreadOptionParameters[0], spreadOptionParameters[7]) * cn(spreadOptionParameters[2] * ls1 + spreadOptionParameters[3] * Math.Log(sK) + spreadOptionParameters[8]);
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
        [ExcelName("SpreadOptionWithGreeks")]
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
          var sqrt_t = Math.Sqrt(t);
          var q1 = vol1 * sqrt_t;
          var q2 = vol2 * sqrt_t;
          var X1 = fwdPrice1 * Math.Exp(-q1 * q1 / 2);
          var X2 = fwdPrice2 * Math.Exp(-q2 * q2 / 2);
          var s = 1 / Math.Sqrt(1 - corr * corr);
          var Kk = strike;
          var c1k = -corr * s / q1;
          var c2k = s / q2;
          var c3k = (corr * Math.Log(X1) / q1 - Math.Log(X2) / q2) * s;
          var c4k = fwdPrice2 * Math.Exp(-corr * corr * q2 * q2 / 2) * Math.Pow(X1, -corr * vol2 / vol1);
          var c5k = q2 / s;
          var c6k = corr * q2 / q1;
          var c7k = c3k - c5k;
          var P = SpreadOptParam(s, Kk, c1k, c2k, c3k, c4k, c5k, c6k, c7k);

          var T1 = LogNormInt(nSteps, fSpread1, strike, -1, q1, fwdPrice1, P);
          var d1 = T1[1];
          var g1 = T1[2];
          var v1 = T1[3];
          var T2 = LogNormInt(nSteps, fSpread2, strike, -1, q1, fwdPrice1, P);
          var d2 = T2[1];
          var g2 = T2[2];
          var v2 = T2[3];
          var c12k = Math.Exp(.5 * corr * q2 * (q1 - corr * q2));
          var c8k = Math.Pow(fwdPrice1, -c6k) * c12k;
          var c10k = fwdPrice2 * c6k * c8k / fwdPrice1;
          result[1] = d1 - c4k * d2 + c10k * T2[0]; //delta1
          result[2] = -c8k * T2[0]; //delta2
          var c9k = -c1k;
          var T3 = OOR2PI * LogNormInt(nSteps, fSpread3, strike, -1, q1, fwdPrice1, P)[0];
          var T4 = LogNormInt(nSteps, fSpread4, strike, -1, q1, fwdPrice1, P)[0];
          result[3] = (g1 - c4k * g2) + c10k * (2 * d2 + ((-c6k - 1) * T2[0] + c9k * T3) / fwdPrice1); //gamma11
          result[4] = c8k * s * T3 / (q2 * fwdPrice2); //gamma22
          result[5] = c8k * (-d2 + (c6k * T2[0] - c9k * T3) / fwdPrice1);  //gamma12
          nSteps = n1 + n2 + n3 + n4;  //TODO there is something about these nuuumsteps that needs fixing.
          result[6] = (v1 - c4k * v2) * sqrt_t + fwdPrice2 * c8k * (
            c6k / vol1 * T4 - (.5 * corr * vol2 * t + c6k / vol1 * Math.Log(fwdPrice1)) * T2[0]); //vega1
          result[7]= fwdPrice2 * c8k * (sqrt_t / s * T3 - corr / vol1 * T4 + (corr / vol1 * Math.Log(fwdPrice1) +
            corr * t * (-.5 * vol1 + corr * vol2)) * T2[0]); //vega2
          result[8] = fwdPrice2 * c8k * (-corr * s * q2 * T3 - vol2 / vol1 * T4 + (vol2 / vol1 * Math.Log(fwdPrice1) + q2 * (
            -.5 * q1 + corr * q2)) * T2[0]); //corrSens
          result[9] = (v1 - c4k * v2) * q1 / (2 * t) + fwdPrice2 * c8k * .5 * vol2 * (1 / (s * sqrt_t) * T3 - 
            corr * (vol1 - corr * vol2) * T2[0]);
          result[0] = T1[0] - c4k * T2[0]; //theta
          result[10] = nSteps;
            return result;
        }

        /// <summary>
        /// Basket option. Pays max(.5 * (S1 + S2) - strike, 0) on the expiry date, 
        /// where S1 & S2 are the spot prices of the assets on the expiry date
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
        public static Vector BasketOptParam(double s1, double bc2, double bc3, double bc4, double bc5, double bc6, double bc7, double bc8)
        {
            var vector = new GeneralVector(8);
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
        /// <param name="P"></param>
        /// <returns></returns>
        public static double fBasket1(Vector P)
        {
          var sK = P[1] - P[0];
          var l1 = Math.Log(P[0]);
          var l2 = Math.Log(sK);
          var h = P[4] * l1 + P[5] * l2 + P[6];
          return -0.5 * sK * cn(h) + P[2] * Math.Exp(P[3] * l1) * cn(h + P[7]);
        }

        /// <summary>
        /// Multivariate real function.
        /// </summary>
        /// <param name="P"></param>
        /// <returns></returns>
        public static double fBasket2(Vector P)
        {
          var sK = P[1] - P[0];
          var l1 = Math.Log(P[0]);
          var l2 = Math.Log(sK);
          var h = P[4] * l1 + P[5] * l2 + P[6];//TODO check why this is not used.
          return -0.5 * sK + P[2] * Math.Exp(P[3] * l1);
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
        [ExcelName("BasketOptionWithGreeks")]
        public double[] BasketOption(
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
              sqrt_t = Math.Sqrt(t);
          var q1 = vol1 * sqrt_t;
          var q2 = vol2 * sqrt_t;
          var X1 = fwdPrice1 * Math.Exp(-q1 * q1 / 2);
          var X2 = fwdPrice2 * Math.Exp(-q2 * q2 / 2);
          var ss = 1 - corr * corr;
          var s = 1 / Math.Sqrt(ss);
          var bc2 = 2 * strike;
          var bc4 = corr * q2 / q1;
          var bc3 = .5 * X2 * Math.Pow(X1, -bc4) * Math.Exp(.5 * q2 * q2 * ss);
          var bc5 = corr * s / q1;
          var bc6 = -s / q2;
          var bc7 = s * (Math.Log(X2) / q2 - corr / q1 * Math.Log(X1));
          var bc8 = -1 / bc6;
          var P = BasketOptParam(s, bc2, bc3, bc4, bc5, bc6, bc7, bc8);
          var n_max = nSteps;

        // For payoff 2.strike < S1 < Infinity (only contributes significantly when vol is high)
          var prem = LogNormInt(n_max, fBasket2, bc2, -1, q1, fwdPrice1, P)[0];
          nSteps += n_max;
        // ... plus payoff when 0 < S1 < 2.strike  (main contribution)
          var result = LogNormInt(nSteps, fBasket1, 0, bc2, q1, fwdPrice1, P);
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
        [ExcelName("QOptionWithGreeks")]
        public double[] QOptWithGreeks(Boolean callFlag, double fwdPrice, 
          double strike, double vol1, double vol2, double corr, double t)
        {
          double sqrt_t;
            var CP = callFlag ? 1 : -1;
          if (fwdPrice <= 0 || vol1 < 0 || vol2 < 0 || t < 0) throw new Exception("Invalid option parameters");
          var result = new[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
          if (t == 0 || strike <= 0 || vol1 == 0)
          {
            result[0] = Math.Max(CP*(fwdPrice - strike), 0);
            result[1] = CP * (fwdPrice - strike) > 0 ? CP : 0;  
            return result;
          }
          var f = Math.Exp(corr * vol1 * vol2 * t);
          var f1 = fwdPrice * f;
          var srt = vol1 * (sqrt_t = Math.Sqrt(t));
          var h1 = Math.Log(f1 / strike) / srt + srt / 2;
          var h2 = h1 - srt;
          var s = CP * strike * cn(CP * h2);
          var d_dfwdPrice = f * CP * cn(CP * h1);
          var v = OOR2PI * Math.Exp(-h1 * h1 / 2);
            result[1] = d_dfwdPrice;
            result[2] = f * v / (fwdPrice * srt);
            result[3] = f1 * v * sqrt_t + d_dfwdPrice * fwdPrice * corr * vol2 * t;
            result[4] = d_dfwdPrice * fwdPrice * corr * vol1 * t;
            result[5] = d_dfwdPrice * fwdPrice * vol1 * vol2 * t;
            result[6] = vol1 * (.5 * f1 * v / sqrt_t + d_dfwdPrice * fwdPrice * corr * vol2);
          result[0] = fwdPrice*d_dfwdPrice - s;
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
        [ExcelName("DigitalOptionWithGreeks")]
        public double[] DigitalWithGreeks(Boolean callFlag, double fwdPrice, double strike, double vol, double t) 
        {
            var CP = callFlag ? 1 : -1;
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
          var q = -OOR2PI * CP * Math.Exp(-h * h / 2);
          var d = q / fsrt;
          var e = q * hsrt;
          var f = q / ksrt;
            result[0] = cn(CP*h);
          result[1] = -d;
          result[2] = d * hsrt / fsrt;
          result[3] = e / vol;
          result[4] = f;
          result[5] = f * (h - srt) / ksrt;
          result[6] = e / (2 * t);
          return result;
        }
       
    }
}