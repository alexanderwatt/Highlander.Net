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

namespace Orion.Constants
{
    public enum ExchangeContractTypeEnum
    {
        ///<summary>
        ///</summary>
        FxFuture,

        ///<summary>
        ///</summary>
        IRFuture,

        ///<summary>
        ///</summary>
        IRFutureOption,

        ///<summary>
        ///</summary>
        CommodityFuture,

        ///<summary>
        ///</summary>
        CommodityFutureSpread
    }

    public static class FuturesProp
    {
        public static string DefaultDomain = "Orion.Configuration";
        public static string DefaultFunction = "Configuration";
        public static string DefaultDataGroup = "Orion.Configuration.Instruments";
        public static string DefaultSourceSystem = "Orion";
        public static string DefaultType = "Future";
        public const string Function = "Function";
        public const string DataGroup = "DataGroup";
        public const string Domain = "Domain";
        public const string Currency = "Currency";
        public const string FuturesType = "FuturesType";
        public const string Ticker = "Ticker";
        public const string ExpiryDate = "ExpiryDate";//MM/DD/YY. Preceded by P if perpetual.
        public const string MarketSector = "MarketSector";//Govt, Corp, Mtge  
        public const string Description = "Description";
        public const string UniqueIdentifier = "UniqueIdentifier";
        public const string AsAtDate = "AsAtDate";
        public const string Exchange = "Exchange";
        public const string ExchangeCountry = "ExchangeCountry";
        public const string SchemaType = "SchemaType";
        public const string SchemaVersion = "SchemaVersion";
        public const string BusinessDayCalendar = "BusinessDayCalendar";
        public const string BusinessDayAdjustments = "BusinessDayAdjustments";
    }

    /// <summary>
    /// Which futures price variable was last given
    /// </summary>
    public enum FuturesPriceEnum
    {
        /// <summary>
        /// 
        /// </summary>
        Price
    }
}