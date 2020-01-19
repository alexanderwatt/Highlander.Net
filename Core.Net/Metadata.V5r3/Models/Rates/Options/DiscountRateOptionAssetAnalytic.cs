#region Using directives

using System;
using nabCap.QR.AnalyticModels.Rates.Options;
using NabCap.QR.ModelFramework;

#endregion

namespace nabCap.QR.AnalyticModels.Rates.Options
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class DiscountRateOptionAssetAnalytic : ModelAnalyticBase<IRateOptionAssetParameters, RateOptionMetrics>, IRateOptionAssetResults
    {
        #region IRateAssetResults Members

        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal NPV
        {
            get { return 0.0m; }
        }

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        /// <value>The accrual factor.</value>
        public decimal AccrualFactor
        {
            get { return 0.0m; }
        }

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public decimal VolatilityAtExpiry
        {
            get { return 0.0m; }
        }

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The quote.</value>
        public Decimal MarketQuote
        {
            get { return AnalyticParameters.PremiumPaymentDiscountFactor; }
        }

        /// <summary>
        /// Gets the Implied Quote.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal ImpliedQuote
        {
            get { return 0.0m; }
        }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal Delta0
        {
            get { return 0.0m; }
        }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal Delta1
        {
            get { return 0.0m; }
        }

        /// <summary>
        /// Gets the second derivative with respect to the Rate.
        /// </summary>
        /// <value>The gamma wrt the forward rate.</value>
        public decimal Gamma0
        {
            get { return 0.0m; }
        }

        /// <summary>
        /// Gets the second derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The gamma wrt the discount rate.</value>
        public decimal Gamma1
        {
            get { return 0.0m; }
        }

        #endregion

        ///// <summary>
        ///// Evaluates the npv.
        ///// </summary>
        ///// <returns></returns>
        //protected virtual Decimal EvaluateNPV()
        //{
        //    return AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction *
        //           (AnalyticParameters.Rate - EvaluateImpliedQuote()) * AnalyticParameters.EndDiscountFactor;
        //}

        ///// <summary>
        ///// Evaluates the delta wrt the fixed rate R.
        ///// </summary>
        ///// <returns></returns>
        //protected virtual Decimal EvaluateDeltaR()
        //{
        //    return AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction *
        //           AnalyticParameters.EndDiscountFactor / 10000;
        //}

        ///// <summary>
        ///// Evaluates the accrual factor.
        ///// </summary>
        ///// <returns></returns>
        //protected virtual Decimal EvaluateAccrualFactor()
        //{
        //    return AnalyticParameters.YearFraction *
        //           AnalyticParameters.EndDiscountFactor;
        //}

        ///// <summary>
        ///// Evaluates the implied quote.
        ///// </summary>
        ///// <returns></returns>
        //protected virtual Decimal EvaluateImpliedQuote()
        //{
        //    return ((AnalyticParameters.StartDiscountFactor / AnalyticParameters.EndDiscountFactor) - cOne) /
        //           AnalyticParameters.YearFraction;
        //}

        ///// <summary>
        ///// Evaluates the discount factor at maturity.
        ///// </summary>
        ///// <returns></returns>
        //protected virtual Decimal EvaluateDiscountFactorAtMaturity()
        //{
        //    return AnalyticParameters.StartDiscountFactor /
        //           (cOne + AnalyticParameters.YearFraction * AnalyticParameters.Rate);
        //}
    }
}