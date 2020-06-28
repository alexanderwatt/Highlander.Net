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

using Highlander.Reporting.Models.V5r3.ForeignExchange.FxLeg;

#endregion

namespace Highlander.Reporting.Models.V5r3.ForeignExchange.FxOptionLeg
{
    public enum FxOptionLegInstrumentMetrics
    {
        BaseCurrencyNPV
        , ForeignCurrencyNPV
    }

    public interface IFxOptionLegInstrumentResults : IFxLegInstrumentResults
    {
        ///// <summary>
        ///// Gets the NPV in reporting currency.
        ///// </summary>
        ///// <value>The NPV.</value>
        //Decimal NPV { get; }

        ///// <summary>
        ///// Gets the base currency NPV.
        ///// </summary>
        ///// <value>The base currency NPV.</value>
        //Decimal BaseCurrencyNPV { get; }

        ///// <summary>
        ///// Gets the foreign currency NPV.
        ///// </summary>
        ///// <value>The foreign currency NPV.</value>
        //Decimal ForeignCurrencyNPV { get; }
    }
}