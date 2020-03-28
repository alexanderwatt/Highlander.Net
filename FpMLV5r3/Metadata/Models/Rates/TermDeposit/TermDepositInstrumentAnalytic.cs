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
using Highlander.Reporting.ModelFramework.V5r3;

namespace Highlander.Reporting.Models.V5r3.Rates.TermDeposit
{
    public class TermDepositInstrumentAnalytic : ModelAnalyticBase<ITermDepositInstrumentParameters, TermDepositInstrumentMetrics>, ITermDepositInstrumentResults
    {
        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal BreakEvenRate => AnalyticParameters.BreakEvenRate;

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal ImpliedQuote => AnalyticParameters.BreakEvenRate;

        ///// <summary>
        ///// Gets the derivative with respect to the Rate.
        ///// </summary>
        ///// <value>The delta wrt the fixed rate.</value>
        //public decimal DeltaR
        //{
        //    get{ return AnalyticParameters.DeltaR; }
        //}

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public decimal DiscountFactorAtMaturity { get; } = 0.0m;

        ///// <summary>
        ///// Gets the derivative with respect to the Rate.
        ///// </summary>
        ///// <value>The historical delta wrt the fixed rate.</value>
        //public decimal HistoricalDeltaR
        //{
        //    get { return AnalyticParameters.HistoricalDeltaR; }
        //}

        /// <summary>
        /// Gets the break even spread.
        /// </summary>
        /// <value>The break even spread.</value>
        public decimal BreakEvenSpread => AnalyticParameters.BreakEvenSpread;

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        public decimal[] PCE => Array.Empty<decimal>();

        /// <summary>
        /// Gets the PCE term.
        /// </summary>
        /// <value>The PCE term.</value>
        public decimal[] PCETerm => Array.Empty<decimal>();

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote => AnalyticParameters.MarketQuote;
    }
}