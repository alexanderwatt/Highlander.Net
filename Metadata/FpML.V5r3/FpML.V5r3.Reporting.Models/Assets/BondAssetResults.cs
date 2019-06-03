namespace Orion.Models.Assets
{
    public class BondAssetResults : IBondAssetResults
    {
        /// <summary>
        /// Gets the forward delta the fixed rate.
        /// </summary>
        public decimal ForwardDelta { get; set; }

        #region Implementation of IBondAssetResults

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal NPV{ get; set; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal ImpliedQuote { get; set; }

        /// <summary>
        /// Gets the accrued coupon.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal AccruedInterest { get; set; }

        /// <summary>
        /// Gets the dirty price.
        /// </summary>
        /// <value>The dirty price.</value>
        public decimal DirtyPrice { get; set; }

        /// <summary>
        /// Gets the clean price.
        /// </summary>
        /// <value>The clean price.</value>
        public decimal CleanPrice { get; set; }

        /// <summary>
        /// Gets the derivative of the price with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal DeltaR { get; set; }

        /// <summary>
        /// Gets the derivative of the price with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal DV01 { get; set; }

        /// <summary>
        /// Gets the convexity.
        /// </summary>
        /// <value>The convexity.</value>
        public decimal Convexity { get; set; }

        /// <summary>
        /// Gets the asset swap spread.
        /// </summary>
        /// <value>The asset swap spread.</value>
        public decimal AssetSwapSpread { get; set; }

        /// <summary>
        /// Gets the zero coupon bond swap spread.
        /// </summary>
        /// <value>The zero coupon bond swap spread.</value>
        public decimal ZSpread { get; set; }

        /// <summary>
        /// Gets the yield to maturity.
        /// </summary>
        /// <value>The yield to maturity.</value>
        public decimal YieldToMaturity { get; set; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote { get; set; }

        /// <summary>
        /// Gets the PAndL
        /// </summary>
        public decimal PandL { get; set; }

        #endregion
    }
}