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

namespace Orion.Models.Futures
{
    public class FuturesAssetResults : FuturesAssetParameters, IFuturesAssetResults
    {
        #region Implementation of IEquityAssetResults

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal NPV{ get; set; }

        /// <summary>
        /// Gets the npv change from the base NPV.
        /// </summary>
        /// <value>The npv change.</value>
        public decimal NPVChange { get; set; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal ImpliedQuote { get; set; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote { get; set; }

        /// <summary>
        /// Gets the profit based on the actual purchase price.
        /// </summary>
        public decimal PandL { get; set; }

        /// <summary>
        /// The inital margin
        /// </summary>
        public decimal InitialMargin { get; set; }

        /// <summary>
        /// THe variation margin
        /// </summary>
        public decimal VariationMargin { get; set; }

        /// <summary>
        /// Gets the Index At Maturity.
        /// </summary>
        public decimal IndexAtMaturity { get; set; }

        /// <summary>
        /// Gets the strike.
        /// </summary>
        public decimal Strike { get; set; }

        /// <summary>
        /// Gets the option volatility.
        /// </summary>
        /// <value>The option volatility.</value>
        public decimal OptionVolatility { get; set; }

        #endregion
    }
}