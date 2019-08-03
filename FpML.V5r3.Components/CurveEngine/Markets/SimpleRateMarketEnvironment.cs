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

using System;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;

namespace Orion.CurveEngine.Markets
{
    ///<summary>
    ///</summary>
    [Serializable]
    public class SimpleRateMarketEnvironment : SimpleMarketEnvironment, ISimpleRateMarketEnvironment
    {

        ///<summary>
        /// A simple market environment can only contain a maximum of 3 curves:
        /// A forecast curve, a discount curve and a volatility surface.
        /// This type is use in priceable asset valuations via the Evaluate method.
        ///</summary>
        public SimpleRateMarketEnvironment()
            : base("Unidentified")
        {}

        ///<summary>
        /// A simple market environment can only contain a maximum of 3 curves:
        /// A forecast curve, a discount curve and a volatility surface.
        /// This type is use in priceable asset valuations via the Evaluate method.
        ///</summary>
        ///<param name="id"></param>
        public SimpleRateMarketEnvironment(string id):base(id)
        {}


        ///<summary>
        /// A simple market environment can only contain a maximum of 3 curves:
        /// A forecast curve, a discount curve and a volatility surface.
        /// This type is use in priceable asset valuations via the Evaluate method.
        ///</summary>
        ///<param name="market">The market</param>
        public SimpleRateMarketEnvironment(Market market)//TODO implement a class factory conversion to IPricingStructure.
            : base(market.id)
        {
        }


        ///<summary>
        /// A simple market environment can only contain a maximum of 3 curves:
        /// A forecast curve, a discount curve and a volatility surface.
        /// This type is use in priceable asset valuations via the Evaluate method.
        ///</summary>
        ///<param name="market">The market. This market only contains one rate curves.</param>
        ///<param name="pricingStructureProperties">The properties order for each pricing structure.</param>
        public SimpleRateMarketEnvironment(Market market, NamedValueSet pricingStructureProperties)
            : base(market.id)
        {
        }

        #region Implementation of ISimpleRateMarketEnvironment

        /// <summary>
        /// Gets the pricing structure.
        /// </summary>
        /// <returns></returns>
        public IRateCurve GetRateCurve()
        {
            var curve = SearchForPricingStructureType("DiscountCurve");

            return (IRateCurve)curve;
        }

        /// <summary>
        /// Gets the pricing structure properties.
        /// </summary>
        /// <returns></returns>
        public NamedValueSet GetRateCurveProperties()
        {
            return TheProperties;
        }

        ///<summary>
        /// Returns an easy to use Pair<fpml> for constructors.</fpml>
        ///</summary>
        ///<returns></returns>
        public Pair<YieldCurve, YieldCurveValuation> GetRateCurveFpML()
        {
            return new Pair<YieldCurve, YieldCurveValuation>((YieldCurve)TheMarket.Items[0], (YieldCurveValuation)TheMarket.Items1[0]);
        }

        #endregion
    }
}