using System;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Calibrators.Bootstrappers
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