using System;

namespace Orion.Analytics.Helpers
{
    /// <summary>
    /// Class that encapsulates the four Hermite Spline Basis functions used 
    /// in the implementation of a Cubic Hermite Spline interpolation.
    /// </summary>
    public class HermiteSplineBasisFunctions
    {
        #region Private Constants
        /// <summary>
        /// Minimum value of the argument to the basis function.
        /// </summary>
        private const decimal ArgMin = 0.0m;

        /// <summary>
        /// Maximum value of the argument to the basis function.
        /// </summary>
        private const decimal ArgMax = 1.0m;

        #endregion private Constants

        #region Public Static Business Logic Methods

        /// <summary>
        /// Computes the Hermite Spline Basis function h00 at a specified value
        /// in the range [0,1].
        /// </summary>
        /// <param name="t">Value at which the Hermite Spline Basis function
        /// is to be evaluated.
        /// Precondition: Argument must be in the range [0,1].</param>
        /// <returns>Value of the Hermite Spline Basis function h00.</returns>
        public static decimal H00(decimal t)
        {
            // Check for a valid argument.
            IsValidArgument(t);
            // Compute and return the value of the basis function.
            decimal h00 = 2.0m * (decimal)Math.Pow(decimal.ToDouble(t), 3.0) -
                          3.0m*t*t + 1.0m;
            return h00;
        }

        /// <summary>
        /// Computes the Hermite Spline Basis function h10 at a specified value
        /// in the range [0,1].
        /// </summary>
        /// <param name="t">Value at which the Hermite Spline Basis function
        /// is to be evaluated.
        /// Precondition: Argument must be in the range [0,1].</param>
        /// <returns>Value of the Hermite Spline Basis function h10.</returns>
        public static decimal H10(decimal t)
        {
            // Check for a valid argument.
            IsValidArgument(t);
            // Compute and return the value of the basis function.
            decimal h10 = (decimal)Math.Pow(decimal.ToDouble(t), 3.0) -
                          2.0m*t*t + t;
            return h10;
        }

        /// <summary>
        /// Computes the Hermite Spline Basis function h01 at a specified value
        /// in the range [0,1].
        /// </summary>
        /// <param name="t">Value at which the Hermite Spline Basis function
        /// is to be evaluated.</param>
        /// <returns>Value of the Hermite Spline Basis function h01.</returns>
        public static decimal H01(decimal t)
        {
            // Check for a valid argument.
            IsValidArgument(t);
            // Compute and return the value of the basis function.
            decimal h01 = -2.0m*(decimal)Math.Pow(decimal.ToDouble(t), 3.0) +
                          3.0m*t*t;
            return h01;
        }

        /// <summary>
        /// Computes the Hermite Spline Basis function h11 at a specified value
        /// in the range [0,1].
        /// </summary>
        /// <param name="t">Value at which the Hermite Spline Basis function
        /// is to be evaluated.</param>
        /// <returns>Value of the Hermite Spline Basis function h11.</returns>
        public static decimal H11(decimal t)
        {
            // Check for a valid argument.
            IsValidArgument(t);
            // Compute and return the value of the basis function.
            decimal h11 = (decimal)Math.Pow(decimal.ToDouble(t), 3.0) - t * t;
            return h11;
        }

        #endregion Public Static Business Logic Methods

        #region Private Data Validation Methods

        /// <summary>
        /// Helper function used to determine whether the argument at which a
        /// Hermite Spline Basis function is to be evaluated is in the
        /// acceptable range: [0,1].
        /// Exception: ArgumentException if argument is found outside of the
        /// range [0,1].
        /// </summary>
        /// <param name="t">Value at which the Hermite Spline Basis function
        /// is to be evaluated.</param>
        private static void IsValidArgument(decimal t)
        {
            if (t >= ArgMin && t <= ArgMax) return;
            // Invalid argument: throw "Exception" object.
            const string errorMessage =
                "Hermite Spline Basis function argument not in [0,1]";
            throw new ArgumentException(errorMessage);
        }        

        #endregion Private Data Validation Methods

    }
}