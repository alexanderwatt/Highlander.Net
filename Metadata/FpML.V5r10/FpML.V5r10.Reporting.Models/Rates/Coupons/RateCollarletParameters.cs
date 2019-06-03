

namespace FpML.V5r10.Reporting.Models.Rates.Coupons
{
    public class RateCollarletParameters : RateCouponParameters, IRateCollarletParameters
    {
        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The cap volatility.</value>
        public decimal CapVolatility { get; set; }

        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The floor volatility.</value>
        public decimal FloorVolatility { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The cap strike.</value>
        public decimal CapStrike { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The floor strike.</value>
        public decimal FloorStrike { get; set; }

    }
}