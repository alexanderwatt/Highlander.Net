using System;

namespace Orion.Models.Commodities
{
    public interface ICommodityAverageAssetParameters
    {
        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        Decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or relevant fx spot rate.
        /// </summary>
        /// <value>The relevant fx spot rate.</value>
        Decimal CommodityForward { get; set; }

        /// <summary>
        /// Gets or sets the star index.
        /// </summary>
        /// <value>The start index.</value>
        Decimal StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the average index.
        /// </summary>
        /// <value>The average index.</value>
        Decimal AverageIndex { get; set; }

        /// <summary>
        /// Gets or relevant spread.
        /// </summary>
        /// <value>The relevant spread.</value>
        Decimal Spread { get; set; }

        /// <summary>
        /// Gets the conversion units..
        /// </summary>
        /// <value>The currency denominator flag.</value>
        bool Currency1PerCurrency2 { get; set; }

        /// <summary>
        /// Gets or relevant fx curve spot rate.
        /// </summary>
        /// <value>The relevant fx curve spot rate.</value>
        Decimal CommodityCurveForward { get; set; }

        /// <summary>
        /// Gets or sets the spot discount factor for currency1.
        /// </summary>
        /// <value>The start discount factor.</value>
        Decimal Ccy1SpotDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the spot discount factor for currency1.
        /// </summary>
        /// <value>The start discount factor.</value>
        Decimal Ccy2SpotDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the year fraction.
        /// </summary>
        /// <value>The year fraction.</value>
        Decimal YearFraction { get; set; }
    }
}