#region Using directives

using System;

#endregion

namespace Orion.Analytics.Distributions
{
    /// <summary>
    /// Interface to a double-precision floating point random number generator.
    /// </summary>
    public interface IContinousRng
    {
        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, 
        /// and less than 1.0.
        /// </returns>
        double NextDouble();

        /// <summary>
        /// Returns an <see cref="Array"/> of random number between 0.0 and 1.0.
        /// </summary>
        /// <param name="r">
        /// A double-precision floating point <see cref="Array"/> to be filled
        /// with random numbers number greater than or equal to 0.0, and less than 1.0.
        /// </param>
        void Next(double[] r);

        /// <summary>
        /// Returns an <see cref="Array"/> of random number between 0.0 and 1.0.
        /// </summary>
        /// <param name="r">
        /// A double-precision floating point <see cref="Array"/> to be filled
        /// with random numbers number greater than or equal to 0.0, and less than 1.0.
        /// </param>
        /// <param name="start">Index of the first elemnt to fill.</param>
        void Next(double[] r, int start);

        /// <summary>
        /// Returns an <see cref="Array"/> of random number between 0.0 and 1.0.
        /// </summary>
        /// <param name="r">
        /// A double-precision floating point <see cref="Array"/> to be filled
        /// with random numbers number greater than or equal to 0.0, and less than 1.0.
        /// </param>
        /// <param name="start">Index of the first elemnt to fill.</param>
        /// <param name="length">Number of random numbers to generate.</param>
        void Next(double[] r, int start, int length);
    }
}