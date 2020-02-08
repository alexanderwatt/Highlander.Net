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
    public interface IRateInstrumentResults : ICouponResults
    {
        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        Decimal BreakEvenRate { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        Decimal DeltaR { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The delta wrt the fixed rate.</value>
        Decimal LocalCurrencyDeltaR { get; }

        /// <summary>
        /// Gets the discount factor at maturity.
        /// </summary>
        /// <value>The discount factor at maturity.</value>
        Decimal DiscountFactorAtMaturity { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        Decimal HistoricalDeltaR { get; }

        /// <summary>
        /// Gets the derivative with respect to the Rate.
        /// </summary>
        /// <value>The historical delta wrt the fixed rate.</value>
        Decimal LocalCurrencyHistoricalDeltaR { get; }

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        Decimal[] PCE { get; }

        /// <summary>
        /// Gets the PCE term.
        /// </summary>
        /// <value>The PCE term.</value>
        Decimal[] PCETerm { get; }

        /// <summary>
        /// Gets the break even stike.
        /// </summary>
        /// <value>The break even strike.</value>
        Decimal BreakEvenStrike { get; }
    }
}
