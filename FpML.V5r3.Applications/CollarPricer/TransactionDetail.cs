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
using Orion.EquityCollarPricer.Exception;
using Orion.EquityCollarPricer.Helpers;

namespace Orion.EquityCollarPricer
{
    /// <summary>
    /// Pay Styles
    /// </summary>
    public enum PayStyleType
    {
        /// <summary>Unknown (Not Specified)</summary>
        NotSpecified = 0,
        /// <summary>Call</summary>
        American = 1,
        /// <summary>Put</summary>
        European = 2,
    }

    /// <summary>
    /// Defines a transaction
    /// </summary>
    public class TransactionDetail
    {
        /// <summary>
        /// Gets the stock id.
        /// </summary>
        /// <value>The stock id.</value>
        public string StockId { get; }

        /// <summary>
        /// Gets or sets the current spot.
        /// </summary>
        /// <value>The current spot.</value>
        public double CurrentSpot { get; set; }


        /// <summary>
        /// Gets or sets the trade date.
        /// </summary>
        /// <value>The trade date.</value>
        public DateTime TradeDate { get; set; }

        /// <summary>
        /// Gets or sets the expiry date.
        /// </summary>
        /// <value>The expiry date.</value>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the pay style.
        /// </summary>
        /// <value>The pay style.</value>
        public PayStyleType PayStyle { get; set; }

        /// <summary>
        /// Gets the strike.
        /// </summary>
        /// <value>The strike.</value>
        public Strike Strike { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDetail"/> class.
        /// </summary>
        /// <param name="stockId">The stock id.</param>
        public TransactionDetail(string stockId)
        {
            InputValidator.IsMissingField("Stock Id", stockId, true);
            StockId = stockId;
            PayStyle = PayStyleType.NotSpecified;
        }

        /// <summary>
        /// Sets the strike.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="strikePrice">The strike price.</param>
        public void SetStrike(OptionType style, Double strikePrice)
        {
             Strike = new Strike(style, strikePrice);
        }

        /// <summary>
        /// Sets the strike.
        /// </summary>
        /// <param name="strike">The strike.</param>
        public void SetStrike(Strike strike)
        {
            Strike = strike;
        }

        /// <summary>
        /// Transactions the complete.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        internal static void TransactionComplete(TransactionDetail transaction)
        {
            InputValidator.NotZero("Spot Price", transaction.CurrentSpot, true);
            InputValidator.NotNull("Trade Date", transaction.TradeDate, true);
            InputValidator.NotNull("Expiry Date", transaction.ExpiryDate, true);
            InputValidator.EnumTypeNotSpecified("Pay Style", transaction.PayStyle, true);
            if (transaction.ExpiryDate <= transaction.TradeDate)
            {
                throw new InvalidValueException(
                    $"Expiry date {transaction.ExpiryDate} must fall after the Trade Darte {transaction.TradeDate}");
            }
        }
    }
}
