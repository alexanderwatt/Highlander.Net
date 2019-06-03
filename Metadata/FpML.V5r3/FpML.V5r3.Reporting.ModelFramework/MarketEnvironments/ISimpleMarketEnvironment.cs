

using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.MarketEnvironments
{
    /// <summary>
    /// The base market environment interface
    /// </summary>
    public interface ISimpleMarketEnvironment : IMarketEnvironment
    {
        /// <summary>
        /// Gets the pricing structure.
        /// </summary>
        /// <returns></returns>
        IPricingStructure GetPricingStructure();

        /// <summary>
        /// Gets the pricing structure properties.
        /// </summary>
        /// <returns></returns>
        NamedValueSet GetPricingStructureProperties();

        ///<summary>
        /// Returns an easy to use Pair<fpml> for constructors.</fpml>
        ///</summary>
        ///<returns></returns>
        Pair<PricingStructure, PricingStructureValuation> GetPricingStructureFpML();
    }
}