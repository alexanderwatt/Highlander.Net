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

#region Usings

using System;
using Highlander.Reporting.Models.V5r3.Generic.Coupons;

#endregion

namespace Highlander.Reporting.Models.V5r3.Rates
{
    [Serializable]
    public class RateInstrumentResults : CouponResults, IRateInstrumentResults
    {
        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal DeltaR { get; set; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        public decimal LocalCurrencyDeltaR { get; set; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        public decimal DiscountFactorAtMaturity { get; set; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        public decimal HistoricalDeltaR { get; set; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        public decimal LocalCurrencyHistoricalDeltaR { get; set; }

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        public Decimal[] PCE { get; set; }

        /// <summary>
        /// Gets the PCE term.
        /// </summary>
        /// <value>The PCE term.</value>
        public Decimal[] PCETerm { get; set; }

        /// <summary>
        /// Gets the break even stike.
        /// </summary>
        /// <value>The break even strike.</value>
        public Decimal BreakEvenStrike { get; set; }
    }
}
