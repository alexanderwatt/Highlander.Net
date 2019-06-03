using System;
using Orion.Models.Rates.Coupons;

namespace Orion.Models.Rates.Coupons
{
    public interface IRateCollarletParameters : IRateCouponParameters
    {
        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The cap volatility.</value>
        Decimal CapVolatility { get; set; }

        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The floor volatility.</value>
        Decimal FloorVolatility { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The cap strike.</value>
        Decimal CapStrike { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The floor strike.</value>
        Decimal FloorStrike { get; set; }
    }
}