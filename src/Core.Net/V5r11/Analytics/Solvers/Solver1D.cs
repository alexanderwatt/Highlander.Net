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
    /// Abstract base class for 1-D solvers.
    /// </summary>
    /// <remarks>
    /// <para>
    ///	This abstract class provides the interface for one-dimensional solvers 
    ///	which can find the zeroes of a given function.
    ///	</para>
    ///	<para>
    ///	The implementation of the algorithms was inspired by 
    ///	<see href="http://www.library.cornell.edu/nr/bookcpdf.html">Numerical Recipes in C</see>, 
    ///	2nd edition, Cambridge University Press, Teukolsky, Vetterling, Flannery - Chapter 9.
    ///	</para>
    ///	<para>
    ///	Some work is needed to resolve the ambiguity of the root finding accuracy
    ///	defition: for some algorithms it is the x-accuracy, for others it is
    ///	f(x)-accuracy.
    ///	</para>
    ///	</remarks>
    ///	<seealso href="http://www.library.cornell.edu/nr/bookcpdf.html">
    ///	Numerical Recipes in C (free online book)
    ///	</seealso>
    public abstract class Solver1D : ISolver1D
    {
        private const int MaxFunctionEvaluations = 100;
        private double _lowerBound;
        private double _upperBound;
        protected bool LowerBoundEnforced;
        protected bool UpperBoundEnforced;

        /// <summary>
        /// Number of function evaluations for the bracketing routine.
        /// </summary>
        /// <remarks>
        /// An <see cref="ApplicationException"/> should be thrown by derived
        /// classes when a bracket is not found after <see cref="MaxEvaluations"/>
        /// of evaluations.
        /// </remarks>
        protected int EvaluationNumber;

        /// <summary>
        /// Derived classes implementing <see>
        ///         <cref>SolveImpl</cref>
        ///     </see>
        ///     assume
        /// that this member was initialized to a valid initial guess.
        /// </summary>
        protected double Root;

        /// <summary>
        /// Derived classes implementing <see>
        ///         <cref>SolveImpl</cref>
        ///     </see>
        ///     assume that
        /// <see cref="XMin"/> and <see cref="XMax"/> form a valid bracket.
        /// </summary>
        protected double XMin;

        /// <summary>
        /// Derived classes implementing <see>
        ///         <cref>SolveImpl</cref>
        ///     </see>
        ///     assume that
        /// <see cref="XMin"/> and <see cref="XMax"/> form a valid bracket.
        /// </summary>
        protected double XMax;

        ///  <summary>
        ///  Derived classes implementing <see>
        ///          <cref>SolveImpl</cref>
        ///      </see>
        ///      assume that
        ///  <see cref="FXMin"/> and <see cref="FXMax"/> contain the values of the
        /// 	function in <see cref="XMin"/> and <see cref="XMax"/>.
        ///  </summary>
        protected double FXMin;

        ///  <summary>
        ///  Derived classes implementing <see>
        ///          <cref>SolveImpl</cref>
        ///      </see>
        ///      assume that
        ///  <see cref="FXMin"/> and <see cref="FXMax"/> contain the values of the
        /// 	function in <see cref="XMin"/> and <see cref="XMax"/>.
        ///  </summary>
        protected double FXMax;

        /// <summary>
        /// Protected constructor for abstract base class.
        /// </summary>
        protected Solver1D()
        {
            MaxEvaluations = MaxFunctionEvaluations;
            LowerBoundEnforced = false;
            UpperBoundEnforced = false;
        }

        /// <summary>
        /// This method returns the zero of the <see cref="IObjectiveFunction"/>
        /// <pramref name="objectiveFunction"/>, determined with the given accuracy.
        /// </summary>
        /// <remarks>
        /// <i>x</i> is considered a zero if <i>|f(x)| &lt; accuracy</i>.
        /// This method contains a bracketing routine to which an initial
        /// guess must be supplied as well as a step used to scan the range
        /// of the possible bracketing values.
        /// </remarks>
        /// <param name="objectiveFunction">The <see cref="ApplicationException"/>.</param>
        /// <param name="xAccuracy">Given accuracy.</param>
        /// <param name="guess">Initial guess used to scan the range
        /// of the possible bracketing values.</param>
        /// <param name="step">Initial step used to scan the range
        /// of the possible bracketing values.</param>
        /// <returns>The zero of the objective function.</returns>
        /// <exception cref="MaxEvaluations">
        /// Thrown when a bracket is not found after the maximum
        /// number of evaluations (see <see cref="IObjectiveFunction"/>).
        /// </exception>
        public double Solve(IObjectiveFunction objectiveFunction, double xAccuracy, double guess, double step)
        {
            const double growthFactor = 1.618;//golden ratio
            int flipflop = -1;
            Root = guess;
            FXMax = objectiveFunction.Value(Root);
            // monotonically crescent bias, as in optionValue(volatility)
            if (Math.Abs(FXMax) <= xAccuracy)
                return Root;
            if (FXMax > 0.0)
            {
                XMin = EnforceBounds(Root - step);
                FXMin = objectiveFunction.Value(XMin);
                XMax = Root;
            }
            else
            {
                XMin = Root;
                FXMin = FXMax;
                XMax = EnforceBounds(Root + step);
                FXMax = objectiveFunction.Value(XMax);
            }
            EvaluationNumber = 2;
            while (EvaluationNumber <= MaxEvaluations)
            {
                if (FXMin * FXMax <= 0.0)
                {
                    if (FXMin == 0.0) return XMin;
                    if (FXMax == 0.0) return XMax;
                    Root = (XMax + XMin) / 2.0;
                    // check whether we really want to pass epsilon
                    return SolveImpl(objectiveFunction,
                        Math.Max(Math.Abs(xAccuracy), Double.Epsilon));
                }
                if (Math.Abs(FXMin) < Math.Abs(FXMax))
                {
                    XMin = EnforceBounds(XMin + growthFactor * (XMin - XMax));
                    FXMin = objectiveFunction.Value(XMin);
                }
                else if (Math.Abs(FXMin) > Math.Abs(FXMax))
                {
                    XMax = EnforceBounds(XMax + growthFactor * (XMax - XMin));
                    FXMax = objectiveFunction.Value(XMax);
                }
                else if (flipflop == -1)
                {
                    XMin = EnforceBounds(XMin + growthFactor * (XMin - XMax));
                    FXMin = objectiveFunction.Value(XMin);
                    EvaluationNumber++;
                    flipflop = 1;
                }
                else if (flipflop == 1)
                {
                    XMax = EnforceBounds(XMax + growthFactor * (XMax - XMin));
                    FXMax = objectiveFunction.Value(XMax);
                    flipflop = -1;
                }
                EvaluationNumber++;
            }
            throw new ApplicationException($"Solver has exceeded its maximum iterations ({MaxEvaluations}).");
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
        /// <param name="objectiveFunction">The <see cref="IObjectiveFunction"/>.</param>
        /// <param name="xAccuracy">Given accuracy.</param>
        /// <param name="guess">Initial guess used to scan the range
        /// of the possible bracketing values.</param>
        /// <param name="xMin">Lower bracket.</param>
        /// <param name="xMax">Upper bracket.</param>
        /// <returns>The zero of the objective function.</returns>
        /// <exception cref="ApplicationException">
        /// Thrown when a bracket is not found after the maximum
        /// number of evaluations (see <see cref="MaxEvaluations"/>).
        /// </exception>
        public double Solve(IObjectiveFunction objectiveFunction, double xAccuracy,
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
            if (UpperBoundEnforced && xMax > _upperBound)
            {
                throw new ArgumentException(
                    $"Solver maximum bracket ({xMax}) is more than upper bound ({_upperBound}).");
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
            FXMin = objectiveFunction.Value(xMin);
            if (Math.Abs(FXMin) < xAccuracy) return xMin;
            FXMax = objectiveFunction.Value(xMax);
            if (Math.Abs(FXMax) < xAccuracy) return xMax;
            EvaluationNumber = 2;
            if (FXMin * FXMax >= 0.0)
                throw new ApplicationException(
                    $"Solver bounds must return different values with different signs; it returned {FXMin} and {FXMax}.");
            Root = guess;
            return SolveImpl(objectiveFunction, Math.Max(Math.Abs(xAccuracy), Double.Epsilon));
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
        /// <param name="derivativeFunction">THe derivative function</param>
        /// <param name="xAccuracy">Given accuracy.</param>
        /// <param name="guess">Initial guess used to scan the range
        /// of the possible bracketing values.</param>
        /// <param name="xMin">Lower bracket.</param>
        /// <param name="xMax">Upper bracket.</param>
        /// <returns>The zero of the objective function.</returns>
        /// <exception cref="ApplicationException">
        /// Thrown when a bracket is not found after the maximum
        /// number of evaluations (see <see cref="MaxEvaluations"/>).
        /// </exception>
        public double Solve(Func<double, double> function, Func<double, double> derivativeFunction, double xAccuracy,
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
            if (UpperBoundEnforced && xMax > _upperBound)
            {
                throw new ArgumentException(
                    $"Solver maximum bracket ({xMax}) is more than upper bound ({_upperBound}).");
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
            return SolveImpl(function, derivativeFunction, Math.Max(Math.Abs(xAccuracy), Double.Epsilon));
        }

        /// <summary>
        /// Maximum number of function evaluations for the bracketing routine. 
        /// An Error is thrown if a bracket is not found after this number of 
        /// evaluations.
        /// </summary>
        public int MaxEvaluations { get; set; }

        /// <summary>
        /// The lower bound for the function domain.
        /// </summary>
        public double LowerBound
        {
            get => _lowerBound;
            set
            {
                _lowerBound = value;
                LowerBoundEnforced = true;
            }
        }

        /// <summary>
        /// The upper bound for the function domain.
        /// </summary>
        public double UpperBound
        {
            get => _upperBound;
            set
            {
                _upperBound = value;
                UpperBoundEnforced = true;
            }
        }

        private double EnforceBounds(double x)
        {
            if (LowerBoundEnforced && x < LowerBound)
            {
                return LowerBound;
            }
            if (UpperBoundEnforced && x > _upperBound)
            {
                return _upperBound;
            }
            return x;
        }

        /// <summary>
        /// This method must be implemented in derived classes and 
        /// contains the actual code which searches for the zeroes of 
        /// the <see cref="IObjectiveFunction"/>.
        /// </summary>
        /// <remarks>
        /// It assumes that:
        /// <list type="bullet">
        /// <item>
        ///   <description>
        ///		<see cref="XMin"/> and <see cref="XMax"/> form a valid bracket; 
        ///	  </description>
        /// </item>
        /// <item>
        ///   <description>
        ///     <see cref="FXMin"/> and <see cref="FXMax"/> contain the values of the
        ///		function in <see cref="XMin"/> and <see cref="XMax"/>; 
        ///	  </description>
        /// </item>
        /// <item>
        ///   <description>
        ///     <see cref="Root"/> was initialized to a valid initial guess.
        ///	  </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="objectiveFunction">The <see cref="IObjectiveFunction"/>.</param>
        /// <param name="xAccuracy">Given accuracy.</param>
        /// <returns>The zero of the objective function.</returns>
        /// <exception cref="ApplicationException">
        /// Thrown when a bracket is not found after the maximum
        /// number of evaluations (see <see cref="MaxEvaluations"/>).
        /// </exception>
        protected abstract double SolveImpl(IObjectiveFunction objectiveFunction, double xAccuracy);

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
        /// 		<see cref="XMin"/> and <see cref="XMax"/> form a valid bracket; 
        /// 	  </description>
        ///  </item>
        ///  <item>
        ///    <description>
        ///      <see cref="FXMin"/> and <see cref="FXMax"/> contain the values of the
        /// 		function in <see cref="XMin"/> and <see cref="XMax"/>; 
        /// 	  </description>
        ///  </item>
        ///  <item>
        ///    <description>
        ///      <see cref="Root"/> was initialized to a valid initial guess.
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
        ///  number of evaluations (see <see cref="MaxEvaluations"/>).
        ///  </exception>
        protected abstract double SolveImpl(Func<double, double> function, Func<double, double> derivativeFunction, double xAccuracy);
    }
}