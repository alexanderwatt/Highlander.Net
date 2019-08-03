/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using Orion.Analytics.Options;
using Orion.ModelFramework;

#endregion

namespace Orion.Models.Rates.Options
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class SimpleRateOptionAssetAnalytic : ModelAnalyticBase<ISimpleOptionAssetParameters, RateOptionMetrics>, ISimpleRateOptionAssetResults
    {
        private const Decimal COne = 1.0m;

        #region IRateAssetResults Members

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
        /// Gets the second derivative with respect to the Time.
        /// </summary>
        /// <value>The theta wrt the forward rate.</value>
        public decimal Theta0 => EvaluateTheta0();

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public decimal VolatilityAtExpiry => EvaluateVolatilityAtExpiry();

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The quote.</value>
        public Decimal MarketQuote => !AnalyticParameters.IsVolatilityQuote ? AnalyticParameters.Premium : AnalyticParameters.Volatility;

        /// <summary>
        /// Gets the Implied Quote.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal ImpliedQuote => EvaluateImpliedQuote();

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        /// <value>The expected value.</value>
        public decimal ExpectedValue => EvaluateExpectedValue();

        /// <summary>
        /// Gets the forward rate.
        /// </summary>
        /// <value>The forward rate.</value>
        public decimal ForwardRate => EvaluateForwardRate();

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal ImpliedStrike => EvaluateBreakEvenRate();

        /// <summary>
        /// Gets the raw value.
        /// </summary>
        /// <value>The raw value.</value>
        public decimal RawValue => EvaluateRawValue();

        /// <summary>
        /// Gets the $ derivative with respect to the Rate.
        /// </summary>
        /// <value>The $ delta wrt the fixed rate.</value>
        public decimal DeltaR => EvaluateDeltaR();

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
        public decimal Gamma0 => EvaluateGamma0();

        /// <summary>
        /// Gets the second derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The gamma wrt the discount rate.</value>
        public decimal Gamma1 => 0.0m;

        /// <summary>
        /// Gets the first derivative with respect to the Vol.
        /// </summary>
        /// <value>The vega wrt the forward rate.</value>
        public decimal Vega0 => EvaluateVega0();

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
        protected Decimal EvaluateRawValue()//TODO this is incorrect.
        {
            decimal result= CalculateOptionValue();
            return result;
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
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateForwardRate()
        {
            return AnalyticParameters.Rate;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateBreakEvenRate()
        {
            return AnalyticParameters.Rate;
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
                result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * CalculateOptionDelta() * AnalyticParameters.PremiumPaymentDiscountFactor;
            }
            return result;
        }

        /// <summary>
        /// Evaluates the gamma wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateGamma0()//TODO
        {
            decimal result;
            if (AnalyticParameters.IsDiscounted)
            {
                result = -EvaluateExpectedValue() * AnalyticParameters.YearFraction / (COne + CalculateOptionGamma() * AnalyticParameters.YearFraction);
            }
            else
            {
                result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * CalculateOptionGamma() * AnalyticParameters.PremiumPaymentDiscountFactor;
            }
            return result;
        }

        /// <summary>
        /// Evaluates the gamma wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateVega0()//TODO
        {
            decimal result;
            if (AnalyticParameters.IsDiscounted)
            {
                result = -EvaluateExpectedValue() * AnalyticParameters.YearFraction / (COne + CalculateOptionVega() * AnalyticParameters.YearFraction);
            }
            else
            {
                result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * CalculateOptionVega() * AnalyticParameters.PremiumPaymentDiscountFactor;
            }
            return result;
        }

        /// <summary>
        /// Evaluates the gamma wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateTheta0()//TODO
        {
            decimal result;
            if (AnalyticParameters.IsDiscounted)
            {
                result = -EvaluateExpectedValue() * AnalyticParameters.YearFraction / (COne + CalculateOptionTheta() * AnalyticParameters.YearFraction);
            }
            else
            {
                result = AnalyticParameters.NotionalAmount * AnalyticParameters.YearFraction * CalculateOptionTheta() * AnalyticParameters.PremiumPaymentDiscountFactor;
            }
            return result;
        }

        /// <summary>
        /// Evaluates the accrual factor.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateAccrualFactor()
        {
            var accrualFactor= AnalyticParameters.YearFraction *
                               AnalyticParameters.EndDiscountFactor;
            return accrualFactor;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateDeltaR()
        {
            var multiplier = 1.0m;
            if (!AnalyticParameters.IsPut)
            {
                multiplier = -1.0m;
            }
            return multiplier * AnalyticParameters.NotionalAmount * EvaluateAccrualFactor() * CalculateOptionDelta();
        }

        /// <summary>
        /// Evaluates the accrual factor.
        /// </summary>
        /// 
        /// <returns></returns>
        protected virtual Decimal EvaluateVolatilityAtExpiry()
        {
            return AnalyticParameters.IsVolatilityQuote ? AnalyticParameters.Volatility : CalculateVolatility();
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateImpliedQuote()
        {
            return !AnalyticParameters.IsVolatilityQuote ? EvaluatePremium() : CalculateVolatility();
        }

        protected virtual Decimal CalculateOptionValue()
        {
            var result = OptionAnalytics.Opt(!AnalyticParameters.IsPut, (double)AnalyticParameters.Rate, (double)AnalyticParameters.Strike,
                                    (double)AnalyticParameters.Volatility, (double)AnalyticParameters.TimeToExpiry);
            return (decimal)result;
        }

        protected virtual Decimal CalculateOptionDelta()
        {
            var result = OptionAnalytics.OptWithGreeks(!AnalyticParameters.IsPut, (double)AnalyticParameters.Rate, (double)AnalyticParameters.Strike,
                                              (double)AnalyticParameters.Volatility, (double)AnalyticParameters.TimeToExpiry)[1];
            return (decimal)result;
        }

        protected virtual Decimal CalculateOptionGamma()
        {
            var result = OptionAnalytics.OptWithGreeks(!AnalyticParameters.IsPut, (double)AnalyticParameters.Rate, (double)AnalyticParameters.Strike,
                                              (double)AnalyticParameters.Volatility, (double)AnalyticParameters.TimeToExpiry)[2];
            return (decimal)result;
        }

        protected virtual Decimal CalculateOptionVega()
        {
            var result = OptionAnalytics.OptWithGreeks(!AnalyticParameters.IsPut, (double)AnalyticParameters.Rate, (double)AnalyticParameters.Strike,
                                              (double)AnalyticParameters.Volatility, (double)AnalyticParameters.TimeToExpiry)[3];
            return (decimal)result;
        }

        protected virtual Decimal CalculateOptionTheta()
        {
            var result = OptionAnalytics.OptWithGreeks(!AnalyticParameters.IsPut, (double)AnalyticParameters.Rate, (double)AnalyticParameters.Strike,
                                              (double)AnalyticParameters.Volatility, (double)AnalyticParameters.TimeToExpiry)[4];
            return (decimal)result;
        }

        protected virtual Decimal CalculateVolatility()
        {
            var result = OptionAnalytics.OptSolveVol(!AnalyticParameters.IsPut, (double)AnalyticParameters.Rate, (double)AnalyticParameters.Strike,
                                              (double)AnalyticParameters.Premium / (double)AnalyticParameters.PremiumPaymentDiscountFactor, 0.0, (double)AnalyticParameters.TimeToExpiry);
            return (decimal)result;
        }
    }
}