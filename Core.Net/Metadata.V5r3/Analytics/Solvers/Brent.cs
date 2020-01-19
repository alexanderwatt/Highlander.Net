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

#region Using directives

using System;
using Highlander.Reporting.Analytics.V5r3.Utilities;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Solvers
{
    /// <summary>
    /// Brent 1-D solver.
    /// </summary>
    /// <remarks>
    ///	<para>
    ///	The implementation of the algorithms was inspired by 
    ///	<see href="http://www.library.cornell.edu/nr/bookcpdf.html">Numerical Recipes in C</see>, 
    ///	2nd edition, Cambridge University Press, Teukolsky, Vetterling, Flannery - 
    ///	<see href="http://www.library.cornell.edu/nr/bookcpdf/c9-2.pdf">
    ///	Chapter 9.3 Van Wijngaarden--Dekker--Brent Method
    ///	</see>.
    ///	</para>
    /// </remarks>
    ///	<seealso href="http://www.library.cornell.edu/nr/bookcpdf.html">
    ///	Numerical Recipes in C (free online book)
    ///	</seealso>
    public sealed class Brent : Solver1D
    {
        /// <summary>
        /// Implementation of the actual code to search for the 0.0es of 
        /// the <see cref="IObjectiveFunction"/>.
        /// </summary>
        /// <remarks>
        /// It assumes that:
        /// <list type="bullet">
        /// <item>
        ///   <description>
        ///		<see cref="Solver1D.XMin"/> and <see cref="Solver1D.XMax"/> form a valid bracket; 
        ///	  </description>
        /// </item>
        /// <item>
        ///   <description>
        ///     <see cref="Solver1D.FXMin"/> and <see cref="Solver1D.FXMax"/> contain the values of the
        ///		function in <see cref="Solver1D.XMin"/> and <see cref="Solver1D.XMax"/>; 
        ///	  </description>
        /// </item>
        /// <item>
        ///   <description>
        ///     <see cref="Solver1D.Root"/> was initialized to a valid initial guess.
        ///	  </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="objectiveFunction">The <see cref="IObjectiveFunction"/>.</param>
        /// <param name="xAccuracy">Given accuracy.</param>
        /// <returns>The 0.0 of the objective function.</returns>
        /// <exception cref="ApplicationException">
        /// Thrown when a bracket is not found after the maximum
        /// number of evaluations (see <see cref="Solver1D.MaxEvaluations"/>).
        /// </exception>
        protected override double SolveImpl(IObjectiveFunction objectiveFunction, double xAccuracy)
        {
            // dummy assignements to avoid compiler warning
            //
            double d = 0.0;
            double e = 0.0;
            Root = XMax;
            double froot = FXMax;
            while (EvaluationNumber++ < MaxEvaluations)
            {
                if (froot > 0.0 && FXMax > 0.0 ||
                    froot < 0.0 && FXMax < 0.0)
                {
                    // Rename _xMin, root, _xMax and adjust bounding interval d
                    //
                    XMax = XMin;
                    FXMax = FXMin;
                    e = d = Root - XMin;
                }
                if (Math.Abs(FXMax) < Math.Abs(froot))
                {
                    XMin = Root;
                    Root = XMax;
                    XMax = XMin;
                    FXMin = froot;
                    froot = FXMax;
                    FXMax = FXMin;
                }
                // Convergence check
                //
                double xAcc1 = 2.0 * Double.Epsilon * Math.Abs(Root) + 0.5 * xAccuracy;
                double xMid = (XMax - Root) / 2.0;
                // Yield return value
                // (exit path)
                //
                if (Math.Abs(xMid) <= xAcc1 || froot == 0.0)
                {
                    return Root;
                }
                if (Math.Abs(e) >= xAcc1 && Math.Abs(FXMin) > Math.Abs(froot))
                {
                    double p, q;
                    double s = froot / FXMin;  // Attempt inverse quadratic interpolation
                    if (XMin == XMax)
                    {
                        p = 2.0 * xMid * s;
                        q = 1.0 - s;
                    }
                    else
                    {
                        q = FXMin / FXMax;
                        double r = froot / FXMax;
                        p = s * (2.0 * xMid * q * (q - r) - (Root - XMin) * (r - 1.0));
                        q = (q - 1.0) * (r - 1.0) * (s - 1.0);
                    }
                    if (p > 0.0) q = -q;  // Check whether in bounds
                    p = Math.Abs(p);
                    double min1 = 3.0 * xMid * q - Math.Abs(xAcc1 * q);
                    double min2 = Math.Abs(e * q);
                    if (2.0 * p < (min1 < min2 ? min1 : min2))
                    {
                        e = d;                // Accept interpolation
                        d = p / q;
                    }
                    else
                    {
                        d = xMid;  // Interpolation failed, use bisection
                        e = d;
                    }
                }
                else
                {
                    //  Bounds decreasing too slowly, use bisection
                    //
                    d = xMid;
                    e = d;
                }
                XMin = Root;
                FXMin = froot;
                if (Math.Abs(d) > xAcc1)
                {
                    Root += d;
                }
                else
                {
                    Root += Math.Sign(xMid) * Math.Abs(xAcc1);
                }
                froot = objectiveFunction.Value(Root);
            }
            throw new ApplicationException("SlvMaxEval");
        }

        ///  <summary>
        ///  This method must be implemented in derived classes and 
        ///  contains the actual code which searches for the zeroes of 
        ///  the <see cref="IObjectiveFunction"/>.
        ///  </summary>
        ///  <remarks>
        ///  It assumes that:
        ///  <list type="bullet">
        ///  <item>
        ///    <description>
        /// 		<see cref="Solver1D.XMin"/> and <see cref="Solver1D.XMax"/> form a valid bracket; 
        /// 	  </description>
        ///  </item>
        ///  <item>
        ///    <description>
        ///      <see cref="Solver1D.FXMin"/> and <see cref="Solver1D.FXMax"/> contain the values of the
        /// 		function in <see cref="Solver1D.XMin"/> and <see cref="Solver1D.XMax"/>; 
        /// 	  </description>
        ///  </item>
        ///  <item>
        ///    <description>
        ///      <see cref="Solver1D.Root"/> was initialized to a valid initial guess.
        /// 	  </description>
        ///  </item>
        ///  </list>
        ///  </remarks>
        ///  <param name="function">The <see cref="IObjectiveFunction"/>.</param>
        /// <param name="derivativeFunction">The derivative function, which is not required for this solver.</param>
        /// <param name="xAccuracy">Given accuracy.</param>
        ///  <returns>The zero of the objective function.</returns>
        ///  <exception cref="ApplicationException">
        ///  Thrown when a bracket is not found after the maximum
        ///  number of evaluations (see <see cref="Solver1D.MaxEvaluations"/>).
        ///  </exception>
        protected override double SolveImpl(Func<double, double> function, Func<double, double> derivativeFunction, double xAccuracy)
        {
            // dummy assignements to avoid compiler warning
            //
            double d = 0.0;
            double e = 0.0;
            Root = XMax;
            double froot = FXMax;
            while (EvaluationNumber++ < MaxEvaluations)
            {
                if (froot > 0.0 && FXMax > 0.0 ||
                    froot < 0.0 && FXMax < 0.0)
                {
                    // Rename _xMin, root, _xMax and adjust bounding interval d
                    //
                    XMax = XMin;
                    FXMax = FXMin;
                    e = d = Root - XMin;
                }
                if (Math.Abs(FXMax) < Math.Abs(froot))
                {
                    XMin = Root;
                    Root = XMax;
                    XMax = XMin;
                    FXMin = froot;
                    froot = FXMax;
                    FXMax = FXMin;
                }
                // Convergence check
                //
                double xAcc1 = 2.0 * Double.Epsilon * Math.Abs(Root) + 0.5 * xAccuracy;
                double xMid = (XMax - Root) / 2.0;
                // Yield return value
                // (exit path)
                //
                if (Math.Abs(xMid) <= xAcc1 || froot == 0.0)
                {
                    return Root;
                }
                if (Math.Abs(e) >= xAcc1 && Math.Abs(FXMin) > Math.Abs(froot))
                {
                    double p, q;
                    double s = froot / FXMin;  // Attempt inverse quadratic interpolation
                    if (XMin == XMax)
                    {
                        p = 2.0 * xMid * s;
                        q = 1.0 - s;
                    }
                    else
                    {
                        q = FXMin / FXMax;
                        double r = froot / FXMax;
                        p = s * (2.0 * xMid * q * (q - r) - (Root - XMin) * (r - 1.0));
                        q = (q - 1.0) * (r - 1.0) * (s - 1.0);
                    }
                    if (p > 0.0) q = -q;  // Check whether in bounds
                    p = Math.Abs(p);
                    double min1 = 3.0 * xMid * q - Math.Abs(xAcc1 * q);
                    double min2 = Math.Abs(e * q);
                    if (2.0 * p < (min1 < min2 ? min1 : min2))
                    {
                        e = d;                // Accept interpolation
                        d = p / q;
                    }
                    else
                    {
                        d = xMid;  // Interpolation failed, use bisection
                        e = d;
                    }
                }
                else
                {
                    //  Bounds decreasing too slowly, use bisection
                    //
                    d = xMid;
                    e = d;
                }
                XMin = Root;
                FXMin = froot;
                if (Math.Abs(d) > xAcc1)
                {
                    Root += d;
                }
                else
                {
                    Root += (Math.Sign(xMid) * Math.Abs(xAcc1)); // SIGN(xAcc1,xMid)
                }
                froot = function(Root);
                //Trace.WriteLine("SolveImpl : x:" + _root + " value: " + froot);
            }
            throw new ApplicationException("SlvMaxEval");
        }

        /// <summary>
        /// This method returns the zero of the <see cref="IObjectiveFunction"/>
        /// <pramref name="objectiveFunction"/>, determined with the given accuracy.
        /// </summary>
        /// <remarks>
        /// <i>x</i> is considered a zero if <i>|f(x)| &lt; accuracy</i>.
        /// An initial guess must be supplied, as well as two values which 
        /// must bracket the zero (i.e., either 
        /// <i>f(x<sub>min</sub>) &gt; 0</i> &amp;&amp; 
        /// <i>f(x<sub>max</sub>) &lt; 0</i>, or 
        /// <i>f(x<sub>min</sub>) &lt; 0</i> &amp;&amp; 
        /// <i>f(x<sub>max</sub>) &gt; 0</i> must be true). 
        /// </remarks>
        /// <param name="function">The <see cref="IObjectiveFunction"/>.</param>
        /// <param name="xAccuracy">Given accuracy.</param>
        /// <param name="guess">Initial guess used to scan the range
        /// of the possible bracketing values.</param>
        /// <param name="xMin">Lower bracket.</param>
        /// <param name="xMax">Upper bracket.</param>
        /// <returns>The zero of the objective function.</returns>
        /// <exception cref="ApplicationException">
        /// Thrown when a bracket is not found after the maximum
        /// number of evaluations (see <see cref="Solver1D.MaxEvaluations"/>).
        /// </exception>
        public double Solve(Func<double, double> function, double xAccuracy,
            double guess, double xMin, double xMax)
        {
            #region Check that input data is valid

            if (xMin >= xMax)
            {
                throw new ArgumentException($"Solver minimum bracket ({xMin}) must be below maximum bracket ({xMax}).");
            }
            if (LowerBoundEnforced && xMin < LowerBound)
            {
                throw new ArgumentException(
                    $"Solver minimum bracket ({xMin}) is less than lower bound ({LowerBound}).");
            }
            if (UpperBoundEnforced && xMax > UpperBound)
            {
                throw new ArgumentException(
                    $"Solver maximum bracket ({xMax}) is more than upper bound ({UpperBound}).");
            }
            // Check if the guess is within specified range (xMin, xMax)
            //
            if (guess < xMin)
            {
                throw new ApplicationException($"Solver guess ({guess}) is less than minimum bracket ({xMin}).");
            }
            if (guess > xMax)
            {
                throw new ApplicationException($"Solver guess ({guess}) is more than maximum bracket ({xMax}).");
            }

            #endregion

            XMin = xMin;
            XMax = xMax;
            FXMin = function(xMin);
            if (Math.Abs(FXMin) < xAccuracy) return xMin;
            FXMax = function(xMax);
            if (Math.Abs(FXMax) < xAccuracy) return xMax;
            EvaluationNumber = 2;
            if (FXMin * FXMax >= 0.0)
                throw new ApplicationException(
                    $"Solver bounds must return different values with different signs; it returned {FXMin} and {FXMax}.");
            Root = guess;
            // dummy assignements to avoid compiler warning
            //
            double d = 0.0;
            double e = 0.0;
            Root = XMax;
            double froot = FXMax;
            while (EvaluationNumber++ < MaxEvaluations)
            {
                if (froot > 0.0 && FXMax > 0.0 ||
                    froot < 0.0 && FXMax < 0.0)
                {
                    // Rename _xMin, root, _xMax and adjust bounding interval d
                    //
                    XMax = XMin;
                    FXMax = FXMin;
                    e = d = Root - XMin;
                }
                if (Math.Abs(FXMax) < Math.Abs(froot))
                {
                    XMin = Root;
                    Root = XMax;
                    XMax = XMin;
                    FXMin = froot;
                    froot = FXMax;
                    FXMax = FXMin;
                }
                // Convergence check
                //
                double xAcc1 = 2.0 * Double.Epsilon * Math.Abs(Root) + 0.5 * xAccuracy;
                double xMid = (XMax - Root) / 2.0;
                // Yield return value
                // (exit path)
                //
                if (Math.Abs(xMid) <= xAcc1 || froot == 0.0)
                {
                    return Root;
                }
                if (Math.Abs(e) >= xAcc1 && Math.Abs(FXMin) > Math.Abs(froot))
                {
                    double p, q;
                    double s = froot / FXMin;  // Attempt inverse quadratic interpolation
                    if (XMin == XMax)
                    {
                        p = 2.0 * xMid * s;
                        q = 1.0 - s;
                    }
                    else
                    {
                        q = FXMin / FXMax;
                        double r = froot / FXMax;
                        p = s * (2.0 * xMid * q * (q - r) - (Root - XMin) * (r - 1.0));
                        q = (q - 1.0) * (r - 1.0) * (s - 1.0);
                    }
                    if (p > 0.0) q = -q;  // Check whether in bounds
                    p = Math.Abs(p);
                    double min1 = 3.0 * xMid * q - Math.Abs(xAcc1 * q);
                    double min2 = Math.Abs(e * q);
                    if (2.0 * p < (min1 < min2 ? min1 : min2))
                    {
                        e = d;                // Accept interpolation
                        d = p / q;
                    }
                    else
                    {
                        d = xMid;  // Interpolation failed, use bisection
                        e = d;
                    }
                }
                else
                {
                    //  Bounds decreasing too slowly, use bisection
                    //
                    d = xMid;
                    e = d;
                }
                XMin = Root;
                FXMin = froot;
                if (Math.Abs(d) > xAcc1)
                {
                    Root += d;
                }
                else
                {
                    Root += Math.Sign(xMid) * Math.Abs(xAcc1); // SIGN(xAcc1,xMid)
                }
                froot = function(Root);
                //Trace.WriteLine("SolveImpl : x:" + _root + " value: " + froot);
            }
            throw new ApplicationException("SlvMaxEval");
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="guessLowerBound">Guess for the low value of the range where the root is supposed to be. Will be expanded if needed.</param>
        /// <param name="guessUpperBound">Guess for the  high value of the range where the root is supposed to be. Will be expanded if needed.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Default 1e-8.</param>
        /// <param name="maxIterations">Maximum number of iterations. Default 100.</param>
        /// <param name="expandFactor">Factor at which to expand the bounds, if needed. Default 1.6.</param>
        /// <param name="maxExpandIteratons">Maximum number of expand iterations. Default 100.</param>
        /// <returns>Returns the root with the specified accuracy.</returns>
        /// <exception cref="Exception"></exception>
        public static double FindRootExpand(Func<double, double> f, double guessLowerBound, double guessUpperBound, double accuracy = 1e-8, int maxIterations = 100, double expandFactor = 1.6, int maxExpandIteratons = 100)
        {
            ZeroBracketing.ExpandReduce(f, ref guessLowerBound, ref guessUpperBound, expandFactor, maxExpandIteratons, maxExpandIteratons * 10);
            return FindRoot(f, guessLowerBound, guessUpperBound, accuracy, maxIterations);
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Default 1e-8.</param>
        /// <param name="maxIterations">Maximum number of iterations. Default 100.</param>
        /// <returns>Returns the root with the specified accuracy.</returns>
        /// <exception cref="Exception"></exception>
        public static double FindRoot(Func<double, double> f, double lowerBound, double upperBound, double accuracy = 1e-8, int maxIterations = 100)
        {
            double root;
            if (TryFindRoot(f, lowerBound, upperBound, accuracy, maxIterations, out root))
            {
                return root;
            }
            throw new Exception("RootFindingFailed");
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="lowerBound">The low value of the range where the root is supposed to be.</param>
        /// <param name="upperBound">The high value of the range where the root is supposed to be.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached.</param>
        /// <param name="maxIterations">Maximum number of iterations. Usually 100.</param>
        /// <param name="root">The root that was found, if any. Undefined if the function returns false.</param>
        /// <returns>True if a root with the specified accuracy was found, else false.</returns>
        public static bool TryFindRoot(Func<double, double> f, double lowerBound, double upperBound, double accuracy, int maxIterations, out double root)
        {
            double fmin = f(lowerBound);
            double fmax = f(upperBound);
            double froot = fmax;
            double d = 0.0, e = 0.0;
            root = upperBound;
            double xMid = double.NaN;
            // Root must be bracketed.
            if (Math.Sign(fmin) == Math.Sign(fmax))
            {
                return false;
            }
            for (int i = 0; i <= maxIterations; i++)
            {
                // adjust bounds
                if (Math.Sign(froot) == Math.Sign(fmax))
                {
                    upperBound = lowerBound;
                    fmax = fmin;
                    e = d = root - lowerBound;
                }
                if (Math.Abs(fmax) < Math.Abs(froot))
                {
                    lowerBound = root;
                    root = upperBound;
                    upperBound = lowerBound;
                    fmin = froot;
                    froot = fmax;
                    fmax = fmin;
                }
                // convergence check
                double xAcc1 = Precision.PositiveDoublePrecision * Math.Abs(root) + 0.5 * accuracy;
                double xMidOld = xMid;
                xMid = (upperBound - root) / 2.0;
                if (Math.Abs(xMid) <= xAcc1 || froot.AlmostEqualNormRelative(0, froot, accuracy))
                {
                    return true;
                }
                if (xMid == xMidOld)
                {
                    // accuracy not sufficient, but cannot be improved further
                    return false;
                }
                if (Math.Abs(e) >= xAcc1 && Math.Abs(fmin) > Math.Abs(froot))
                {
                    // Attempt inverse quadratic interpolation
                    double s = froot / fmin;
                    double p;
                    double q;
                    if (lowerBound.AlmostEqualRelative(upperBound))
                    {
                        p = 2.0 * xMid * s;
                        q = 1.0 - s;
                    }
                    else
                    {
                        q = fmin / fmax;
                        double r = froot / fmax;
                        p = s * (2.0 * xMid * q * (q - r) - (root - lowerBound) * (r - 1.0));
                        q = (q - 1.0) * (r - 1.0) * (s - 1.0);
                    }
                    if (p > 0.0)
                    {
                        // Check whether in bounds
                        q = -q;
                    }
                    p = Math.Abs(p);
                    if (2.0 * p < Math.Min(3.0 * xMid * q - Math.Abs(xAcc1 * q), Math.Abs(e * q)))
                    {
                        // Accept interpolation
                        e = d;
                        d = p / q;
                    }
                    else
                    {
                        // Interpolation failed, use bisection
                        d = xMid;
                        e = d;
                    }
                }
                else
                {
                    // Bounds decreasing too slowly, use bisection
                    d = xMid;
                    e = d;
                }
                lowerBound = root;
                fmin = froot;
                if (Math.Abs(d) > xAcc1)
                {
                    root += d;
                }
                else
                {
                    root += Sign(xAcc1, xMid);
                }
                froot = f(root);
            }
            return false;
        }

        /// <summary>Helper method useful for preventing rounding errors.</summary>
        /// <returns>a*sign(b)</returns>
        private static double Sign(double a, double b)
        {
            return b >= 0 ? (a >= 0 ? a : -a) : (a >= 0 ? -a : a);
        }
    }

    public static class ZeroBracketing
    {     
        /// <summary>Detect a range containing at least one root.</summary>
        /// <param name="f">The function to detect roots from.</param>
        /// <param name="lowerBound">Lower value of the range.</param>
        /// <param name="upperBound">Upper value of the range</param>
        /// <param name="factor">The growing factor of research. Usually 1.6.</param>
        /// <param name="maxIterations">Maximum number of iterations. Usually 50.</param>
        /// <returns>True if the bracketing operation succeeded, false otherwise.</returns>
        /// <remarks>This iterative methods stops when two values with opposite signs are found.</remarks>
        public static bool Expand(Func<double, double> f, ref double lowerBound, ref double upperBound, double factor = 1.6, int maxIterations = 50)
        {
            double originalLowerBound = lowerBound;
            double originalUpperBound = upperBound;
            if (lowerBound >= upperBound)
            {
                throw new ArgumentOutOfRangeException(nameof(upperBound), string.Format("ArgumentOutOfRangeGreater"));
            }
            double fmin = f(lowerBound);
            double fmax = f(upperBound);
            for (int i = 0; i < maxIterations; i++)
            {
                if (Math.Sign(fmin) != Math.Sign(fmax))
                {
                    return true;
                }
                if (Math.Abs(fmin) < Math.Abs(fmax))
                {
                    lowerBound += factor * (lowerBound - upperBound);
                    fmin = f(lowerBound);
                }
                else
                {
                    upperBound += factor * (upperBound - lowerBound);
                    fmax = f(upperBound);
                }
            }
            lowerBound = originalLowerBound;
            upperBound = originalUpperBound;
            return false;
        }

        public static bool Reduce(Func<double, double> f, ref double lowerBound, ref double upperBound, int subdivisions = 1000)
        {
            double originalLowerBound = lowerBound;
            double originalUpperBound = upperBound;
            if (lowerBound >= upperBound)
            {
                throw new ArgumentOutOfRangeException(nameof(upperBound), string.Format("ArgumentOutOfRangeGreater"));
            }
            double fmin = f(lowerBound);
            double fmax = f(upperBound);
            if (Math.Sign(fmin) != Math.Sign(fmax))
            {
                return true;
            }
            double subdiv = (upperBound - lowerBound) / subdivisions;
            double smin = lowerBound;
            int sign = Math.Sign(fmin);
            for (int k = 0; k < subdivisions; k++)
            {
                double smax = smin + subdiv;
                double sfmax = f(smax);
                if (double.IsInfinity(sfmax))
                {
                    // expand interval to include pole
                    smin = smax;
                    continue;
                }
                if (Math.Sign(sfmax) != sign)
                {
                    lowerBound = smin;
                    upperBound = smax;
                    return true;
                }
                smin = smax;
            }
            lowerBound = originalLowerBound;
            upperBound = originalUpperBound;
            return false;
        }

        public static bool ExpandReduce(Func<double, double> f, ref double lowerBound, ref double upperBound, double expansionFactor = 1.6, int expansionMaxIterations = 50, int reduceSubdivisions = 100)
        {
            return Expand(f, ref lowerBound, ref upperBound, expansionFactor, expansionMaxIterations) || Reduce(f, ref lowerBound, ref upperBound, reduceSubdivisions);
        }
    }
}