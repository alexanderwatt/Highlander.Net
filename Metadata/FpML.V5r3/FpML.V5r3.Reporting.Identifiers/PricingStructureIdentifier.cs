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

using System;
using FpML.V5r3.Reporting.Helpers;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.Constants;
using Orion.ModelFramework.Identifiers;

namespace Orion.Identifiers
{
    /// <summary>
    /// PricingStructureIdentifier
    /// </summary>
    public class PricingStructureIdentifier : Identifier, IPricingStructureIdentifier
    {
        /// <summary>
        /// PricingStructureType
        /// </summary>
        public PricingStructureTypeEnum PricingStructureType { get; protected set; }

        /// <summary>
        /// CurveName
        /// </summary>
        public String CurveName { get; protected set; }

        /// <summary>
        /// Currency
        /// </summary>
        public Currency Currency { get; protected set; }

        /// <summary>
        /// Algorithm
        /// </summary>
        public string Algorithm { get; protected set; }

        /// <summary>
        /// BuildDateTime
        /// </summary>
        public DateTime BuildDateTime { get; protected set; }

        /// <summary>
        /// BaseDate
        /// </summary>
        public DateTime BaseDate { get; protected set; }

        /// <summary>
        /// Domain
        /// </summary>
        public String Domain { get; protected set; }

        /// <summary>
        /// SourceSystem
        /// </summary>
        public String SourceSystem { get; protected set; }

        /// <summary>
        /// MarketName
        /// </summary>
        public String MarketAndDate { get; protected set; }

        /// <summary>
        /// Stress
        /// </summary>
        public String StressName { get; protected set; }

        /// <summary>
        /// Market
        /// </summary>
        public String Market { get; protected set; }

        /// <summary>
        /// DataType
        /// </summary>
        public String DataType { get; protected set; }

        /// <summary>
        /// MarketDate
        /// </summary>
        public DateTime MarketDate { get; protected set; }

        /// <summary>
        /// Domain
        /// </summary>
        public string Index { get; protected set; }

