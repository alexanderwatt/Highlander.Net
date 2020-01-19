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

using System;

namespace Highlander.Equities
{
  /// <summary>
  /// Summary description for MathFunction.
  /// </summary>
  public class MathFunction
  {
      private const double M_PI = 3.14159265358979323846;

      /** The is NOT THE square root of PI */
      private const double SQRT_PI = 0.56418958354775628695;

      /** The square root of 2 */
    private static readonly double SQRT_2 = Math.Sqrt(2.0);


    // Coefficients for approximation to  erf  in first interval

    private static double[] ERF_A =
		{ 
			3.16112374387056560e+00, 1.13864154151050156e+02,
			3.77485237685302021e+02, 3.20937758913846947e+03,
			1.85777706184603153e-01
		};

    private static double[] ERF_B =
		{
			2.36012909523441209E01, 2.44024637934444173E02,
			1.28261652607737228E03, 2.84423683343917062E03
		};

    // Coefficients for approximation to  erfc  in second interval

    private static double[] ERF_C =
		{
			5.64188496988670089e-01, 8.88314979438837594e+00,
			6.61191906371416295e+01, 2.98635138197400131e+02,
			8.81952221241769090e+02, 1.71204761263407058e+03,
			2.05107837782607147e+03, 1.23033935479799725e+03,
			2.15311535474403846e-08
		};

    private static double[] ERF_D =
		{
			1.57449261107098347e+01, 1.17693950891312499e+02,
			5.37181101862009858e+02, 1.62138957456669019e+03,
			3.29079923573345963e+03, 4.36261909014324716e+03,
			3.43936767414372164e+03, 1.23033935480374942e+03
		};

    // Coefficients for approximation to  erfc  in third interval

    private static double[] ERF_P =
		{
			3.05326634961232344e-01, 3.60344899949804439e-01,
			1.25781726111229246e-01, 1.60837851487422766e-02,
			6.58749161529837803e-04, 1.63153871373020978e-02
		};

    private static double[] ERF_Q =
		{
			2.56852019228982242e+00, 1.87295284992346047e+00,
			5.27905102951428412e-01, 6.05183413124413191e-02,
			2.33520497626869185e-03
		};

    private static double ERF_THRESHOLD = 0.46875;

    // Hardware dependant constants calculated for typical IEEE machine.
    // Sun, Intel, etc

    // the smallest positive floating-point number.
    private static double ERF_X_MIN = 2.2250738585072e-308;

    // the largest positive floating-point number.
    private static double ERF_X_INF = 1.7976931348623e308;

    // the largest negative argument to erfcx;
    // the negative of the solution to the equation  2*exp(x*x) = XINF.
    private static double ERF_X_NEG = -1.0 * Math.Sqrt(Math.Log(ERF_X_INF / 2.0));

    // argument below which erf(x) may be represented by  2*x/sqrt(pi)  and above
    // which  x*x  will not underflow.  A conservative value is the largest 
    // machine number X such that  1.0 + X = 1.0  to machine precision.
    private static double ERF_X_SMALL = 1.110223024625156663e-16;

    // largest argument acceptable to ERFC;  solution to the equation:
    // W(x) * (1-0.5/x**2) = XMIN,  where  W(x) = exp(-x*x)/[x*sqrt(pi)].
    private static double ERF_X_BIG = 26.543e+00;

    // argument above which  1.0 - 1/(2*x*x) = 1.0  to machine precision.  A 
    // conservative value is  1/[2*sqrt(XSMALL)]
    private static double ERF_X_HUGE = 1.0 / (2.0 * Math.Sqrt(ERF_X_SMALL));

    // largest acceptable argument to ERFCX;
    // the minimum of XINF and 1/[sqrt(pi)*XMIN].
    private static double ERF_X_MAX = Math.Min(ERF_X_INF, (1 / (Math.Sqrt(M_PI) * ERF_X_MIN)));


    /**
     * The AINT(A) Fortran 77 function.
     * Returns A with the fractional portion of its magnitude truncated and its
     * sign preserved. (Also called "truncation towards zero".)
     */
    private static double AINT(double A)
    {
      long n = Convert.ToInt32(A);
      return Convert.ToDouble(n);
    }



    // the error function

