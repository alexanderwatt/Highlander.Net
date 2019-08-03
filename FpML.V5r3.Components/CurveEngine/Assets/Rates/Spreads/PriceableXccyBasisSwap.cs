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
using FpML.V5r3.Reporting;
using Orion.ModelFramework;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Builds a cross currency basis swap.
    /// </summary>
    public class PriceableXccyBasisSwap : PriceableBasisSwap
    {
        #region Properties

        /// <summary>
        /// The today fx rate
        /// </summary>
        public Decimal FxRate { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableXccyBasisSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The nodeStruct.</param>
        /// <param name="fixingCalendar">The fixing Calendar</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="spread">The spread.</param>
        public PriceableXccyBasisSwap(DateTime baseDate, BasisSwapNodeStruct nodeStruct,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, BasicQuotation spread)
            : base(baseDate, nodeStruct, fixingCalendar, paymentCalendar, spread)
        {}
    }
}