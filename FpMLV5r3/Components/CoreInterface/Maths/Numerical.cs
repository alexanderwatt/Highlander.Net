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

#region Using Directives

using System;
using Highlander.CurveEngine.V5r3;
using Highlander.Reporting.Analytics.V5r3.Distributions;

#endregion

namespace Highlander.Core.Interface.V5r3.Maths
{
    /// <summary>
    ///
    /// </summary>
    public class Numerical
    {
        
        #region Information

        ///<summary>
        /// Gets the version of the assembly
        ///</summary>
        ///<returns></returns>
        public string GetCurveEngineVersionInfo() => Information.GetVersionInfo();

        #endregion

        #region Distribution Functions

        ///// <summary>
        ///// Computes the cumulative distribution function of the binomial distribution.
        ///// </summary>
        ///// <param name="probability">The probability of success per trial.</param>
        ///// <param name="numberOfTrials">The number of trials.</param>
        ///// <param name="valueToCalculate">The value to calculate the distribution for.</param>
        ///// <returns>The calculated value</returns>
        ///// <exception cref="ArgumentOutOfRangeException">If <paramref name="probability"/> is not in the interval [0.0,1.0].</exception>
        ///// <exception cref="ArgumentOutOfRangeException">If <paramref name="numberOfTrials"/> is negative.</exception>
        //public double Binomial(
        //    double probability,
        //    int numberOfTrials,
        //    double valueToCalculate)
        //{
        //    var distribution = new BinomialDistribution(probability, numberOfTrials);
        //    return distribution.CumulativeDistribution(valueToCalculate);
        //}

        /// <summary>
        /// Computes the cumulative distribution function of the log-normal distribution.
        /// </summary>
        /// <param name="probability">The probability of success per trial.</param>
        /// <param name="numberOfTrials">The number of trials.</param>
        /// <param name="valueToCalculate">The value to calculate the distribution for.</param>
        /// <returns>The calculated value</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="probability"/> is not in the interval [0.0,1.0].</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="numberOfTrials"/> is negative.</exception>
        public double LogNormal(
            double probability,
            int numberOfTrials,
            double valueToCalculate)
        {
            var distribution = new LognormalDistribution(probability, numberOfTrials);
            return distribution.CumulativeDistribution(valueToCalculate);
        }

        ///// <summary>
        ///// Computes the cumulative distribution function of the exponential distribution.
        ///// </summary>
        ///// <param name="lambda">The lambda expression.</param>
        ///// <param name="valueToCalculate">The value to calculate the distribution for.</param>
        ///// <returns>The calculated value</returns>
        //public double Exponential(
        //    double lambda,
        //    double valueToCalculate)
        //{
        //    var distribution = new ExponentialDistribution(lambda);
        //    return distribution.CumulativeDistribution(valueToCalculate);
        //}

        /// <summary>
        /// Computes the cumulative distribution function of the normal distribution.
        /// </summary>
        /// <param name="valueToCalculate">The value to calculate the normal distribution for</param>
        /// <returns>The calculated value</returns>
        public double Normal(double valueToCalculate)
        {
            var normalDistribution = new NormalDistribution();
            return normalDistribution.CumulativeDistribution(valueToCalculate);
        }

        #endregion

        #region Normal Distribution

        // ---------------------------------------------------------------------
        // Business logic methods.
        // ---------------------------------------------------------------------
        /// <summary>
        /// Evaluates the cumulative distribution function for the standard
        /// normal (Gaussian) distribution at particular value.
        /// </summary>
        /// <param name="x"></param>
        /// <returns>CumulativeNormal(x): cumulative distribution function for the standard
        /// normal distribution. </returns>
        public double ProbabilityDistributionFunction(double x)
        {
            return NormalDistribution.Function(x);
        }

        ///// <summary>
        ///// The z-score of a variable in a normal distribution.
        ///// </summary>
        ///// <param name="x"></param>
        ///// <returns>the z score.</returns>
        //public Double ZScore(Double x)
        //{
        //    var distribution = new NormalDistribution();
        //    return distribution.ZScore(x);
        //}

        /// <summary>
        /// The InverseDistributionFunction of a probability in a normal distribution.
        /// </summary>
        /// <param name="probability"></param>
        /// <returns>The x value.</returns>
        public double InverseDistributionFunction(double probability)
        {
            return InvCumulativeNormalDistribution.Function(probability);
        }

        ///// <summary>
        ///// The distribution function of a variable in a normal distribution.
        ///// </summary>
        ///// <param name="x"></param>
        ///// <returns>the z score.</returns>
        //public Double DistributionFunction(Double x)
        //{
        //    var distribution = new NormalDistribution(x);
        //    return distribution.DistributionFunction(x);
        //}

        #endregion
    }
}
