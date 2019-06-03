namespace Orion.Analytics.Interpolations
{
    /// <summary>
    /// Interface class from which all classes that encapsulate functionality
    /// for two dimensional interpolation are derived.
    /// All classes that perform two dimensional interpolation are derived
    /// from this interface class to allow polymorphic behaviour in the type
    /// of two dimensional interpolation, for example Linear or Cubic Spline.
    /// Concrete derived classes must implement the method Interpolate.
    /// </summary>
    public interface ITwoDimensionalInterpolation
    {
        #region Polymorphic Method

        /// <summary>
        /// Encapsulates the particular methodology, for example Linear or
        /// Cubic Spline, used to compute a two dimensional interpolation.
        /// Concrete derived classes must implement this method.
        /// </summary>
        /// <param name="columnTarget">The column (horizontal) target.</param>
        /// <param name="rowTarget">The row (vertical) target.</param>
        /// <returns>Interpolated value at the desired two dimensional target.
        /// </returns>
        double ValueAt(double columnTarget, double rowTarget);

        #endregion Polymorphic Method
    }
}