using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Orion.Equity.VolatilityCalculator.Exception;

namespace Orion.Equity.VolatilityCalculator
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
        public LeadStock() {  }

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
        public LeadStock(DateTime date, decimal spot, string id, string name, RateCurve rc, List<Dividend> dc) : base(date, spot, id, name, rc, dc ) { }


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
            // 2. Ensure that furtherst child expiry does not extend beyond furthest observable parent

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
                (String.CompareOrdinal(stockItem.AssetId, stock.AssetId) == 0 &&
                 String.CompareOrdinal(stockItem.Name, stock.Name) == 0)
                );
        }

        ///// <summary>
        ///// Loads the stocks from XML DOM.
        ///// </summary>
        ///// <param name="stocksDom">The stocks DOM.</param>
        ///// <returns></returns>
        //static public LeadStock[] LoadStocksFromXmlDom(XmlDocument stocksDom)
        //{
        //    string leadStockXPath = "//LeadStock";
        //    XmlNodeList leadStockNodeList = stocksDom.SelectNodes(leadStockXPath);

        //    return StockHelper.LoadLeadStocksFromStockNodeList(leadStockNodeList);
        //}
    }
}
