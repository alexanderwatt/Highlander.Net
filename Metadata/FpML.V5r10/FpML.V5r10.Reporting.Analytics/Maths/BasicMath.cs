using System;

namespace Orion.Analytics.Maths
{
    ///// <summary>
    ///// Saclar objective function.
    ///// </summary>
    //public abstract class Scalobjfn
    //{
    //    /// <summary>
    //    /// the value result.
    //    /// </summary>
    //    /// <param name="x"></param>
    //    /// <returns></returns>
    //    public abstract double Value(double x);

    //    /// <summary>
    //    /// THe derivative.
    //    /// </summary>
    //    /// <param name="x"></param>
    //    /// <returns></returns>
    //    public abstract double Deriv(double x);
    //}

    ///// <summary>
    ///// Black SCholes objective function.
    ///// </summary>
    //public class BSobjfn : Scalobjfn
    //{
    //    /// <summary>
    //    /// The mean x value.
    //    /// </summary>
    //    public double Meanx;

    //    /// <summary>
    //    /// The mean y value.
    //    /// </summary>
    //    public double Meany;

    //    /// <summary>
    //    /// Main constructor.
    //    /// </summary>
    //    /// <param name="meanx"></param>
    //    /// <param name="meany"></param>
    //    public BSobjfn(double meanx, double meany) { Meanx = meanx; Meany = meany; }

    //    /// <summary>
    //    /// The objective function value method.
    //    /// </summary>
    //    /// <param name="variance"></param>
    //    /// <returns></returns>
    //    public override double Value(double variance)
    //    {
    //        return BasicMath.Blackscholes(Meanx, Meany, variance);
    //    }

    //    /// <summary>
    //    /// The objective function driv method.
    //    /// </summary>
    //    /// <param name="variance"></param>
    //    /// <returns></returns>
    //    public override double Deriv(double variance)
    //    {
    //        return BasicMath.BlackscholesWithGreeks(Meanx, Meany, variance, true)[3];
    //    }
    //}

    ///// <summary>
    ///// A polynomila objective function.
    ///// </summary>
    //public class Polyobjfn : Scalobjfn
    //{
    //    /// <summary>
    //    /// coefficents
    //    /// </summary>
    //    protected double[] Coeffs;

    //    /// <summary>
    //    /// poly order.
    //    /// </summary>
    //    protected int Polyorder;

    //    /// <summary>
    //    /// the main ctor.
    //    /// </summary>
    //    /// <param name="coeffs"></param>
    //    /// <param name="polyorder"></param>
    //    public Polyobjfn(double[] coeffs, int polyorder) { Coeffs = coeffs; Polyorder = polyorder; }

    //    /// <summary>
    //    /// The value.
    //    /// </summary>
    //    /// <param name="xvalue"></param>
    //    /// <returns></returns>
    //    public override double Value(double xvalue) { return BasicMath.Polyval(Coeffs, xvalue, Polyorder); }

    //    /// <summary>
    //    /// The deriv.
    //    /// </summary>
    //    /// <param name="x"></param>
    //    /// <returns></returns>
    //    public override double Deriv(double x)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    ///// <summary>
    ///// Another objective function.
    ///// </summary>
    //public class CAPobjfn : Scalobjfn
    //{
    //    /// <summary>
    //    /// forwards
    //    /// </summary>
    //    protected double[] Forwards;

    //    /// <summary>
    //    /// weights.
    //    /// </summary>
    //    protected double[] Weights;

    //    /// <summary>
    //    /// Settlement times.
    //    /// </summary>
    //    protected double[] Settimes;

    //    /// <summary>
    //    /// strikes.
    //    /// </summary>
    //    protected double[] Strikes;

    //    /// <summary>
    //    /// A flat strike.
    //    /// </summary>
    //    protected double Flatstrike;

    //    /// <summary>
    //    /// number of coupons.
    //    /// </summary>
    //    protected int Numcoups;

    //    /// <summary>
    //    /// The grreks.
    //    /// </summary>
    //    protected bool Greeks;

    //    /// <summary>
    //    /// numcoups, liborfwds, weights, settimes, flatstrike, strikes
    //    /// </summary>
    //    /// <param name="forwards"></param>
    //    /// <param name="weights"></param>
    //    /// <param name="settimes"></param>
    //    /// <param name="strikes"></param>
    //    /// <param name="flatstrike"></param>
    //    /// <param name="numcoups"></param>
    //    /// <param name="greeks"></param>
    //    public CAPobjfn(int numcoups,
    //        double[] forwards,
    //        double[] weights,
    //        double[] settimes,
    //        double flatstrike,
    //        double[] strikes,
    //         bool greeks)
    //    {
    //        Numcoups = numcoups;
    //        Forwards = forwards;
    //        Weights = weights;
    //        Settimes = settimes;
    //        Flatstrike = flatstrike;
    //        Strikes = strikes;
    //        Greeks = greeks;
    //    }

