// Model Analytics
using Orion.ModelFramework.Assets;

namespace Orion.CurveEngine.Assets
{
    ///<summary>
    ///</summary>
    public abstract class PriceableCreditAssetController : AssetControllerBase, IPriceableCreditAssetController
    {
        #region IPriceableAssetController Members

        /// <summary>
        /// Gets the default probability at maturity.
        /// </summary>
        /// <value>The default probability at maturity.</value>
        public abstract decimal SurvivalProbabilityAtMaturity { get; }

        #endregion

    }
}