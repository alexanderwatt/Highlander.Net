namespace Orion.Models.Rates.Bonds
{
    public class BondTransactionParameters : IBondTransactionParameters
    {
        /// <summary>
        /// Gets or sets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public string[] Metrics { get; set; }

        /// <summary>
        /// Flag that sets whether the quote is yield to maturity or dirty price - as a decimal
        /// </summary>
        public bool IsYTMQuote { get; set; }

        /// <summary>
        /// Flag that sets whether the first coupon is ex div.
        /// </summary>
        public bool IsExDiv { get; set; }

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        public decimal Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        /// <value>The quote.</value>
        public decimal Quote { get; set; }

        /// <summary>
        /// Gets or sets the notional.
        /// </summary>
        /// <value>The notional.</value>
        public decimal NotionalAmount { get; set; }

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
        /// The currency
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The reporting currency
        /// </summary>
        public string ReportingCurrency { get; set; }

        /// <summary>
        /// Gets or sets the weightings.
        /// </summary>
        /// <value>The weightings.</value>
        public decimal[] Weightings { get; set; }

        /// <summary>
        /// Gets or sets the coupon rate.
        /// </summary>
        /// <value>The coupon rate.</value>
        public decimal CouponRate { get; set; }

        /// <summary>
        /// Gets or sets the accrual year fractions.
        /// </summary>
        /// <value>The accrual year fractions.</value>
        public decimal[] AccrualYearFractions { get; set; }

        /// <summary>
        /// The market quote.
        /// </summary>
        public decimal MarketQuote { get; set; }

        /// <summary>
        /// Gets or sets the frequency as an int.
        /// </summary>
        /// <value>The frequency.</value>
        public int Frequency { get; set; }

        /// <summary>
        /// Gets or sets the purchase price: dirty price.
        /// </summary>
        /// <value>The frequency.</value>
        public decimal PurchasePrice { get; set; }

        /// <summary>
        /// Is this from the buyers perspective.
        /// </summary>
        public bool IsBuyerInd { get; set; }
        //public decimal FixedRate { get; set; }
        //public decimal DeltaR { get; set; }
        //public decimal HistoricalDeltaR { get; set; }
        //public decimal AccrualFactor { get; set; }
        //public decimal FloatingNPV { get; set; }
        //public decimal NPV { get; set; }
        //public bool IsBuyerInd { get; set; }
        //public bool IsDiscounted { get; set; }
        //public decimal StreamAccrualFactor { get; set; }
        //public decimal StreamNPV { get; set; }
        //public decimal StreamFloatingNPV { get; set; }
        //public decimal TargetNPV { get; set; }

        /// <summary>
        /// Gets/Sets the payment discount fractions to be used for break even rate.
        /// </summary>
        public decimal[] PaymentDiscountFactors { get; set; }
    }
}