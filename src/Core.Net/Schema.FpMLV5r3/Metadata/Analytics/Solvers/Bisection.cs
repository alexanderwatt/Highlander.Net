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
    /// Bisection 1-D solver.
    /// </summary>
    /// <remarks>
    ///	<para>
    ///	The implementation of the algorithms was inspired by 
    ///	<see href="http://www.library.cornell.edu/nr/bookcpdf.html">Numerical Recipes in C</see>, 
    ///	2nd edition, Cambridge University Press, Teukolsky, Vetterling, Flannery - 
    ///	<see href="http://www.library.cornell.edu/nr/bookcpdf/c9-1.pdf">
    ///	Chapter 9.1 Bracketing and Bisection
    ///	</see>.
    ///	</para>
    /// </remarks>
    ///	<seealso href="http://www.library.cornell.edu/nr/bookcpdf.html">
    ///	Numerical Recipes in C (free online book)
    ///	</seealso>
    //[ RcsId("$Id: Bisection.cs,v 1.0 2006/10/02 15:57:52 Highlander Exp $") ] 
    //[ ComVisible(true) ]
    //[ ClassInterface(ClassInterfaceType.None) ]
    public sealed class Bisection : Solver1D
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
        protected override double SolveImpl(IObjectiveFunction objectiveFunction,
            double xAccuracy)
        {
            // Orient the search so that f>0 lies at root_+dx
            double dx;
            if (FXMin < 0.0)
            {
                dx = XMax - XMin;
                Root = XMin;
            }
            else
            {
                dx = XMin - XMax;
                Root = XMax;
            }
            while (EvaluationNumber++ < MaxEvaluations)
            {
                dx /= 2.0;
                double xMid = Root + dx;
                double fMid = objectiveFunction.Value(xMid);
                if (fMid <= 0.0)
                    Root = xMid;
                if (Math.Abs(dx) < xAccuracy || fMid == 0.0)
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
        /// <param name="derivativeFunction">The derivative function, which is not required for this solver.</param>
        /// <param name="xAccuracy">Given accuracy.</param>
        ///  <returns>The zero of the objective function.</returns>
        ///  <exception cref="ApplicationException">
        ///  Thrown when a bracket is not found after the maximum
        ///  number of evaluations (see <see cref="Solver1D.MaxEvaluations"/>).
        ///  </exception>
        protected override double SolveImpl(Func<double, double> function, Func<double, double> derivativeFunction, double xAccuracy)
        {
            // Orient the search so that f>0 lies at root_+dx
            double dx;
            if (FXMin < 0.0)
            {
                dx = XMax - XMin;
                Root = XMin;
            }
            else
            {
                dx = XMin - XMax;
                Root = XMax;
            }
            while (EvaluationNumber++ < MaxEvaluations)
            {
                dx /= 2.0;
                double xMid = Root + dx;
                double fMid = function(xMid);
                if (fMid <= 0.0)
                    Root = xMid;
                if (Math.Abs(dx) < xAccuracy || fMid == 0.0)
                    return Root;
            }
            throw new ApplicationException("SlvMaxEval");
        }
    }
}