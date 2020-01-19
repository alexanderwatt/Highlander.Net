namespace Highlander.Reporting.Models.V5r3.Rates.Bonds
{
    public interface IBondTransactionParameters
    {
        /// <summary>
        /// Flag that sets whether the quote is yield to maturity or dirty price - as a decimal
        /// </summary>
        bool IsYTMQuote { get; set; }

        /// <summary>
        /// Flag that sets whether the first coupon is ex div.
        /// </summary>
        bool IsExDiv { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        decimal Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        /// <value>The quote.</value>
        decimal Quote { get; set; }

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        decimal NotionalAmount { get; set; }

        /// <summary>
        /// Gets or sets the accrued factor.
        /// </summary>
        /// <value>The accrued factor.</value>
        decimal AccruedFactor { get; set; }

        /// <summary>
        /// Gets or sets the remaining accrued factor.
        /// </summary>
        /// <value>The remaining accrued factor.</value>
        decimal RemainingAccruedFactor { get; set; }

        /// <summary>
        /// The currency
        /// </summary>
        string Currency { get; set; }

        /// <summary>
        /// The reporting currency
        /// </summary>
        string ReportingCurrency { get; set; }

        /// <summary>
        /// Gets or sets the weightings.
        /// </summary>
        /// <value>The weightings.</value>
        decimal[] Weightings { get; set; }

        /// <summary>
        /// Gets or sets the coupon rate.
        /// </summary>
        /// <value>The coupon rate.</value>
        decimal CouponRate { get; set; }

        /// <summary>
        /// Gets or sets the accrual year fractions.
        /// </summary>
        /// <value>The accrual year fractions.</value>
        decimal[] AccrualYearFractions { get; set; }

        /// <summary>
        /// The market quote
        /// </summary>
        decimal MarketQuote { get; set; }

        /// <summary>
        /// Gets or sets the frequency as an int.
        /// </summary>
        /// <value>The frequency.</value>
        int Frequency { get; set; }

        /// <summary>
        /// Gets or sets the purchase price: dirty price.
        /// </summary>
        /// <value>The frequency.</value>
        decimal PurchasePrice { get; set; }

        //decimal DeltaR { get; set; }

        //decimal HistoricalDeltaR { get; set; }

        //decimal AccrualFactor { get; set; }

        //decimal FloatingNPV  { get; set; }

        //decimal NPV  { get; set; }

        /// <summary>
        /// Is this from the buyers perspective.
        /// </summary>
        bool IsBuyerInd { get; set; }

        //bool IsDiscounted { get; set; }

        //decimal StreamAccrualFactor { get; set; }

        //decimal StreamNPV { get; set; }

        //decimal StreamFloatingNPV { get; set; }

        //decimal TargetNPV { get; set; }

        /// <summary>
        /// Gets/Sets the payment discount fractions to be used for break even rate.
        /// </summary>
        decimal[] PaymentDiscountFactors { get; set; }
    }
}