    //    /// <summary>
    //    /// The value method
    //    /// </summary>
    //    /// <param name="vol"></param>
    //    /// <returns></returns>
    //    public override double Value(double vol)
    //    {
    //        var sum = 0.0;
    //        if (Strikes == null)		// flat strike case
    //        {
    //            for (var i = 1; i <= Numcoups; i++)
    //            {
    //                sum += Weights[i] * BasicMath.Blackscholes(Forwards[i], Flatstrike, Math.Pow(vol, 2) * Settimes[i]);
    //            }
    //        }
    //        else				// vector strike case
    //        {
    //            for (var i = 1; i <= Numcoups; i++)
    //            {
    //                sum += Weights[i] * BasicMath.Blackscholes(Forwards[i], Strikes[i], Math.Pow(vol, 2) * Settimes[i]);
    //            }
    //        }
    //        return sum;
    //    }

    //    /// <summary>
    //    /// The deriv mnethod.
    //    /// </summary>
    //    /// <param name="vol"></param>
    //    /// <returns></returns>
    //    public override double Deriv(double vol)
    //    {
    //        var sum = 0.0;
    //        if (Strikes == null)
    //        {
    //            for (var i = 1; i <= Numcoups; i++)
    //            {
    //                var result = BasicMath.BlackscholesWithGreeks(Forwards[i], Flatstrike, Math.Pow(vol, 2) * Settimes[i], Greeks);
    //                sum += Weights[i] * result[3] * 2.0 * vol * Settimes[i];
    //            }
    //        }
    //        else
    //        {
    //            for (var i = 1; i <= Numcoups; i++)
    //            {
    //                var result = BasicMath.BlackscholesWithGreeks(Forwards[i], Strikes[i], Math.Pow(vol, 2) * Settimes[i], Greeks);
    //                sum += Weights[i] * result[3] * 2.0 * vol * Settimes[i];
    //            }
    //        }
    //        return sum;
    //    }
    //}

    /// <summary>
    /// Summary description for MathFunction.
    /// </summary>
    public class BasicMath
    {
        // Coefficients for approximation to  erf  in first interval

        private static readonly double[] ErfA =
        {
            3.16112374387056560e+00, 1.13864154151050156e+02,
            3.77485237685302021e+02, 3.20937758913846947e+03,
            1.85777706184603153e-01
        };

        private static readonly double[] ErfB =
        {
            2.36012909523441209E01, 2.44024637934444173E02,
            1.28261652607737228E03, 2.84423683343917062E03
        };

        // Coefficients for approximation to  erfc  in second interval

        private static readonly double[] ErfC =
        {
            5.64188496988670089e-01, 8.88314979438837594e+00,
            6.61191906371416295e+01, 2.98635138197400131e+02,
            8.81952221241769090e+02, 1.71204761263407058e+03,
            2.05107837782607147e+03, 1.23033935479799725e+03,
            2.15311535474403846e-08
        };

        private static readonly double[] ErfD =
        {
            1.57449261107098347e+01, 1.17693950891312499e+02,
            5.37181101862009858e+02, 1.62138957456669019e+03,
            3.29079923573345963e+03, 4.36261909014324716e+03,
            3.43936767414372164e+03, 1.23033935480374942e+03
        };

        // Coefficients for approximation to  erfc  in third interval

        private static readonly double[] ErfP =
        {
            3.05326634961232344e-01, 3.60344899949804439e-01,
            1.25781726111229246e-01, 1.60837851487422766e-02,
            6.58749161529837803e-04, 1.63153871373020978e-02
        };

        private static readonly double[] ErfQ =
        {
            2.56852019228982242e+00, 1.87295284992346047e+00,
            5.27905102951428412e-01, 6.05183413124413191e-02,
            2.33520497626869185e-03
        };

        private const double ErfThreshold = 0.46875;

        // Hardware dependant constants calculated for typical IEEE machine.
        // Sun, Intel, etc

        // the smallest positive floating-point number.
        private const double ErfXMin = 2.2250738585072e-308;

        // the largest positive floating-point number.
        private const double ErfXInf = 1.7976931348623e308;

        // the largest negative argument to erfcx;
        // the negative of the solution to the equation  2*exp(x*x) = XINF.
        private static readonly double ErfXNeg = -1.0 * Math.Sqrt(Math.Log(ErfXInf / 2.0));

