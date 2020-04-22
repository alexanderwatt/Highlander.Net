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

using System;

namespace Highlander.Constants
{
    [Serializable]
    public static class EquityProp
    {
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

    [Serializable]
    public static class PropertyProp
    {
        public static string DefaultType = "Property";
        public const string Function = "Function";
        public const string DataGroup = "DataGroup";
        public const string Domain = "Domain";
        public const string SourceSystem = "SourceSystem";
        public const string Currency = "Currency";
        public const string PropertyType = "PropertyType";
        public const string PropertyIdentifier = "PropertyIdentifier";
        public const string PropertySector = "PropertySector";
        public const string Description = "Description";
        public const string UniqueIdentifier = "UniqueIdentifier";
        public const string AsAtDate = "AsAtDate";
        public const string PropertyIndex = "PropertyIndex";
        public const string City = "City";
        public const string PostCode = "PostCode";
        public const string ShortName = "ShortName";
        public const string SchemaType = "SchemaType";
        public const string SchemaVersion = "SchemaVersion";
        public const string BusinessDayCalendar = "BusinessDayCalendar";
        public const string BusinessDayAdjustments = "BusinessDayAdjustments";
    }

    [Serializable]
    public static class LeaseProp
    {
        public static string DefaultType = "Property";
        public const string Function = "Function";
        public const string DataGroup = "DataGroup";
        public const string Domain = "Domain";
        public const string SourceSystem = "SourceSystem";
        public const string Currency = "Currency";
        public const string Description = "Description";
        public const string UniqueIdentifier = "UniqueIdentifier";
        public const string AsAtDate = "AsAtDate";
        public const string SchemaType = "SchemaType";
        public const string SchemaVersion = "SchemaVersion";
        public const string Tenant = "Tenant";
        public const string ReferenceProperty = "ReferenceProperty";
        public const string UpfrontAmount = "UpfrontAmount";
        public const string NextReviewDate = "NextReviewDate";
        public const string GrossAmount = "GrossAmount";
        public const string PaymentFrequency = "PaymentFrequency";
        public const string PaymentDate = "PaymentDate";
        public const string ReviewChange = "ReviewChange";
        public const string ShopNumber = "ShopNumber";
        public const string UnitsOfArea = "UnitsOfArea";
        public const string ReviewFrequency = "ReviewFrequency";
        public const string LeaseType = "LeaseType";
        public const string LeaseTenor = "LeaseTenor";
        public const string Maturity = "Maturity";
        public const string LeaseIdentifier = "LeaseIdentifier";
        public const string Area = "Area";
        public const string StepUp = "StepUp";
        public const string ReferencePropertyIdentifier = "ReferencePropertyIdentifier";
        public const string BusinessDayCalendar = "BusinessDayCalendar";
        public const string BusinessDayAdjustments = "BusinessDayAdjustments";
        public const string RollConvention = "RollConvention";
    }
}