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

namespace Highlander.Reporting.Models.V5r3.Futures
{
    public interface IFuturesAssetParameters
    {
        /// <summary>
        /// Flag that sets whether the contract is deliverable.
        /// </summary>
        Boolean IsDeliverable{ get; set; }

        /// <summary>
        /// The starting npv.
        /// </summary>
        Decimal BaseNPV { get; set; }

        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        /// <value>The quote.</value>
        Decimal Quote { get; set; }

        /// <summary>
        /// Gets or sets the number.
        /// </summary>
        /// <value>The number.</value>
        int NumberOfContracts { get; set; }

        /// <summary>
        /// Gets or sets the trade price.
        /// </summary>
        /// <value>The trade price.</value>
        Decimal TradePrice { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        Decimal Multiplier { get; set; }

        /// <summary>
        /// The accrual period as a decimal.
        /// </summary>
        Decimal AccrualPeriod { get; set; }

        /// <summary>
        /// Gets or sets the contract notional.
        /// </summary>
        ///  <value>The contract notional.</value>
        Decimal ContractNotional { get; set; }

        /// <summary>
        /// Gets or sets the settlement discount factor.
        /// </summary>
        /// <value>The quote.</value>
        Decimal SettlementDiscountFactor { get; set; }
    }
}