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
    public static class AlgorithmsProp
    {
        public const string GenericName = "Configuration.Algorithm";
        public const string RulePath = "Highlander.Configuration.Algorithm";
        public const string Tolerance = "Tolerance";
        public const string Bootstrapper = "Bootstrapper";
        public const string BootstrapperInterpolation = "BootstrapperInterpolation";
        public const string CurveInterpolation = "CurveInterpolation";
        public const string UnderlyingCurve = "UnderlyingCurve";
        public const string CompoundingFrequency = "CompoundingFrequency";
        public const string ExtrapolationPermitted = "ExtrapolationPermitted";
        public const string DayCounter = "DayCounter";
        public const string Default = "Default";
    }

    public static class IdentifiersProp
    {
        public const string UniqueIdentifier = "UniqueIdentifier";
        public const string Exchange = "Exchange";
        public const string ContractCode = "ContractCode";
        public const string Identifier = "Identifier";
        public const string Usage = "Usage";
    }

    public static class MarketsProp
    {
        public const string Market = "Market";
        public const string MarketDate = "MarketDate";
    }

    public static class PriceQuoteProp
    {
        public const string Absolute = "Absolute";
    }

    //public static class LocationCalendarYearProp//In the process of being deprecated.
    //{
    //    public const string RulePath = "Highlander.ReferenceData.RDMHolidays";
    //    public const string BusinessCenter = "BusinessCenter";
    //    public const string Year = "Year";
    //}

    public static class BusinessCenterCalendarProp
    {
        public const string RulePath = "Highlander.ReferenceData.BusinessCenterHolidays";
        public const string GenericName = "ReferenceData.BusinessCenterHolidays";
        public const string BusinessCenter = "BusinessCenter";
    }

    public static class BusinessCentreDateRulesProp
    {
        public const string RulePath = "Highlander.Configuration.DateRules.BusinessCenterDateRules";
        public const string GenericName = "Configuration.DateRules.BusinessCenterDateRules";
    }

    public static class CentralBankDateRulesProp
    {
        public const string RulePath = "Highlander.Configuration.DateRules.CentralBankDateRules";
        public const string GenericName = "Configuration.DateRules.CentralBankDateRules";
    }

    public static class LastTradingDayDateRulesProp
    {
        public const string RulePath = "Highlander.Configuration.DateRules.LastTradingDayDateRules";
        public const string GenericName = "Configuration.DateRules.LastTradingDayDateRules";
    }

    public static class InstrumentConfigData
    {
        public const string RulePath = "Highlander.Configuration.Instrument";
        public const string GenericName = "Configuration.Instrument";
    }

    public static class FixedIncomeData
    {
        public const string GenericName = "ReferenceData.FixedIncome";
    }

    public static class EquityExchangeData
    {
        public const string GenericName = "Configuration.Exchanges";
    }

    public static class EquityAssetData
    {
        public const string GenericName = "ReferenceData.Equity";
    }
}
