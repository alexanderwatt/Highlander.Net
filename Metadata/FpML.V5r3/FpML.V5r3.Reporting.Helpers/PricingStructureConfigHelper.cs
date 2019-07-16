/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

#endregion

using Orion.Util.Helpers;

namespace FpML.V5r3.Reporting.Helpers
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