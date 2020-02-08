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

namespace Highlander.Reporting.Models.V5r3.Rates.Fra
{
    public enum FraInstrumentMetrics { MarketQuote, BreakEvenRate, BreakEvenSpread, ImpliedQuote, PCE, PCETerm }

    public interface IFraInstrumentResults
    {
        ///// <summary>
        ///// Gets the break even rate.
        ///// </summary>
        ///// <value>The break even rate.</value>
        //Decimal BreakEvenRate { get; }

        ///// <summary>
        ///// Gets the break even spread.
        ///// </summary>
        ///// <value>The break even spread.</value>
        //Decimal BreakEvenSpread { get; }

        ///// <summary>
        ///// Gets the break even rate.
        ///// </summary>
        ///// <value>The break even rate.</value>
        //Decimal ImpliedQuote { get; }

        ///// <summary>
        ///// Gets the market rate.
        ///// </summary>
        ///// <value>The market rate.</value>
        //Decimal MarketQuote { get;}

        ///// <summary>
        ///// Gets the PCE.
        ///// </summary>
        ///// <value>The PCE.</value>
        //Decimal[] PCE { get; }

        ///// <summary>
        ///// Gets the PCE term.
        ///// </summary>
        ///// <value>The PCE term.</value>
        //int[] PCETerm { get; }
  
    }
}