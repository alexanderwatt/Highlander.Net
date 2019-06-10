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

#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Orion.Constants;
using Orion.CurveEngine.Markets;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;
using IMarketEnvironment = Orion.ModelFramework.IMarketEnvironment;

#endregion

namespace Orion.CurveEngine.Helpers
{
    /// <summary>
    /// A useful asset helper class.
    /// </summary>
    public class MarketEnvironmentHelper
    {
        /// <summary>
        /// Creates a stream environment.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="discountCurve"></param>
        /// <param name="forecastCurve"></param>
        /// <param name="fxCurve"></param>
        /// <param name="volSurface"></param>
        /// <returns></returns>
        public static ISwapLegEnvironment CreateInterestRateStreamEnvironment(DateTime baseDate, IRateCurve discountCurve, IRateCurve forecastCurve, IFxCurve fxCurve, IVolatilitySurface volSurface)
        {
            var market = new SwapLegEnvironment();
            market.AddPricingStructure(InterestRateStreamPSTypes.DiscountCurve.ToString(), discountCurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastCurve.ToString(), forecastCurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.ReportingCurrencyFxCurve.ToString(), fxCurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastIndexVolatilitySurface.ToString(), volSurface);
            return market;
        }

        /// <summary>
        /// Creates a stream environment.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="discountCurve"></param>
        /// <param name="forecastCurve"></param>
        /// <param name="fxCurve"></param>
        /// <returns></returns>
        public static ISwapLegEnvironment CreateInterestRateStreamEnvironment(DateTime baseDate, IRateCurve discountCurve, IRateCurve forecastCurve, IFxCurve fxCurve)
        {
            var market = new SwapLegEnvironment();
            market.AddPricingStructure(InterestRateStreamPSTypes.DiscountCurve.ToString(), discountCurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastCurve.ToString(), forecastCurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.ReportingCurrencyFxCurve.ToString(), fxCurve);
            return market;
        }

        ///<summary>
        /// Creates a market environment for use in calculations.
        ///</summary>
        ///<param name="marketName">The name of the market environment.</param>
        ///<param name="curve">The curve.</param>
        ///<returns></returns>
        public static IMarketEnvironment Create(string marketName, IPricingStructure curve)
        {
            var environment = new MarketEnvironment(marketName);
            var name = (PricingStructureIdentifier)curve.GetPricingStructureId();
            environment.AddPricingStructure(name.UniqueIdentifier, curve);
            return environment;
        }

        ///<summary>
        /// Creates a market environment for use in calculations.
        ///</summary>
        ///<param name="marketName">The name of the market environment.</param>
        ///<param name="uniqueIdentifier">The unique Identifier.</param>
        ///<param name="curve">The curve.</param>
        ///<returns></returns>
        public static ISimpleMarketEnvironment CreateSimpleEnvironment(string marketName, string uniqueIdentifier, IPricingStructure curve)
        {
            var environment = new SimpleMarketEnvironment(marketName, uniqueIdentifier, curve);
            return environment;
        }

        ///<summary>
        /// Creates a market environment for use in calculations.
        ///</summary>
        ///<param name="marketName">The name of the market environment.</param>
        ///<param name="curve">The curve.</param>
        ///<returns></returns>
        public static IMarketEnvironment Create(string marketName, List<IPricingStructure> curve)
        {
            var environment = new MarketEnvironment(marketName);
            foreach (var structure in curve)
            {
                var name = (PricingStructureIdentifier)structure.GetPricingStructureId();
                environment.AddPricingStructure(name.UniqueIdentifier, structure);
            }
            return environment;
        }

        /// <summary>
        /// Creates a market environment.
        /// </summary>
        /// <param name="market"></param>
        /// <returns></returns>
        public static IMarketEnvironment CreateMarketEnvironment(string market)
        {
            var marketEnvironment = new MarketEnvironment(market);
            return marketEnvironment;
        }

