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
using System.Collections.Generic;
using System.Xml.Serialization;
using Highlander.Equities;
using Highlander.Utilities.Exception;

namespace Highlander.Equity.Calculator.V5r3
{
    /// <summary>
    /// Defines a lad stock and extends Stock class
    /// </summary>
    [Serializable]
    public class LeadStock : Stock
    {

        private readonly List<IStock> _subsidiaries = new List<IStock>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LeadStock"/> class.
        /// </summary>
        public LeadStock() 
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeadStock"/> class.
        /// </summary>
        /// <param name="spot"></param>
        /// <param name="id">The id.</param>
        /// <param name="name">The name.</param>
        /// <param name="date"></param>
        /// <param name="rc"></param>
        /// <param name="dc"></param>
        /// <example>
        ///     <code>
        ///     // Creates a BHP lead stock instance
        ///     IStock stock = new Stock("123", "BHP");
        ///     </code>
        /// </example>
        public LeadStock(DateTime date, decimal spot, string id, string name, RateCurve rc, List<Dividend> dc) : base(date, spot, id, name, rc, dc ) 
        { }

        /// <summary>
        /// Gets the subsidiary stocks.
        /// </summary>
        /// <value>The subsidiary stocks.</value>
        [XmlArray("SubsidiaryStocks")]
        public IStock[] SubsidiaryStocks
        {
            get
            {
                IStock[] stocks = new IStock[_subsidiaries.Count];
                if (_subsidiaries.Count > 0)
                {
                    _subsidiaries.CopyTo(stocks, 0);
                }
                return stocks;
            }
        }

        /// <summary>
        /// Adds the subsidiary.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <example>
        ///     <code>
        ///     // Adds the AMP subsidiary stock to the BHP lead stock instance
        ///     IStock stock = new Stock("123", "BHP");
        ///     LeadStock lead = new LeadStock("456", "AMP");
        ///     lead.AddSubsidiary(stock);
        ///     </code>
        /// </example>
        public void AddSubsidiary(IStock stock)
        {
            // 1. Ensure that parent/child expiries are not duplicated
            // 2. Ensure that furthest child expiry does not extend beyond furthest observable parent
            //StockHelper.CheckChildExpiriesWellDefined(this, stock);
            IStock matchedStock = FindSubsidiary(stock);
            if (matchedStock == null)
            {
                _subsidiaries.Add(stock);
            }
            else
            {
                throw new DuplicateNotAllowedException($"Stock with Asset Id {stock.AssetId} already exists");
            }
            //StockHelper.CheckChildStrikesWellDefined(this, stock);                                

        }

        /// <summary>
        /// Removes the subsidiary.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <example>
        ///     <code>
        ///     // Removes all subsidiary stocks from the BHP lead stock instance
        ///     LeadStock lead = new LeadStock("456", "BHP");
        ///     foreach(IStock stock in lead.SubsidiaryStocks)
        ///     {
        ///         lead.RemoveSubsidiary(stock);
        ///     }
        ///     </code>
        /// </example>
        public void RemoveSubsidiary(IStock stock)
        {
            IStock matchedStock = FindSubsidiary(stock);
            if (matchedStock != null)
            {
                _subsidiaries.Remove(matchedStock);
            }
        }

        /// <summary>
        /// Finds the subsidiary.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <returns></returns>
        private IStock FindSubsidiary(IStock stock)
        {
            return _subsidiaries.Find(
                stockItem =>
                (string.CompareOrdinal(stockItem.AssetId, stock.AssetId) == 0 &&
                 string.CompareOrdinal(stockItem.Name, stock.Name) == 0)
                );
        }
    }
}
