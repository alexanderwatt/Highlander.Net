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
using Orion.ModelFramework.MarketEnvironments;

namespace Orion.CurveEngine.Markets
{
    ///<summary>
    ///</summary>
    [Serializable]
    public class FxLegEnvironment : MarketEnvironment, IFxLegEnvironment
    {
        /// <summary>
        /// ExchangeCurrencyMarket1
        /// </summary>
        public ISwapLegEnvironment ExchangeCurrencyMarket1 { get; set; }

        /// <summary>
        /// ExchangeCurrencyMarket2
        /// </summary>
        public ISwapLegEnvironment ExchangeCurrencyMarket2 { get; set; }

        ///<summary>
        /// A market environment can only contain 2 swap market environments:
        /// Each swap market can contain a maximum of:
        /// A forecast rate curve, a discount curve, a commodity curve, 
        /// a reporting currency fx curve and a volatility surface.
        /// This type is use in priceable instruments valuations via the Evaluate method.
        ///</summary>
        ///<param name="id"></param>
        ///<param name="exchangeCurrencyMarket1"></param>
        ///<param name="exchangeCurrencyMarket2"></param>
        public FxLegEnvironment(string id, ISwapLegEnvironment exchangeCurrencyMarket1, ISwapLegEnvironment exchangeCurrencyMarket2)
            : base(id)
        {
            ExchangeCurrencyMarket1 = exchangeCurrencyMarket1;
            ExchangeCurrencyMarket2 = exchangeCurrencyMarket2;
        }

        ///<summary>
        /// Gets the relevant environment.
        ///</summary>
        ///<returns></returns>
        public ISwapLegEnvironment GetExchangeCurrencyPaymentEnvironment1()
        {
            return ExchangeCurrencyMarket1;
        }

        ///<summary>
        /// Gets the relevant environment.
        ///</summary>
        ///<returns></returns>
        public ISwapLegEnvironment GetExchangeCurrencyPaymentEnvironment2()
        {
            return ExchangeCurrencyMarket2;
        }
    }
}