        /// <summary>
        /// Stores the structure.
        /// </summary>
        /// <param name="marketEnvironment">The market environment.</param>
        /// <param name="name">The name.</param>
        /// <param name="pricingStructure">The pricing structure.</param>
        /// <returns></returns>
        public static IMarketEnvironment StoreStructure(IMarketEnvironment marketEnvironment, string name, IPricingStructure pricingStructure)
        {
            var market = (MarketEnvironment)marketEnvironment;
            if (!StructureExists(marketEnvironment, name))
            {
                market.AddPricingStructure(name, pricingStructure);
            }
            return market;
        }

        /// <summary>
        /// Structures the exists.
        /// </summary>
        /// <param name="marketEnvironment">The market environment.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        internal static Boolean StructureExists(IMarketEnvironment marketEnvironment, string name)
        {
            return StructureExists(marketEnvironment.GetPricingStructures(), name);
        }

        /// <summary>
        /// Structures the exists.
        /// </summary>
        /// <param name="structures">The structures.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        internal static Boolean StructureExists(IDictionary<string, IPricingStructure> structures, string name)
        {
            return structures.Keys.Contains(name);
        }

        ///// <summary>
        ///// Creates the specified market.
        ///// </summary>
        ///// <param name="market">The market.</param>
        ///// <param name="rateCurveTOCurveTermsMap">The rate curve TO curve terms map.</param>
        ///// <param name="fxCurveTOCurveTermsMap">The fx curve TO curve terms map.</param>
        ///// <param name="volCubeTOCubeTermsMap"></param>
        ///// <param name="curveTOLegRefMap">The curve TO leg ref map.</param>
        ///// <param name="volSurfaceTOSurfaceTermsMap"></param>
        ///// <returns></returns>
        //static internal IMarketEnvironment Create(string market,
        //                                          IDictionary<string, IRateCurveTerms> rateCurveTOCurveTermsMap,
        //                                          IDictionary<string, IFXCurveTerms> fxCurveTOCurveTermsMap,
        //                                          IDictionary<string, IVolatilitySurfaceTerms> volSurfaceTOSurfaceTermsMap,
        //                                          IDictionary<string, IVolatilitySurfaceTerms> volCubeTOCubeTermsMap,
        //                                          IDictionary<string, List<string>> curveTOLegRefMap)
        //{
        //    IDictionary<string, IPricingStructure> curveRefToCreatedCurveMap = new Dictionary<string, IPricingStructure>();

        //    if (rateCurveTOCurveTermsMap != null)
        //    {
        //        foreach (string curveRefKey in rateCurveTOCurveTermsMap.Keys)
        //        {
        //            IPricingStructure curve = RateCurveHelper.RequestCurve(rateCurveTOCurveTermsMap[curveRefKey]);
        //            curveRefToCreatedCurveMap[curveRefKey] = curve;
        //        }
        //    }

        //    if (fxCurveTOCurveTermsMap != null)
        //    {
        //        foreach (string curveRefKey in fxCurveTOCurveTermsMap.Keys)
        //        {
        //            IPricingStructure curve = FXCurveHelper.RequestCurve(fxCurveTOCurveTermsMap[curveRefKey]);
        //            curveRefToCreatedCurveMap[curveRefKey] = curve;
        //        }
        //    }

        //    if (volSurfaceTOSurfaceTermsMap != null)
        //    {
        //        foreach (string curveRefKey in volSurfaceTOSurfaceTermsMap.Keys)
        //        {
        //            IPricingStructure curve = VolatilitySurfaceHelper.RequestSurface(volSurfaceTOSurfaceTermsMap[curveRefKey]);
        //            curveRefToCreatedCurveMap[curveRefKey] = curve;
        //        }
        //    }

        //    if (volCubeTOCubeTermsMap != null)
        //    {
        //        foreach (string curveRefKey in volCubeTOCubeTermsMap.Keys)
        //        {
        //            IVolatilityCube curve = VolatilityCubeHelper.RequestCube(volCubeTOCubeTermsMap[curveRefKey]);
        //            curveRefToCreatedCurveMap[curveRefKey] = curve;
        //        }
        //    }

