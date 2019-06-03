#region Using directives

using System;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.ModelFramework.PricingStructures
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