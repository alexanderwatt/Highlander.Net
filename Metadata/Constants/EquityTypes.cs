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

namespace Orion.Constants
{
    public static class EquityProp
    {
        public static string DefaultDomain = "Orion.ReferenceData";
        public static string DefaultFunction = "ReferenceData";
        public static string DefaultDataGroup = "Orion.ReferenceData.Equity";
        public static string DefaultSourceSystem = "Orion";
        public static string DefaultType = "Equity";
        public const string Function = "Function";
        public const string DataGroup = "DataGroup";
        public const string Domain = "Domain";
        public const string SourceSystem = "SourceSystem";
        public const string Currency = "Currency";
        public const string SecurityType = "SecurityType";
        public const string Ticker = "Ticker";
        public const string MarketSector = "MarketSector";//Govt, Corp, Mtge
        public const string ISIN = "ISIN";
        public const string CUSIP = "CUSIP";
        public const string SEDOL = "SEDOL";
        public const string BSYM = "BSYM";   
        public const string Description = "Description";
        public const string UniqueIdentifier = "UniqueIdentifier";
        public const string AsAtDate = "AsAtDate";
        public const string Issuer = "Issuer";
        public const string SchemaType = "SchemaType";
        public const string SchemaVersion = "SchemaVersion";
        public const string Exchange = "Exchange";
        public const string ExchangeMIC = "IDMICPRIMEXCH";
        public const string ReferenceEquity = "ReferenceEquity";
    }

    public static class PropertyProp
    {
        public static string DefaultDomain = "Orion.ReferenceData";
        public static string DefaultFunction = "ReferenceData";
        public static string DefaultDataGroup = "Orion.ReferenceData.Property";
        public static string DefaultSourceSystem = "Orion";
        public static string DefaultType = "Property";
        public const string Function = "Function";
        public const string DataGroup = "DataGroup";
        public const string Domain = "Domain";
        public const string SourceSystem = "SourceSystem";
        public const string Currency = "Currency";
        public const string PropertyType = "PropertyType";
        public const string PropertySector = "PropertySector";//Govt, Corp, Mtge
        public const string Description = "Description";
        public const string UniqueIdentifier = "UniqueIdentifier";
        public const string AsAtDate = "AsAtDate";
        public const string PropertyIndex = "PropertyIndex";
        public const string SchemaType = "SchemaType";
        public const string SchemaVersion = "SchemaVersion";
        public const string Exchange = "Exchange";
        public const string ReferenceProperty = "ReferenceProperty";
    }
}