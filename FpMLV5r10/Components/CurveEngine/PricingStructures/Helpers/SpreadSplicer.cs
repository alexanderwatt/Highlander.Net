#region Using Directives

using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework.Assets;

#endregion

namespace Orion.CurveEngine.PricingStructures.Helpers
{
    /// <summary>
    /// A special interpolator for spreads.
    /// </summary>
    public static class SpreadSplicer
    {
        /// <summary>
        /// Adds the extra points defined using the spreadcontrollers.
        /// </summary>
        /// <param name="yieldCurveValuation"></param>
        /// <param name="priceableSpreadAssets"></param>
        /// <returns></returns>
        public static YieldCurveValuation SpreadController(PricingStructureValuation yieldCurveValuation, IPriceableSpreadAssetController[] priceableSpreadAssets)
        {
            var curveValuation = (YieldCurveValuation)yieldCurveValuation;
            var curve = curveValuation.discountFactorCurve;
            return curveValuation;
        }

        /// <summary>
        /// Gets the termpoints from a priceablespreadasset controller.
        /// </summary>
        /// <param name="spreadAsset"></param>
        /// <returns></returns>
        public static TermPoint[] SpreadAssetMapper(IPriceableSpreadAssetController spreadAsset)
        {
            var points = new TermPoint[1];
            return points;
        }
    }
}