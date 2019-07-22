/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace FpML.V5r10.Reporting.Models.Futures
{
    public class FuturesAssetParameters : IFuturesAssetParameters
    {
        public string[] Metrics { get; set; }

        #region IFuturesAssetParameters Members

        /// <summary>
        /// Gets the base NPV.
        /// </summary>
        /// <value>The npv.</value>
        public Decimal BaseNPV { get; set; }

        /// <summary>
        /// Flag that sets whether the contract is deliverable.
        /// </summary>
        public bool IsDeliverable { get; set; }

        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        /// <value>The quote.</value>
        public decimal Quote { get; set; }

        /// <summary>
        /// The number of contracts
        /// </summary>
        public int NumberOfContracts { get; set; }

        /// <summary>
        /// The trade price
        /// </summary>
        public decimal TradePrice { get; set; }

        /// <summary>
        /// The multiplier which must be set.
        /// </summary>
        public decimal Multiplier { get; set; }

        /// <summary>
        /// The accrual period as a decimal.
        /// </summary>
        public decimal AccrualPeriod { get; set; }

        /// <summary>
        /// Gets or sets the contract notional.
        /// </summary>
        ///  <value>The contract notional.</value>
        public Decimal ContractNotional { get; set; }

        /// <summary>
        /// Gets or sets the settlement discount factor.
        /// </summary>
        /// <value>The quote.</value>
        public Decimal SettlementDiscountFactor { get; set; }

        #endregion
    }
}