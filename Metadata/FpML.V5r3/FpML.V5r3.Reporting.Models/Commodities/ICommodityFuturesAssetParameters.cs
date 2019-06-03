using System;

namespace Orion.Models.Commodities
{
    public interface ICommodityFuturesAssetParameters
    {
        /// <summary>
        /// Gets or position.
        /// </summary>
        /// <value>The position.</value>
        int Position { get; set; }

        /// <summary>
        /// Gets or UnitAmount.
        /// </summary>
        /// <value>The UnitAmount.</value>
        int UnitAmount { get; set; }

        /// <summary>
        /// Gets or sets the point value.
        /// </summary>
        /// <value>The point value.</value>
        Decimal PointValue { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        Decimal Index { get; set; }

        /// <summary>
        /// The time to exiry of the future.
        /// </summary>
        /// <value>The time to expiry of the future.</value>
        Decimal TimeToExpiry { get; set; }
    }
}