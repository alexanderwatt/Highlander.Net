#region Usings

using System;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using FpML.V5r10.Reporting.Models.Generic.Coupons;

#endregion

namespace FpML.V5r10.Reporting.Models.Rates.Coupons
{
    public class RateCouponParameters: CouponParameters, IRateCouponParameters
    {
        /// <summary>
        /// Gets or sets the fx rate.
        /// </summary>
        ///  <value>The fx rate.</value>
        public Decimal? StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the Payment Discount Factor.
        /// </summary>
        ///  <value>The Payment Discount Factor.</value>
        public Decimal? EndDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the forecast curve.
        /// </summary>
        /// <value>The forecast curve.</value>
        public IRateCurve ForecastCurve { get; set; }

        /// <summary>
        /// Gets or sets the volatility surface.
        /// </summary>
        /// <value>The volatility surface.</value>
        public IVolatilitySurface VolatilitySurface { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        public Decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the base rate.
        /// </summary>
        /// <value>The base rate.</value>
        public Decimal BaseRate { get; set; }

        /// <summary>
        /// Gets or sets the discount type.
        /// </summary>
        /// <value>The type.</value>
        public DiscountType DiscountType { get; set; }

        /// <summary>
        /// Gets or sets the discount rate.
        /// </summary>
        /// <value>The rate.</value>
        public decimal? DiscountRate { get; set; }

        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public decimal? Volatility { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The strike.</value>
        public decimal? Strike { get; set; }

        /// <summary>
        /// Gets or sets the swap accrual factor.
        /// </summary>
        /// <value>The swap accrual factor.</value>
        public decimal SwapAccrualFactor { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The premium.</value>
        public Decimal Premium { get; set; }

        /// <summary>
        /// Gets or sets the iscall flag.
        /// </summary>
        /// <value>The iscall flag.</value>
        public bool IsCall { get; set; }

        /// <summary>
        /// Gets or sets the expiry year fraction.
        /// </summary>
        /// <value>The expiry year fraction.</value>
        public decimal ExpiryYearFraction { get; set; }
    }
}