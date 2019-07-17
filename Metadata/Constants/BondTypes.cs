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
    /// <summary>
    /// Which bond time variable was last given
    /// </summary>
    public enum BondTimeVar
    {
        /// <summary>
        /// 
        /// </summary>
        BtvNone = 0,

        /// <summary>
        /// 
        /// </summary>
        BtvDeal,

        /// <summary>
        /// 
        /// </summary>
        BtvValue,

        /// <summary>
        /// 
        /// </summary>
        BtvSett
    }

    /// <summary>
    /// Which bond price variable was last given
    /// </summary>
    public enum BondPriceVar
    {
        /// <summary>
        /// 
        /// </summary>
        BpvNone = 0,

        /// <summary>
        /// 
        /// </summary>
        BpvClean,

        /// <summary>
        /// 
        /// </summary>
        BpvDirty,

        /// <summary>
        /// 
        /// </summary>
        BpvYield
    }

    public enum BondTypesEnum
    {
        ///<summary>
        /// Australian Government Bonds
        ///</summary>
        AGB,

        ///<summary>
        /// Australian Corporate Bonds
        ///</summary>
        ACB,

        ///<summary>
        /// German BOBL
        ///</summary>
        BOBL,

        ///<summary>
        ///  German Bund
        ///</summary>
        Bund,

        ///<summary>
        /// British Gilt
        ///</summary>
        Gilt,

        ///<summary>
        /// US  treasury
        ///</summary>
        USTreas,

        ///<summary>
        /// Japanese Government Bond
        ///</summary>
        JGB
    } ;

    public enum MarketSectorEnum
    {
        ///<summary>
        ///</summary>
        Govt,

        ///<summary>
        ///</summary>
        Corp,

        ///<summary>
        ///</summary>
        Mtge,

        ///<summary>
        ///</summary>
        Equity,

        ///<summary>
        ///</summary>
        Comdty,

        ///<summary>
        ///</summary>
        Pfd,

        ///<summary>
        ///</summary>
        Index,

        ///<summary>
        ///</summary>
        Curncy
    }


    public enum PropertyTypeEnum
    {
        ///<summary>
        ///</summary>
        Residential,

        ///<summary>
        ///</summary>
        Commercial,

        ///<summary>
        ///</summary>
        Investment
    } ;

    public enum SecurityTypeEnum
    {
        ///<summary>
        ///</summary>
        Govt,

        ///<summary>
        ///</summary>
        Corp,

        ///<summary>
        ///</summary>
        Mtge,

        ///<summary>
        ///</summary>
        Equity,

        ///<summary>
        ///</summary>
        Comdty,

        ///<summary>
        ///</summary>
        Pfd,

        ///<summary>
        ///</summary>
        Index,

        ///<summary>
        ///</summary>
        Curncy,

        ///<summary>
        ///</summary>
        Muni,

        ///<summary>
        ///</summary>
        Mmkt
    } ;

    public static class BondProp
    {
        public static string DefaultDomain = "Orion.ReferenceData";
        public static string DefaultFunction = "ReferenceData";
        public static string DefaultDataGroup = "Orion.ReferenceData.FixedIncome";
        public static string DefaultSourceSystem = "Orion";
        public static string DefaultType = "Bond";
        public const string Function = "Function";
        public const string DataGroup = "DataGroup";
        public const string Domain = "Domain";
        //public const string SourceSystem = "SourceSystem";
        public const string Currency = "Currency";
        public const string BondType = "BondType";
        public const string Ticker = "Ticker";
        public const string Coupon = "Coupon";//Number, 0, F(loating), V(ariable) or L(oan)
        public const string MaturityDate = "MaturityDate";//MM/DD/YY. Preceded by P if perpetual.
        public const string MarketSector = "MarketSector";//Govt, Corp, Mtge
        //public const string ISIN = "ISIN";
        //public const string CUSIP = "CUSIP";
        //public const string SEDOL = "SEDOL";
        //public const string BSYM = "BSYM";   
        public const string Description = "Description";
        public const string UniqueIdentifier = "UniqueIdentifier";
        public const string AsAtDate = "AsAtDate";
        public const string Issuer = "Issuer";
        public const string CreditSeniority = "CreditSeniority";
        public const string SchemaType = "SchemaType";
        public const string SchemaVersion = "SchemaVersion";
        public const string BusinessDayCalendar = "BusinessDayCalendar";
        public const string BusinessDayAdjustments = "BusinessDayAdjustments";
        public const string ReferenceBond = "ReferenceBond";
    }

    /// <summary>
    /// Which bond price variable was last given
    /// </summary>
    public enum BondPriceEnum
    {
        /// <summary>
        /// 
        /// </summary>
        Accrued,

        /// <summary>
        /// 
        /// </summary>
        CleanPrice,

        /// <summary>
        /// 
        /// </summary>
        DirtyPrice,

        /// <summary>
        /// 
        /// </summary>
        YieldToMaturity,
    }
}