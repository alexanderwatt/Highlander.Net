using System;
using Orion.Util.Helpers;

namespace FpML.V5r10.Reporting.ModelFramework.Calibrators.Bootstrappers
{
    /// <summary>
    /// Base interface defines the data required by all Bootstrap controllers
    /// </summary>
    public interface IBootstrapControllerData
    {
        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <value>The quoted asset set.</value>
        Pair<PricingStructure, PricingStructureValuation> PricingStructureData { get; set; }
    }
}