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

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Solvers
{
    /// <summary>
    /// Safe (braketed) Newton 1-D solver.
    /// </summary>
    /// <remarks>
    ///	<para>
    ///	The implementation of the algorithms was inspired by 
    ///	<see href="http://www.library.cornell.edu/nr/bookcpdf.html">Numerical Recipes in C</see>, 
    ///	2nd edition, Cambridge University Press, Teukolsky, Vetterling, Flannery - 
    ///	<see href="http://www.library.cornell.edu/nr/bookcpdf/c9-2.pdf">
    ///	Chapter 9.4 Newton-Raphson Method Using Derivative
    ///	</see>.
    ///	</para>
    /// </remarks>
    ///	<seealso href="http://www.library.cornell.edu/nr/bookcpdf.html">
    ///	Numerical Recipes in C (free online book)
    ///	</seealso>
    public sealed class NewtonSafe : Solver1D
    {
        /// <summary>
        /// Implementation of the actual code to search for the zeroes of 
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
        /// <returns>The zero of the objective function.</returns>
        /// <exception cref="ApplicationException">
        /// Thrown when a bracket is not found after the maximum
        /// number of evaluations (see <see cref="Solver1D.MaxEvaluations"/>).
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// Thrown by the <see cref="ObjectiveFunction"/> base class
        /// when the function's derivative has not been implemented.
        /// </exception>
        protected override double SolveImpl(IObjectiveFunction objectiveFunction,
            double xAccuracy)
        {
            // Orient the search so that f(xl) < 0
            double xh, xl;
            if (FXMin < 0.0) { xl = XMin; xh = XMax; }
            else { xh = XMin; xl = XMax; }

            // the "stepsize before last"
            double dxold = XMax - XMin;
            // it was dxold=QL_FABS(_xMax_-_xMin_); in Numerical Recipes
            // here (_xMax_-_xMin_ > 0) is verified in the constructor
            // and the last step
            double dx = dxold;
            double froot = objectiveFunction.Value(Root);
            double dfroot = objectiveFunction.Derivative(Root);
            // the objectiveFunction throws  System.NotImplementedException
            while (++EvaluationNumber < MaxEvaluations)
            {
                // Bisect if (out of range || not decreasing fast enough)
                if ((((Root - xh) * dfroot - froot) * ((Root - xl) * dfroot - froot) > 0.0)
                    || (Math.Abs(2.0 * froot) > Math.Abs(dxold * dfroot)))
                {
                    dxold = dx;
                    dx = (xh - xl) / 2.0;
                    Root = xl + dx;
                }
                else
                {
                    dxold = dx;
                    dx = froot / dfroot;
                    Root -= dx;
                }
                // Convergence criterion
                if (Math.Abs(dx) < xAccuracy)
                    return Root;

                froot = objectiveFunction.Value(Root);
                dfroot = objectiveFunction.Derivative(Root);

                if (froot < 0.0)
                    xl = Root;
                else
                    xh = Root;
            }
            throw new ApplicationException(String.Format(
                "SlvMaxEval")
            );
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
        /// <param name="derivativeFunction">The derivative function.</param>
        /// <param name="xAccuracy">Given accuracy.</param>
        ///  <returns>The zero of the objective function.</returns>
        ///  <exception cref="ApplicationException">
        ///  Thrown when a bracket is not found after the maximum
        ///  number of evaluations (see <see cref="Solver1D.MaxEvaluations"/>).
        ///  </exception>
        protected override double SolveImpl(Func<double, double> function, Func<double, double> derivativeFunction, double xAccuracy)
        {
            // Orient the search so that f(xl) < 0
            double xh, xl;
            if (FXMin < 0.0) { xl = XMin; xh = XMax; }
            else { xh = XMin; xl = XMax; }

            // the "stepsize before last"
            double dxold = XMax - XMin;
            // it was dxold=QL_FABS(_xMax_-_xMin_); in Numerical Recipes
            // here (_xMax_-_xMin_ > 0) is verified in the constructor
            // and the last step
            double dx = dxold;
            double froot = function(Root);
            double dfroot = derivativeFunction(Root);
            // the objectiveFunction throws  System.NotImplementedException
            while (++EvaluationNumber < MaxEvaluations)
            {
                // Bisect if (out of range || not decreasing fast enough)
                if ((((Root - xh) * dfroot - froot) * ((Root - xl) * dfroot - froot) > 0.0)
                    || (Math.Abs(2.0 * froot) > Math.Abs(dxold * dfroot)))
                {
                    dxold = dx;
                    dx = (xh - xl) / 2.0;
                    Root = xl + dx;
                }
                else
                {
                    dxold = dx;
                    dx = froot / dfroot;
                    Root -= dx;
                }
                // Convergence criterion
                if (Math.Abs(dx) < xAccuracy)
                    return Root;

                froot = function(Root);
                dfroot = derivativeFunction(Root);

                if (froot < 0.0)
                    xl = Root;
                else
                    xh = Root;
            }
            throw new ApplicationException(String.Format(
                "SlvMaxEval")
            );
        }
    }
}