        // argument below which erf(x) may be represented by  2*x/sqrt(pi)  and above
        // which  x*x  will not underflow.  A conservative value is the largest 
        // machine number X such that  1.0 + X = 1.0  to machine precision.
        private const double ErfXSmall = 1.110223024625156663e-16;

        // largest argument acceptable to ERFC;  solution to the equation:
        // W(x) * (1-0.5/x**2) = XMIN,  where  W(x) = exp(-x*x)/[x*sqrt(pi)].
        private const double ErfXBig = 26.543e+00;

        // argument above which  1.0 - 1/(2*x*x) = 1.0  to machine precision.  A 
        // conservative value is  1/[2*sqrt(XSMALL)]
        private static readonly double ErfXHuge = 1.0 / (2.0 * Math.Sqrt(ErfXSmall));

        // largest acceptable argument to ERFCX;
        // the minimum of XINF and 1/[sqrt(pi)*XMIN].
        private static readonly double ErfXMax = Math.Min(ErfXInf, 1 / (Constants.SqrtPi * ErfXMin));

        //const double RootTwo = 1.4142135623731;
        ////        const double MagicSeven = 1.19697977039;
        //private static readonly double DBL_LOG_MAX = Math.Log(double.MaxValue) - 1.0;
        ////        const double LoCEV = -0.5;	// allowed range for CEV values
        //const double HiCEV = 1.5;
        //const double RADIX = 2.0;

        private static readonly double[] NDistArray = {-1.26551223, 1.00002368, 0.37409196, 0.09678418,
                                                                     -0.18628806, 0.27886807, -1.13520398, 1.48851587, -0.82215223, 0.17087277 };
        /// <summary>
        /// CND function
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double CND(double p)
        {
            return 0.5 * Infcalerf(-p / Constants.Sqrt2, 1);
        }

        /// <summary>
        /// A simple dmax function.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double Dmax(double x, double y)
        {
            return x > y ? x : y;
        }

        /// <summary>
        /// A simple ndist function.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Ndist(double x)
        {
            var z = Math.Abs(x / Constants.Sqrt2);
            var k = 2.0 / (2.0 + z);
            var value = 0.5 * k * Math.Exp(-z * z + NDistArray[0] + k * (NDistArray[1] + k * (NDistArray[2] + k * (NDistArray[3] + k *
                                                                                                               (NDistArray[4] + k * (NDistArray[5] + k * (NDistArray[6] + k *
                                                                                                                                                                (NDistArray[7] + k * (NDistArray[8] + k * NDistArray[9])))))))));
            if (x >= 0.0)
            {
                return 1.0 - value;
            }
            return value;
        }

  
        /// <summary>
        /// A simple black sholes function.
        /// </summary>
        /// <param name="meanx"></param>
        /// <param name="meany"></param>
        /// <param name="termvar"></param>
        /// <returns></returns>
        public static double Blackscholes(double meanx, double meany, double termvar)
        {
            return BlackscholesWithGreeks(meanx, meany, termvar, false)[0];
        }

        /// <summary>
        /// Returns the value, plus all the main greeks, if requested.
        /// </summary>
        /// <param name="meanx"></param>
        /// <param name="meany"></param>
        /// <param name="termvar"></param>
        /// <param name="greekFlag"></param>
        /// <returns>The value and greeks[0] = d/dmeanx, greeks[1] = d/dmeany, greeks[2] = d/dvar, greeks[3] = d2/dmeanx2, greeks[4] = d2/dmeany2</returns>
        public static double[] BlackscholesWithGreeks(double meanx, double meany, double termvar, bool greekFlag)
        {
            double value;

            if ((meany < 0.0) && (meanx < 0.0))
                throw new Exception("blackscholes: at least one forward must be non-negative");
            if ((termvar <= 0.0) || (meanx <= 0.0) || (meany <= 0.0))
            {
                value = Dmax(meanx - meany, 0.0);
                double[] result;
                if (greekFlag)
                {
                    result = new double[6];
                    if (value > 0.0)
                    {
                        result[0] = value;
                        result[1] = 1.0;
                        result[2] = -1.0;
                        result[3] = 0.0;
                        result[4] = 0.0;
                        result[5] = 0.0;
                    }
                    else
                    {
                        result[0] = value;
                        result[1] = 0.0;
                        result[2] = 0.0;
                        result[3] = 0.0;
                        result[4] = 0.0;
                        result[5] = 0.0;
                    }
                }
                else
                {
                    result = new double[1];
                    result[0] = value;
                }
                return result;
            }
            var logfwd = Math.Log(meanx / meany);
            var sigma = Math.Sqrt(termvar);
            var argone = (logfwd + 0.5 * termvar) / sigma;
            var phione = Ndist(argone);
            var phitwo = Ndist((logfwd - 0.5 * termvar) / sigma);
            value = meanx * phione - meany * phitwo;
            if (greekFlag)
            {
                var result = new double[6];
                var fDens = Math.Exp(-0.5 * Math.Pow(argone, 2)) / Math.Sqrt(2.0 * Math.PI);
                result[0] = value;
                result[1] = phione;
                result[2] = -phitwo;
                result[3] = meanx * fDens / (2.0 * sigma);
                result[4] = fDens / (meanx * sigma);
                result[5] = Math.Pow(meanx / meany, 2) * result[4];		// TOP TIP: F^2 V_FF(F,k) = k^2 V_kk(F,k)
                return result;
            }
            else
            {
                var result = new double[1];
                result[0] = value;
                return result;
            }
        }

