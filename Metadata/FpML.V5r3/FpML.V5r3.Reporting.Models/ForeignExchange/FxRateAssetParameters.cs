using System;

namespace Orion.Models.ForeignExchange
{
    public class FxRateAssetParameters : IFxRateAssetParameters
    {
        private decimal _notionalAmount = 1.0m;

        public string[] Metrics { get; set; }

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        public decimal NotionalAmount { get { return _notionalAmount; } set { _notionalAmount = value; } }

        /// <summary>
        /// Gets the currency denominator flag.
        /// </summary>
        /// <value>The currency denominator flag.</value>
        public bool Currency1PerCurrency2 { get; set; }

        /// <summary>
        /// Gets or relevant fx spot rate.
        /// </summary>
        /// <value>The relevant fx spot rate.</value>
        public decimal FxRate { get; set; }

        /// <summary>
        /// Gets or relevant fx curve spot rate.
        /// </summary>
        /// <value>The relevant fx curve spot rate.</value>
        public decimal FxCurveSpotRate { get; set; }

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