        //    IMarketEnvironment marketEnvironment = null;
        //    if (curveRefToCreatedCurveMap.Keys != null && curveRefToCreatedCurveMap.Keys.Count > 0)
        //    {
        //        marketEnvironment = CreateMarketEnvironment(market);

        //        foreach (string curveRefKey in curveRefToCreatedCurveMap.Keys)
        //        {
        //            List<string> legRefs = curveTOLegRefMap[curveRefKey];

        //            IPricingStructure ps = curveRefToCreatedCurveMap[curveRefKey];
        //            if (ps != null)
        //            {
        //                foreach (string legRef in legRefs)
        //                {
        //                    StoreStructure(marketEnvironment, legRef, ps);
        //                }
        //            }
        //        }
        //    }
        //    return marketEnvironment;
        //}

        ///// <summary>
        ///// Parses the pricing structures.
        ///// </summary>
        ///// <param name="structures">The structures.</param>
        ///// <returns></returns>
        //static internal NamedValueSet ParsePricingStructures(IDictionary<string, IPricingStructure> structures)
        //{
        //    var resultsContainer = new NamedValueSet();
        //    var sortedStructures = new SortedDictionary<string, IPricingStructure>(structures);

        //    foreach (string key in sortedStructures.Keys)
        //    {
        //        var results = new object[2];
        //        IDictionary<DateTime, Decimal> resultsDict = null;
        //        if (sortedStructures[key] is IRateCurve)
        //        {
        //            resultsDict = RateCurveHelper.GetDiscountFactors((IRateCurve)sortedStructures[key]);
        //        }
        //        else if (sortedStructures[key] is IFxCurve)
        //        {
        //            resultsDict = FXCurveHelper.GetForwardRates((IFxCurve)sortedStructures[key]);
        //        }

        //        if (resultsDict == null || resultsDict.Keys.Count <= 0) continue;
        //        results[0] = new List<DateTime>(resultsDict.Keys).ToArray();
        //        results[1] = new List<Decimal>(resultsDict.Values).ToArray();
        //        resultsContainer.Set(new NamedValue(key.Replace("-", "."), results));
        //    }
        //    return resultsContainer;
        //}

        /// <summary>
        /// Updates the map.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pricingStructureTermsRef">The pricing structure terms ref.</param>
        /// <param name="curveTerms">The curve terms.</param>
        /// <param name="curveToCurveTermsMap">The curve TO curve terms map.</param>
        /// <param name="curveToLegRefMap">The curve TO leg ref map.</param>
        /// <param name="curveReferenceKey">The curve reference key.</param>
        internal static void UpdateMap<T>(string pricingStructureTermsRef, T curveTerms, IDictionary<string, T> curveToCurveTermsMap, IDictionary<string, List<string>> curveToLegRefMap, string curveReferenceKey)
        {
            string lowerCurveReferenceKey = curveReferenceKey.ToLowerInvariant();
            curveToCurveTermsMap[lowerCurveReferenceKey] = curveTerms;
            if (curveToLegRefMap.ContainsKey(lowerCurveReferenceKey))
            {
                curveToLegRefMap[lowerCurveReferenceKey].Add(pricingStructureTermsRef);
            }
            else
            {
                curveToLegRefMap[lowerCurveReferenceKey] = new List<string>(new[] { pricingStructureTermsRef });
            }
        }


        ///// <summary>
        ///// Builds the market.
        ///// </summary>
        ///// <param name="market">The market.</param>
        ///// <param name="pricingStructureTerms">The pricing structure terms.</param>
        ///// <param name="resultsContainer">The results container.</param>
        ///// <returns></returns>
        //public static IMarketEnvironment BuildMarket(string market, IDictionary<string, IDictionary<string, object>> pricingStructureTerms, NamedValueSet resultsContainer)
        //{
        //    const string cLowerRateCurve = "ratecurve";
        //    const string cLowerFXCurve = "fxcurve";
        //    const string cLowerVolSurface = "ratevolatilitymatrix";
        //    const string cLowerVolCube = "ratevolatilitycube";
        //    const string cPricingStructureTypeKey = CurveProp.PricingStructureType;

