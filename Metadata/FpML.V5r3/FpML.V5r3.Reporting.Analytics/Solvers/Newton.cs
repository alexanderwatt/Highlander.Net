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

namespace Orion.Analytics.Solvers
{
    /// <summary>
    /// Newton 1-D solver.
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
    public sealed class Newton : Solver1D
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
            while (EvaluationNumber++ < MaxEvaluations)
            {
                double froot = objectiveFunction.Value(Root);
                double dfroot = objectiveFunction.Derivative(Root);
                double dx = froot / dfroot;
                Root -= dx;
                // jumped out of brackets, switch to NewtonSafe
                if ((XMin - Root) * (Root - XMax) < 0.0)
                {
                    ISolver1D newtonSafe = new NewtonSafe();
                    newtonSafe.MaxEvaluations -= EvaluationNumber;
                    return newtonSafe.Solve(objectiveFunction, xAccuracy,
                        Root + dx, XMin, XMax);
                }
                if (Math.Abs(dx) < xAccuracy)
                    return Root;
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
        /// <param name="derivativeFunction">The derivative function.</param>
        /// <param name="xAccuracy">Given accuracy.</param>
        ///  <returns>The zero of the objective function.</returns>
        ///  <exception cref="ApplicationException">
        ///  Thrown when a bracket is not found after the maximum
        ///  number of evaluations (see <see cref="Solver1D.MaxEvaluations"/>).
        ///  </exception>
        protected override double SolveImpl(Func<double, double> function, Func<double, double> derivativeFunction, double xAccuracy)
        {
            while (EvaluationNumber++ < MaxEvaluations)
            {
                double froot = function(Root);
                double dfroot = derivativeFunction(Root);
                double dx = froot / dfroot;
                Root -= dx;
                // jumped out of brackets, switch to NewtonSafe
                if ((XMin - Root) * (Root - XMax) < 0.0)
                {
                    ISolver1D newtonSafe = new NewtonSafe();
                    newtonSafe.MaxEvaluations -= EvaluationNumber;
                    return newtonSafe.Solve(function, derivativeFunction, xAccuracy,
                        Root + dx, XMin, XMax);
                }
                if (Math.Abs(dx) < xAccuracy)
                    return Root;
            }
            throw new ApplicationException("SlvMaxEval");
        }
    }
}