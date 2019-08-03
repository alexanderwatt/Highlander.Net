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

using System;
using System.Linq;
using System.Collections.Generic;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;

#endregion

namespace Orion.CurveEngine.Factory
{
    /// <summary>
    /// 
    /// </summary>
    public class MarketFactory
    {
        private readonly List<Pair<YieldCurve, YieldCurveValuation>> _yieldCurves = new List<Pair<YieldCurve, YieldCurveValuation>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pair"></param>
        public  void    AddYieldCurve(Pair<YieldCurve, YieldCurveValuation> pair)
        {
            _yieldCurves.Add(pair);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public  Market  Create()
        {
            var market = new Market();
            var listYieldCurve = new List<PricingStructure>();
            var listYieldCurveValuation = new List<PricingStructureValuation>();
            foreach(var pair in _yieldCurves)
            {
                listYieldCurve.Add(pair.First);
                listYieldCurveValuation.Add(pair.Second);
            }
            market.Items = listYieldCurve.ToArray();
            market.Items1 = listYieldCurveValuation.ToArray();
            return market;
        }
       
    }

    /// <summary>
    /// 
    /// </summary>
    public class MarketHelper
    {
        private readonly IDictionary<String, Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>> _curves = new Dictionary<String, Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>>();

        private void AddPricingStructure(PricingStructure ps, PricingStructureValuation psv, NamedValueSet nvs)
        {
            var triple = new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(ps, psv, nvs);
            var identifier = nvs.GetValue<string>("UniqueIdentifier");
            _curves.Add(identifier, triple);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="market"></param>
        /// <param name="pricingStructureProperties"></param>
        /// <returns></returns>
        public IDictionary<String, Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>> Create(Market market, List<NamedValueSet> pricingStructureProperties)
        {
            var numberOfCurves = market.Items.Length;
            if (numberOfCurves == market.Items1.Length && numberOfCurves == pricingStructureProperties.Count)
            {
                var index = 0;
                foreach (var curve in market.Items)
                {
                    AddPricingStructure(curve, market.Items1[index], pricingStructureProperties[index]);
                    index++;
                }
            }
            return _curves;
        }

        /// <summary>
        /// Returns properties from the pricing structure. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="marketEnvironmentId">The market id.</param>
        /// <param name="fpmlData">The FPML data.</param>
        public static Market CreateMarketFromFpML(string marketEnvironmentId, List<IPricingStructure> fpmlData)
        {
            var data = fpmlData.Select(curve => curve.GetFpMLData()).ToList();
            var market = CreateMarketFromFpML(marketEnvironmentId, data);
            return market;
        }

        /// <summary>
        /// Returns properties from the pricing structure. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="marketEnvironmentId">The market id.</param>
        /// <param name="fpmlData">The FPML data.</param>
        public static Market CreateMarketFromFpML(string marketEnvironmentId, List<Pair<PricingStructure, PricingStructureValuation>> fpmlData)
        {
            var market = new Market
            {
                id = marketEnvironmentId,
            };
            var curves = new List<PricingStructure>();
            var curveValuations = new List<PricingStructureValuation>();
            foreach (var pair in fpmlData)
            {
                curves.Add(pair.First);
                curveValuations.Add(pair.Second);
            }
            market.Items = curves.ToArray();
            market.Items1 = curveValuations.ToArray();
            return market;
        }

        /// <summary>
        /// Returns properties from the pricing structure. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="marketEnvironmentId">The market id.</param>
        /// <param name="fpmlData">The FPML data.</param>
        public static Market CreateMarketFromFpML(string marketEnvironmentId, Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            var market = new Market
            {
                id = marketEnvironmentId,
                Items = new PricingStructure[1],
                Items1 = new PricingStructureValuation[1]
            };
            market.Items[0] = fpmlData.First;
            market.Items1[0] = fpmlData.Second;
            return market;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FxMarketFactory
    {
        private readonly List<Pair<FxCurve, FxCurveValuation>> _fxCurves = new List<Pair<FxCurve, FxCurveValuation>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pair"></param>
        public void AddFxCurve(Pair<FxCurve, FxCurveValuation> pair)
        {
            _fxCurves.Add(pair);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Market Create()
        {
            var market = new Market();
            var listFxCurve = new List<PricingStructure>();
            var listFxCurveValuation = new List<PricingStructureValuation>();
            foreach (var pair in _fxCurves)
            {
                listFxCurve.Add(pair.First);
                listFxCurveValuation.Add(pair.Second);
            }
            market.Items = listFxCurve.ToArray();
            market.Items1 = listFxCurveValuation.ToArray();
            return market;
        }
    }
}