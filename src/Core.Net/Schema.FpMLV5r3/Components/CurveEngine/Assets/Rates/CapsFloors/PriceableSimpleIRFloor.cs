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
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.NamedValues;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates.CapsFloors
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableSimpleIRFloor : PriceableSimpleIRCap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRCap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="interestRateCap">An interest Rate Cap.</param>
        /// <param name="properties">THe properties, including strike information.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="marketQuotes">The market Quote: premium, normal volatility or lognormal volatility.</param>
        public PriceableSimpleIRFloor(DateTime baseDate, SimpleIRCapNodeStruct interestRateCap,
            NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicAssetValuation marketQuotes)
            : base(baseDate, interestRateCap, properties, fixingCalendar, paymentCalendar, marketQuotes)
        {
            IsCap = false;
        }
    }
}