        //    IDictionary<string, List<string>> curveTOLegRefMap = new Dictionary<string, List<string>>();
        //    IDictionary<string, IRateCurveTerms> rateCurveTOCurveTermsMap = new Dictionary<string, IRateCurveTerms>();
        //    IDictionary<string, IFXCurveTerms> fxCurveTOCurveTermsMap = new Dictionary<string, IFXCurveTerms>();
        //    IDictionary<string, IVolatilitySurfaceTerms> volSurfaceTOSurfaceTermsMap = new Dictionary<string, IVolatilitySurfaceTerms>();
        //    IDictionary<string, IVolatilitySurfaceTerms> volCubeTOCubeTermsMap = new Dictionary<string, IVolatilitySurfaceTerms>();

        //    foreach (string pricingStructureTermsRef in pricingStructureTerms.Keys)
        //    {
        //        IDictionary<string, object> termProps = pricingStructureTerms[pricingStructureTermsRef];

        //        if (termProps == null) continue;
        //        var propKeys = new List<string>(termProps.Keys);

        //        string psTypeKey = propKeys.Find(item => string.Compare(item, cPricingStructureTypeKey, true) == 0);

        //        if (string.IsNullOrEmpty(psTypeKey))
        //        {
        //            continue;
        //        }

        //        IPricingStructureTerms curveTerms;
        //        switch (termProps[psTypeKey].ToString().ToLowerInvariant())
        //        {
        //            case cLowerRateCurve:
        //                curveTerms = new RateCurveTerms(termProps);
        //                UpdateMap(pricingStructureTermsRef, (IRateCurveTerms)curveTerms, rateCurveTOCurveTermsMap, curveTOLegRefMap, curveTerms.ReferenceKey);
        //                break;
        //            case cLowerFXCurve:
        //                curveTerms = new FXCurveTerms(termProps);
        //                UpdateMap(pricingStructureTermsRef, (IFXCurveTerms)curveTerms, fxCurveTOCurveTermsMap, curveTOLegRefMap, curveTerms.ReferenceKey);
        //                break;
        //            case cLowerVolSurface:
        //                curveTerms = new VolatilitySurfaceTerms(termProps);
        //                UpdateMap(pricingStructureTermsRef, (VolatilitySurfaceTerms)curveTerms, volSurfaceTOSurfaceTermsMap, curveTOLegRefMap, curveTerms.ReferenceKey);
        //                break;
        //            case cLowerVolCube:
        //                curveTerms = new VolatilitySurfaceTerms(termProps);
        //                UpdateMap(pricingStructureTermsRef, (VolatilitySurfaceTerms)curveTerms, volCubeTOCubeTermsMap, curveTOLegRefMap, curveTerms.ReferenceKey);
        //                break;
        //        }
        //    }
        //    IMarketEnvironment marketEnvironment = Create(market, rateCurveTOCurveTermsMap, fxCurveTOCurveTermsMap, volSurfaceTOSurfaceTermsMap, volCubeTOCubeTermsMap, curveTOLegRefMap);
        //    if (marketEnvironment != null)
        //    {
        //        IDictionary<string, IPricingStructure> structures = marketEnvironment.GetPricingStructures();
        //        resultsContainer.Add(ParsePricingStructures(structures));
        //    }
        //    return marketEnvironment;
        //}

