﻿#region Using Directives

using System;
using System.Runtime.InteropServices;
using HLV5r3.Helpers;
using Microsoft.Win32;
using Orion.Analytics.Distributions;

#endregion

namespace HLV5r3.Maths
{
    /// <summary>
    /// This class will expose functions to excel
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("A9CBC77B-1F36-4E30-AE94-CE7F21EEBEB5")]
    public class Numerics
    {
        
        #region Constructor

        #endregion

        #region Registration

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComRegisterFunction]
        public static void RegisterFunction(Type type)
        {
            Registry.ClassesRoot.CreateSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"));
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(ApplicationHelper.GetSubKeyName(type, "InprocServer32"), true);
            key?.SetValue("", Environment.SystemDirectory + @"\mscoree.dll", RegistryValueKind.String);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComUnregisterFunction]
        public static void UnregisterFunction(Type type)
        {
            Registry.ClassesRoot.DeleteSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"), false);
        }

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
        /// Computes the cumulative distribution function of the lognormal distribution.
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
        /// <returns>CummulativeNormal(x): cumulative distribution function for the standard
        /// normal distribution. </returns>
        public Double ProbabilityDistributionFunction(Double x)
        {
            return NormalDistribution.Function(x);
        }

        ///// <summary>
        ///// The zscore of a variable in a normal distribution.
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
        public Double InverseDistributionFunction(Double probability)
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
