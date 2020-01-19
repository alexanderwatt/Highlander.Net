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

namespace Highlander.CurveEngine.V5r3.Assets.Rates.CapFloorLet
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableDiscountCaplet : PriceableCaplet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableCaplet"/> class.
        /// This is a special case for use with the factry for bootstrapping, as it
        /// uses no calendar logic. This is done by the factory.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">A special class containing all salient data required.</param>
        /// <param name="properties">The properties set This includes strike information.</param>
        /// <param name="fixingCalendar">The fixing/expiry calendar></param>
        /// <param name="paymentCalendar">The paymentCalendar calendar.</param>
        /// <param name="notional">The notional. The default value is 1.00m.</param>
        /// <param name="marketQuotes">The market quotes, including the volatility and possibly the fixed rate as a decimal contained in a basic quotation.</param>
        public PriceableDiscountCaplet(DateTime baseDate, RateOptionNodeStruct nodeStruct, NamedValueSet properties, IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar, Decimal notional, BasicAssetValuation marketQuotes)
            : base(baseDate, nodeStruct, properties, fixingCalendar, paymentCalendar, notional, marketQuotes)
        {
            IsDiscounted = true;
        }    
    }
}
