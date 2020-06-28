#region Using directives

#endregion

using Orion.Util.Helpers;

namespace FpML.V5r3.Confirmation
{
    public class PricingStructureConfigHelper
    {
        //public static Market CreateYieldCurveConfiguration(string curveId, string[] assetIdentifiers)
        //{
        //    var market = new Market {id = curveId};

        //    //Create the quotedAssetSet.
        //    var qas = QuotedAssetSetFactory.Parse(assetIdentifiers);

        //    //Create the curve information.
        //    var curve = new YieldCurve {id = curveId};

        //    //reate the valuation structure that contains qas.
        //    var curveValuation = new YieldCurveValuation {id = curveId, inputs = qas};

        //    //Set the market.
        //    market.Items = new[] { (PricingStructure)curve };
        //    market.Items1 = new[] { (PricingStructureValuation)curveValuation };

        //    return market;
        //}

        public static Market CreateFxCurveConfiguration(string curveId, string currency1, string currency2, string quoteBasis, FxRateSet quotedAssetSet)
        {
            //<QuoteBasisEnum>
            var basis = EnumHelper.Parse<QuoteBasisEnum>(quoteBasis, true);
            var quotedCurrencyPair = QuotedCurrencyPair.Create(currency1, currency2, basis);
            var currency = CurrencyHelper.Parse(currency1);
            var market = new Market { id = curveId};

            //Create the curve information.
            var curve = new FxCurve { id = curveId, name = curveId, currency = currency, quotedCurrencyPair = quotedCurrencyPair };

            //reate the valuation structure that contains qas.
            var curveValuation = new FxCurveValuation { id = curveId, spotRate = quotedAssetSet };

            //Set the market.
            market.Items = new[] { (PricingStructure)curve };
            market.Items1 = new[] { (PricingStructureValuation)curveValuation };

            return market;
        }

        public static Market CreateYieldCurveConfiguration(string curveId, QuotedAssetSet quotedAssetSet)
        {
            var market = new Market { id = curveId };

            //Create the curve information.
            var curve = new YieldCurve { id = curveId };

            //reate the valuation structure that contains qas.
            var curveValuation = new YieldCurveValuation { id = curveId, inputs = quotedAssetSet };

            //Set the market.
            market.Items = new[] { (PricingStructure)curve };
            market.Items1 = new[] { (PricingStructureValuation)curveValuation };

            return market;
        }
    }
}