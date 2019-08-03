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
using Orion.ModelFramework;

namespace Orion.Models.Futures
{
    /// <summary>
    /// Base Rate Asset Analytic
    /// </summary>
    public class FuturesOptionAssetAnalytic : ModelAnalyticBase<IFuturesOptionAssetParameters, FuturesOptionMetrics>, IFuturesOptionAssetResults
    {
        public FuturesOptionAssetAnalytic()
        {}

        public FuturesOptionAssetAnalytic(decimal initialMargin)
        {
            InitialMargin = initialMargin;
        }

        #region IFuturesAssetResults Members

        /// <summary>
        /// Gets the npv change from the base NPV.
        /// </summary>
        /// <value>The npv change.</value>
        public decimal NPVChange => EvaluateNPVChange();

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public Decimal ImpliedQuote => AnalyticParameters.Quote;

        /// <summary>
        /// Gets the forward delta.
        /// </summary>
        public decimal ForwardDelta { get; set; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        public Decimal NPV => EvaluateNPV();

        /// <summary>
        /// Gets the profit based on the actual purchase price.
        /// </summary>
        public Decimal PandL => EvaluatePandL();

        /// <summary>
        /// 
        /// </summary>
        public decimal InitialMargin { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal VariationMargin => EvaluatePandL();

        /// <summary>
        /// Gets the index
        /// </summary>
        public decimal IndexAtMaturity => AnalyticParameters.FuturesPrice;

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public Decimal MarketQuote => AnalyticParameters.Quote;

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateNPV()
        {
            var dp = AnalyticParameters.NumberOfContracts * AnalyticParameters.AccrualPeriod * (AnalyticParameters.TradePrice - AnalyticParameters.Quote);
            return AnalyticParameters.Multiplier * dp * AnalyticParameters.SettlementDiscountFactor * AnalyticParameters.ContractNotional;
        }

        /// <summary>
        /// Evaluates the npv.
        /// </summary>
        /// <returns></returns>
        private Decimal EvaluateNPVChange()
        {
            var dp = EvaluateNPV();
            return AnalyticParameters.BaseNPV - dp;
        }

        private Decimal EvaluatePandL()
        {
            //This does not discount the profit.
            var pl = AnalyticParameters.NumberOfContracts * AnalyticParameters.AccrualPeriod * (AnalyticParameters.TradePrice - AnalyticParameters.Quote);
            return AnalyticParameters.Multiplier * pl * AnalyticParameters.SettlementDiscountFactor * AnalyticParameters.ContractNotional;
        }

        /// <summary>
        /// Gets the strike.
        /// </summary>
        public decimal Strike => AnalyticParameters.Strike;

        /// <summary>
        /// Gets the strike.
        /// </summary>
        public decimal ImpliedStrike => AnalyticParameters.Strike;

        /// <summary>
        /// Gets the option volatility.
        /// </summary>
        /// <value>The option volatility.</value>
        public decimal OptionVolatility => AnalyticParameters.Quote;

        /// <summary>
        /// Gets the spot delta.
        /// </summary>
        /// <value>The spot delta.</value>
        public decimal SpotDelta { get; }

        #endregion
    }
}