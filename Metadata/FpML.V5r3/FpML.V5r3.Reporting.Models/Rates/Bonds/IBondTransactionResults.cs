using System;

namespace Orion.Models.Rates.Bonds
{
    public enum BondTransactionMetrics
    {
        MarketQuote
        , BreakEvenRate
        , BreakEvenSpread 
        , ImpliedQuote
        , DirtyPrice 
        , CleanPrice 
        , NPV 
        , AccruedInterest  
        , Convexity 
        , DeltaR 
        , DV01 
        , AssetSwapSpread 
        , ZSpread 
        , YieldToMaturity
    }

    public interface IBondTransactionResults
    {
        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        decimal NPV { get; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        decimal ImpliedQuote { get; }

        /// <summary>
        /// Gets the accrued coupon.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        decimal AccruedInterest { get; }

        /// <summary>
        /// Gets the dirty price.
        /// </summary>
        /// <value>The dirty price.</value>
        decimal DirtyPrice { get; }

        /// <summary>
        /// Gets the clean price.
        /// </summary>
        /// <value>The clean price.</value>
        decimal CleanPrice { get; }

        /// <summary>
        /// Gets the derivative of the price with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        decimal DeltaR { get; }

        /// <summary>
        /// Gets the derivative of the price with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        decimal DV01 { get; }

        /// <summary>
        /// Gets the convexity.
        /// </summary>
        /// <value>The convexity.</value>
        decimal Convexity { get; }

        /// <summary>
        /// Gets the asset swap spread.
        /// </summary>
        /// <value>The asset swap spread.</value>
        decimal AssetSwapSpread { get; }

        /// <summary>
        /// Gets the zero coupon bond swap spread.
        /// </summary>
        /// <value>The zero coupon bond swap spread.</value>
        decimal ZSpread { get; }

        /// <summary>
        /// Gets the yield to maturity.
        /// </summary>
        /// <value>The yield to maturity.</value>
        decimal YieldToMaturity { get; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        decimal MarketQuote { get; }
    }
}