﻿/*
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
using Highlander.Reporting.Analytics.V5r3.Rates;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.Models.V5r3.Assets;

namespace Highlander.Reporting.Models.V5r3.Rates.Futures
{
    public class NZDFuturesAssetAnalytic : ModelAnalyticBase<IRateFuturesAssetParameters, RateMetrics>, IRateAssetResults
    {
        private const decimal COne = 1.0m;

        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public decimal NPV => EvaluateNPV();

        /// <summary>
        /// Gets the npv change form a base NPV.
        /// </summary>
        /// <value>The npv change.</value>
        public decimal NPVChange => EvaluateNPVChange();

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The quote.</value>
        public decimal ImpliedQuote => EvaluateImpliedQuote();

        /// <summary>
        /// Gets the delta wrt the fixed rate R.
        /// </summary>
        public decimal DeltaR => EvaluateDeltaR();

        /// <summary>
        /// Gets the convexity adjustment.
        /// </summary>
        public decimal AccrualFactor => EvaluateAccrualFactor();

        /// <summary>
        /// Gets the convexity adjustment.
        /// </summary>
        public decimal ConvexityAdjustment => EvaluateConvexityAdjustment(AnalyticParameters.Rate);

        /// <summary>
        /// Gets the adjusted rate.
        /// </summary>
        /// <value>The rate.</value>
        public decimal AdjustedRate => EvaluateAdjustedRate();

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public decimal DiscountFactorAtMaturity => EvaluateDiscountFactorAtMaturity();

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote => EvaluateMarketRate();

        /// <summary>
        /// Gets the Index At Maturity.
        /// </summary>
        public decimal IndexAtMaturity => EvaluateMarketRate();

        /// <summary>
        /// Gets the PandL.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal PandL => 0.0m;

        /// <summary>
        /// Gets the initial margin.
        /// </summary>
        /// <value>The inital margin.</value>
        public decimal InitialMargin => 0.0m;

        /// <summary>
        /// Gets the variation margin.
        /// </summary>
        /// <value>The variation margin.</value>
        public decimal VariationMargin => 0.0m;

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        protected virtual decimal EvaluateNPVChange()
        {
            return EvaluateNPV() - AnalyticParameters.BaseNPV;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private decimal EvaluateImpliedQuote()//TODo the adjustment needs to be at the rate, not the implied rate.
        {
            try
            {
                var result = FuturesAnalytics.FuturesImpliedQuoteFromMarginAdjustedWithArrears(EvaluateImpliedRate(),
                                                                                               (double)AnalyticParameters.YearFraction,
                                                                                               (double)AnalyticParameters.TimeToExpiry,
                                                                                               (double)AnalyticParameters.Volatility);
                return result;
            }
            catch
            {
                throw new Exception("Real solution does not exist");
            }
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private decimal EvaluateConvexityAdjustment(decimal rate)
        {
            return FuturesAnalytics.FuturesMarginWithArrearsConvexityAdjustment(rate,
                                                                                (double)AnalyticParameters.YearFraction,
                                                                                (double)AnalyticParameters.TimeToExpiry,
                                                                                (double)AnalyticParameters.Volatility);
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        public virtual decimal EvaluateDiscountFactorAtMaturity()
        {
            return AnalyticParameters.StartDiscountFactor / (COne + AnalyticParameters.YearFraction * EvaluateAdjustedRate());
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private decimal EvaluateNPV()
        {
            return AnalyticParameters.NumberOfContracts * (AnalyticParameters.Rate - ImpliedQuote) * 250000m;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        private decimal EvaluateAdjustedRate()
        {
            return EvaluateMarketRate() - EvaluateConvexityAdjustment(AnalyticParameters.Rate);
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        private decimal EvaluateDeltaR()//TODO this is not correct.
        {
            return AnalyticParameters.NumberOfContracts * 25m;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        private decimal EvaluateMarketRate()//TODO this is not correct.
        {
            return AnalyticParameters.Rate;
        }

        /// <summary>
        /// Evaluates the accrual factor
        /// </summary>
        /// <returns></returns>
        public decimal EvaluateAccrualFactor()
        {
            return 0.25m;
        }

        /// <summary>
        /// Evaluates the discount factor at maturity.
        /// </summary>
        /// <returns></returns>
        public decimal EvaluateImpliedRate()
        {
            var rate = (AnalyticParameters.StartDiscountFactor / AnalyticParameters.EndDiscountFactor - COne) / AnalyticParameters.YearFraction;
            return rate;
        }
    }
}