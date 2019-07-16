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

using System.Collections.Generic;
using Orion.Util.Helpers;

namespace FpML.V5r3.Reporting.Helpers
{
    public class MarketFactory
    {
        private readonly List<Pair<PricingStructure, PricingStructureValuation>> _pricingStructures = new List<Pair<PricingStructure, PricingStructureValuation>>();

        public void AddPricingStructure(Pair<PricingStructure, PricingStructureValuation> pair)
        {
            _pricingStructures.Add(pair);
        }

        public void AddYieldCurve(Pair<YieldCurve, YieldCurveValuation> pair)
        {
            var pricingStructurePair = new Pair<PricingStructure, PricingStructureValuation>(pair.First, pair.Second);
            _pricingStructures.Add(pricingStructurePair);
        }

        public  Market  Create()
        {
            var market = new Market();
            var listPricingStructure = new List<PricingStructure>();
            var listPricingStructureValuation = new List<PricingStructureValuation>();
            foreach (Pair<PricingStructure, PricingStructureValuation> pair in _pricingStructures)
            {
                listPricingStructure.Add(pair.First);
                listPricingStructureValuation.Add(pair.Second);
            }
            market.Items = listPricingStructure.ToArray();
            market.Items1 = listPricingStructureValuation.ToArray();
            return market;
        }


       
    }
}