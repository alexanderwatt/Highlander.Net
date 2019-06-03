#region Using directives

using System;
using Orion.Models.Rates.Options;
using Orion.Analytics.Options;
using Orion.ModelFramework;

#endregion

namespace Orion.Models.ForeignExchange
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class FxOptionAssetAnalytic : ModelAnalyticBase<ISimpleOptionAssetParameters, FxOptionMetrics>, IFxOptionAssetResults
    {
        private const Decimal COne = 1.0m;

        #region IFXAssetResults Members

        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal NPV => EvaluateNPV();

        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal Premium => EvaluatePremium();

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public decimal VolatilityAtExpiry => EvaluateVolatilityAtExpiry();

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The quote.</value>
        public Decimal MarketQuote => !AnalyticParameters.IsVolatiltiyQuote ? AnalyticParameters.Premium : AnalyticParameters.Volatility;

        /// <summary>
        /// Gets the Implied Quote.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal ImpliedQuote => EvaluateImpliedQuote();

        /// <summary>
        /// Gets the accrual factor.
        /// </summary>
        public decimal AccrualFactor => EvaluateAccrualFactor();

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal Delta0 => EvaluateDelta0();

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal Delta1 => 0.0m;

        /// <summary>
        /// Gets the second derivative with respect to the Rate.
        /// </summary>
        /// <value>The gamma wrt the forward rate.</value>
        public decimal Gamma0 => 0.0m;

        /// <summary>
        /// Gets the second derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The gamma wrt the discount rate.</value>
        public decimal Gamma1 => 0.0m;

        #endregion

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateNPV()
        {
            return EvaluateExpectedValue() * AnalyticParameters.PremiumPaymentDiscountFactor;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluatePremium()
        {
            return CalculateOptionValue() * AnalyticParameters.PremiumPaymentDiscountFactor;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateExpectedValue()//TODO this is incorrect.
        {
            decimal result;
            if (AnalyticParameters.IsDiscounted)
            {
                result = AnalyticParameters.NotionalAmount - AnalyticParameters.NotionalAmount / (COne + CalculateOptionValue() * AnalyticParameters.YearFraction);
            }
            else
            {
                result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * CalculateOptionValue();
            }
            return result;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateDelta0()//TODO
        {
            decimal result;
            if (AnalyticParameters.IsDiscounted)
            {
                result = -EvaluateExpectedValue() * AnalyticParameters.YearFraction / (COne + CalculateOptionDelta() * AnalyticParameters.YearFraction);
            }
            else
            {
                result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * AnalyticParameters.PremiumPaymentDiscountFactor;
            }
            return result;
        }

        /// <summary>
        /// Evaluates the accrual factor.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateAccrualFactor()
        {
            return AnalyticParameters.YearFraction *
                   AnalyticParameters.EndDiscountFactor;
        }/// <summary>
        /// Evaluates the accrual factor.
        /// </summary>
        /// 
        /// <returns></returns>
        protected virtual Decimal EvaluateVolatilityAtExpiry()
        {
            return AnalyticParameters.IsVolatiltiyQuote ? AnalyticParameters.Volatility : CalculateVolatility();
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateImpliedQuote()
        {
            return !AnalyticParameters.IsVolatiltiyQuote ? EvaluatePremium() : CalculateVolatility();
        }

        protected virtual Decimal CalculateOptionValue()
        {
            var result = OptionAnalytics.Opt(AnalyticParameters.IsPut, (double)AnalyticParameters.Rate, (double)AnalyticParameters.Strike,
                                             (double)AnalyticParameters.Volatility, (double)AnalyticParameters.TimeToExpiry);
            //result = BlackModel.GetPutOptionValue(AnalyticParameters.ForwardRate, AnalyticParameters.Strike, AnalyticParameters.Volatility, AnalyticParameters.CurveYearFraction);
            return (decimal)result;
        }

        protected virtual Decimal CalculateOptionDelta()
        {
            var result = OptionAnalytics.OptWithGreeks(AnalyticParameters.IsPut, (double)AnalyticParameters.Rate, (double)AnalyticParameters.Strike,
                                                       (double)AnalyticParameters.Volatility, (double)AnalyticParameters.TimeToExpiry)[1];
            //result = BlackModel.GetPutOptionValue(AnalyticParameters.ForwardRate, AnalyticParameters.Strike, AnalyticParameters.Volatility, AnalyticParameters.CurveYearFraction);
            return (decimal)result;
        }

        protected virtual Decimal CalculateVolatility()
        {
            var result = OptionAnalytics.OptSolveVol(AnalyticParameters.IsPut, (double)AnalyticParameters.Rate, (double)AnalyticParameters.Strike,
                                                     (double)AnalyticParameters.Premium / (double)AnalyticParameters.PremiumPaymentDiscountFactor, 0.0, (double)AnalyticParameters.TimeToExpiry);
            //result = BlackModel.GetPutOptionValue(AnalyticParameters.ForwardRate, AnalyticParameters.Strike, AnalyticParameters.Volatility, AnalyticParameters.CurveYearFraction);
            return (decimal)result;
        }

    }
}