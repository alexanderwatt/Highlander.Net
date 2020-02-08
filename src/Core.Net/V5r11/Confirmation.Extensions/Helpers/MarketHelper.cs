using System.Collections.Generic;
using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
{
    public static class MarketHelper
    {
        public static YieldCurve[] GetYieldCurves(Market market)
        {
            List<YieldCurve> list = new List<YieldCurve>();

            foreach (YieldCurve yieldCurve in market.Items)
            {
                list.Add(yieldCurve);
            }

            return list.ToArray();
        }

        public static YieldCurveValuation[] GetYieldCurveValuations(Market market)
        {
            List<YieldCurveValuation> list = new List<YieldCurveValuation>();

            foreach (YieldCurveValuation yieldCurveValuation in market.Items1)
            {
                list.Add(yieldCurveValuation);
            }

            return list.ToArray();
        }
    }
}