
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.PricingStructures;

namespace Orion.ModelFramework.MarketEnvironments
{
    /// <summary>
    /// The base market environment interface
    /// </summary>
    public interface ISimpleRateMarketEnvironment : IMarketEnvironment
    {
        /// <summary>
        /// Gets the pricing structure.
        /// </summary>
        /// <returns></returns>
        IRateCurve GetRateCurve();

        /// <summary>
        /// Gets the pricing structure properties.
        /// </summary>
        /// <returns></returns>
        NamedValueSet GetRateCurveProperties();

        ///<summary>
        /// Returns an easy to use Pair<fpml> for constructors.</fpml>
        ///</summary>
        ///<returns></returns>
        Pair<YieldCurve, YieldCurveValuation> GetRateCurveFpML();
    }
}