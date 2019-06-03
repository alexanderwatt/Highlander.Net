#region usings

using System;
using Orion.Models.Generic.Coupons;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.Models.Rates.Coupons
{
        public enum DiscountType
    {
        None,
        ISDA,
        AFMA
    }

    public interface IRateCouponParameters: ICouponParameters
    {
        /// <summary>
        /// Gets or sets the fx rate.
        /// </summary>
        ///  <value>The fx rate.</value>
        Decimal? StartDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the Payment Discount Factor.
        /// </summary>
        ///  <value>The Payment Discount Factor.</value>
        Decimal? EndDiscountFactor { get; set; }

        /// <summary>
        /// Gets or sets the forecast curve.
        /// </summary>
        /// <value>The forecast curve.</value>
        IRateCurve ForecastCurve { get; set; }

        /// <summary>
        /// Gets or sets the volatility surface.
        /// </summary>
        /// <value>The volatility surface.</value>
        IVolatilitySurface VolatilitySurface { get; set; }

        /// <summary>
        /// Gets or sets the rate.
        /// </summary>
        /// <value>The rate.</value>
        Decimal Rate { get; set; }
        
        /// <summary>
        /// Gets or sets the base rate.
        /// </summary>
        /// <value>The base rate.</value>
        Decimal BaseRate { get; set; }

        /// <summary>
        /// Gets or sets the discount type.
        /// </summary>
        /// <value>The type.</value>
        DiscountType DiscountType { get; set; }

        /// <summary>
        /// Gets or sets the discount rate.
        /// </summary>
        /// <value>The rate.</value>
        Decimal? DiscountRate { get; set; }

        /// <summary>
        /// Gets or sets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        Decimal? Volatility { get; set; }

        /// <summary>
        /// Gets or sets the strike.
        /// </summary>
        /// <value>The strike.</value>
        Decimal? Strike { get; set; }

        /// <summary>
        /// Gets or sets the swap accrual factor.
        /// </summary>
        /// <value>The swap accrual factor.</value>
        decimal SwapAccrualFactor { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        /// <value>The premium.</value>
        Decimal Premium { get; set; }

        /// <summary>
        /// Gets or sets the iscall flag.
        /// </summary>
        /// <value>The iscall flag.</value>
        bool IsCall { get; set; }

        /// <summary>
        /// Gets or sets the expiry year fraction.
        /// </summary>
        /// <value>The expiry year fraction.</value>
        Decimal ExpiryYearFraction { get; set; }

    }
}