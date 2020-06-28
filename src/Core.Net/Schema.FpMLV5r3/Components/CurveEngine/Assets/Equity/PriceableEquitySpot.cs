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

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Equity
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableEquitySpot : PriceableEquityAsset
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableEquitySpot"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="position">The underlysing prosiiotn.</param>
        /// <param name="equity">The equity</param>
        /// <param name="settlementCalendar">The settlement Calendar.</param>
        /// <param name="marketQuote">The market quote.</param>
        public PriceableEquitySpot(DateTime baseDate, int position, EquityNodeStruct equity, 
        IBusinessCalendar settlementCalendar, BasicQuotation marketQuote)
            : base(baseDate, position, equity.SettlementDate, settlementCalendar, marketQuote)
        {
            //Issuer = equity.Equity.id;
            IsXD = IsExDiv();
            //EquityCurveName = CurveNameHelpers.GetEquityCurveName(Currency.Value, Issuer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interpolatedSpace"></param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            return Quote.value;//TODO This will only work for ytm quotes.
        }
    }
}