#region Using directives

#endregion

namespace FpML.V5r10.Reporting.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface ISecuredRateCurve : IRateCurve
    {
        /// <summary>
        /// Gets the collateral security.
        /// </summary>
        /// <returns></returns>
        Asset GetSecurity();
    }
}