        /// <summary>
        /// Domain
        /// </summary>
        public string IndexTenor { get; protected set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PricingStructureIdentifier"/> class.
        /// </summary>
        /// <param name="properties">The properties. they must include:
        /// PricingStructureType, CurveName, BuildDateTime and Algorithm.</param>
        public PricingStructureIdentifier(NamedValueSet properties)
            : base(properties)
        {
            DataType = "Market";
            SetProperties();
            UpdateProperties();
        }

        private void UpdateProperties()
        {
            Properties.Set(CurveProp.UniqueIdentifier, UniqueIdentifier);
            Properties.Set("Domain", Domain);
            Properties.Set(CurveProp.Market, Market);
            Properties.Set(CurveProp.MarketAndDate, MarketAndDate);
        }

        private void SetProperties()
        {
            SourceSystem = PropertyHelper.ExtractSourceSystem(Properties);
            PricingStructureType = PropertyHelper.ExtractPricingStructureType(Properties);
            Domain = SourceSystem + '.' + DataType;
            BuildDateTime = PropertyHelper.ExtractBuildDateTime(Properties);
            BaseDate = PropertyHelper.ExtractBaseDate(Properties);
            Algorithm = PropertyHelper.ExtractAlgorithm(Properties);
            Market = PropertyHelper.ExtractMarket(Properties);
            MarketAndDate = PropertyHelper.ExtractMarketAndDate(Properties);
            Index = PropertyHelper.ExtractIndex(Properties);
            IndexTenor = PropertyHelper.ExtractIndexTenor(Properties);
            CurveName = PropertyHelper.ExtractCurveName(Properties);
            Currency = CurrencyHelper.Parse(PropertyHelper.ExtractCurrency(Properties));
            DateTime? marketDate = PropertyHelper.ExtractMarketDate(Properties);
            MarketDate = marketDate ?? MarketDate;
            StressName = Properties.GetValue<string>(CurveProp.StressName, false);
            NameSpace = Properties.GetValue<string>(EnvironmentProp.NameSpace, false);
            Id = BuildId();
            UniqueIdentifier = SetUniqueId();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PricingStructureIdentifier"/> class.
        /// </summary>
        /// <param name="pricingStructureType">Type of the curve.</param>
        /// <param name="curveName">Name of the index.</param>
        /// <param name="buildDateTime">The build date time.</param>
        /// <param name="algorithm">The algorithm.</param>
        public PricingStructureIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, 
            DateTime buildDateTime, String algorithm)
        {
            DataType = "Market";
            SourceSystem = "Orion";
            Domain = SourceSystem + '.' + DataType;
            PricingStructureType = pricingStructureType;
            CurveName = curveName;
            BuildDateTime = buildDateTime;
            BaseDate = buildDateTime.Date;
            Algorithm = algorithm;
            Properties = new NamedValueSet();
            Properties.Set("DataType", "Market");
            Properties.Set(CurveProp.SourceSystem, "Orion");
            Properties.Set(CurveProp.PricingStructureType, PricingStructureType.ToString());
            Properties.Set(CurveProp.BuildDateTime, BuildDateTime);
            Properties.Set(CurveProp.BaseDate, BaseDate);
            Properties.Set(CurveProp.Algorithm, Algorithm);
            Properties.Set(CurveProp.CurveName, CurveName);
            SetProperties();
            UpdateProperties();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PricingStructureIdentifier"/> class.
        /// </summary>
        /// <param name="pricingStructureType">Type of the curve.</param>
        /// <param name="curveName">Name of the index.</param>
        /// <param name="buildDateTime">The build date time.</param>
        public PricingStructureIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, DateTime buildDateTime)
            : this(pricingStructureType, curveName, buildDateTime, "Default")
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PricingStructureIdentifier"/> class.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        public PricingStructureIdentifier(string curveId)
        {
            Properties = new NamedValueSet();
            DataType = "Market";
            SourceSystem = "Orion";
            Domain = SourceSystem + '.' + DataType;
            BuildDateTime = DateTime.Now;
            Properties.Set(CurveProp.BuildDateTime, BuildDateTime);
            Algorithm = "Default";
            Id = curveId;
            Properties.Set("Identifier", Id);
            Properties.Set(CurveProp.Algorithm, Algorithm);
            Properties.Set("DataType", "Market");
            Properties.Set(CurveProp.SourceSystem, "Orion");
            Properties.Set("Domain", SourceSystem + '.' + DataType);
            string[] idParts = curveId.Split('.');
            if (idParts.Length>=2)
            {
                try
                {
                    PricingStructureType = EnumHelper.Parse<PricingStructureTypeEnum>(idParts[0]);
                    Properties.Set(CurveProp.PricingStructureType, PricingStructureType.ToString());
                    CurveName = idParts[1];
                    Properties.Set(CurveProp.CurveName, CurveName);
                    Currency = CurrencyHelper.Parse(PropertyHelper.ExtractCurrency(Properties));
                    if (idParts.Length >=3)
                    {
                        BaseDate = DateTime.Parse(idParts[2]);
                        Properties.Set(CurveProp.BaseDate, BaseDate);
                        if (idParts.Length > 3)
                        {
                            Algorithm = idParts[3];
                            Properties.Set(CurveProp.Algorithm, Algorithm);
                        }
                    }
                    UniqueIdentifier = SetUniqueId();
                    Properties.Set(CurveProp.UniqueIdentifier, UniqueIdentifier);
                    SetProperties();
                }
                catch (System.Exception ex)
                {
                    throw new ArgumentException($"Invalid pricingStructureId {curveId} - unable to parse", ex);
                }
            } 
        }

        /// <summary>
        /// Builds the id.
        /// </summary>
        /// <returns></returns>
        protected string BuildId() //TODO Should the algorithm be included?
        {
            string id = Properties.GetString("Identifier", false);
            if (string.IsNullOrEmpty(id))
            {
                string suffix = Properties.GetString("Usage", false);
                if (string.IsNullOrEmpty(suffix))
                {
                    suffix = Properties.GetString(CurveProp.StressName, false);
                }
                PricingStructureTypeEnum pricingStructureType;
                switch (PricingStructureType)
                {
                    case PricingStructureTypeEnum.RateBasisCurve:
                    case PricingStructureTypeEnum.ClearedRateCurve:
                    case PricingStructureTypeEnum.BondFinancingCurve:
                    case PricingStructureTypeEnum.BondFinancingBasisCurve:
                    case PricingStructureTypeEnum.RateSpreadCurve:
                    case PricingStructureTypeEnum.XccySpreadCurve:
                        pricingStructureType = PricingStructureTypeEnum.RateCurve;
                        break;
                    case PricingStructureTypeEnum.BondDiscountCurve:
                        pricingStructureType = PricingStructureTypeEnum.DiscountCurve;
                        break;
                    case PricingStructureTypeEnum.RateXccyCurve:
                        pricingStructureType = PricingStructureTypeEnum.DiscountCurve;
                        break;
                    case PricingStructureTypeEnum.CommoditySpreadCurve:
                        pricingStructureType = PricingStructureTypeEnum.CommodityCurve;
                        break;
                    default:
                        pricingStructureType = PricingStructureType;
                        break;
                }
                id = !string.IsNullOrEmpty(suffix) ? $"{pricingStructureType}.{CurveName}.{suffix}" : $"{pricingStructureType}.{CurveName}";
            }
            return id;
        }

        /// <summary>
        /// Builds the id.
        /// </summary>
        /// <returns></returns>
        private string SetUniqueId()
        {
            string uniqueIdentifier = Properties.GetValue<string>(CurveProp.UniqueIdentifier, null) ?? BuildUniqueId();
            return uniqueIdentifier;
        }

        protected string BuildUniqueId()
        {
            string id = BuildId();
            //if(StressName != null)
            //{
            //    id = StressName + '.' + id;
            //}
            return $"{DataType}.{MarketAndDate}.{id}";
        }

        // helper specifically for stress curve identifiers
        public static IPricingStructureIdentifier CreateMarketCurveIdentifier(
            NamedValueSet initProps,
            string marketName,
            DateTime? marketDate,
            string curveType,
            string curveName,
            string stressName)
        {
            var tempProps = new NamedValueSet(initProps);
            tempProps.Set(CurveProp.Market, marketName);
            tempProps.Set(CurveProp.MarketDate, null);
            if (marketDate != null)
                tempProps.Set(CurveProp.MarketDate, (DateTime)marketDate);
            if (curveType != null)
                tempProps.Set(CurveProp.PricingStructureType, curveType);
            if (curveName != null)
                tempProps.Set(CurveProp.CurveName, curveName);
            if (stressName != null)
                tempProps.Set(CurveProp.StressName, stressName);
            // hack - clear other config properties
            tempProps.Set(CurveProp.MarketAndDate, null);
            tempProps.Set(CurveProp.UniqueIdentifier, null);
            tempProps.Set(CurveProp.Function, null);
            tempProps.Set(CurveProp.DataGroup, null);
            // hack - set ref curve unique id
            var refCurveTypeName = tempProps.GetValue<string>(CurveProp.ReferenceCurveName, null);
            if (refCurveTypeName != null)
            {
                string marketEnv = marketDate.HasValue
                    ? marketName + "." + marketDate.Value.ToString(CurveProp.MarketDateFormat) 
                    : marketName;
                tempProps.Set(
                    CurveProp.ReferenceCurveUniqueId,
                    $"Market.{marketEnv}.{refCurveTypeName}");
            }
            return new PricingStructureIdentifier(tempProps);
        }

        public static string ValidFxCurveIdentifier(string marketName, string baseCurrency, string quoteCurrency, string stress, bool invert)
        {
            string id = null;
            const string pst = "FxCurve.";
            if (marketName != null && baseCurrency != null && quoteCurrency != null)
            {
                var curveName = pst + baseCurrency + '-' + quoteCurrency;
                if (invert)
                {
                    curveName = pst + quoteCurrency + '-' + baseCurrency;
                }
                id = stress != null ? $"Market.{marketName}.{curveName}.{stress}" : $"Market.{marketName}.{curveName}";
            }
            return id;
        }

        public static string ValidRateCurveIdentifier(string marketName, string curveName, string stress)
        {
            string id = null;
            var market = marketName.Split('.')[0];
            if (market != CurveConst.NAB_EOD && market != CurveConst.TEST_EOD)
            {
                id = stress != null ? $"Market.{marketName}.{curveName}.{stress}" : $"Market.{marketName}.{curveName}";
            }
            return id;
        }

        public static string ValidVolatilityCurveIdentifier(string marketName, string curveName, string stress)
        {
            string id = null;
            var market = marketName.Split('.')[0];
            if (market != CurveConst.NAB_EOD && market != CurveConst.TEST_EOD)
            {
                id = stress != null ? $"Market.{marketName}.{curveName}.{stress}" : $"Market.{marketName}.{curveName}";
            }
            return id;
        }
    }
}