        /// <summary>
        /// poynomial value.
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="xvalue"></param>
        /// <param name="polyorder"></param>
        /// <returns></returns>
        public static double Polyval(double[] poly, double xvalue, int polyorder)
        {
            var sum = 0.0;
            for (var i = polyorder - 1; i >= 0; i--)
            {
                sum = sum * xvalue + poly[i];
            }
            return sum;
        }

        /**
     * The AINT(A) Fortran 77 function.
     * Returns A with the fractional portion of its magnitude truncated and its
     * sign preserved. (Also called "truncation towards zero".)
     */
        private static double Aint(double a)
        {
            long n = Convert.ToInt32(a);
            return Convert.ToDouble(n);
        }

        // the error function
        private static double Infcalerf(double arg, int jint)
        {
            double result;
            var x = arg;
            var y = Math.Abs(x);
            double xNum, xDen, ySq;

            if (y <= ErfThreshold)
            {
                // Evaluate erf for  |x| <= 0.46875
                ySq = 0.0;

                if (y > ErfXSmall)
                    ySq = y * y;

                xNum = ErfA[4] * ySq;
                xDen = ySq;

                for (int i = 0; i < 3; ++i)
                {
                    xNum = (xNum + ErfA[i]) * ySq;
                    xDen = (xDen + ErfB[i]) * ySq;
                }
                result = x * (xNum + ErfA[3]) / (xDen + ErfB[3]);
                if (jint != 0)
                    result = 1 - result;

                if (jint == 2)
                    result = Math.Exp(ySq) * result;
                return result;
            }
            if (y <= 4.0)
            {
                // Evaluate erfc for 0.46875 <= |x| <= 4.0
                xNum = ErfC[8] * y;
                xDen = y;
                for (var i = 0; i < 7; ++i)
                {
                    xNum = (xNum + ErfC[i]) * y;
                    xDen = (xDen + ErfD[i]) * y;
                }
                result = (xNum + ErfC[7]) / (xDen + ErfD[7]);

                if (jint != 2)
                {
                    ySq = Aint(y * 16.0) / 16.0;
                    double del = (y - ySq) * (y + ySq);
                    result = Math.Exp(-ySq * ySq) * Math.Exp(-del) * result;
                }
            }
            else
            {
                // Evaluate erfc for |x| > 4.0
                result = 0.0;
                if (y >= ErfXBig)
                {
                    if (jint != 2 || y >= ErfXMax)
                    {
                    }
                    else if (y >= ErfXHuge)
                    {
                        result = Constants.SqrtPi / y;
                    }
                }
                else
                {
                    ySq = 1.0 / (y * y);
                    xNum = ErfP[5] * ySq;
                    xDen = ySq;
                    for (var i = 0; i < 4; ++i)
                    {
                        xNum = (xNum + ErfP[i]) * ySq;
                        xDen = (xDen + ErfQ[i]) * ySq;
                    }
                    result = ySq * (xNum + ErfP[4]) / (xDen + ErfQ[4]);
                    result = (Constants.SqrtPi - result) / y;
                    if (jint != 2)
                    {
                        ySq = Aint(y * 16.0) / 16.0;
                        var del = (y - ySq) * (y + ySq);
                        result = Math.Exp(-ySq * ySq) * Math.Exp(-del) * result;
                    }
                }
            }
            // Fix up for negative argument, erf, etc.
            switch (jint)
            {
                case 0:
                    result = (0.5 - result) + 0.5;
                    if (x < 0)
                        result = -result;
                    break;
                case 1:
                    if (x < 0)
                        result = 2.0 - result;
                    break;
                default:
                    if (x < 0)
                    {
                        if (x < ErfXNeg)
                            result = ErfXInf;
                        else
                        {
                            ySq = Aint(x * 16.0) / 16.0;
                            var del = (x - ySq) * (x + ySq);
                            y = Math.Exp(ySq * ySq) * Math.Exp(del);
                            result = (y + y) - result;
                        }
                    }
                    break;
            }
            return result;
        }
    }
}
