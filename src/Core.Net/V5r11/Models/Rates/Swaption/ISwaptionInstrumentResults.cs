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

#endregion

namespace Highlander.Reporting.Models.V5r3.Rates.Swaption
{
    public enum SwaptionInstrumentMetrics { MarketQuote, ImpliedQuote, BreakEvenStrike, PCE, PCETerm, NPV, Delta, StrikeDelta, FixedLegAccrualFactor }

    public interface ISwaptionInstrumentResults
    {
        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        Decimal ImpliedQuote { get; }

        /// <summary>
        /// Gets the market rate/spread, depending on the swap type.
        /// </summary>
        /// <value>The market rate/spread, depending on the swap type.</value>
        Decimal MarketQuote { get; }

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

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The vlaue.</value>
        Decimal NPV { get; }

        /// <summary>
        /// Gets the delta.
        /// </summary>
        /// <value>The delta.</value>
        Decimal Delta { get; }

        /// <summary>
        /// Gets the strike delta.
        /// </summary>
        /// <value>The strike delta.</value>
        Decimal StrikeDelta { get; }

        /// <summary>
        /// Gets the fixed leg accrual factor of a fixed/float swap.
        /// </summary>
        /// <value>The fixed leg accrual factor of a fixed/float swap.</value>
        Decimal FixedLegAccrualFactor { get; }
    }
}