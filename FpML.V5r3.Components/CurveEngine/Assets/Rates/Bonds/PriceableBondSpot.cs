/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using Orion.Constants;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableBondSpot : PriceableBondAsset
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleBond"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The bond nodeStruct</param>
        /// <param name="settlementCalendar">The settlement Calendar.</param>
        /// <param name="marketQuote">The market quote.</param>
        /// <param name="quoteType">THe quote Type</param>
        public PriceableBondSpot(DateTime baseDate, BondNodeStruct nodeStruct, IBusinessCalendar settlementCalendar, BasicQuotation marketQuote, BondPriceEnum quoteType)
            : base(baseDate, nodeStruct.Bond.faceAmount, nodeStruct.Bond.currency, nodeStruct.SettlementDate, nodeStruct.ExDivDate, nodeStruct.BusinessDayAdjustments, marketQuote, quoteType)
        {
            Id = nodeStruct.Bond.id;
            SettlementDateCalendar = settlementCalendar;    
            //Get the settlement date
            SettlementDate = GetSettlementDate(baseDate, settlementCalendar, nodeStruct.SettlementDate);
            MaturityDate = SettlementDate; 
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

        /// <summary>
        /// Gets the adjusted termination date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return MaturityDate;
        }
    }
}