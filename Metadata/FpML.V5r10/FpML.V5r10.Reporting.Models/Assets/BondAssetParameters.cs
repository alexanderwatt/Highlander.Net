using System;

namespace FpML.V5r10.Reporting.Models.Assets
{
    public class BondAssetParameters : IBondAssetParameters
    {
        public string[] Metrics { get; set; }

        #region ISwapAssetParameters Members

        /// <summary>
        /// Flag that sets whether the quote is yield to maturiy or dirty price - as a decimal
        /// </summary>
        public bool IsYTMQuote { get; set; }

        /// <summary>
        /// Flag that sets whether the forst coupon is ex div.
        /// </summary>
        public bool IsExDiv { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        public Decimal Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        /// <value>The quote.</value>
        public decimal Quote { get; set; }

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        public decimal NotionalAmount{ get; set; }

        /// <summary>
        /// Gets or sets the accrued factor.
        /// </summary>
        /// <value>The accrued factor.</value>
        public decimal AccruedFactor { get; set; }

        /// <summary>
        /// Gets or sets the remaining accrued factor.
        /// </summary>
        /// <value>The remaining accrued factor.</value>
        public decimal RemainingAccruedFactor { get; set; }

        /// <summary>
        /// Gets or sets the accrual discount factors.
        /// </summary>
        /// <value>The accrual discount factors.</value>
        public decimal[] AccrualDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the spread leg discount factors.
        /// </summary>
        /// <value>The accrual discount factors.</value>
        public decimal[] SpreadDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the payment discount factors.
        /// </summary>
        /// <value>The payment discount factors.</value>
        public decimal[] PaymentDiscountFactors { get; set; }

        /// <summary>
        /// Gets or sets the weightings.
        /// </summary>
        /// <value>The weightings.</value>
        public Decimal[] Weightings { get; set; }

        /// <summary>
        /// Gets or sets the coupon rate.
        /// </summary>
        /// <value>The coupon rate.</value>
        public decimal CouponRate { get; set; }

        /// <summary>
        /// Gets or sets the deltaR that is used as the denominator in assetswap calculations.
        /// </summary>
        /// <value>The deltaR.</value>
        public decimal SpreadDeltaR { get; set; }

        /// <summary>
        /// Gets or sets the accraul year fractions.
        /// </summary>
        /// <value>The accrual year fractions.</value>
        public decimal[] AccrualYearFractions { get; set; }

        /// <summary>
        /// Gets or sets the spread leg year fractions.
        /// </summary>
        /// <value>The spread leg year fractions.</value>
        public decimal[] SpreadYearFractions { get; set; }

        /// <summary>
        /// Gets or sets the frequency as an int.
        /// </summary>
        /// <value>The frequency.</value>
        public int Frequency { get; set; }

        /// <summary>
        /// THe purchase price as a dirty price.
        /// </summary>
        public decimal PurchasePrice { get; set; }

        #endregion
    }
}