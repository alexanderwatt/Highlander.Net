
using System;

namespace FpML.V5r10.Reporting.Models.Commodities
{
    public class CommodityAverageAssetParameters : ICommodityAverageAssetParameters
    {
        public CommodityAverageAssetParameters()
        {
            NotionalAmount = 1.0m;
        }

        public string[] Metrics { get; set; }

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        public Decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the star index.
        /// </summary>
        /// <value>The start index.</value>
        public Decimal StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the average index.
        /// </summary>
        /// <value>The average index.</value>
        public Decimal AverageIndex { get; set; }

        /// <summary>
        /// Gets or relevant spread.
        /// </summary>
        /// <value>The relevant spread.</value>
        public Decimal Spread { get; set; }

        /// <summary>
        /// Gets the conversion units..
        /// </summary>
        /// <value>The currency denominator flag.</value>
        public bool Currency1PerCurrency2 { get; set; }

        /// <summary>
        /// Gets or relevant fx spot rate.
        /// </summary>
        /// <value>The relevant fx spot rate.</value>
        public Decimal CommodityForward { get; set; }

        /// <summary>
        /// Gets or relevant fx curve spot rate.
        /// </summary>
        /// <value>The relevant fx curve spot rate.</value>
        public decimal CommodityCurveForward { get; set; }

        /// <summary>
        /// Gets or sets the spot discount factor for currency1.
        /// </summary>
        /// <value>The start discount factor.</value>
        public decimal Ccy1SpotDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the spot discount factor for currency1.
        /// </summary>
        /// <value>The start discount factor.</value>
        public decimal Ccy2SpotDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        public decimal YearFraction { get; set; }
    }
}