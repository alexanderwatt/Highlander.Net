#region Usings

using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework.Calibrators.Bootstrappers
{
    /// <summary>
    /// A bootstrap controller interface. 
    /// D - Denotes a generic type for bootstrap data
    /// </summary>
    public interface IBootstrapper
    {
        /// <summary>
        /// Gets the term curve data.
        /// </summary>
        /// <value>The term curve data. This is null if the pricing structure is not a curve.</value>
        TermCurve GetTermCurveData { get; }

        /// <summary>
        /// Gets the surface data.
        /// </summary>
        /// <value>The surface data. This is null if the pricing structure is not a surface.</value>
        MultiDimensionalPricingData GetSurfaceData { get; }
    }
}