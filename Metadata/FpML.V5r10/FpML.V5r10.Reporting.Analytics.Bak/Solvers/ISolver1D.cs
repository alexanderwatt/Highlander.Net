
#region Using directives

using System;

#endregion

namespace Orion.Analytics.Solvers
{
    /// <summary>
    /// Interface for 1-D solvers.
    /// </summary>
    public interface ISolver1D
    {
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
        double Solve(IObjectiveFunction objectiveFunction,
            double xAccuracy, double guess, double step);

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
        double Solve(IObjectiveFunction objectiveFunction, double xAccuracy,
            double guess, double xMin, double xMax);

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
        double Solve(Func<double, double> function, Func<double, double> derivativeFunction, double xAccuracy,
            double guess, double xMin, double xMax);

        /// <summary>
        /// Maximum number of function evaluations for the bracketing routine. 
        /// An Error is thrown if a bracket is not found after this number of 
        /// evaluations.
        /// </summary>
        int MaxEvaluations
        {
            get; set;
        }

        /// <summary>
        /// The lower bound for the function domain.
        /// </summary>
        double LowerBound
        {
            get; set;
        }

        /// <summary>
        /// The upper bound for the function domain.
        /// </summary>
        double UpperBound
        {
            get; set;
        }

    };
}