// Model Analytics

using FpML.V5r3.Reporting;
using Orion.ModelFramework.Assets;

namespace Orion.CurveEngine.Assets
{
    ///<summary>
    ///</summary>
    public abstract class PriceableFxAssetController : AssetControllerBase, IPriceableFxAssetController
    {
        /// <summary>
        /// The Rate quotation
        /// </summary>
        public BasicQuotation FxRate => MarketQuote;

        /// <summary>
        /// 
        /// </summary>
        public FxRateAsset FxRateAsset { get; protected set; }

        #region IPriceableAssetController Members

        /////<summary>
        /////</summary>
        //public new BasicQuotation MarketQuote
        //{
        //    get { return FxRate; }
        //    set { FxRate = value; }
        //}

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public abstract decimal ForwardAtMaturity { get; }


        #endregion
    }
}