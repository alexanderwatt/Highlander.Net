﻿/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.Models.Assets;
using Highlander.Numerics.Rates;

namespace FpML.V5r10.Reporting.Models.Rates.Futures
{
    public class CashFuturesAssetAnalytic : ModelAnalyticBase<IRateFuturesAssetParameters, RateMetrics>, IRateAssetResults
    {
        private const Decimal COne = 1.0m;

        /// <summary>
        /// Gets the NPV.
        /// </summary>
        /// <value>The NPV.</value>
        public Decimal NPV => EvaluateNPV();

        /// <summary>
        /// Gets the npv change form a base NPV.
        /// </summary>
        /// <value>The npv change.</value>
        public Decimal NPVChange => EvaluateNPVChange();

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote => EvaluateImpliedQuote();

        /// <summary>
        /// Gets the delta wrt the fixed rate R.
        /// </summary>
        /// <value>The delta wrt to R.</value>
        public Decimal DeltaR => EvaluateDeltaR();

        /// <summary>
        /// Eurodollar accrual factor.
        /// </summary>
        public Decimal AccrualFactor => EvaluateAccrualFactor();

        /// <summary>
        /// Gets the convexity adjustment;
        /// </summary>
        public Decimal ConvexityAdjustment => EvaluateConvexityAdjustment(AnalyticParameters.Rate);

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public Decimal DiscountFactorAtMaturity => EvaluateDiscountFactorAtMaturity();

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote => EvaluateMarketRate();

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
        /// Gets the intial margin.
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
        protected virtual Decimal EvaluateNPVChange()
        {
            return EvaluateNPV() - AnalyticParameters.BaseNPV;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateDeltaR()//TODO this is not correct.
        {
            return AnalyticParameters.NumberOfContracts * 24.66m;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateImpliedQuote()
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
                throw new System.Exception("Real solution does not exist");
            }
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateConvexityAdjustment(Decimal rate)
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
        public virtual Decimal EvaluateDiscountFactorAtMaturity()
        {
            return AnalyticParameters.StartDiscountFactor / (COne + AnalyticParameters.YearFraction * EvaluateAdjustedRate());
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
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateMarketRate()//TODO this is not correct.
        {
            return AnalyticParameters.Rate;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateNPV()
        {
            return AnalyticParameters.NumberOfContracts * (AnalyticParameters.Rate - ImpliedQuote) * 246600m;

        }

        /// <summary>
        /// Evaluates the accrual factor
        /// </summary>
        /// <returns></returns>
        public Decimal EvaluateAccrualFactor()
        {
            return 0.2466m;
        }

        /// <summary>
        /// Evaluates the delta wrt the fixed rate R.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateAdjustedRate()
        {
            return EvaluateMarketRate() - EvaluateConvexityAdjustment(AnalyticParameters.Rate);
        }
    }
}