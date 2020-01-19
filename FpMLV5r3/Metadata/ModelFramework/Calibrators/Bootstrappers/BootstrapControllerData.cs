
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Calibrators.Bootstrappers
{
    /// <summary>
    /// Base class which defines the data required by all bootstrap controllers
    /// </summary>
    public class BootstrapControllerData: IBootstrapControllerData
    {

        #region IBootstrapControllerData Members

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <value>The quoted asset set.</value>
        public Pair<PricingStructure, PricingStructureValuation> PricingStructureData   { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="BootstrapControllerData"/> class.
        /// </summary>
        /// <param name="yieldCurve">The yield curve.</param>
        /// <param name="pricingStructureValuation">The pricing structure valuation.</param>
        public BootstrapControllerData(PricingStructure yieldCurve, PricingStructureValuation pricingStructureValuation)
        {
            PricingStructureData = new Pair<PricingStructure, PricingStructureValuation>(yieldCurve, pricingStructureValuation);
        }
    }
}