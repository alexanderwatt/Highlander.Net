/*
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

#region Usings

using System;

#endregion

namespace FpML.V5r10.Reporting.Models.Rates.Swaption
{
    public class SwaptionInstrumentResults : ISwaptionInstrumentResults
    {
        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal ImpliedQuote { get; set; }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal MarketQuote { get; set; }

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
        public decimal BreakEvenStrike { get; set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public Decimal NPV { get; set; }

        /// <summary>
        /// Gets the delta.
        /// </summary>
        /// <value>The delta.</value>
        public Decimal Delta { get; set; }

        /// <summary>
        /// Gets the strike delta.
        /// </summary>
        /// <value>The strike delta.</value>
        public Decimal StrikeDelta { get; set; }

        /// <summary>
        /// Gets the fixed leg accrual factor of a fixed/float swap.
        /// </summary>
        /// <value>The fixed leg accrual factor of a fixed/float swap.</value>
        public Decimal FixedLegAccrualFactor { get; set; }
    }
}