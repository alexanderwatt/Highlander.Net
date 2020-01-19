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

using System;
using Highlander.Reporting.Analytics.V5r3.Options;
using Highlander.Reporting.Analytics.V5r3.Rates;
using Highlander.Reporting.ModelFramework.V5r3;

namespace Highlander.Reporting.Models.V5r3.Rates.FuturesOptions
{
    public class BankBillsFuturesOptionAssetAnalytic : ModelAnalyticBase<IRateFuturesOptionAssetParameters, RateFuturesOptionMetrics>, IRateFuturesOptionAssetResults
    {
        private const Decimal COne = 1.0m;

        /// <summary>
        /// Gets the forward delta.
        /// </summary>
        public decimal ForwardDelta { get; set; }

        /// <summary>
        /// Get the NPV
        /// </summary>
        public Decimal NPV => EvaluateNPV();

        /// <summary>
        /// Gets the npv change from the base NPV.
        /// </summary>
        /// <value>The npv change.</value>
        public decimal NPVChange => EvaluateNPVChange();

        /// <summary>
        /// Gets the option volatility.
        /// </summary>
        /// <value>The option volatility.</value>
        public decimal OptionVolatility => EvaluateOptionVolatility();

        /// <summary>
        /// Gets the spot delta.
        /// </summary>
        /// <value>The spot delta.</value>
        public decimal SpotDelta { get; }

        /// <summary>
        /// Gets the convexity adjustment.
        /// </summary>
        public Decimal ConvexityAdjustment => EvaluateConvexityAdjustment();

        /// <summary>
        /// Gets the adjusted rate.
        /// </summary>
        /// <value>The rate.</value>
        public Decimal AdjustedRate => EvaluateAdjustedRate();

        /// <summary>
        /// Gets the Index At Maturity.
        /// </summary>
        public decimal IndexAtMaturity => EvaluateIndex();

        /// <summary>
        /// Gets the PandL.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal PandL => EvaluatePandL();

        /// <summary>
        /// Gets the initial margin.
        /// </summary>
        /// <value>The inital margin.</value>
        public decimal InitialMargin { get; }

        /// <summary>
        /// Gets the variation margin.
        /// </summary>
        /// <value>The variation margin.</value>
        public decimal VariationMargin { get; }

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

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluateNPV()
        {
            return EvaluateExpectedValue();
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected virtual Decimal EvaluatePremium()
        {
            return CalculateOptionValue();
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateRawValue()//TODO this is incorrect.
        {
            decimal result = CalculateOptionValue();
            return result;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateExpectedValue()//TODO this is incorrect.
        {
            decimal result = AnalyticParameters.NumberOfContracts * 500000 *
                             (EvaluateMarketBillPrice() - EvaluateImpliedBillPrice());
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
            decimal result = EvaluateExpectedValue() * CalculateOptionDelta();
            return result;
        }

        /// <summary>
        /// Evaluates the gamma wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateGamma0()//TODO
        {
            decimal result = EvaluateExpectedValue() * CalculateOptionGamma();
            return result;
        }

        /// <summary>
        /// Evaluates the gamma wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateVega0()//TODO
        {
            decimal result = EvaluateExpectedValue() * CalculateOptionVega();
            return result;
        }

        /// <summary>
        /// Evaluates the gamma wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        protected Decimal EvaluateTheta0()//TODO
        {
            decimal result = EvaluateExpectedValue() * CalculateOptionTheta();
            return result;
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
            return multiplier * EvaluateExpectedValue() * CalculateOptionDelta();
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
                (double)AnalyticParameters.Premium, 0.0, (double)AnalyticParameters.TimeToExpiry);
            return (decimal)result;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateConvexityAdjustment()
        {
            return FuturesAnalytics.FuturesMarginConvexityAdjustment(AnalyticParameters.Rate,
                                                                     (double)AnalyticParameters.TimeToExpiry, (double)AnalyticParameters.Volatility);
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        public virtual Decimal EvaluateOptionVolatility()
        {
            return AnalyticParameters.Volatility;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        public virtual Decimal EvaluateDiscountFactorAtMaturity()
        {
            return AnalyticParameters.StartDiscountFactor / (COne + AnalyticParameters.YearFraction * (AnalyticParameters.Rate - EvaluateConvexityAdjustment()));
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateNPVChange()
        {
            return AnalyticParameters.BaseNPV - EvaluateNPV();
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateIndex()
        {
            return AnalyticParameters.Rate;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluatePandL()
        {
            return AnalyticParameters.BaseNPV - EvaluateNPV();
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateAdjustedRate()
        {
            return EvaluateMarketRate() - EvaluateConvexityAdjustment();
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateMarketRate()
        {
            return AnalyticParameters.Rate;
        }

        /// <summary>
        /// Evaluates the accrual factor
        /// </summary>
        /// <returns></returns>
        public Decimal EvaluateAccrualFactor()
        {
            return 90.0m/365.0m;//Based on a 90 day bill.
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        public Decimal EvaluateImpliedRate()
        {
            var rate = (AnalyticParameters.StartDiscountFactor / AnalyticParameters.EndDiscountFactor - COne) / AnalyticParameters.YearFraction;
            return rate;
        }

        /// <summary>
        /// Evaluates the bill price underlying the future.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateMarketBillPrice()
        {
            return 1.0m / (COne + EvaluateMarketRate() * AnalyticParameters.YearFraction);
        }

        /// <summary>
        /// Evaluates the bill price from the curve.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateImpliedBillPrice()
        {
            return 1.0m / (COne + ImpliedQuote * AnalyticParameters.YearFraction);
        }
    }
}