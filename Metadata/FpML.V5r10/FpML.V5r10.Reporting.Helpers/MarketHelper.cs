using System.Linq;

namespace FpML.V5r10.Reporting.Helpers
{
    public static class MarketHelper
    {
        public static YieldCurve[] GetYieldCurves(Market market)
        {
            return market.Items.Cast<YieldCurve>().ToArray();
        }

        public static YieldCurveValuation[] GetYieldCurveValuations(Market market)
        {
            return market.Items1.Cast<YieldCurveValuation>().ToArray();
        }
    }
}