        /// <summary>
        /// Validates the market.
        /// </summary>
        /// <param name="pricingStructures"></param>
        /// <param name="market"></param>
        public static void ValidateMarket(string[] pricingStructures, IMarketEnvironment market)
        {
            var missingStructures = new List<string>();
            if (market == null)
                throw new ArgumentNullException(nameof(market), "A valid market has not been supplied");
            IDictionary<string, IPricingStructure> ps = market.GetPricingStructures();
            if (market != null && ps.Count == 0)
            {
                throw new ApplicationException($"{market.Id} market does not contain any curves");
            }
            if (ps.Count <= 0)
            {
            }
            else
            {
                missingStructures.AddRange(pricingStructures.Where(structure => !StructureExists(ps, structure)));
                if (missingStructures.Count > 0)
                {
                    throw new ApplicationException(
                        $"{string.Join(", ", missingStructures.ToArray())} curves not found in {market.Id} market");
                }
            }
        }

        /// <summary>
        /// Resolve the rate curve identifier.
        /// </summary>
        /// <param name="curveId"></param>
        /// <param name="currency"></param>
        /// <param name="tenor"></param>
        public static void ResolveRateCurveIdentifier(string curveId, out string currency, out string tenor)
        {
            string[] mainParts = curveId.Split('.');
            if (mainParts.Length != 2)
            {
                throw new ArgumentException("curveId must be made up of two parts: [CurveType].[Details]");
            }
            string[] nameParts = mainParts[1].Split('-');
            if (nameParts.Length < 3)
            {
                throw new ArgumentException("curveId details must be made up of at least three parts: [CurveType].[XXX.IndexName.Tenor]");
            }
            currency = nameParts[0];
            tenor = nameParts.Last();
            if (tenor.Equals("SENIOR", StringComparison.OrdinalIgnoreCase))
            {
                tenor = null;
            }
        }

        /// <summary>
        /// Resolve the rate curve identifier.
        /// </summary>
        /// <param name="curveName"></param>
        /// <param name="currency"></param>
        /// <param name="tenor"></param>
        public static void ResolveRateCurveName(string curveName, out string currency, out string tenor)
        {
            string[] mainParts = curveName.Split('.');
            string[] nameParts = mainParts.Last().Split('-');
            if (nameParts.Length < 3)
            {
                throw new ArgumentException("curveId details must be made up of at least three parts: [CurveType].[XXX.IndexName.Tenor]");
            }
            currency = nameParts[0];
            tenor = nameParts.Last();
            if (tenor.Equals("SENIOR", StringComparison.OrdinalIgnoreCase))
            {
                tenor = null;
            }
        }

        /// <summary>
        /// Resolve the rate curve identifier.
        /// </summary>
        /// <param name="curveName"></param>
        /// <param name="currency"></param>
        /// <param name="tenor"></param>
        public static void ResolveVolatilityCurveName(string curveName, out string currency, out string tenor)
        {
            string[] mainParts = curveName.Split('.');
            string[] nameParts = mainParts.Last().Split('-');
            if (nameParts.Length < 3)
            {
                throw new ArgumentException("curveId details must be made up of at least three parts: [CurveType].[XXX.IndexName.Tenor]");
            }
            currency = nameParts[0];
            tenor = nameParts.Last();
        }

        /// <summary>
        /// Resolve the rate curve identifier.
        /// </summary>
        /// <param name="curveId"></param>
        /// <param name="curveName"></param>
        public static void ResolveRateCurveIdentifier(string curveId, out string curveName)
        {
            string[] mainParts = curveId.Split('.');
            if (mainParts.Length < 2) // != has been changed to handle equity and bond curves.
            {
                throw new ArgumentException("curveId must be made up of at least two parts: [CurveType].[Details] or [SecuritiesType].[Details].[Exchange]");
            }
            if (mainParts.Length == 2)
            {
                curveName = mainParts.Last();
            }
            else
            {
                curveName = mainParts[1] + '.' + mainParts[2];
            }
        }

        /// <summary>
        /// Resolve the fx curve identifier.
        /// </summary>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <returns></returns>
        public static string ResolveFxCurveNames(string currency1, string currency2)
        {
            string id = $"FxCurve.{currency1}-{currency2}";
            return id;
        }
    }
}