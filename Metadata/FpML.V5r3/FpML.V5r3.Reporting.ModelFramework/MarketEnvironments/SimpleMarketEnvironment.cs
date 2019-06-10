/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.MarketEnvironments
{
    ///<summary>
    ///</summary>
    [Serializable]
    public class SimpleMarketEnvironment : MarketEnvironment, ISimpleMarketEnvironment
    {
        ///<summary>
        /// THe pid of the only pricing structure contained.
        ///</summary>
        public string PricingStructureIdentifier { get; set; }

        ///<summary>
        /// The properties.
        ///</summary>
        public NamedValueSet TheProperties { get; set; }

        ///<summary>
        ///</summary>
        public SimpleMarketEnvironment()
            : base("Unidentified")
        {}

        ///<summary>
        ///</summary>
        ///<param name="id"></param>
        public SimpleMarketEnvironment(string id)
            : base(id)
        {}
                
        ///<summary>
        /// This converts an FpMl representation into an IPricingStructure.
        ///</summary>
        ///<param name="market">The market.</param>
        ///<param name="pricingStructureProperties">The properties order for each pricing structure.</param>
        public SimpleMarketEnvironment(Market market, NamedValueSet pricingStructureProperties)
            : base(market.id)
        {
            TheMarket = market;
            TheProperties = pricingStructureProperties;
            NotYetConverted = true;
        }


        ///<summary>
        /// A simple market environment can only contain a maximum of 1 pricingStructure:
        /// </summary>
        ///<param name="marketId">The marketId</param>
        ///<param name="uniqueIdentifier">The uniqueIdentifier</param>
        ///<param name="pricingStructure">The pricingStructure</param>
        public SimpleMarketEnvironment(string marketId, string uniqueIdentifier, IPricingStructure pricingStructure)
            : base(marketId)
        {
            PricingStructureIdentifier = uniqueIdentifier;
            PricingStructures.Add(PricingStructureIdentifier, pricingStructure);
        }

        public IPricingStructure GetPricingStructure()
        {
            return PricingStructures[PricingStructureIdentifier];
        }

        /// <summary>
        /// Gets the pricing structure properties.
        /// </summary>
        /// <returns></returns>
        public NamedValueSet GetPricingStructureProperties()
        {
            return TheProperties;
        }

        ///<summary>
        /// Returns an easy to use Pair<fpml> for constructors.</fpml>
        ///</summary>
        ///<returns></returns>
        public Pair<PricingStructure, PricingStructureValuation> GetPricingStructureFpML()
        {
            return new Pair<PricingStructure, PricingStructureValuation>(TheMarket.Items[0], TheMarket.Items1[0]);
        }
    }
}