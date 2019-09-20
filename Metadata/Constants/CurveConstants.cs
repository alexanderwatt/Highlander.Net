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
    public static class CurveProp
    {
        public const string MarketAndDate = "MarketName";
        public const string Market = "Market";
        public const string MarketDate = "MarketDate";
        public const string MarketDateFormat = "yyyy-MM-dd";
        public const string StressName = "StressName";
        public const string PricingStructureType = "PricingStructureType";
        public const string CurveName = "CurveName";
        public const string BaseCurveType = "BaseCurveType";
        public const string Function = "Function";
        public const string DataGroup = "DataGroup";
        public const string SourceSystem = "SourceSystem";
        public const string Currency1 = "Currency";
        public const string Currency2 = "Currency2";
        public const string CurrencyPair = "CurrencyPair";
        public const string QuoteBasis = "QuoteBasis";
        public const string Algorithm = "Algorithm";
        public const string Type = "Type";
        public const string BaseDate = "BaseDate";
        public const string UniqueIdentifier = "UniqueIdentifier";
        public const string BuildDateTime = "BuildDateTime";
        public const string IndexName = "IndexName";
        public const string IndexTenor = "IndexTenor";
        public const string CreditInstrumentId = "CreditInstrumentId";
        public const string CreditSeniority = "CreditSeniority";
        public const string ReferenceCurveName = "ReferenceCurveName";
        public const string ReferenceCurveUniqueId = "ReferenceCurveUniqueId";
        public const string ReferenceFxCurveName = "ReferenceFxCurveName";
        public const string ReferenceFxCurveUniqueId = "ReferenceFxCurveUniqueId";
        public const string ReferenceFxCurve2Name = "ReferenceFxCurve2Name";
        public const string ReferenceFxCurve2UniqueId = "ReferenceFxCurve2UniqueId";
        public const string ReferenceCurrency2CurveName = "ReferenceCurrency2CurveName";
        public const string ReferenceCurrency2CurveId = "ReferenceCurrency2CurveId";
        public const string Tolerance = "Tolerance";
        public const string OptimizeBuild = "OptimizeBuild";
        public const string ReferenceBond = "ReferenceBond";
        public const string BootStrap = "BootStrap";
        public const string CurveType = "CurveType";
        public const string Identifier = "Identifier";
        public const string AssetClass = "AssetClass";
        public const string ContractCode = "ContractCode";
        public const string ExchangeContractType = "ExchangeContractType";
        public const string Instrument = "Instrument";
        public const string Exchange = "Exchange";
        public const string AssetId = "AssetId";
        public const string AssetType = "AssetType";
        public const string ValuationDate = "ValuationDate";
        public const string StrikeQuoteUnits = "StrikeQuoteUnits";
        public const string MeasureType = "MeasureType";
        public const string QuoteUnits = "QuoteUnits";
        public const string EngineHandle = "EngineHandle";
        public const string CompoundingFrequency = "CompoundingFrequency";
        public const string ExtrapolationPermitted = "ExtrapolationPermitted";
        public const string BootstrapperInterpolation = "BootstrapperInterpolation";
        public const string BaseCurve = "BaseCurve";
    }

    public static class CurveConst
    {
        public const string QR_EOD = "QR_EOD";
        public const string QR_LIVE = "QR_LIVE";
        public const string NAB_EOD = "NAB_EOD";
        public const string SYD_LIVE = "SYD_LIVE";
        public const string TEST_EOD = "TEST_EOD";
        public const string LOCAL_USER = "LOCAL_USER";
    }    
    
    ///<summary>
    /// The type of curve evolution to use.
    ///</summary>
    public enum PricingStructureEvolutionType
    {
        ///<summary>
        /// Decay from the forward value to the spot value.
        ///</summary>
        ForwardToSpot,

        ///<summary>
        /// Evolution from the spot value to the forwad value.
        ///</summary>
        SpotToForward
    }

    ///<summary>
    /// The type of perturbation to use oon a child curve.
    ///</summary>
    public enum PricingStructureRiskSetType
    {
        ///<summary>
        /// Base curve.
        ///</summary>
        Parent,

        ///<summary>
        /// Dependent curve.
        ///</summary>
        Child,

        /// <summary>
        /// Both
        /// </summary>
        Hybrid
    }

    ///<summary>
    /// The type of curve.
    ///</summary>
    public enum CurveType
    {
        ///<summary>
        /// Base curve.
        ///</summary>
        Parent,

        ///<summary>
        /// Dependent curve.
        ///</summary>
        Child
    }

    ///<summary>
    /// The curve asset class.
    ///</summary>
    public enum AssetClass
    {
        ///<summary>
        /// Rate.
        ///</summary>
        Rates,

        ///<summary>
        /// Fx.
        ///</summary>
        Fx,

        ///<summary>
        /// Equity.
        ///</summary>
        Equity,

        ///<summary>
        /// Credit.
        ///</summary>
        Credit,

        ///<summary>
        /// Inflation.
        ///</summary>
        Inflation,

        ///<summary>
        /// Commodity.
        ///</summary>
        Commodity,

        ///<summary>
        /// Energy.
        ///</summary>
        Energy
    }
}
