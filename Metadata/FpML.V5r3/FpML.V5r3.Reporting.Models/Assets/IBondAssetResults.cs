using System;

namespace Orion.Models.Assets
{
    public enum BondMetrics
    {
        DirtyPrice, 
        CleanPrice, 
        NPV, 
        AccruedInterest, 
        ImpliedQuote, 
        Convexity, 
        DeltaR, 
        DV01, 
        MarketQuote, 
        AssetSwapSpread, 
        ZSpread, 
        YieldToMaturity,
        PandL
    }

    public interface IBondAssetResults
    {
        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal NPV { get; }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        Decimal ImpliedQuote { get; }
    
        /// <summary>
        /// Gets the accrued coupon.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        Decimal AccruedInterest{ get; }

        /// <summary>
        /// Gets the dirty price.
        /// </summary>
        /// <value>The dirty price.</value>
        Decimal DirtyPrice { get; }

        /// <summary>
        /// Gets the clean price.
        /// </summary>
        /// <value>The clean price.</value>
        Decimal CleanPrice { get; }

        /// <summary>
        /// Gets the derivative of the price with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        Decimal DeltaR { get; }

        /// <summary>
        /// Gets the derivative of the price with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        Decimal DV01 { get; }

        /// <summary>
        /// Gets the convexity.
        /// </summary>
        /// <value>The convexity.</value>
        Decimal Convexity { get; }

        /// <summary>
        /// Gets the asset swap spread.
        /// </summary>
        /// <value>The asset swap spread.</value>
        Decimal AssetSwapSpread { get; }

        /// <summary>
        /// Gets the zero coupon bond swap spread.
        /// </summary>
        /// <value>The zero coupon bond swap spread.</value>
        Decimal ZSpread { get; }

        /// <summary>
        /// Gets the yield to maturity.
        /// </summary>
        /// <value>The yield to maturity.</value>
        Decimal YieldToMaturity { get; }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        Decimal MarketQuote { get; }

        /// <summary>
        /// Gets the PandL.
        /// </summary>
        /// <value>The market quote.</value>
        Decimal PandL { get; }
    }
}