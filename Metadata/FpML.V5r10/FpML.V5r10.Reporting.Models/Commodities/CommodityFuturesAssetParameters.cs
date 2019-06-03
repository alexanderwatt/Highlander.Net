using System;

namespace FpML.V5r10.Reporting.Models.Commodities
{
    public class CommodityFuturesAssetParameters : ICommodityFuturesAssetParameters
    {
        public CommodityFuturesAssetParameters()
        {
            Position = 1;
        }

        /// <summary>
        /// Gets or position.
        /// </summary>
        /// <value>The position.</value>
        public int Position { get; set; }

        /// <summary>
        /// Gets or amount.
        /// </summary>
        /// <value>The number of underlying units per contract.</value>
        public int UnitAmount { get; set; }

        /// <summary>
        /// Gets or sets the point value.
        /// </summary>
        /// <value>The point value.</value>
        public decimal PointValue { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public Decimal Index { get; set; }

        /// <summary>
        /// The time to exiry of the future.
        /// </summary>
        /// <value>The time to expiry of the future.</value>
        public Decimal TimeToExpiry { get; set; }
    }
}