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

namespace Highlander.Constants
{
    public enum StrikeQuoteUnitsEnum
    {
        ///<summary>
        ///</summary>
        DecimalRate,

        ///<summary>
        ///</summary>
        ATMMoneyness,

        ///<summary>
        ///</summary>
        ATMFlatMoneyness,

        ///<summary>
        ///</summary>
        ForwardDelta,

        ///<summary>
        ///</summary>
        SpotDelta
    }

    public enum MeasureTypesEnum
    {
        ///<summary>
        ///</summary>
        Volatility,

        ///<summary>
        ///</summary>
        Premium
    }

    public enum QuoteUnitsEnum
    {
        ///<summary>
        ///</summary>
        LogNormalVolatility,

        ///<summary>
        ///</summary>
        NormalVolatility,

        /// <summary>
        /// 
        /// </summary>
        PercentOfForwardNotional
    }
}