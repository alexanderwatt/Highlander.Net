

namespace Orion.Analytics.Interpolators
{
    /// <summary>
    /// Interface class from which all classes that encapsulate functionality
    /// for one dimensional interpolation are derived.
    /// All classes that perform one dimensional interpolation are derived
    /// from this interface class to allow polymorphic behaviour in the type
    /// of one dimensional interpolation, for example Linear or Cubic Spline.
    /// Concrete derived classes must implement the method Interpolate.
    /// </summary>
    public interface IOneDimensionalInterpolation
    {
        #region Interface Class Methods to be Implemented

        /// <summary>
        /// Encapsulates the particular methodology, for example Linear or
        /// Cubic Spline, used to compute a one dimensional interpolation.
        /// Concrete derived classes must implement this method.
        /// Note: provided for backward compatibility.
        /// </summary>
        /// <param name="target">Value at which to compute the interpolation.
        /// </param>
        /// <returns>Interpolated value at the desired target.</returns>
        double Interpolate(double target);

        /// <summary>
        /// Encapsulates the particular methodology, for example Linear or
        /// Cubic Spline, used to compute a one dimensional interpolation.
        /// Concrete derived classes must implement this method.
        /// Note: Overload of the Interpolate method is provided to allow for
        /// backward compatibility and for improved accuracy offered by the
        /// Decimal data type in numerical computations.
        /// </summary>
        /// <param name="target">Value at which to compute the interpolation.
        /// </param>
        /// <returns>Interpolated value at the desired target.</returns>
        decimal Interpolate(decimal target);

        #endregion Interface Class Methods to be Implemented
    }
}