    private static double infcalerf(double arg, int jint)
    {
      double result = 0.0;
      double x = arg;
      double y = Math.Abs(x);
      double x_num, x_den, y_sq;

      if (y <= ERF_THRESHOLD)
      {
        // Evaluate erf for  |x| <= 0.46875
        y_sq = 0.0;

        if (y > ERF_X_SMALL)
          y_sq = y * y;

        x_num = ERF_A[4] * y_sq;
        x_den = y_sq;

        for (int i = 0; i < 3; ++i)
        {
          x_num = (x_num + ERF_A[i]) * y_sq;
          x_den = (x_den + ERF_B[i]) * y_sq;
        }

        result = x * (x_num + ERF_A[3]) / (x_den + ERF_B[3]);
        if (jint != 0)
          result = 1 - result;

        if (jint == 2)
          result = Math.Exp(y_sq) * result;

        return result;
      }
        if (y <= 4.0)
        {
            // Evaluate erfc for 0.46875 <= |x| <= 4.0
            x_num = ERF_C[8] * y;
            x_den = y;

            for (int i = 0; i < 7; ++i)
            {
                x_num = (x_num + ERF_C[i]) * y;
                x_den = (x_den + ERF_D[i]) * y;
            }
            result = (x_num + ERF_C[7]) / (x_den + ERF_D[7]);

            if (jint != 2)
            {
                y_sq = AINT(y * 16.0) / 16.0;
                double del = (y - y_sq) * (y + y_sq);
                result = Math.Exp(-y_sq * y_sq) * Math.Exp(-del) * result;
            }
        }
        else
        {
            // Evaluate erfc for |x| > 4.0
            result = 0.0;
            if (y >= ERF_X_BIG)
            {
                if (jint != 2 || y >= ERF_X_MAX)
                {
                }
                else if (y >= ERF_X_HUGE)
                {
                    result = SQRT_PI / y;
                }
            }
            else
            {
                y_sq = 1.0 / (y * y);
                x_num = ERF_P[5] * y_sq;
                x_den = y_sq;

                for (int i = 0; i < 4; ++i)
                {
                    x_num = (x_num + ERF_P[i]) * y_sq;
                    x_den = (x_den + ERF_Q[i]) * y_sq;
                }

                result = y_sq * (x_num + ERF_P[4]) / (x_den + ERF_Q[4]);
                result = (SQRT_PI - result) / y;

                if (jint != 2)
                {
                    y_sq = AINT(y * 16.0) / 16.0;
                    double del = (y - y_sq) * (y + y_sq);
                    result = Math.Exp(-y_sq * y_sq) * Math.Exp(-del) * result;
                }
            }
        }

        // Fix up for negative argument, erf, etc.
      if (jint == 0)
      {
        result = (0.5 - result) + 0.5;
        if (x < 0)
          result = -result;
      }
      else if (jint == 1)
      {
        if (x < 0)
          result = 2.0 - result;
      }
      else
      {
        if (x < 0)
        {
          if (x < ERF_X_NEG)
            result = ERF_X_INF;
          else
          {
            y_sq = AINT(x * 16.0) / 16.0;
            double del = (x - y_sq) * (x + y_sq);
            y = Math.Exp(y_sq * y_sq) * Math.Exp(del);
            result = (y + y) - result;
          }
        }
      }
      return result;
    }

    // ------------------------------------------------------------------------- //

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static double CND(double p)
    {
      return 0.5 * infcalerf(-p / SQRT_2, 1);
    }


    //
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="rho"></param>
    /// <returns></returns>
    public double CBND(double a, double b, double rho)
    {
      double[] x = { 0.24840615, 0.39233107, 0.21141819, 0.03324666, 0.00082485334 };
      double[] y = { 0.10024215, 0.48281397, 1.0609498, 1.7797294, 2.6697604 };
      const double pi = 3.141592638;
      double res = 0.0;

      if (Math.Abs(rho) >= 1.0) { return res; }

      double a1 = a / Math.Sqrt(2 * (1 - rho * rho));
      double b1 = b / Math.Sqrt(2 * (1 - rho * rho));

      if ((a <= 0.0) && (b <= 0.0) && (rho <= 0.0))
      {
        double sum = 0.0;
        for (int idx = 0; idx != 5; idx++)
        {
          for (int jdx = 0; jdx != 5; jdx++)
          {
            sum += x[idx] * x[jdx] * Math.Exp(a1 * (2.0 * y[idx] - a1)
              + b1 * (2.0 * y[jdx] - b1) + 2.0 * rho * (y[idx] - a1) * (y[jdx] - b1));
          }
        }
        res = sum * Math.Sqrt(1.0 - rho * rho) / pi;
      }
      else if ((a <= 0.0) && (b >= 0.0))
      {
        res = CND(a) - CBND(a, -b, -rho);
      }
      else if ((a >= 0.0) && (b <= 0.0))
      {
        res = CND(b) - CBND(-a, b, -rho);
      }
      else if ((a >= 0.0) && (b >= 0.0))
      {
        res = CND(a) + CND(b) - 1.0 + CBND(-a, -b, rho);
      }
      else if ((a <= 0.0) && (b <= 0.0) && (rho > 0.0))
      {
        double rho1 = (b - rho * a) / Math.Sqrt(a * a - 2 * rho * a * b + b * b);
        double rho2 = (a - rho * b) / Math.Sqrt(a * a - 2 * rho * a * b + b * b);

        res = CBND(a, 0.0, rho1) + CBND(b, 0.0, rho2);
      }

      return res;
    }

  }
}
