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

#region Usings

using System;
using System.Globalization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using FpML.V5r3.Reporting;
using Metadata.Common;
using Orion.Analytics.DayCounters;
using Orion.CalendarEngine;
using Orion.CalendarEngine.Helpers;
using Orion.CalendarEngine.Rules;
using Orion.CurveEngine.Assets;
using Orion.CurveEngine.Assets.Helpers;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.Markets;
using Orion.CurveEngine.PricingStructures.Helpers;
using Orion.CurveEngine.PricingStructures.Surfaces;
using Orion.CurveEngine.PricingStructures.Interpolators;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.PricingStructures;
using Orion.ModelFramework.MarketEnvironments;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.CurveEngine.PricingStructures.Cubes;
using Orion.Analytics.Interpolations.Points;
using Orion.CurveEngine.Assets.Rates.CapsFloors;
using Exception = System.Exception;
using FxCurve = Orion.CurveEngine.PricingStructures.Curves.FxCurve;

#endregion

namespace Orion.CurveEngine
{
    /// <summary>
    /// Caches all the curves creates.
    /// </summary>
    public class CurveEngine : ICurveEngine
    {
        #region Private fields

        /// <summary>
        /// 
        /// </summary>
        public readonly ILogger Logger;

        /// <summary>
        /// 
        /// </summary>
        public readonly string NameSpace;

        /// <summary>
        /// 
        /// </summary>
        public readonly ICoreCache Cache;

        internal const string DefaultTenor = "3M";

        //public Pedersen Pedersen { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// The main constructor.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        public CurveEngine(ILogger logger, ICoreCache cache)
            : this(logger, cache, EnvironmentProp.DefaultNameSpace)
        {
        }

        /// <summary>
        /// The main constructor.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        public CurveEngine(ILogger logger, ICoreCache cache, string nameSpace)
        {
            Cache = cache;
            Logger = logger;
            NameSpace = nameSpace;
            //Pedersen = new Pedersen();
            ////1. Load up relevant data. If the data is not available then the engine will not run.
            //If there is no cache then resource files can be loaded!
            try
            {
                CacheInstruments();
                CacheDateRules();
                //CacheHolidays();
                CacheBusinessCenterCalendars();
                //CacheScenarioRules();
                CacheAlgorithms();
            }
            catch (Exception ex)
            {
                logger.Log(ex);
            }
        }

        #endregion

        #region ICurveEngine Interface

        /// <summary>
        /// Creates a user defined calendar.
        /// </summary>
        /// <param name="calendarProperties">THe calendar properties must include a valid FpML business center name.</param>
        /// <param name="holidaysDates">The dates that are in the defined calendar.</param>
        /// <returns></returns>
        public BusinessCenterCalendar CreateCalendar(NamedValueSet calendarProperties, List<DateTime> holidaysDates)
        {
            //Build the calendar.
            var result = BusinessCalendarHelper.CreateCalendar(calendarProperties, holidaysDates);
            return result;
        }

        ///// <summary>
        ///// This interface provides a single point of entry to bootstrap all curves, 
        ///// including spread curves and cross currency spread curves.
        ///// In the case of spread curves, the reference curve may also need to be bootstrapped. 
        ///// </summary>
        ///// <param name="dependentCurves">For a simple rate curve, there would be only a single ICoreItem.
        ///// The type of the ICoreItem is always a Market with only a single PricingStructure and PricingStructureValuation. 
        ///// The QuotedAssetSet must be populated with market data.
        ///// Also, there must be properties that define the algorithm.
        ///// If no market data is provided, then the internal market data will be used.</param>
        ///// <param name="calendars">Each curve requires a calendar. In the event of no calendar provided,
        ///// the internal system calendars will be used.</param>
        ///// <returns></returns>
        //public ICoreItem BootstrapPricingStructure(List<ICoreItem> dependentCurves, List<BusinessCenterCalendar> calendars)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

        #region Serialisable Data Using ICoreCache

        /// <summary>
        /// Gets the instrument.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public Instrument GetInstrument(string uniqueId)
        {
            var id = NameSpace + "." + uniqueId;
            return Cache.LoadObject<Instrument>(id);
        }

        /// <summary>
        /// Sets the rate curve in market.
        /// </summary>
        /// <param name="requestProperties">Some extra properties.</param>
        /// <param name="maxNumberToReturn">The maximum number to return.</param>
        /// <returns></returns>
        public IEnumerable<IPricingStructure> GetPricingStructures(NamedValueSet requestProperties, int maxNumberToReturn)
        {
            // Create the querying filter
            if (requestProperties == null) throw new ArgumentNullException(nameof(requestProperties));
            //The new filter with OR on arrays.
            requestProperties.Set(EnvironmentProp.NameSpace, NameSpace);
            IExpression queryExpr = Expr.BoolAND(requestProperties);
            // Load items
            var items = Cache.LoadItems<Market>(queryExpr);
            //// Convert to curves
            // Order
            var result = items.OrderBy(a => a.AppProps.GetValue("BuildDateTime", DateTime.MinValue))
                         .Take(maxNumberToReturn);
            var structures = new List<IPricingStructure>();
            foreach(var element in result)
            {
                IPricingStructure ps = null;
                try
                {
                    ps = TranslateToPricingStructure(element.AppProps, element.Data as Market, out string id);
                    Logger.LogDebug(id);
                }
                catch (Exception e)
                {
                    Logger.LogDebug(e.ToString());
                }
                if (ps != null)
                {
                    structures.Add(ps);
                }
            }
            return structures;
        }

        /// <summary>
        /// Gets the curve items.
        /// </summary>
        /// <param name="marketName"></param>
        /// <param name="stress"></param>
        /// <param name="currencyFilters"></param>
        /// <returns></returns>
        public List<ICoreItem> GetCurveItems(string marketName, string stress, IExpression currencyFilters)
        {
            IExpression stressFilter = string.IsNullOrEmpty(stress)
                ? Expr.IsNull(CurveProp.StressName)
                : Expr.IsEQU(CurveProp.StressName, stress);
            IExpression filter = Expr.BoolAND(stressFilter,
                                              Expr.IsEQU(CurveProp.MarketAndDate, marketName),
                                              Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.FxCurve.ToString()),
                                              Expr.IsEQU(EnvironmentProp.NameSpace, NameSpace),
                                              currencyFilters);
            return Cache.LoadItems<Market>(filter);
        }

        /// <summary>
        /// Saves the curve
        /// </summary>
        /// <param name="market"></param>
        /// <param name="uniqueId"></param>
        /// <param name="properties"></param>
        /// <param name="timespan"></param>
        public void SaveCurve(Market market, string uniqueId, NamedValueSet properties, TimeSpan timespan)
        {
            var id = NameSpace + "." + uniqueId;
            Cache.SaveObject(market, id, properties, timespan);
        }

        /// <summary>
        /// Saves the curve
        /// </summary>
        /// <param name="market"></param>
        /// <param name="uniqueId"></param>
        /// <param name="properties"></param>
        public void SaveCurve(Market market, string uniqueId, NamedValueSet properties)
        {
            var id = NameSpace + "." + uniqueId;
            Cache.SaveObject(market, id, properties, TimeSpan.MaxValue);
        }

        /// <summary>
        /// Saves the curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="uniqueId"></param>
        /// <param name="properties"></param>
        public void SaveCurve(IPricingStructure curve, string uniqueId, NamedValueSet properties)
        {
            var market = curve.GetMarket();
            var id = NameSpace + "." + uniqueId;
            Cache.SaveObject(market, id, properties, TimeSpan.MaxValue);
        }

        /// <summary>
        /// Saves the curve.
        /// </summary>
        /// <param name="curve"></param>
        public void SaveCurve(IPricingStructure curve)
        {
            SaveCurve(curve, TimeSpan.MaxValue);
        }

        /// <summary>
        /// Saves the curve.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="curve"></param>
        public static void SaveCurve(ICoreCache cache, string nameSpace, IPricingStructure curve)
        {
            SaveCurve(cache, nameSpace, curve, TimeSpan.MaxValue);
        }

        /// <summary>
        /// Saves the curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="timeOut"></param>
        public void SaveCurve(IPricingStructure curve, TimeSpan timeOut)
        {
            var properties = AddPricingStructureProperties(curve, curve.GetPricingStructureId().Properties);
            var market = curve.GetMarket();
            properties.Set(EnvironmentProp.NameSpace, NameSpace);
            Cache.SaveObject(market, NameSpace + "." + curve.GetPricingStructureId().UniqueIdentifier, properties, timeOut);
        }

        /// <summary>
        /// Saves the curve.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="curve"></param>
        /// <param name="timeOut"></param>
        public static void SaveCurve(ICoreCache cache, string nameSpace, IPricingStructure curve, TimeSpan timeOut)
        {
            var properties = AddPricingStructureProperties(curve, curve.GetPricingStructureId().Properties);
            var market = curve.GetMarket();
            properties.Set(EnvironmentProp.NameSpace, nameSpace);
            cache.SaveObject(market, nameSpace + "." + curve.GetPricingStructureId().UniqueIdentifier, properties, timeOut);
        }

        /// <summary>
        /// Saves the curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="expiryDate"></param>
        public void SaveCurve(IPricingStructure curve, DateTime expiryDate)
        {
            var properties = AddPricingStructureProperties(curve, curve.GetPricingStructureId().Properties);
            var market = curve.GetMarket();
            properties.Set(EnvironmentProp.NameSpace, NameSpace);
            Cache.SaveObject(market, NameSpace + "." + curve.GetPricingStructureId().UniqueIdentifier, properties, expiryDate - DateTime.Now);
        }

        /// <summary>
        /// Gets the curves.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The NameSpace</param>
        /// <param name="product">The product.</param>
        /// <param name="market">The market.</param>
        /// <param name="baseCurrency">The base Currency.</param>
        /// <param name="forceBootstrap">The force a bootstrap flag.</param>
        /// <returns></returns>
        public static IMarketEnvironment GetMarket(ILogger logger, ICoreCache cache, string nameSpace, Product product, string market, string baseCurrency, Boolean forceBootstrap)
        {
            var curveNames = product.GetRequiredPricingStructures();
            var marketEnvironment = new MarketEnvironment {Id = market};
            foreach (var ps in curveNames)
            {
                var id = "Market." + market + "." + ps;
                var curve = GetCurve(logger, cache, nameSpace, id, forceBootstrap);
                marketEnvironment.AddPricingStructure(ps, curve);
            }
            var currencies = product.GetRequiredCurrencies();
            foreach (var ps in currencies)
            {
                if(ps!=baseCurrency)
                {
                    var psName = "FxCurve." + ps + "-" + baseCurrency;
                    var curve = CurveLoader.LoadFxCurve(logger, cache, nameSpace, market, ps, baseCurrency, null);//TODO  the name of ps is incorrect!
                    marketEnvironment.AddPricingStructure(psName, curve);
                }
            }
            return marketEnvironment;
        }

        /// <summary>
        /// Gets the curves.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The NameSpace</param>
        /// <param name="curveMap">The curve Map.</param>
        /// <param name="forceBootstrap">The force a bootstrap flag.</param>
        /// <returns></returns>
        public static IMarketEnvironment GetCurves(ILogger logger, ICoreCache cache, String nameSpace, List<Pair<string, string>> curveMap, Boolean forceBootstrap)
        {
            var marketEnvironment = new MarketEnvironment();
            foreach (var pair in curveMap)
            {
                var curve = GetCurve(logger, cache, nameSpace, pair.Second, forceBootstrap);
                marketEnvironment.AddPricingStructure(pair.First, curve);
            }
            return marketEnvironment;
        }

        /// <summary>
        /// Gets the curve.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The NameSpace</param>
        /// <param name="uniqueName">The uniqueName.</param>
        /// <param name="forceBootstrap">The force a bootstrap flag.</param>
        /// <returns></returns>
        public static IPricingStructure GetCurve(ILogger logger, ICoreCache cache, String nameSpace, String uniqueName, Boolean forceBootstrap)
        {
            var item = cache.LoadItem<Market>(nameSpace + "." + uniqueName);
            if (item == null)
                return null;
            var deserializedMarket = (Market)item.Data;
            NamedValueSet properties = item.AppProps;
            if (forceBootstrap)
            {
                properties.Set("Bootstrap", true);
            }
            //Handle rate basis curves that are dependent on another ratecurve.
            //TODO This functionality needs to be extended for calibrations (bootstrapping),
            //TODO where there is AccountReference dependency on one or more pricing structures.
            var pst = PropertyHelper.ExtractPricingStructureType(properties);
            if (pst == PricingStructureTypeEnum.RateBasisCurve)
            {
                //Get the reference curve identifier.
                var refCurveId = properties.GetValue<string>(CurveProp.ReferenceCurveUniqueId, true);
                //Load the data.
                var refItem = cache.LoadItem<Market>(nameSpace + "." + refCurveId);
                var deserializedRefCurveMarket = (Market)refItem.Data;
                var refCurveProperties = refItem.AppProps;
                //Format the ref curve data and call the pricing structure helper.
                var refCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                        deserializedRefCurveMarket.Items[0],
                        deserializedRefCurveMarket.Items1[0], refCurveProperties);
                var spreadCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(deserializedMarket.Items[0],
                                                                                            deserializedMarket.Items1[0],
                                                                                            properties);
                //create and set the pricing structure
                var psBasis = PricingStructureFactory.Create(logger, cache, nameSpace, null, null, refCurveFpmlTriplet, spreadCurveFpmlTriplet);
                return psBasis;
            }
            if (pst == PricingStructureTypeEnum.RateXccyCurve)
            {
                //Get the reference curve identifier.
                var refCurveId = properties.GetValue<string>(CurveProp.ReferenceCurveUniqueId, true);
                //Load the data.
                var refItem = cache.LoadItem<Market>(nameSpace + "." + refCurveId);
                var deserializedRefCurveMarket = (Market)refItem.Data;
                var refCurveProperties = refItem.AppProps;
                //Get the reference curve identifier.
                var refFxCurveId = properties.GetValue<string>(CurveProp.ReferenceFxCurveUniqueId, true);
                //Load the data.
                var refFxItem = cache.LoadItem<Market>(nameSpace + "." + refFxCurveId);
                var deserializedRefFxCurveMarket = (Market)refFxItem.Data;
                var refFxCurveProperties = refFxItem.AppProps;
                //Get the currency2 curve identifier.
                var currency2CurveId = properties.GetValue<string>(CurveProp.ReferenceCurrency2CurveId, true);
                //Load the data.
                var currency2Item = cache.LoadItem<Market>(nameSpace + "." + currency2CurveId);
                var deserializedCurrency2CurveMarket = (Market)currency2Item.Data;
                var refCurrencyCurveProperties = currency2Item.AppProps;
                //Format the ref curve data and call the pricing structure helper.
                var refCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                        deserializedRefCurveMarket.Items[0],
                        deserializedRefCurveMarket.Items1[0], refCurveProperties);
                var refFxCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                        deserializedRefFxCurveMarket.Items[0],
                        deserializedRefFxCurveMarket.Items1[0], refFxCurveProperties);
                var currency2CurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                        deserializedCurrency2CurveMarket.Items[0],
                        deserializedCurrency2CurveMarket.Items1[0], refCurrencyCurveProperties);
                var spreadCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(deserializedMarket.Items[0],
                                                                                            deserializedMarket.Items1[0],
                                                                                            properties);
                //create and set the pricing structure
                var psBasis = PricingStructureFactory.Create(logger, cache, nameSpace, null, null, refCurveFpmlTriplet, refFxCurveFpmlTriplet,
                                                                    currency2CurveFpmlTriplet, spreadCurveFpmlTriplet);
                return psBasis;
            }
            // Create FpML pair from Market
            //
            var fpmlPair = new Pair<PricingStructure, PricingStructureValuation>(deserializedMarket.Items[0],
                                                                                 deserializedMarket.Items1[0]);
            //create and set the pricing structure
            var ps = PricingStructureFactory.Create(logger, cache, nameSpace, null, null, fpmlPair, properties);
            return ps;
        }

        /// <summary>
        /// Gets the curve.
        /// </summary>
        /// <param name="uniqueName">The uniqueName.</param>
        /// <param name="forceBootstrap">The force a bootstrap flag.</param>
        /// <returns></returns>
        public IPricingStructure GetCurve(String uniqueName, Boolean forceBootstrap)
        {
            var id = NameSpace + "." + uniqueName;
            var item = Cache.LoadItem<Market>(id);
            if (item == null)
                return null;
            var deserializedMarket = (Market)item.Data;
            NamedValueSet properties = item.AppProps;
            if (forceBootstrap)
            {
                properties.Set("Bootstrap", true);
            }
            //Handle rate basis curves that are dependent on another ratecurve.
            //TODO This functionality needs to be extended for calibrations (bootstrapping),
            //TODO where there is AccountReference dependency on one or more pricing structures.
            var pst = PropertyHelper.ExtractPricingStructureType(properties);
            if (pst == PricingStructureTypeEnum.RateBasisCurve)
            {
                //Get the reference curve identifier.
                var refCurveId = properties.GetValue<string>(CurveProp.ReferenceCurveUniqueId, true);
                //Load the data.
                var refItem = Cache.LoadItem<Market>(NameSpace + "." + refCurveId);
                var deserializedRefCurveMarket = (Market)refItem.Data;
                var refCurveProperties = refItem.AppProps;
                //Format the ref curve data and call the pricing structure helper.
                var refCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                        deserializedRefCurveMarket.Items[0],
                        deserializedRefCurveMarket.Items1[0], refCurveProperties);
                var spreadCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(deserializedMarket.Items[0],
                                                                                            deserializedMarket.Items1[0],
                                                                                            properties);
                //create and set the pricing structure
                var psBasis = PricingStructureFactory.Create(Logger, Cache, NameSpace, null, null, refCurveFpmlTriplet, spreadCurveFpmlTriplet);
                return psBasis;
            }
            if (pst == PricingStructureTypeEnum.RateXccyCurve)
            {
                //Get the reference curve identifier.
                var refCurveId = properties.GetValue<string>(CurveProp.ReferenceCurveUniqueId, true);
                //Load the data.
                var refItem = Cache.LoadItem<Market>(NameSpace + "." + refCurveId);
                var deserializedRefCurveMarket = (Market)refItem.Data;
                var refCurveProperties = refItem.AppProps;
                //Get the reference curve identifier.
                var refFxCurveId = properties.GetValue<string>(CurveProp.ReferenceFxCurveUniqueId, true);
                //Load the data.
                var refFxItem = Cache.LoadItem<Market>(NameSpace + "." + refFxCurveId);
                var deserializedRefFxCurveMarket = (Market)refFxItem.Data;
                var refFxCurveProperties = refFxItem.AppProps;
                //Get the currency2 curve identifier.
                var currency2CurveId = properties.GetValue<string>(CurveProp.ReferenceCurrency2CurveId, true);
                //Load the data.
                var currency2Item = Cache.LoadItem<Market>(NameSpace + "." + currency2CurveId);
                var deserializedCurrency2CurveMarket = (Market)currency2Item.Data;
                var refCurrencyCurveProperties = currency2Item.AppProps;
                //Format the ref curve data and call the pricing structure helper.
                var refCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                        deserializedRefCurveMarket.Items[0],
                        deserializedRefCurveMarket.Items1[0], refCurveProperties);
                var refFxCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                        deserializedRefFxCurveMarket.Items[0],
                        deserializedRefFxCurveMarket.Items1[0], refFxCurveProperties);
                var currency2CurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(
                        deserializedCurrency2CurveMarket.Items[0],
                        deserializedCurrency2CurveMarket.Items1[0], refCurrencyCurveProperties);
                var spreadCurveFpmlTriplet =
                    new Triplet<PricingStructure, PricingStructureValuation, NamedValueSet>(deserializedMarket.Items[0],
                                                                                            deserializedMarket.Items1[0],
                                                                                            properties);
                //create and set the pricing structure
                var psBasis = PricingStructureFactory.Create(Logger, Cache, NameSpace, null, null, refCurveFpmlTriplet, refFxCurveFpmlTriplet,
                                                                    currency2CurveFpmlTriplet, spreadCurveFpmlTriplet);
                return psBasis;
            }
            // Create FpML pair from Market
            //
            var fpmlPair = new Pair<PricingStructure, PricingStructureValuation>(deserializedMarket.Items[0],
                                                                                 deserializedMarket.Items1[0]);
            //create and set the pricing structure
            var ps = PricingStructureFactory.Create(Logger, Cache, NameSpace, null, null, fpmlPair, properties);
            return ps;
        }

        #endregion

        #region Business Calendars

        /// <summary>
        /// De-dupes the specified calendar names.
        /// </summary>
        /// <param name="calendarNames">The calendar names.</param>
        /// <returns></returns>
        private static string[] Dedupe(IEnumerable<string> calendarNames)
        {
            var names = new List<string>();
            foreach (string name in calendarNames)
            {
                if (!names.Contains(name))
                {
                    names.Add(name);
                }
            }
            return names.ToArray();
        }

        /// <summary>
        /// Creates a consolidated business calendar for a given set of business centers
        /// </summary>
        /// <param name="centers">The centers.</param>
        /// <returns></returns>
        public IBusinessCalendar ToBusinessCalendar(BusinessCenters centers)
        {
            var calendars = centers.businessCenter.Select(businessCenter => businessCenter.Value).ToArray();
            var dps = GetDateRuleParser(calendars, NameSpace);
            var significantDays = GetSignificantDates(Dedupe(dps.FpmlName));
            IBusinessCalendar result = new BusinessCalendar(significantDays, dps);
            return result;
        }

        private IDateRuleParser GetDateRuleParser(string[] calendars, string nameSpace)
        {
            var path = nameSpace + "." + BusinessCentreDateRulesProp.GenericName;
            var loadedObject = Cache.LoadObject<DateRules>(path);
            var dps = new DateRuleParser(calendars, loadedObject.DateRuleProfile);
            return dps;
        }


        /// <summary>
        /// Significant dates for the requested business centers. This uses a query and will be slower than using specific years.
        /// </summary>
        /// <param name="businessCenters">The city names.</param>
        /// <returns></returns>
        public List<SignificantDay> GetSignificantDates(string[] businessCenters)
        {
            var dateList = new List<SignificantDay>();
            //The new filter with OR on arrays..
            var path = NameSpace + "." + BusinessCenterCalendarProp.GenericName;
            foreach (var centre in businessCenters)
            {
                var identifier = path + '.' + centre;
                var loadedObject = Cache.LoadObject<BusinessCenterCalendar>(identifier);
                if (loadedObject?.Holidays != null)
                {
                    var dates = loadedObject.Holidays.Select(dr => (DateTime)dr.Item);
                    // Get the dates as one list
                    var significantDates
                        = dates.Select(dr => new SignificantDay { Date = dr, ObservedSignificantDayDate = dr, Name = centre });
                    dateList.AddRange(significantDates);
                }
            }
            return dateList;
        }

        #endregion

        #region Generic FX Curve Load

        /// <summary>
        /// Gets the fx curve.
        /// </summary>
        /// <param name="marketName"></param>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public IFxCurve LoadFxCurve(string marketName, string currency1, string currency2, string stress)
        {
            var fxCurve = CurveLoader.LoadFxCurve(Logger, Cache, NameSpace, null, null, marketName, currency1, currency2, stress);
                return fxCurve;
        }

        #endregion

        #region Generic Interest Rate and Discount Curve Load

        /// <summary>
        /// Gets the ir curves.
        /// </summary>
        /// <param name="marketName"></param>
        /// <param name="curveName"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public IPricingStructure LoadDiscountCurve(string marketName, string curveName, string stress)
        {
            var curve = CurveLoader.LoadInterestRateCurve(Logger, Cache, NameSpace, null, null, marketName, curveName, stress);
            return curve;
        }

        /// <summary>
        /// Gets the ir curves.
        /// </summary>
        /// <param name="marketName"></param>
        /// <param name="currency"></param>
        /// <param name="creditInstrumentId"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public IPricingStructure LoadDiscountCurve(string marketName, string currency, string creditInstrumentId, string stress)
        {
            var curve = CurveLoader.LoadDiscountCurve(Logger, Cache, NameSpace, null, null, marketName, currency, creditInstrumentId, stress);
            return curve;
        }

        /// <summary>
        /// Gets the ir curves.
        /// </summary>
        /// <param name="marketName"></param>
        /// <param name="curveName"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public IRateCurve LoadInterestRateCurve(string marketName, string curveName, string stress)
        {
            var curve = CurveLoader.LoadInterestRateCurve(Logger, Cache, NameSpace,  null, null, marketName, curveName, stress);
            return curve;
        }

        /// <summary>
        /// Gets the ir curves.
        /// </summary>
        /// <param name="marketName"></param>
        /// <param name="currency"></param>
        /// <param name="tenor"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public IRateCurve LoadInterestRateCurve(string marketName, string currency, string tenor, string stress)
        {
            var curve = CurveLoader.LoadInterestRateCurve(Logger, Cache, NameSpace, null, null, marketName, currency, tenor, stress);
            return curve;
        }

        /// <summary>
        /// Gets xccy spread curve.
        /// </summary>
        /// <param name="marketName"></param>
        /// <param name="currency"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public IRateCurve LoadXccySpreadCurve(string marketName, string currency, string stress)
        {
            var newCurveTypeFilters = Expr.IsEQU(CurveProp.PricingStructureType, PricingStructureTypeEnum.XccySpreadCurve.ToString());
            return CurveLoader.LoadInterestRateCurve(Logger, Cache, NameSpace, null, null, marketName, currency, null, stress, newCurveTypeFilters);
        }

        #endregion

        #region Generic Volatility Curve Load      

        /// <summary>
        /// Gets the ir curves.
        /// </summary>
        /// <param name="marketName"></param>
        /// <param name="curveName"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public IVolatilitySurface LoadVolatilityCurve(string marketName, string curveName, string stress)
        {
            var curve = CurveLoader.LoadVolatilityCurve(Logger, Cache, NameSpace, null, null, marketName, curveName, stress);
            return curve;
        }

        /// <summary>
        /// Gets the ir curves.
        /// </summary>
        /// <param name="marketName"></param>
        /// <param name="currency"></param>
        /// <param name="tenor"></param>
        /// <param name="stress"></param>
        /// <returns></returns>
        public IVolatilitySurface LoadVolatilityCurve(string marketName, string currency, string tenor, string stress)
        {
            var curve = CurveLoader.LoadVolatilityCurve(Logger, Cache, NameSpace, null, null, marketName, currency, tenor, stress);
            return curve;
        }

        #endregion

        #region Curve Querying

        /// <summary>
        /// Returns the header information for all curves matching the query properties.
        /// </summary>
        /// <param name="query">The query properties. A 2-column array of names and values.</param>
        /// <returns></returns>
        public object[,] CurvesQuery(object[,] query)
        {
            int rowMin = query.GetLowerBound(0);
            int rowMax = query.GetUpperBound(0);
            int colMin = query.GetLowerBound(1);
            int colMax = query.GetUpperBound(1);
            if (colMax - colMin + 1 != 3)
                throw new ApplicationException("Input parameters must be 3 columns (name/op/value)!");
            IExpression whereExpr = null;
            for (int row = rowMin; row <= rowMax; row++)
            {
                int colName = colMin + 0;
                int colOp = colMin + 1;
                int colValue = colMin + 2;
                string name = query[row, colName]?.ToString();
                string op = query[row, colOp]?.ToString();
                object value = query[row, colValue];
                if (name != null && op != null && value != null)
                {
                    op = op.ToLower().Trim();
                    if ((op == "equ") || (op == "=="))
                        whereExpr = Expr.BoolAND(Expr.IsEQU(name, value), whereExpr);
                    else if (op == "neq" || op == "!=")
                        whereExpr = Expr.BoolAND(Expr.IsNEQ(name, value), whereExpr);
                    else if (op == "geq" || op == ">=")
                        whereExpr = Expr.BoolAND(Expr.IsGEQ(name, value), whereExpr);
                    else if (op == "leq" || op == "<=")
                        whereExpr = Expr.BoolAND(Expr.IsLEQ(name, value), whereExpr);
                    else if (op == "gtr" || op == ">")
                        whereExpr = Expr.BoolAND(Expr.IsGTR(name, value), whereExpr);
                    else if (op == "lss" || op == "<")
                        whereExpr = Expr.BoolAND(Expr.IsLSS(name, value), whereExpr);
                    else if (op == "starts")
                        whereExpr = Expr.BoolAND(Expr.StartsWith(name, value.ToString()), whereExpr);
                    else if (op == "ends")
                        whereExpr = Expr.BoolAND(Expr.EndsWith(name, value.ToString()), whereExpr);
                    else if (op == "contains")
                        whereExpr = Expr.BoolAND(Expr.Contains(name, value.ToString()), whereExpr);
                    else
                        throw new ApplicationException("Unknown Operator: '" + op + "'");
                }
            }
            return CurvesQuery(whereExpr);
        }

        /// <summary>
        /// Returns the header information for all curves matching the query properties.
        /// </summary>
        /// <param name="whereExpr">The query properties.</param>
        /// <returns></returns>
        public object[,] CurvesQuery(IExpression whereExpr)
        {
            List<ICoreItem> items = Cache.LoadItems(typeof(Market), whereExpr);
            var result = new object[items.Count + 1, 15];
            // add heading row
            result[0, 0] = CurveProp.Market;
            result[0, 1] = CurveProp.Currency1;
            result[0, 2] = CurveProp.Currency2;
            result[0, 3] = CurveProp.Algorithm;
            result[0, 4] = CurveProp.BaseDate;
            result[0, 5] = CurveProp.BuildDateTime;
            result[0, 6] = CurveProp.CurveType;
            result[0, 7] = CurveProp.CurveName;
            result[0, 8] = CurveProp.PricingStructureType;
            result[0, 9] = CurveProp.ReferenceCurveName;
            result[0, 10] = CurveProp.SourceSystem;
            result[0, 11] = CurveProp.StressName;
            result[0, 12] = CurveProp.UniqueIdentifier;
            result[0, 13] = CurveProp.AssetClass;
            result[0, 14] = CurveProp.MarketDate;
            // now add data rows
            int curveNum = 1;
            foreach (ICoreItem item in items)
            {
                try
                {
                    result[curveNum, 0] = item.AppProps.GetValue(CurveProp.Market, "Unknown");
                    result[curveNum, 1] = item.AppProps.GetValue(CurveProp.Currency1, "Unknown");
                    result[curveNum, 2] = item.AppProps.GetValue(CurveProp.Currency2, "None");
                    result[curveNum, 3] = item.AppProps.GetValue(CurveProp.Algorithm, "Unknown");
                    result[curveNum, 4] = item.AppProps.GetValue(CurveProp.BaseDate, new DateTime()).ToShortDateString();
                    result[curveNum, 5] = item.AppProps.GetValue(CurveProp.BuildDateTime, new DateTime()).ToShortDateString();
                    result[curveNum, 6] = item.AppProps.GetValue(CurveProp.CurveType, "Unknown");
                    result[curveNum, 7] = item.AppProps.GetValue(CurveProp.CurveName, "Unknown");
                    result[curveNum, 8] = item.AppProps.GetValue(CurveProp.PricingStructureType, "Unknown");
                    result[curveNum, 9] = item.AppProps.GetValue(CurveProp.ReferenceCurveName, "None");
                    result[curveNum, 10] = item.AppProps.GetValue(CurveProp.SourceSystem, "SpreadSheet");
                    result[curveNum, 11] = item.AppProps.GetValue(CurveProp.StressName, "None");
                    result[curveNum, 12] = item.AppProps.GetValue(CurveProp.UniqueIdentifier, "Unknown");
                    result[curveNum, 13] = item.AppProps.GetValue(CurveProp.AssetClass, "Unknown");
                    result[curveNum, 14] = item.AppProps.GetValue(CurveProp.MarketDate, new DateTime()).ToShortDateString();
                    curveNum++;
                }
                catch (Exception e)
                {
                    Logger.LogWarning("CurveEngine.CurvesQuery: Exception: {0}", e);
                }
            }
            return result;
        }

        #endregion

        #region Assets Local Saving, Retrieval and Evaluation

        /// <summary>
        /// Load Asset Config from the XML in the database
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="asset"></param>
        /// <param name="maturityTenor"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        public object[,] GetAssetConfigurationData(string currency, string asset, string maturityTenor, string variant)
        {
            #region Validate and convert arguments

            if (string.IsNullOrEmpty(asset))
            {
                throw new ArgumentException("Asset must be entered");
            }
            if (string.IsNullOrEmpty(currency))
            {
                throw new ArgumentException("Currency must be entered");
            }
            currency = currency.ToUpper();
            if (!string.IsNullOrEmpty(maturityTenor))
            {
                maturityTenor = maturityTenor.ToUpper();
            }
            string id = $"{currency}-{asset}-{maturityTenor}";
            if (!string.IsNullOrEmpty(variant))
            {
                id = $"{currency}-{asset}-{maturityTenor}-{variant.ToUpper()}";
            }

            #endregion

            Instrument instrument = InstrumentDataHelper.GetInstrumentConfigurationData(Cache, NameSpace, id);
            return ConvertAssetToRange(instrument);
        }

        private static object[,] ConvertAssetToRange(Instrument instrument)
        {
            var output = new List<Pair<string, string>>();
            string xml = XmlSerializerHelper.SerializeToString(instrument);
            var nodes = new XmlDocument();
            nodes.LoadXml(xml);
            output.AddRange(ConvertXmlToMatrix(nodes, ""));
            var outputRange = new object[output.Count, 2];
            // Remove the first row, XML def row
            output.RemoveAt(0);
            int i = 0;
            foreach (var pair in output)
            {
                outputRange[i, 0] = pair.First;
                outputRange[i, 1] = pair.Second;
                i++;
            }
            return outputRange;
        }

        private static IEnumerable<Pair<string, string>> ConvertXmlToMatrix(XmlNode instrumentNode, string nodeName)
        {
            var output = new List<Pair<string, string>>();
            foreach (XmlNode node in instrumentNode)
            {
                if (node.ChildNodes.Count == 0)
                {
                    string value = node.Value ?? "";
                    string name = node.NodeType == XmlNodeType.Text ? nodeName : nodeName + "." + node.Name;
                    output.Add(new Pair<string, string>(name, value));
                }
                else
                {
                    string newNodeName = nodeName == "" ? node.Name : nodeName + "." + node.Name;
                    output.AddRange(ConvertXmlToMatrix(node, newNodeName));
                }
            }
            return output;
        }

        /// <summary>
        /// Evaluates the metrics for asset set.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="assetReferenceKeys">The asset reference keys.</param>
        /// <param name="referenceDate">The reference date.</param>
        /// <returns></returns>
        public QuotedAssetSet EvaluateLocalAssets(List<string> metrics, string curveId,
                                                                List<string> assetReferenceKeys, DateTime referenceDate)
        {
            var qas = new QuotedAssetSet();
            var curve = GetCurve(curveId, false);
            var marketName = ((PricingStructureIdentifier)curve.GetPricingStructureId()).Market;
            var marketEnvironment = MarketEnvironmentHelper.CreateSimpleEnvironment(marketName, curveId, curve);
            var basicAssetValuations = new List<BasicAssetValuation>();
            foreach (string assetReferenceKey in assetReferenceKeys)
            {
                var id = NameSpace + "." + assetReferenceKey;
                var loadedObject = Cache.LoadObject<MarketInstrument>(id);
                if (loadedObject != null)
                {
                    IPriceableAssetController priceableAsset = loadedObject.GetPriceableAsset();
                    var controllerData = PriceableAssetFactory.CreateAssetControllerData(metrics.ToArray(), referenceDate, marketEnvironment);
                    BasicAssetValuation basicAssetValuation = priceableAsset.Calculate(controllerData);
                    //  Set reference to priceableAsset's id to basicAssetValuation
                    basicAssetValuation.objectReference = new AnyAssetReference { href = assetReferenceKey };
                    basicAssetValuations.Add(basicAssetValuation);//TODO add the assets into this?
                }
            }
            qas.assetQuote = basicAssetValuations.ToArray();
            return qas;
        }

        /// <summary>
        /// Creates the specified asset. This a factory for simple assets, where the configuration data stored
        /// in the cache is used for constructing the priceable asset.
        /// </summary>
        /// <param name="rate">The rate. In the case of a bond this would be the yield-to-maturity.</param>
        /// <param name="additional">The additional. For a future this is the volatility. For a bond this is the coupon.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public string CreateLocalAsset(Decimal rate, Decimal additional, NamedValueSet properties)
        {
            //sets the default.
            const string result = "Asset not built-";
            ////make sure there is an AssetId.
            var assetIdentifier = PropertyHelper.ExtractStringProperty(CurveProp.AssetId, properties);
            if (assetIdentifier == null)
            {
                return result + "because of a non-existent AssetId property.";
            }
            //create the asset-basic asset valuation pair.
            var asset = AssetHelper.Parse(assetIdentifier, rate, additional);
            //sets up the unique identifier.
            var uniqueIdentifier = PropertyHelper.ExtractStringProperty(CurveProp.UniqueIdentifier, properties);
            if (uniqueIdentifier == "Unknown Property.")
            {
                var identifier = new AssetIdentifier(rate, additional, properties);
                uniqueIdentifier = identifier.UniqueIdentifier;
            }
            //create the priceable asset.
            var priceableAsset = CreatePriceableAsset(asset.Second, properties);
            //set the priceable asset in the cache.
            SetLocalAsset(uniqueIdentifier, priceableAsset, properties);
            //return the cache id.
            return uniqueIdentifier;
        }

        /// <summary>
        /// Creates the specified asset. This a factory for simple assets, where the configuration data stored
        /// in the cache is used for constructing the priceable asset.
        /// </summary>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="measureType">The additional.</param>
        /// <param name="priceQuoteUnits">The units of measure.</param>
        /// <param name="properties">the properties.</param>
        /// <returns>The string id.</returns>
        public string CreateLocalAsset(List<Decimal> values, List<string> measureType, List<string> priceQuoteUnits,
                                         NamedValueSet properties)
        {
            var clonedNvs = new NamedValueSet(properties.ToDictionary());
            //Build the priceable asset.
            var priceableAsset = PriceableAssetFactory.BuildPriceableAsset(Logger, Cache, NameSpace, values.ToArray(), measureType.ToArray(), priceQuoteUnits.ToArray(),
                                         clonedNvs, null, null, out string message);
            if (message == "Asset built successfully.")
            {
                //get the uniqueId.
                var uniqueId = PropertyHelper.ExtractStringProperty(CurveProp.UniqueIdentifier, clonedNvs);
                //set the priceable asset in the cache.
                SetLocalAsset(uniqueId, priceableAsset, clonedNvs);
                //return the cache id.
                return uniqueId;
            }
            return message;
        }

        /// <summary>
        /// Creates the specified asset identifiers.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="measureTypes">The measure types. Currently supports MarketQuote and Volatility.</param>
        /// <param name="priceQuoteUnits">The price quote units. Currently supports Rates and LogNormalVolatility.</param>
        /// <returns></returns>
        /// <param name="properties"></param>
        public List<string> CreateLocalAssets(List<string> assetIdentifiers, List<Decimal> values,
                                            List<string> measureTypes, List<string> priceQuoteUnits, NamedValueSet properties)
        {
            if (assetIdentifiers.Count != values.Count && assetIdentifiers.Count != priceQuoteUnits.Count && assetIdentifiers.Count != measureTypes.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(values), "the rates do not match the number of assets");
            }
            var priceableAssets = new List<string>();
            try
            {
                var baseDate = properties.GetValue<DateTime>(CurveProp.BaseDate, true);
                var uniqueIds = (IEnumerable<string>)assetIdentifiers;
                foreach (var assetIdentifier in uniqueIds.Distinct())
                {
                    var index = 0;
                    var vals = new List<Decimal>();
                    var measures = new List<String>();
                    var quotes = new List<String>();
                    foreach (var ids in assetIdentifiers)
                    {
                        index++;
                        if (ids != assetIdentifier) continue;
                        vals.Add(values[index - 1]);
                        measures.Add(measureTypes[index - 1]);
                        quotes.Add(priceQuoteUnits[index - 1]);
                    }
                    var nvs = PriceableAssetFactory.BuildPropertiesForAssets("Local", assetIdentifier, baseDate);
                    var newProperties = CurveHelper.CombinePropertySetsClone(properties, nvs);
                    priceableAssets.Add(CreateLocalAsset(vals, measures, quotes,
                                                    newProperties));
                }
                return priceableAssets;
            }
            catch (Exception ex)
            {
                throw new Exception("No base date provided.", ex);
            }
        }

        /// <summary>
        /// Creates the specified asset identifiers.
        /// </summary>
        /// <param name="baseDates">The base dates.</param>
        /// <param name="rates">The rates.</param>
        /// <param name="additional">The additional.</param>
        /// <returns></returns>
        /// <param name="properties"></param>
        public List<string> CreateLocalAssets(List<DateTime> baseDates, List<Decimal> rates,
                                            List<Decimal> additional, NamedValueSet properties)
        {
            if (baseDates.Count != rates.Count && baseDates.Count != additional.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rates), "the rates do not match the number of assets");
            }
            var index = 0;
            var priceableAssets = new List<string>();
            //There must be an assetId in the property set.
            foreach (var baseDate in baseDates)
            {
                properties.Set(CurveProp.BaseDate, baseDate);
                priceableAssets.Add(CreateLocalAsset(rates[index], additional[index], properties));
                index++;
            }
            return priceableAssets;
        }

        /// <summary>
        /// Creates the specified assets.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="rates">The rates.</param>
        /// <param name="additional">The additional.</param>
        /// <returns></returns>
        /// <param name="properties">This must include a BaseDate.</param>
        public List<string> CreateLocalAssets(List<string> assetIdentifiers, List<Decimal> rates,
                                            List<Decimal> additional, NamedValueSet properties)
        {
            if (assetIdentifiers.Count != rates.Count && assetIdentifiers.Count != additional.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rates), "the rates do not match the number of assets");
            }
            var index = 0;
            var priceableAssets = new List<string>();
            try
            {
                var baseDate = properties.GetValue<DateTime>(CurveProp.BaseDate, true);
                foreach (var assetIdentifier in assetIdentifiers)
                {
                    var extraProperties = PriceableAssetFactory.BuildPropertiesForAssets("Local", assetIdentifier, baseDate);
                    var propCopy = CurveHelper.CombinePropertySetsClone(properties, extraProperties);
                    priceableAssets.Add(CreateLocalAsset(rates[index], additional[index], propCopy));
                    index++;
                }
                return priceableAssets;
            }
            catch (Exception ex)
            {
                throw new Exception("No base date provided.", ex);
            }

        }

        /// <summary>
        /// Creates the specified assets.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="rates">The rates.</param>
        /// <param name="additional">The additional.</param>
        /// <returns></returns>
        public List<string> CreateLocalAssets(List<string> assetIdentifiers, DateTime baseDate, List<Decimal> rates,
                                            List<Decimal> additional)
        {
            if (assetIdentifiers.Count != rates.Count && assetIdentifiers.Count != additional.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rates), "the rates do not match the number of assets");
            }
            var index = 0;
            var priceableAssets = new List<string>();
            foreach (var assetIdentifier in assetIdentifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets("Local", assetIdentifier, baseDate);
                priceableAssets.Add(CreateLocalAsset(rates[index], additional[index], properties));
                index++;
            }
            return priceableAssets;
        }

        /// <summary>
        /// Creates the specified asset.
        /// </summary>
        /// <param name="assetIdentifier"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="coupon">The coupon.</param>
        /// <param name="maturityDate">The maturity date..</param>
        /// <param name="frequency">The coupon frequency.</param>
        /// <param name="ytm">The ytm.</param>
        /// <param name="daycount">The daycount used for repo, accrual and coupons.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>The string id.</returns>
        public string CreateLocalBond(string assetIdentifier, DateTime baseDate, DateTime maturityDate, Decimal coupon,
            string daycount, string frequency, Decimal ytm, NamedValueSet properties)
        {
            properties.Set(CurveProp.BaseDate, baseDate);
            properties.Set("Maturity", maturityDate);
            properties.Set(EnvironmentProp.NameSpace, NameSpace);
            properties.Set("Coupon", coupon);
            var asset = AssetHelper.ParseBond(assetIdentifier, maturityDate, coupon, daycount, frequency, ytm);
            var assetId = PropertyHelper.ExtractStringProperty("AssetId", properties);
            properties.Set("AssetId", assetId);
            var fixedRate = MarketQuoteHelper.FindQuotationByMeasureType("MarketQuote", asset.Second.quote);
            var normalisedRate = MarketQuoteHelper.NormalisePriceUnits(fixedRate, "DecimalRate");
            var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(Cache, NameSpace, assetId);
            var nodeStruct = (BondNodeStruct)instrument.InstrumentNodeItem;
            string uniqueId;
            var uniqueIdentifier = PropertyHelper.ExtractStringProperty(CurveProp.UniqueIdentifier, properties);
            if (uniqueIdentifier != null)
            {
                uniqueId = uniqueIdentifier;
            }
            else
            {
                uniqueId = assetIdentifier + '-' + coupon + '-' + maturityDate.ToShortDateString();
                properties.Set(CurveProp.UniqueIdentifier, uniqueId);
            }
            var paymentCalendar = ToBusinessCalendar(nodeStruct.BusinessDayAdjustments.businessCenters);
            var settlementCalendar = ToBusinessCalendar(nodeStruct.ExDivDate.businessCenters);
            var priceableAsset = new PriceableSimpleBond(baseDate, nodeStruct, settlementCalendar, paymentCalendar, normalisedRate, BondPriceEnum.YieldToMaturity);
            SetLocalAsset(uniqueId, priceableAsset, properties);
            return uniqueId;
        }

        /// <summary>
        /// Creates the specified asset.
        /// </summary>
        /// <param name="assetIdentifiers"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="coupons">The coupons.</param>
        /// <param name="maturityDates">The maturity dates.</param>
        /// <param name="ytms">The yield to maturities.</param>
        /// <returns>The string id.</returns>
        public List<string> CreateLocalBonds(List<string> assetIdentifiers, DateTime baseDate, List<DateTime> maturityDates, List<Decimal> coupons, List<Decimal> ytms)
        {
            if (assetIdentifiers.Count != maturityDates.Count && assetIdentifiers.Count != coupons.Count && assetIdentifiers.Count != maturityDates.Count && assetIdentifiers.Count != ytms.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(coupons), "the rates do not match the number of assets");
            }
            var index = 0;
            var priceableAssets = new List<string>();
            foreach (var assetIdentifier in assetIdentifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForBondAssets("Local", assetIdentifier, baseDate, coupons[index], maturityDates[index]);
                priceableAssets.Add(CreateLocalAsset(ytms[index], 0.0m, properties));
                index++;
            }
            return priceableAssets;
        }

        /// <summary>
        /// Creates the specified asset.
        /// </summary>
        /// <param name="assetIdentifiers"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="coupons">The coupons.</param>
        /// <param name="maturityDates">The maturity dates.</param>
        /// <param name="ytms">The yield to maturities.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>The string id.</returns>
        public List<string> CreateLocalBonds(List<string> assetIdentifiers, DateTime baseDate, List<DateTime> maturityDates, List<Decimal> coupons, List<Decimal> ytms, NamedValueSet properties)
        {
            if (assetIdentifiers.Count != maturityDates.Count && assetIdentifiers.Count != coupons.Count && assetIdentifiers.Count != maturityDates.Count && assetIdentifiers.Count != ytms.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(coupons), "the rates do not match the number of assets");
            }
            var index = 0;
            var priceableAssets = new List<string>();
            foreach (var assetIdentifier in assetIdentifiers)
            {
                var nvs = PriceableAssetFactory.BuildPropertiesForBondAssets("Local", assetIdentifier, baseDate, coupons[index], maturityDates[index]);
                var namedValueSet = CurveHelper.CombinePropertySetsClone(properties, nvs);
                priceableAssets.Add(CreateLocalAsset(ytms[index], 0.0m, namedValueSet));
                index++;
            }
            return priceableAssets;
        }


        /// <summary>
        /// Creates the specified asset.
        /// </summary>
        /// <param name="assetIdentifiers"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="coupons">The coupons.</param>
        /// <param name="maturityDates">The maturity dates.</param>
        /// <param name="frequencies">The coupon frequency.</param>
        /// <param name="ytms">The yield to maturities.</param>
        /// <param name="daycounts">The daycounts used for repo, accrual and coupons.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>The string id.</returns>
        public List<string> CreateLocalBonds(List<string> assetIdentifiers, DateTime baseDate, List<DateTime> maturityDates, List<Decimal> coupons, List<string> daycounts, List<string> frequencies, List<Decimal> ytms, NamedValueSet properties)
        {
            if (assetIdentifiers.Count != maturityDates.Count && assetIdentifiers.Count != coupons.Count && assetIdentifiers.Count != daycounts.Count && assetIdentifiers.Count != ytms.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(coupons), "the rates do not match the number of assets");
            }
            var index = 0;
            var priceableAssets = new List<string>();
            foreach (var assetIdentifier in assetIdentifiers)
            {
                priceableAssets.Add(CreateLocalBond(assetIdentifier, baseDate, maturityDates[index], coupons[index], daycounts[index], frequencies[index], ytms[index], properties));
                index++;
            }
            return priceableAssets;
        }

        /// <summary>
        /// Creates the specified asset. This a factory for simple assets, where the configuration data stored
        /// in the cache is used for constructing the priceable asset.
        /// </summary>
        /// <param name="priceableAsset">The priceable Asset.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public string SetIRSwap(IPriceableAssetController priceableAsset, NamedValueSet properties)
        {
            //sets the default.
            var uniqueId = "Asset not built-";
            //AssetType property - make sure it exists.
            var assetType = PropertyHelper.ExtractStringProperty("AssetType", properties);
            if (assetType == null)
            {
                return uniqueId + "because of a non-existent AssetType property.";
            }
            //make sure there is an AssetId.
            var assetIdentifier = PropertyHelper.ExtractStringProperty("AssetId", properties);
            if (assetIdentifier == null)
            {
                return uniqueId + "because of a non-existent AssetId property.";
            }
            //sets up the unique identifier.
            var uniqueIdentifier = PropertyHelper.ExtractStringProperty(CurveProp.UniqueIdentifier, properties);
            if (uniqueIdentifier == "Unknown Property.")
            {
                uniqueId = assetIdentifier;
            }
            //set the priceable asset in the cache.
            SetLocalAsset(uniqueId, priceableAsset, properties);
            //return the cache id.
            return uniqueId;
        }

        /// <summary>
        /// Creates the specified surface asset.
        /// </summary>
        /// <param name="assetIdentifier"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="measureType">The additional.</param>
        /// <param name="priceQuoteUnits">The units of measure.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>The string id.</returns>
        public string CreateLocalSurfaceAsset(string assetIdentifier, DateTime baseDate, Decimal strike, Decimal[] values, string[] measureType, string[] priceQuoteUnits,
                                                NamedValueSet properties)
        {
            var asset = AssetHelper.CreateAssetPair(assetIdentifier, values, measureType, priceQuoteUnits);
            var uniqueId = assetIdentifier + '-' + strike + '-' + baseDate.ToShortDateString();
            var priceableAsset = CreatePriceableSurfaceAsset(strike, asset, baseDate);
            SetLocalAsset(uniqueId, priceableAsset, properties);
            return uniqueId;
        }

        /// <summary>
        /// Creates the specified surface asset.
        /// </summary>
        /// <param name="assetIdentifier"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="volatility">The volatility.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>The id.</returns>
        public string CreateLocalSurfaceAsset(string assetIdentifier, DateTime baseDate, Decimal volatility, Decimal strike,
                                                NamedValueSet properties)
        {
            var asset = AssetHelper.ParseSurface(assetIdentifier, volatility);//TODO check this.
            var uniqueId = assetIdentifier + '-' + strike + '-' + baseDate.ToShortDateString();
            var priceableAsset = CreatePriceableSurfaceAsset(strike, asset, baseDate);
            SetLocalAsset(uniqueId, priceableAsset, properties);
            return uniqueId;
        }

        /// <summary>
        /// Creates the specified surface asset.
        /// </summary>
        /// <param name="assetIdentifier">The ids.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="volatility">The volatilities.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>The id.</returns>
        public string[,] CreateLocalSurfaceAssets(List<string> assetIdentifier, DateTime baseDate, object[,] volatility, List<Decimal> strikes,
                                                    NamedValueSet properties)
        {
            var rows = assetIdentifier.Count;
            var cols = strikes.Count;
            var assetReferenceKeys = new string[rows, cols];
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    assetReferenceKeys[i, j] = CreateLocalSurfaceAsset(assetIdentifier[i], baseDate, Convert.ToDecimal(volatility[i, j]), strikes[j], properties);
                }
            }
            return assetReferenceKeys;
        }

        ///<summary>
        /// Sets the priceable asset into the cache.
        ///</summary>
        ///<param name="priceableAssetKey">The unique Id.</param>
        ///<param name="priceableAsset">The asset.</param>
        ///<param name="properties">The properties.</param>
        ///<returns></returns>
        public string SetLocalAsset(string priceableAssetKey, IPriceableAssetController priceableAsset,
                                                      NamedValueSet properties)
        {
            var marketInstrument = new MarketInstrument(priceableAsset.GetType().Name, priceableAsset);
            var id = NameSpace + "." + priceableAssetKey;
            Cache.SavePrivateObject(marketInstrument, id, properties);
            return priceableAssetKey;
        }

        /// <summary>
        /// Gets the priceable asset.
        /// </summary>
        /// <param name="assetIdentifier">The asset identifier.</param>//TODO to speed up a type is required.
        /// <returns></returns>
        public IPriceableAssetController GetLocalAsset(string assetIdentifier)
        {
            var id = NameSpace + "." + assetIdentifier;
            var loadedObject = Cache.LoadPrivateItem<MarketInstrument>(id);
            if (loadedObject == null)
            {
                var message = $"The object with name '{assetIdentifier}' is not in the local cache.";
                throw new ApplicationException(message);
            }
            var priceableAsset = loadedObject.Data as MarketInstrument;
            return priceableAsset?.GetPriceableAsset();
        }

        #endregion

        #region Asset Creation and Evaluation

        ///<summary>
        /// This function is a simple was to create a swap that has sequential roll dates and is adjusted, 
        /// with no spread on the floating side.
        /// The function will return a series of metrics for that swap, depending on what is requested.
        ///</summary>
        /// <remarks>
        /// The assumption is that only one curve is required for calculation, as the floating leg always 
        /// values to zero when including principal exchanges.
        /// </remarks>
        ///<param name="curveId">The curve id. This must have been created already.</param>
        ///<param name="currency">The configuration currency.
        /// This is used to get all default values and must exist in the cache.</param>
        ///<param name="baseDate">The base date.</param>
        ///<param name="fixedRate">The fixed rate.</param>
        ///<param name="businessDayConvention">The businessDayConvention.</param>
        ///<param name="notionalWeights">The notional weights.</param>
        ///<param name="fixedLegDayCount">The daycount.</param>
        ///<param name="notional">The notional.</param>
        ///<param name="businessCentersAsString">The business centers.</param>
        ///<param name="metric">The metric.</param>
        ///<param name="dates">The dates.</param>
        ///<returns></returns>
        public Decimal GetStructuredSwapMetric(String curveId, String currency, DateTime baseDate,
            Decimal notional, List<DateTime> dates, List<Decimal> notionalWeights, String fixedLegDayCount, Decimal fixedRate,
            string businessDayConvention, string businessCentersAsString, string metric)
        {
            var result = 0.0m;
            //Create a dummy rate.
            var bav = new BasicAssetValuation();
            var quote = BasicQuotationHelper.Create(fixedRate, "MarketQuote", "DecimalRate");
            bav.quote = new[] { quote };
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayConvention,
                                                                             businessCentersAsString);
            //create the swap.
            var swap = CreateInterestRateSwap("Local", baseDate, MoneyHelper.GetAmount(notional, currency), dates.ToArray(),
                                                                    notionalWeights.ToArray(), fixedLegDayCount, businessDayAdjustments, bav);
            //Get the AssetControllerData.
            var market = new SimpleRateMarketEnvironment();
            var bavMetric = new BasicAssetValuation();
            var quoteMetric = BasicQuotationHelper.Create(0.0m, metric);
            bavMetric.quote = new[] { quoteMetric };
            //Get the curve.
            var ratecurve = (IRateCurve)GetCurve(curveId, false);//This requires an IPricingStructure to be cached.
            market.AddPricingStructure("DiscountCurve", ratecurve);
            market.PricingStructureIdentifier = "DiscountCurve";
            var assetControllerData = new AssetControllerData(bavMetric, baseDate, market);
            //Create the interpolator and get the implied quote.
            var valuation = swap.Calculate(assetControllerData);
            if (valuation != null)
            {
                result = valuation.quote[0].value;
            }
            return result;
        }

        ///<summary>
        /// Calculates the implied quote for a swap.
        ///</summary>
        ///<param name="curveId">The curve id. This must have been created already.</param>
        ///<param name="currency">The configuration currency.
        /// This is used to get all default values and must exist in the cache.</param>
        ///<param name="baseDate">The base date.</param>
        ///<param name="spotDate">The spot date.</param>
        ///<param name="fixedLegDayCount">The daycount.</param>
        ///<param name="term">The term of the swap.</param>
        ///<param name="paymentFrequency">The payment frequency.</param>
        ///<param name="rollConvention">The roll convention. This can include EOM, but is normally the roll day.</param>
        ///<returns></returns>
        public Decimal GetSwapImpliedQuote(String curveId, String currency, DateTime baseDate, DateTime spotDate, String fixedLegDayCount,
            string term, string paymentFrequency, string rollConvention)
        {
            //Create a dummy rate.
            var bav = new BasicAssetValuation();
            var quote = BasicQuotationHelper.Create(.05m, "MarketQuote", "DecimalRate");
            bav.quote = new[] { quote };
            //create the swap.
            var swap = PriceableAssetFactory.CreateInterestRateSwap(Logger, Cache, NameSpace, currency, baseDate, spotDate,
                                                                    fixedLegDayCount, term, paymentFrequency,
                                                                    rollConvention, null, null, bav);
            //Get the cached curve.
            var ratecurve = (IRateCurve)GetCurve(curveId, false);//This requires an IPricingStructure to be cached.
            var termCurve = ratecurve.GetTermCurve();
            //Create the interpolator and get the implied quote.
            var interpolator = new TermCurveInterpolator(termCurve, baseDate, Actual365.Instance);
            var impliedQuote = swap.CalculateImpliedQuote(interpolator);
            return impliedQuote;
        }

        ///<summary>
        /// This function is a simple was to create a cap that has sequential roll dates and is adjusted, 
        /// This function does however allow different conventions on the floating side and a non-zero spread.
        /// The function will return a series of metrics for that cap, depending on what is requested.
        ///</summary>
        /// <remarks>
        /// The assumption is that only one curve is required for calculation, as the floating leg always 
        /// values to zero when including principal exchanges.
        /// </remarks>
        ///<param name="rateCurveId">The curve id. This must have been created already.</param>
        ///<param name="volCurveId">The vol curve id.</param>
        ///<param name="isCap">The isCap flag: if true, the product is a cap, if false, a floor.</param>
        ///<param name="currency">The configuration currency.
        /// This is used to get all default values and must exist in the cache.</param>
        ///<param name="baseDate">The base date.</param>
        ///<param name="indexTenor">The index tenor.</param>
        ///<param name="rollBackward">If tru, roll back from the maturity date. Otherwise roll forward from the effective date.</param>
        ///<param name="paymentBusinessDayConvention">The payment businessDayConvention.</param>
        ///<param name="strike">The strike rate.</param>
        ///<param name="lastResetsAsArray">An array of last Reset rates. This may be null.</param>
        ///<param name="includeStubFlag">Include the first reset flag.</param>
        ///<param name="dayCount">The daycount.</param>
        ///<param name="maturityTerm">The maturity term.</param>
        ///<param name="rollFrequency">THe roll frequency of the caplets.</param>
        ///<param name="notional">The notional.</param>
        ///<param name="resetPeriod">The reset period e.g. -2D.</param>
        ///<param name="paymentBusinessCentersAsString">The payment business centers.</param>
        ///<param name="resetBusinessCentersAsString">The rest business days.</param>
        ///<param name="metricsAsArray">The metrics as an array.</param>
        ///<param name="effectiveDate">The roll dates with the maturity date included..</param>
        ///<param name="resetBusinessDayConvention">The reset business day convention.</param>
        ///<param name="floatingRateIndex">The floating rate index.</param>
        ///<param name="cashFlowDetail">A detail cashflow flag - mainly for debugging.</param>
        /// <param name="properties">The properties Range: currency, baseDate and isDiscounted.</param>
        ///<returns>The value of the metric requested.</returns>
        public Object[,] GetIRCapFloorMetrics(String rateCurveId, String volCurveId,
            Boolean isCap, String currency, DateTime baseDate, DateTime effectiveDate,
            String maturityTerm, String rollFrequency, Double notional, double strike,
            List<double> lastResetsAsArray, Boolean includeStubFlag, String dayCount,
            string floatingRateIndex, string indexTenor, Boolean rollBackward,
            string paymentBusinessDayConvention, string paymentBusinessCentersAsString,
            string resetBusinessDayConvention, string resetBusinessCentersAsString,
            string resetPeriod, List<string> metricsAsArray, Boolean cashFlowDetail, NamedValueSet properties)
        {
            if (paymentBusinessDayConvention == null) throw new ArgumentNullException(nameof(paymentBusinessDayConvention));
            var result = new Dictionary<string, decimal>();
            //Remove any empty cell values.          
            List<Double> resetRates = null;
            if (lastResetsAsArray.Count > 0)
            {
                resetRates = lastResetsAsArray;
            }
            //Create the BusinessDayConvention.
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(paymentBusinessDayConvention,
                                                                             paymentBusinessCentersAsString);
            //Create the relative date offset.
            var resetOffset = RelativeDateOffsetHelper.Create(resetPeriod, DayTypeEnum.Business,
                                                              resetBusinessDayConvention, resetBusinessCentersAsString, "StartDate");
            //Create the relative date offset.
            var forecastRate = ForecastRateIndexHelper.Parse(floatingRateIndex, indexTenor);
            //create the cap/floor.
            var capfloor = isCap
                               ? PriceableAssetFactory.CreateIRCap(Logger, Cache, NameSpace, baseDate, effectiveDate, maturityTerm, currency,
                                                                     rollFrequency, includeStubFlag, notional, strike, rollBackward, resetRates,
                                                                     resetOffset, businessDayAdjustments, dayCount, forecastRate, null, null, properties)
                               : PriceableAssetFactory.CreateIRFloor(Logger, Cache, NameSpace, baseDate, effectiveDate, maturityTerm, currency,
                                                                     rollFrequency, includeStubFlag, notional, strike, rollBackward, resetRates,
                                                                     resetOffset, businessDayAdjustments, dayCount, forecastRate, null, null, properties);
            //Get the AssetControllerData.
            var market = new SwapLegEnvironment();
            var bavMetric = new BasicAssetValuation
            {
                quote =
                    metricsAsArray.Select(metric => BasicQuotationHelper.Create(0.0m, metric)).ToArray
                    ()
            };
            //Set the quote metrics.
            //Get the curve.
            var ratecurve = (IRateCurve)GetCurve(rateCurveId, false);//This requires an IPricingStructure to be cached.
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastCurve.ToString(), ratecurve);
            market.AddPricingStructure(InterestRateStreamPSTypes.DiscountCurve.ToString(), (IRateCurve)ratecurve.Clone());
            var volcurve = (IVolatilitySurface)GetCurve(volCurveId, false);//This requires an IPricingStructure to be cached.
            market.AddPricingStructure(InterestRateStreamPSTypes.ForecastIndexVolatilitySurface.ToString(), volcurve);
            //market.PricingStructureIdentifier = "CapMarket";
            var assetControllerData = new AssetControllerData(bavMetric, baseDate, market);
            //Create the interpolator and get the implied quote.
            var valuation = capfloor.Calculate(assetControllerData);
            if (valuation == null) return ArrayHelper.ConvertDictionaryTo2DArray(result);
            //Check to see if details are required.
            if (cashFlowDetail)
            {
                foreach (var metricQuote in valuation.quote)
                {
                    if (metricQuote.informationSource != null)
                    {
                        result.Add(metricQuote.measureType.Value + metricQuote.informationSource[0].rateSource.Value, metricQuote.value);
                    }
                    else
                    {
                        result.Add(metricQuote.measureType.Value, metricQuote.value);
                    }
                }
                return ArrayHelper.ConvertDictionaryTo2DArray(result);
            }
            //Return a scalar sum if details are not required.
            result = MetricsHelper.AggregateMetrics(valuation, new List<string>(metricsAsArray));
            return ArrayHelper.ConvertDictionaryTo2DArray(result);
        }

        /// <summary>
        /// Creates the specified asset.
        /// </summary>
        /// <param name="assetIdentifiers"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="coupons">The coupons.</param>
        /// <param name="maturityDates">The maturity dates.</param>
        /// <param name="frequencies">The coupon frequency.</param>
        /// <param name="ytms">The yield to maturities.</param>
        /// <param name="daycounts">The daycounts used for repo, accrual and coupons.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>The string id.</returns>
        public List<IPriceableAssetController> CreateBonds(List<string> assetIdentifiers, DateTime baseDate, List<DateTime> maturityDates, List<Decimal> coupons,
            List<string> daycounts, List<string> frequencies, List<Decimal> ytms, NamedValueSet properties)
        {
            if (assetIdentifiers.Count != maturityDates.Count && assetIdentifiers.Count != coupons.Count && assetIdentifiers.Count != daycounts.Count && assetIdentifiers.Count != ytms.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(coupons), "the rates do not match the number of assets");
            }
            var index = 0;
            var priceableAssets = new List<IPriceableAssetController>();
            foreach (var assetIdentifier in assetIdentifiers)
            {
                priceableAssets.Add(CreateBond(assetIdentifier, baseDate, maturityDates[index], coupons[index], daycounts[index], frequencies[index], ytms[index], properties));
                index++;
            }
            return priceableAssets;
        }

        /// <summary>
        /// Creates the specified asset.
        /// </summary>
        /// <param name="assetIdentifier"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="coupon">The coupon.</param>
        /// <param name="maturityDate">The maturity date..</param>
        /// <param name="frequency">The coupon frequency.</param>
        /// <param name="ytm">The ytm.</param>
        /// <param name="daycount">The daycount used for repo, accrual and coupons.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>The string id.</returns>
        public IPriceableAssetController CreateBond(string assetIdentifier, DateTime baseDate, DateTime maturityDate, Decimal coupon, 
            string daycount, string frequency, Decimal ytm, NamedValueSet properties)
        {
            properties.Set(CurveProp.BaseDate, baseDate);
            properties.Set("Maturity", maturityDate);
            properties.Set("Coupon", coupon);
            var asset = AssetHelper.ParseBond(assetIdentifier, maturityDate, coupon, daycount, frequency, ytm);
            var assetId = PropertyHelper.ExtractStringProperty("AssetId", properties);
            properties.Set("AssetId", assetId);
            var fixedRate = MarketQuoteHelper.FindQuotationByMeasureType("MarketQuote", asset.Second.quote);
            var normalisedRate = MarketQuoteHelper.NormalisePriceUnits(fixedRate, "DecimalRate");
            var instrument = InstrumentDataHelper.GetInstrumentConfigurationData(Cache, NameSpace, assetId);
            var nodeStruct = (BondNodeStruct)instrument.InstrumentNodeItem;
            string uniqueId;
            var uniqueIdentifier = properties.GetValue<string>(CurveProp.UniqueIdentifier, false);
            if (uniqueIdentifier != null)
            {
                uniqueId = uniqueIdentifier;
            }
            else
            {
                uniqueId = assetIdentifier + '-' + coupon + '-' + maturityDate.ToShortDateString();
            }
            properties.Set(CurveProp.UniqueIdentifier, uniqueId);
            var priceableAsset = new PriceableSimpleBond(baseDate, nodeStruct, null, null, normalisedRate, BondPriceEnum.YieldToMaturity);
            return priceableAsset;
        }

        /// <summary>
        /// Evaluates the metrics for asset set.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="assetReferenceKeys">The asset reference keys.</param>
        /// <param name="referenceDate">The reference date.</param>
        /// <returns></returns>
        public QuotedAssetSet EvaluateMetricsForAssetSet(List<string> metrics, string curveId,
                                                                List<string> assetReferenceKeys, DateTime referenceDate)
        {
            var qas = new QuotedAssetSet();
            var curve = GetCurve(curveId, false);
            var marketName = ((PricingStructureIdentifier)curve.GetPricingStructureId()).Market;
            var marketEnvironment = MarketEnvironmentHelper.CreateSimpleEnvironment(marketName, curveId, curve);
            var basicAssetValuations = new List<BasicAssetValuation>();
            foreach (string assetReferenceKey in assetReferenceKeys)
            {
                IPriceableAssetController priceableAsset;
                var id = NameSpace + "." + assetReferenceKey;
                var loadedObject = Cache.LoadObject<MarketInstrument>(id);
                if (loadedObject != null)
                {
                    priceableAsset = loadedObject.GetPriceableAsset();
                }
                else
                {
                    // if not in the cache then create 
                    // bonds and curveAssets will be in the cache, standard assets will not
                    Pair<Asset, BasicAssetValuation> asset = AssetHelper.Parse(assetReferenceKey, 0, 0);
                    priceableAsset = PriceableAssetFactory.Create(Logger, Cache, NameSpace, assetReferenceKey, referenceDate, asset.Second, null, null);
                }
                var controllerData = PriceableAssetFactory.CreateAssetControllerData(metrics.ToArray(), referenceDate, marketEnvironment);
                BasicAssetValuation basicAssetValuation = priceableAsset.Calculate(controllerData);
                //  Set reference to priceableAsset's id to basicAssetValuation
                basicAssetValuation.objectReference = new AnyAssetReference { href = assetReferenceKey };
                basicAssetValuations.Add(basicAssetValuation);//TODO add the assets into this?
            }
            qas.assetQuote = basicAssetValuations.ToArray();
            return qas;
        }


        ///<summary>
        ///General create function.
        ///Extract the AssetId property. eg AUD-IRSwap-3Y.
        ///Extract the Currency AUD, the AssetType IRSwap and Maturity Tenor from the InstrumentIdentifier.
        ///Query the cache with the above properties and return the Instrument set.
        ///If this set is null, return an error message, asking to create a default instrument.
        ///Can use the CreateInstrument function in Excel.
        ///Check whether there is an instrument with an ExtraItem and see if it matches with the Maturity Tenor.
        ///Need to cover the case of Future Code as the ExtraItem if a future vor futures option and floating rate index if
        ///the asset is a rate index.
        ///Find the marketQuote and the volatility? in the BAV.
        ///Extract the base date from the properties.
        ///Call the PriceableAsset create functions to create an IPriceableAssetController.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="notional">The notional.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="quotation">The quotation.</param>
        /// <returns>A priceable asset controller.</returns>
        /// <param name="fixingCalendar">The fixing calendar.</param>
        /// <param name="paymentCalendar">The payment calendar.</param>
        ///<returns></returns>
        public IPriceableAssetController CreatePriceableSurfaceAsset(Asset asset, Decimal? notional, Decimal strike, DateTime baseDate, BasicQuotation quotation,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            var result = PriceableAssetFactory.CreateSurface(Logger, Cache, NameSpace, asset, notional, strike, baseDate, quotation, fixingCalendar, paymentCalendar);
            return result;
        }

        /// <summary>
        /// Creates the priceable surface asset.
        /// </summary>
        /// <param name="strike">The strike of the option.</param>
        /// <param name="quotedAsset">The quoted asset set.</param>
        /// <param name="baseDate">The base date.</param>
        /// <returns></returns>
        public IPriceableAssetController CreatePriceableSurfaceAsset(decimal strike, Pair<Asset, BasicAssetValuation> quotedAsset,
                                                                            DateTime baseDate)
        {
            var asset = quotedAsset.First;//extract first item from QuotedAssetSet
            var basicAssetValuation = quotedAsset.Second;
            var basicQuotations = new List<BasicQuotation>(basicAssetValuation.quote);
            var bqMarketQuote = MarketQuoteHelper.FindQuotationByMeasureType("MarketQuote", basicQuotations);
            IPriceableAssetController priceableAsset = null;
            if (bqMarketQuote != null)
            {
                priceableAsset = PriceableAssetFactory.CreateSurface(Logger, Cache, NameSpace, asset, strike, baseDate, bqMarketQuote, null, null);
            }
            return priceableAsset;
        }

        ///<summary>
        ///General create function.
        ///Extract the AssetId property. eg AUD-IRSwap-3Y.
        ///Extract the Currency AUD, the AssetType IRSwap and Maturity Tenor from the InstrumentIdentifier.
        ///Query the cache with the above properties and return the Instrument set.
        ///If this set is null, return an error message, asking to create a default instrument.
        ///Can use the CreateInstrument function in Excel.
        ///Check whether there is an instrument with an ExtraItem and see if it matches with the Maturity Tenor.
        ///Need to cover the case of Future Code as the ExtraItem if a future vor futures option and floating rate index if
        ///the asset is a rate index.
        ///Find the marketQuote and the volatility? in the BAV.
        ///Extract the base date from the properties.
        ///Call the PriceableAsset create functions to create an IPriceableAssetController.
        ///</summary>
        ///<param name="baseDate">THe base Date.</param>
        ///<param name="ids">The asset ids.</param>
        ///<param name="values">The asset values.</param>
        ///<param name="fixingcalendar">The fixing calendar. If null, a new is constructed based on the business calendars.</param>
        ///<param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        ///<returns></returns>
        public List<IPriceableRateAssetController> CreatePriceableRateAssets(DateTime baseDate, string[] ids, decimal[] values,
            IBusinessCalendar fixingcalendar, IBusinessCalendar paymentCalendar)
        {
            var assets = PriceableAssetFactory.CreatePriceableRateAssets(Logger, Cache, NameSpace, baseDate, ids, values, null, fixingcalendar, paymentCalendar);
            return assets;
        }

        ///<summary>
        ///General create function.
        ///Extract the AssetId property. eg AUD-IRSwap-3Y.
        ///Extract the Currency AUD, the AssetType IRSwap and Maturity Tenor from the InstrumentIdentifier.
        ///Query the cache with the above properties and return the Instrument set.
        ///If this set is null, return an error message, asking to create a default instrument.
        ///Can use the CreateInstrument function in Excel.
        ///Check whether there is an instrument with an ExtraItem and see if it matches with the Maturity Tenor.
        ///Need to cover the case of Future Code as the ExtraItem if a future vor futures option and floating rate index if
        ///the asset is a rate index.
        ///Find the marketQuote and the volatility? in the BAV.
        ///Extract the base date from the properties.
        ///Call the PriceableAsset create functions to create an IPriceableAssetController.
        ///</summary>
        ///<param name="bav">THe basic asset valuation.</param>
        ///<param name="properties">The asset properties.</param>
        ///<param name="fixingcalendar">The fixing calendar. If null, a new is constructed based on the business calendars.</param>
        ///<param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        ///<returns></returns>
        public IPriceableAssetController CreatePriceableAsset(BasicAssetValuation bav, NamedValueSet properties,
            IBusinessCalendar fixingcalendar, IBusinessCalendar paymentCalendar)
        {
            var asset = PriceableAssetFactory.Create(Logger, Cache, NameSpace, bav, properties, fixingcalendar, paymentCalendar);
            return asset;
        }

        /// <summary>
        /// Creates the specified asset. This a factory for simple assets, where the configuration data stored
        /// in the cache is used for constructing the priceable asset.
        /// </summary>
        /// <param name="rate">The rate. In the case of a bond this would be the yield-to-maturity.</param>
        /// <param name="additional">The additional. For a future this is the volatility. For a bond this is the coupon.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public IPriceableAssetController CreatePriceableAsset(Decimal rate, Decimal additional, NamedValueSet properties)
        {
            //sets the default.
            //AssetType property - make sure it exists.
            var assetType = PropertyHelper.ExtractStringProperty("AssetType", properties);
            if (assetType == null)
            {
                return null;
            }
            Boolean isBondType = (assetType.Equals("BOND", StringComparison.OrdinalIgnoreCase));
            //make sure there is an AssetId.
            var assetIdentifier = PropertyHelper.ExtractStringProperty("AssetId", properties);
            if (assetIdentifier == null)
            {
                return null;
            }
            //check there is a maturity property if it is a bond type.
            if (isBondType)
            {
                //make sure there is a maturity for the bond.
                DateTime? bondMaturity = PropertyHelper.ExtractDateTimeProperty("Maturity", properties);
                if (bondMaturity == null)
                {
                    return null;
                }

                //make sure there is a coupon for the bond.
                Decimal? coupon = PropertyHelper.ExtractDecimalProperty("Coupon", properties);
                if (coupon == null)
                {
                    return null;
                }
            }
            //make sure there is a base date.
            var baseDate = PropertyHelper.ExtractDateTimeProperty(CurveProp.BaseDate, properties);
            if (baseDate == null)
            {
                return null;
            }
            //create the asset-basic asset valuation pair.
            var asset = AssetHelper.Parse(assetIdentifier, rate, additional);
            //create the priceable asset.
            var priceableAsset = PriceableAssetFactory.Create(Logger, Cache, NameSpace, asset.Second, properties, null, null);
            //return the cache id.
            return priceableAsset;
        }

        /// <summary>
        /// Creates the specified asset. This a factory for simple assets, where the configuration data stored
        /// in the cache is used for constructing the priceable asset.
        /// </summary>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="measureType">The additional.</param>
        /// <param name="priceQuoteUnits">The units of measure.</param>
        /// <param name="properties">the properties.</param>
        /// <returns>The string id.</returns>
        public IPriceableAssetController CreatePriceableAsset(List<Decimal> values, List<string> measureType, List<string> priceQuoteUnits,
                                         NamedValueSet properties)
        {
            var clonedNvs = new NamedValueSet(properties.ToDictionary());
            //Build the priceable asset.
            var priceableAsset = PriceableAssetFactory.BuildPriceableAsset(Logger, Cache, NameSpace, values.ToArray(), measureType.ToArray(), priceQuoteUnits.ToArray(),
                                         clonedNvs, null, null, out _);
            return priceableAsset;
        }
        /// <summary>
        /// Creates the specified asset identifiers.
        /// </summary>
        /// <param name="baseDates">The base dates.</param>
        /// <param name="rates">The rates.</param>
        /// <param name="additional">The additional.</param>
        /// <returns></returns>
        /// <param name="properties"></param>
        public List<IPriceableAssetController> CreatePriceableAssets(List<DateTime> baseDates, List<Decimal> rates,
                                           List<Decimal> additional, NamedValueSet properties)
        {
            if ((baseDates.Count != rates.Count) && (baseDates.Count != additional.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(rates), "the rates do not match the number of assets");
            }
            var index = 0;
            var priceableAssets = new List<IPriceableAssetController>();
            //There must be an assetId in the property set.
            foreach (var baseDate in baseDates)
            {
                properties.Set(CurveProp.BaseDate, baseDate);
                priceableAssets.Add(CreatePriceableAsset(rates[index], additional[index], properties));
                index++;
            }
            return priceableAssets;
        }

        /// <summary>
        /// Creates the specified assets.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="rates">The rates.</param>
        /// <param name="additional">The additional.</param>
        /// <returns></returns>
        /// <param name="properties">This must include a BaseDate.</param>
        public List<IPriceableAssetController> CreatePriceableAssets(List<string> assetIdentifiers, List<Decimal> rates,
                                            List<Decimal> additional, NamedValueSet properties)
        {
            if ((assetIdentifiers.Count != rates.Count) && (assetIdentifiers.Count != additional.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(rates), "the rates do not match the number of assets");
            }
            var index = 0;
            var priceableAssets = new List<IPriceableAssetController>();
            try
            {
                var baseDate = properties.GetValue<DateTime>(CurveProp.BaseDate, true);
                foreach (var assetIdentifier in assetIdentifiers)
                {
                    var extraProperties = PriceableAssetFactory.BuildPropertiesForAssets("Local", assetIdentifier, baseDate);
                    properties = CurveHelper.CombinePropertySetsClone(properties, extraProperties);
                    priceableAssets.Add(CreatePriceableAsset(rates[index], additional[index], properties));
                    index++;
                }
                return priceableAssets;
            }
            catch (Exception ex)
            {
                throw new Exception("No base date provided.", ex);
            }
        }

        /// <summary>
        /// Creates the specified asset identifiers.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="measureTypes">The measure types. Currently supports MarketQuote and Volatility.</param>
        /// <param name="priceQuoteUnits">The price quote units. Currently supports Rates and LogNormalVolatility.</param>
        /// <returns></returns>
        /// <param name="properties"></param>
        public List<IPriceableAssetController> CreatePriceableAssets(List<string> assetIdentifiers, List<Decimal> values,
                                            List<string> measureTypes, List<string> priceQuoteUnits, NamedValueSet properties)
        {
            if ((assetIdentifiers.Count != values.Count) && (assetIdentifiers.Count != priceQuoteUnits.Count) && (assetIdentifiers.Count != measureTypes.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(values), "the rates do not match the number of assets");
            }
            var priceableAssets = new List<IPriceableAssetController>();
            try
            {
                var baseDate = properties.GetValue<DateTime>(CurveProp.BaseDate, true);
                var uniqueIds = (IEnumerable<string>)assetIdentifiers;
                foreach (var assetIdentifier in uniqueIds.Distinct())
                {
                    var index = 0;
                    var vals = new List<Decimal>();
                    var measures = new List<String>();
                    var quotes = new List<String>();
                    foreach (var ids in assetIdentifiers)
                    {
                        index++;
                        if (ids != assetIdentifier) continue;
                        vals.Add(values[index - 1]);
                        measures.Add(measureTypes[index - 1]);
                        quotes.Add(priceQuoteUnits[index - 1]);
                    }
                    var nvs = PriceableAssetFactory.BuildPropertiesForAssets("Local", assetIdentifier, baseDate);
                    var newProperties = CurveHelper.CombinePropertySetsClone(properties, nvs);
                    priceableAssets.Add(CreatePriceableAsset(vals, measures, quotes,
                                                    newProperties));
                }
                return priceableAssets;
            }
            catch (Exception ex)
            {
                throw new Exception("No base date provided.", ex);
            }
        }

        /// <summary>
        /// Creates the specified assets.
        /// </summary>
        /// <param name="assetIdentifiers">The asset identifiers.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="rates">The rates.</param>
        /// <param name="additional">The additional.</param>
        /// <returns></returns>
        public List<IPriceableAssetController> CreatePriceableAssets(string[] assetIdentifiers, DateTime baseDate, Decimal[] rates,
                                            Decimal[] additional)
        {
            if ((assetIdentifiers.Length != rates.Length) && (assetIdentifiers.Length != additional.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(rates), "the rates do not match the number of assets");
            }
            var index = 0;
            var priceableAssets = new List<IPriceableAssetController>();
            foreach (var assetIdentifier in assetIdentifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(NameSpace, assetIdentifier, baseDate);
                priceableAssets.Add(CreatePriceableAsset(rates[index], additional[index], properties));
                index++;
            }
            return priceableAssets;
        }

        ///<summary>
        ///General create function.
        ///Extract the AssetId property. eg AUD-IRSwap-3Y.
        ///Extract the Currency AUD, the AssetType IRSwap and Maturity Tenor from the InstrumentIdentifier.
        ///Query the cache with the above properties and return the Instrument set.
        ///If this set is null, return an error message, asking to create a default instrument.
        ///Can use the CreateInstrument function in Excel.
        ///Check whether there is an instrument with an ExtraItem and see if it matches with the Maturity Tenor.
        ///Need to cover the case of Future Code as the ExtraItem if a future vor futures option and floating rate index if
        ///the asset is a rate index.
        ///Find the marketQuote and the volatility? in the BAV.
        ///Extract the base date from the properties.
        ///Call the PriceableAsset create functions to create an IPriceableAssetController.
        ///</summary>
        ///<param name="bav">THe basic asset valuation.</param>
        ///<param name="properties">The asset properties.</param>
        ///<returns></returns>
        public IPriceableAssetController CreatePriceableAsset(BasicAssetValuation bav, NamedValueSet properties)
        {
            var asset = PriceableAssetFactory.Create(Logger, Cache, NameSpace, bav, properties, null, null);
            return asset;
        }

        #endregion

        #region Instruments

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="discountingType">The discounting type.</param>
        /// <param name="effectiveDate">The base date.</param>
        /// <param name="tenor">The maturity tenor.</param>
        /// <param name="fxdDayFraction">The fixed leg day fraction.</param>
        /// <param name="businessCenters">The payment business centers.</param>
        /// <param name="businessDayConvention">The payment business day convention.</param>
        /// <param name="fxdFrequency">The business day adjustments.</param>
        /// <param name="underlyingRateIndex">Index of the rate.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="currency">THe currency.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="paymentCalendar">The paymentCalendar.</param>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        public PriceableSimpleIRSwap CreateSimpleIRSwap(string id, DateTime baseDate, string currency,
                                     decimal amount, DiscountingTypeEnum? discountingType,
                                     DateTime effectiveDate, string tenor, string fxdDayFraction,
                                     string businessCenters, string businessDayConvention, string fxdFrequency,
                                     RateIndex underlyingRateIndex, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicQuotation fixedRate)
        {
            var instrument = PriceableAssetFactory.CreateSimpleIRSwap(Logger, Cache, NameSpace, id, baseDate, currency, amount, discountingType,
                effectiveDate, tenor, fxdDayFraction, businessCenters, businessDayConvention, fxdFrequency, underlyingRateIndex, fixingCalendar, paymentCalendar, fixedRate);
            return instrument;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        /// </summary>
        /// <param name="fixedLegDayCount">The fixed leg daycount basis.</param>
        /// <param name="term">The term of the swap.</param>
        /// <param name="paymentFrequency">The payment frequency.</param>
        /// <param name="rollConvention">The roll convention.</param>
        /// <param name="bav">The basic asset valuation.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="spotDate">The spot date.</param>
        /// <param name="fixingCalendar">The fixing calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        public IPriceableAssetController CreateInterestRateSwap(String currency, DateTime baseDate, DateTime spotDate, String fixedLegDayCount,
            string term, string paymentFrequency, string rollConvention, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, BasicAssetValuation bav)
        {
            var instrument = PriceableAssetFactory.CreateInterestRateSwap(Logger, Cache, NameSpace, currency, baseDate, spotDate, fixedLegDayCount, term, paymentFrequency, rollConvention, fixingCalendar, paymentCalendar, bav);
            return instrument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleIRSwap"/> class.
        /// </summary>
        /// <param name="notionalWeights">THe notional weights.</param>
        /// <param name="fixedLegDayCount">The fixed leg daycount basis.</param>
        /// <param name="fixingDateAdjustments">The fixingDateAdjustments.</param>
        /// <param name="bav">The basic asset valuation.</param>
        /// <param name="identifier">The swap configuration identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="notional">The actual first notional. THis must be consistent with the weights.</param>
        /// <param name="dates">The dates.</param>
        public IPriceableAssetController CreateInterestRateSwap(String identifier, DateTime baseDate,
            Money notional, DateTime[] dates, Decimal[] notionalWeights, String fixedLegDayCount,
            BusinessDayAdjustments fixingDateAdjustments, BasicAssetValuation bav)
        {
            var instrument = PriceableAssetFactory.CreateInterestRateSwap(identifier, baseDate, notional, dates, notionalWeights, fixedLegDayCount, fixingDateAdjustments, bav);
            return instrument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleBond"/> class.
        /// </summary>
        /// <param name="maturityDate">THe maturity Date.</param>
        /// <param name="dayCountFraction">The fixed leg daycount basis.</param>
        /// <param name="couponFrequency">The coupon frequency.</param>
        /// <param name="issuerName">The issuer name.</param>
        /// <param name="rollConvention">The roll convention e.g. FOLLOWING.</param>
        /// <param name="businessCenters">The business centers e.g. AUSY.</param>
        /// <param name="ytm">The ytm.</param>
        /// <param name="curve">The curve to use for calculations.</param>
        /// <param name="identifier">The identifier of the bond.</param>
        /// <param name="valuationDate">The base date and valuation Date.</param>
        /// <param name="settlementDate">The settlement date.</param>
        /// <param name="exDivDate">The next ex div date.</param>
        /// <param name="notional">The actual first notional. This must be consistent with the weights.</param>
        /// <param name="coupon">The coupon.</param>
        /// <param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        public Decimal GetBondAssetSwapSpread(string identifier, DateTime valuationDate, DateTime settlementDate, DateTime exDivDate,
            Money notional, Decimal coupon, DateTime maturityDate, String dayCountFraction, string couponFrequency, String issuerName,
            string rollConvention, string businessCenters, IRateCurve curve, Decimal ytm, IBusinessCalendar paymentCalendar)
        {
            var asw = PriceableAssetFactory.GetBondAssetSwapSpread(Logger, Cache, NameSpace, identifier, valuationDate, settlementDate, exDivDate, notional, coupon, maturityDate,
                dayCountFraction, couponFrequency, issuerName, rollConvention, businessCenters, curve, ytm, paymentCalendar);
            return asw;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Assets.PriceableIRSwap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="spotDate">the spot date.</param>
        /// <param name="notional">The notional amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="paymentBusinessDayAdjustments">The business day adjustments.</param>
        /// <param name="fixingDateOffset">The fixing date business day adjustments.</param>
        /// <param name="floatingResetRates">The reset rates.</param>
        /// <param name="fixingCalendar">The fixing calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="paymentCalendar">The payment calendar. If null, a new is constructed based on the business calendars.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        /// <param name="spread">The spread on the floating leg.</param>
        /// <param name="properties">The properties Range: currency, baseDate and isDiscounted.</param>
        public PriceableIRSwap CreateIRSwap(DateTime baseDate, DateTime spotDate,
                                Double notional, Currency currency, BusinessDayAdjustments paymentBusinessDayAdjustments,
                                RelativeDateOffset fixingDateOffset, List<Decimal> floatingResetRates,
                                IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar,
                                BasicQuotation fixedRate, BasicQuotation spread, NamedValueSet properties)
        {
            var instrument = PriceableAssetFactory.CreateIRSwap(Logger, Cache, NameSpace, spotDate, notional,
                paymentBusinessDayAdjustments, fixingDateOffset, floatingResetRates, fixingCalendar, paymentCalendar, fixedRate, spread, properties);
            return instrument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRSwap"/> class.
        /// </summary>
        /// <param name="fixedNotionals">The fixed notional. This must match the relevant date array i.e. count - 1</param>
        /// <param name="floatingResetRates">An array of override rest rates for the floating leg.</param>
        /// <param name="floatingLegDayCount">The floating leg daycount.</param>
        /// <param name="dateAdjustments">The fixingDateAdjustments.</param>
        /// <param name="bav">The basic asset valuation.</param>
        /// <param name="fixedRollDates">The fixed roll dates, including the final maturity.</param>
        /// <param name="floatingRollDates">The floating roll dates, including the final maturity.</param>
        /// <param name="floatingNotionals">The floating leg notional. This must match the relevant date array i.e. count - 1.</param>
        /// <param name="properties">The properties Range: currency, baseDate and isDiscounted.</param>
        public PriceableIRSwap CreateIRSwap(List<DateTime> fixedRollDates, List<Decimal> fixedNotionals,
            List<DateTime> floatingRollDates, List<Decimal> floatingNotionals, List<Decimal> floatingResetRates,
            String floatingLegDayCount, BusinessDayAdjustments dateAdjustments, BasicAssetValuation bav, NamedValueSet properties)
        {
            var instrument = PriceableAssetFactory.CreateIRSwap(fixedRollDates, 
                fixedNotionals, floatingRollDates, floatingNotionals, 
                floatingResetRates, dateAdjustments, bav, properties);
            return instrument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRCap"/> class.
        /// </summary>
        /// <param name="includeStubFlag">A flag. If true: include the first caplet.</param>
        /// <param name="notional">The notional.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="rollBackwardFlag">A flag, if true, rolls the dates backward from the maturity date. 
        /// Currently, rollForward is not implemented.</param>
        /// <param name="lastResets">A list of reset rates. This may be null.</param>
        /// <param name="resetConvention">The reset Convention.</param>
        /// <param name="dayCount">The leg daycount.</param>
        /// <param name="paymentDateAdjustments">The payment Date Adjustments.</param>
        /// <param name="floatingIndex">The floating index.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="term">THe term of the cap.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="effectiveDate">The effective Date.</param>
        /// <param name="paymentFrequency">The cap roll frequency.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="properties">The properties Range: currency, baseDate and isDiscounted.</param>
        public PriceableIRCap CreateIRCap(DateTime baseDate,
            DateTime effectiveDate, string term, string currency, string paymentFrequency,
            Boolean includeStubFlag, double notional, double strike, Boolean rollBackwardFlag,
            List<double> lastResets, RelativeDateOffset resetConvention,
            BusinessDayAdjustments paymentDateAdjustments, string dayCount, ForecastRateIndex floatingIndex,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, NamedValueSet properties)
        {
            var instrument = PriceableAssetFactory.CreateIRCap(Logger, Cache, NameSpace, baseDate, effectiveDate, term, currency, paymentFrequency, includeStubFlag, notional, strike,
                rollBackwardFlag, lastResets, resetConvention, paymentDateAdjustments, dayCount, floatingIndex, fixingCalendar, paymentCalendar, properties);
            return instrument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRFloor"/> class.
        /// </summary>
        /// <param name="includeStubFlag">A flag. If true: include the first caplet.</param>
        /// <param name="notional">The notional.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="rollBackwardFlag">A flag, if true, rolls the dates backward from the maturity date. 
        /// Currently, rollForward is not implemented.</param>
        /// <param name="lastResets">A list of reset rates. This may be null.</param>
        /// <param name="resetConvention">The reset Convention.</param>
        /// <param name="dayCount">The leg daycount.</param>
        /// <param name="paymentDateAdjustments">The payment Date Adjustments.</param>
        /// <param name="floatingIndex">The floating index.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="term">THe term of the cap.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="effectiveDate">The effective Date.</param>
        /// <param name="paymentFrequency">The cap roll frequency.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="properties">The properties Range: currency, baseDate and isDiscounted.</param>
        public PriceableIRFloor CreateIRFloor(DateTime baseDate,
            DateTime effectiveDate, string term, string currency, string paymentFrequency,
            Boolean includeStubFlag, double notional, double strike, Boolean rollBackwardFlag,
            List<double> lastResets, RelativeDateOffset resetConvention,
            BusinessDayAdjustments paymentDateAdjustments, string dayCount, ForecastRateIndex floatingIndex,
            IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar, NamedValueSet properties)
        {
            var instrument = PriceableAssetFactory.CreateIRFloor(Logger, Cache, NameSpace, baseDate, effectiveDate, term, currency, paymentFrequency, includeStubFlag, notional, strike,
                rollBackwardFlag, lastResets, resetConvention, paymentDateAdjustments, dayCount, floatingIndex, fixingCalendar, paymentCalendar, properties);
            return instrument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRCap"/> class.
        /// </summary>
        /// <param name="notionals">The notionals.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="resets">An array of reset rates. This may be null.</param>
        /// <param name="resetConvention">The reset Convention.</param>
        /// <param name="dayCount">The leg daycount.</param>
        /// <param name="paymentDateAdjustments">The payment Date Adjustments.</param>
        /// <param name="floatingIndex">The floating index.</param>
        /// <param name="identifier">The swap configuration identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="rollDates">The roll dates.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="properties">The properties Range: currency, baseDate and isDiscounted.</param>
        public PriceableIRCap CreateIRCap(String identifier, DateTime baseDate, string currency,
            List<DateTime> rollDates, List<double> notionals, List<double> strikes, List<double> resets, RelativeDateOffset resetConvention,
            BusinessDayAdjustments paymentDateAdjustments, string dayCount, ForecastRateIndex floatingIndex, IBusinessCalendar fixingCalendar, NamedValueSet properties)
        {
            var instrument = PriceableAssetFactory.CreateIRCap(Logger, Cache, NameSpace, identifier, baseDate, currency, rollDates, notionals, strikes, resets,
                resetConvention, paymentDateAdjustments, dayCount, floatingIndex, fixingCalendar, properties);
            return instrument;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRFloor"/> class.
        /// </summary>
        /// <param name="notionals">The notionals.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="resets">An array of reset rates. This may be null.</param>
        /// <param name="resetConvention">The reset Convention.</param>
        /// <param name="dayCount">The leg daycount.</param>
        /// <param name="paymentDateAdjustments">The payment Date Adjustments.</param>
        /// <param name="floatingIndex">The floating index.</param>
        /// <param name="identifier">The swap configuration identifier.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="rollDates">The roll dates.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="properties">The properties Range: currency, baseDate and isDiscounted.</param>
        public PriceableIRFloor CreateIRFloor(String identifier, DateTime baseDate, string currency,
            List<DateTime> rollDates, List<double> notionals, List<double> strikes, List<double> resets, RelativeDateOffset resetConvention,
            BusinessDayAdjustments paymentDateAdjustments, string dayCount, ForecastRateIndex floatingIndex, IBusinessCalendar fixingCalendar, NamedValueSet properties)
        {
            var instrument = PriceableAssetFactory.CreateIRFloor(Logger, Cache, NameSpace, identifier, baseDate, currency, rollDates, notionals, strikes, resets,
                resetConvention, paymentDateAdjustments, dayCount, floatingIndex, fixingCalendar, properties);
            return instrument;
        }

        #endregion

        #region Transient Data Using Private ICoreCache - Should get rid of all private pricingstructures

        ///<summary>
        /// Sets the priceable asset into the cache.
        ///</summary>
        ///<param name="uniqueId">The unique Id.</param>
        ///<returns></returns>
        public ICoreItem GetPrivatePriceableAsset(string uniqueId)
        {
            var id = NameSpace + "." + uniqueId;
            return Cache.LoadPrivateItem<AssetControllerBase>(id);
        }

        /// <summary>
        /// Gets the priceable asset.
        /// </summary>
        /// <param name="requestProperties">The request Properties.</param>
        /// <returns></returns>
        public List<ICoreItem> GetPrivatePriceableAssets(NamedValueSet requestProperties)
        {
            // Create the querying filter
            if (requestProperties == null) throw new ArgumentNullException(nameof(requestProperties));
            //The new filter with OR on arrays..
            requestProperties.Set(EnvironmentProp.NameSpace, NameSpace);
            var queryExpr = Expr.BoolAND(requestProperties);
            var loadedObjects = Cache.LoadPrivateItems<MarketInstrument>(queryExpr);
            if (0 == loadedObjects.Count)
            {
                var message = $"The search using this expression '{queryExpr.DisplayString()}' yielded no results.";
                throw new ApplicationException(message);
            }
            return loadedObjects;
        }

        ///<summary>
        /// Sets the priceable asset into the cache.
        ///</summary>
        ///<param name="uniqueId">The unique Id.</param>
        ///<param name="priceableAsset">The asset.</param>
        ///<param name="properties">The properties.</param>
        ///<returns></returns>
        public void SavePrivatePriceableAsset(string uniqueId, AssetControllerBase priceableAsset, NamedValueSet properties)
        {
            var marketInstrument = new MarketInstrument(priceableAsset.GetType().Name, priceableAsset);
            var id = NameSpace + "." + uniqueId;
            Cache.SavePrivateObject(marketInstrument, id, properties);
        }

        ///// <summary>
        ///// Sets the curve.
        ///// </summary>
        ///// <param name="uniqueId">The uniqueId.</param>
        ///// <returns></returns>
        //public ICoreItem GetPrivatePricingStructure(string uniqueId)
        //{
        //    return _localStore.LoadPrivateItem<PricingStructureBase>(uniqueId);
        //}

        ///// <summary>
        ///// Gets the curves.
        ///// </summary>
        ///// <param name="requestProperties">The requestProperties.</param>
        ///// <returns></returns>
        //public List<ICoreItem> GetPrivatePricingStructures(NamedValueSet requestProperties)
        //{
        //    // Create the querying filter
        //    if (requestProperties == null) throw new ArgumentNullException("requestProperties");
        //    //The new filter with OR on arrays..
        //    var queryExpr = Expr.BoolAND(requestProperties);
        //    return _localStore.LoadPrivateItems<PricingStructureBase>(queryExpr);
        //}

        ///// <summary>
        ///// Sets the curve.
        ///// </summary>
        ///// <param name="pricingStructure">The curve.</param>
        ///// <returns></returns>
        //public string SavePrivatePricingStructure(PricingStructureBase pricingStructure)
        //{
        //    var id = (PricingStructureIdentifier)pricingStructure.GetPricingStructureId();
        //    var uniqueId = id.UniqueIdentifier;
        //    NamedValueSet properties = pricingStructure.GetPricingStructureId().Properties;
        //    _localStore.SavePrivateObject(pricingStructure, uniqueId, properties);
        //    return uniqueId;
        //}

        ///// <summary>
        ///// Creates the specified curve type.
        ///// </summary>
        ///// <param name="properties">The properties.</param>
        ///// <param name="instruments">The instruments.</param>
        ///// <param name="values">The adjusted rates.</param>
        ///// <param name="measureTypes">The measure types.</param>
        ///// <param name="priceQuoteUnits">The price quote units.</param>
        ///// <returns></returns>
        //public string CreatePrivateRateCurve(NamedValueSet properties, string[] instruments, decimal[] values,
        //                            string[] measureTypes, string[] priceQuoteUnits)
        //{
        //    var rateCurve = new RateCurve(properties, instruments, values, measureTypes, priceQuoteUnits);
        //    return SavePrivatePricingStructure(rateCurve);
        //}


        ///// <summary>
        ///// Creates the specified curve type.
        ///// </summary>
        ///// <param name="properties">The properties.</param>
        ///// <param name="instruments">The instruments.</param>
        ///// <param name="values"></param>
        ///// <returns></returns>
        //public string CreatePrivatePricingStructure(NamedValueSet properties, string[] instruments, decimal[] values)
        //{
        //    return CreatePrivatePricingStructure(properties, instruments, values, null);
        //}

        ///// <summary>
        ///// Creates the specified curve type.
        ///// </summary>
        ///// <param name="properties">The properties.</param>
        ///// <param name="instruments">The instruments.</param>
        ///// <param name="values">The adjusted rates.</param>
        ///// <param name="additional">The additional.</param>
        ///// <returns></returns>
        //public string CreatePrivatePricingStructure(NamedValueSet properties, string[] instruments,
        //                                               decimal[] values, decimal[] additional)
        //{
        //    var pricingStructure = CreateCurve(properties, instruments, values, additional);
        //    var psId = (PricingStructureIdentifier)pricingStructure.GetPricingStructureId();
        //    if (psId.UniqueIdentifier == null)
        //    {
        //        throw new InvalidOperationException("The pricing structure was not built.");
        //    }
        //    return SavePrivatePricingStructure((PricingStructureBase)pricingStructure);
        //}

        ///// <summary>
        ///// Gets the value assuming the curve base date.
        ///// </summary>
        ///// <param name="pricingStructureId"></param>
        ///// <param name="targetDate">The target date.</param>
        ///// <returns></returns>
        //public Double GetValue(string pricingStructureId, DateTime targetDate)
        //{
        //    var pricingStructure = (PricingStructureBase)GetPrivatePricingStructure(pricingStructureId).Data;
        //    IPoint point = new DateTimePoint1D(targetDate);
        //    return (double)pricingStructure.GetValue(point).Value;
        //}

        ///// <summary>
        ///// Gets the value assuming the curve base date.
        ///// </summary>
        ///// <param name="pricingStructureId"></param>
        ///// <param name="targetDates">The target dates.</param>
        ///// <returns></returns>
        //public List<Double> GetValues(string pricingStructureId, DateTime[] targetDates)
        //{
        //    var pricingStructure = (PricingStructureBase)GetPrivatePricingStructure(pricingStructureId).Data;

        //    return targetDates.Select(date => new DateTimePoint1D(date)).Select(point => (double)pricingStructure.GetValue(point).Value).ToList();
        //}

        ///// <summary>
        ///// Gets the value from a base date.
        ///// </summary>
        ///// <param name="pricingStructureId"></param>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="targetDate">The target date.</param>
        ///// <returns></returns>
        //public Double GetValue(string pricingStructureId, DateTime baseDate,
        //                              DateTime targetDate)
        //{
        //    var pricingStructure = (PricingStructureBase)GetPrivatePricingStructure(pricingStructureId).Data;
        //    IPoint point = new DateTimePoint1D(baseDate, targetDate);
        //    return (double)pricingStructure.GetValue(point).Value;
        //}

        ///// <summary>
        ///// Gets the value from a base date.
        ///// </summary>
        ///// <param name="pricingStructureId"></param>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="targetDates">The target dates.</param>
        ///// <returns></returns>
        //public List<Double> GetValues(string pricingStructureId, DateTime baseDate,
        //                              DateTime[] targetDates)
        //{
        //    var pricingStructure = (PricingStructureBase)GetPrivatePricingStructure(pricingStructureId).Data;
        //    return targetDates.Select(date => new DateTimePoint1D(baseDate, date)).Select(point => (double)pricingStructure.GetValue(point).Value).ToList();
        //}

        ///// <summary>
        ///// Gets the value from a base date.
        ///// </summary>
        ///// <param name="pricingStructureId"></param>
        ///// <param name="valuationDate">The base date.</param>
        ///// <param name="targetDate">The target date.</param>
        ///// <returns></returns>
        //public Double GetHorizonValue(string pricingStructureId, DateTime valuationDate,
        //                              DateTime targetDate)
        //{
        //    var pricingStructure = (PricingStructureBase)GetPrivatePricingStructure(pricingStructureId).Data;
        //    var baseDate = pricingStructure.GetBaseDate();
        //    IPoint point = new DateTimePoint1D(baseDate, valuationDate);
        //    var df = (double)pricingStructure.GetValue(point).Value;
        //    IPoint point2 = new DateTimePoint1D(baseDate, targetDate);
        //    var df1 = (double)pricingStructure.GetValue(point2).Value;
        //    return df1 / df;
        //}

        ///// <summary>
        ///// Gets the value from a base date.
        ///// </summary>
        ///// <param name="pricingStructureId"></param>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="targetDate">The target date.</param>
        ///// <returns></returns>
        //public Double GetValue2(string pricingStructureId, DateTime baseDate,
        //                               DateTime targetDate)
        //{
        //    var pricingStructure = (PricingStructureBase)GetPrivatePricingStructure(pricingStructureId).Data;
        //    IPoint point = new DateTimePoint1D(baseDate, targetDate);
        //    return (double)pricingStructure.GetValue(point).Value;
        //}

        #endregion

        #region Load General Configuration Data

        /// <summary>
        /// Caches the instruments from the database.
        /// </summary>
        private void CacheBusinessCenterCalendars()
        {
            IExpression queryExpr = Expr.IsEQU(EnvironmentProp.NameSpace, NameSpace);
            List<ICoreItem> items = Cache.LoadItems<BusinessCenterCalendar>(queryExpr);
            if (items.Count == 0)
            {
                throw new ApplicationException(
                    $"The search using the query '{Expr.ALL.DisplayString()}' yielded no results.");
            }
        }

        /// <summary>
        /// Caches the instruments from the database.
        /// </summary>
        public void CacheHolidays()
        {
            IExpression queryExpr = Expr.IsEQU(EnvironmentProp.NameSpace, NameSpace);
            List<ICoreItem> items = Cache.LoadItems<BusinessCenterCalendar>(queryExpr);
            if (items.Count == 0)
            {
                throw new ApplicationException(
                    $"The search using the query '{queryExpr.DisplayString()}' yielded no results.");
            }
        }

        /// <summary>
        /// Caches the instruments from the database.
        /// </summary>
        private void CacheInstruments()
        {
            IExpression queryExpr = Expr.IsEQU(EnvironmentProp.NameSpace, NameSpace);
            List<ICoreItem> items = Cache.LoadItems<Instrument>(queryExpr);
            if (items.Count == 0)
            {
                throw new ApplicationException(
                    $"The search using the query '{queryExpr.DisplayString()}' yielded no results.");
            }
        }

        /// <summary>
        /// Caches the instruments from the database.
        /// </summary>
        private void CacheAlgorithms()
        {
            IExpression queryExpr = Expr.IsEQU(EnvironmentProp.NameSpace, NameSpace);
            List<ICoreItem> items = Cache.LoadItems<Algorithm>(queryExpr);
            if (items.Count == 0)
            {
                throw new ApplicationException(
                    $"The search using the query '{queryExpr.DisplayString()}' yielded no results.");
            }
        }

        ///// <summary>
        ///// Get the scenario rules from the cache
        ///// </summary>
        ///// <returns></returns>
        //private void CacheScenarioRules()
        //{
        //    List<ICoreItem> items = _Cache.LoadItems<ScenarioRule>(Expr.ALL);
        //    if (items.Count == 0)
        //    {
        //        throw new ApplicationException(String.Format(
        //            "The search using the query '{0}' yielded no results.", Expr.ALL.DisplayString()));
        //    }
        //}

        /// <summary>
        /// Gets the central bank date rules.
        /// </summary>
        /// <returns></returns>
        public void CacheDateRules()
        {
            IExpression queryExpr = Expr.IsEQU(EnvironmentProp.NameSpace, NameSpace);
            List<ICoreItem> items = Cache.LoadItems<DateRules>(queryExpr);
            if (items.Count == 0)
            {
                throw new ApplicationException(
                    $"The search using the query '{queryExpr.DisplayString()}' yielded no results.");
            }
        }

        #endregion

        #region Pricing Structure Creators

        /// <summary>
        /// Creates the specified curve type.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="adjustedRates">The adjusted rates.</param>
        /// <param name="dfDates">The df Dates.</param>
        /// <param name="dfs">The dfs.</param>
        /// <returns></returns>
        public IPricingStructure CreateFincadRateCurve(NamedValueSet properties, string[] instruments, Decimal[] adjustedRates,
                                    DateTime[] dfDates, decimal[] dfs)
        {
            var rateCurveId = new RateCurveIdentifier(properties);
            var psh = GetAlgorithmHolder(PricingStructureTypeEnum.RateCurve, rateCurveId.Algorithm);
            var rateCurve = new RateCurve(properties, psh, instruments, adjustedRates, dfDates, dfs);
            return rateCurve;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public PricingStructureAlgorithmsHolder GetGenericRateCurveAlgorithmHolder()
        {
            var holder = new GenericRateCurveAlgorithmHolder(Logger, Cache, NameSpace);
            return holder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pricingStructureType"></param>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        public PricingStructureAlgorithmsHolder GetAlgorithmHolder(PricingStructureTypeEnum pricingStructureType, string algorithm)
        {
            var holder = new PricingStructureAlgorithmsHolder(Logger, Cache, NameSpace, pricingStructureType, algorithm);
            return holder;
        }

        ///<summary>
        ///</summary>
        ///<param name="propertyNames"></param>
        ///<param name="propertyValues"></param>
        ///<param name="instruments"></param>
        ///<param name="rates"></param>
        ///<param name="interpolationFrequency"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public object[][] CreateZeroCurve(string[] propertyNames, object[] propertyValues, string[] instruments, double[] rates, string interpolationFrequency)
        {
            var structure = PricingStructureFactory.CreateZeroCurve(Logger, Cache, NameSpace, null, null, propertyNames, propertyValues, instruments, rates, interpolationFrequency);
            return structure;
        }

        /// <summary>
        /// Create a pricing structures
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="headers">THe value headers</param>
        /// <param name="values"></param>
        /// <returns></returns>
        public List<IPricingStructure> CreatePricingStructures(NamedValueSet properties, IList<String> headers, object[,] values)
        {
            var structures = PricingStructureFactory.CreatePricingStructures(Logger, Cache, NameSpace, null, null, properties, headers, values);
            return structures;
        }

        /// <summary>
        /// Create a pricing structure
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public IPricingStructure CreatePricingStructure(NamedValueSet properties, object[,] values)
        {
            var structure = PricingStructureFactory.CreatePricingStructure(Logger, Cache, NameSpace, null, null, properties, values);
            return structure;
        }

        /// <summary>
        /// Create a pricing structure
        /// </summary>
        /// <param name="properties">The curve properties.</param>
        /// <param name="values">The instrument values.</param>
        /// <param name="algorithm">THe algorithm to use.</param>
        /// <returns></returns>
        public IPricingStructure CreatePricingStructure(NamedValueSet properties, object[,] values, Algorithm algorithm )
        {
            var structure = PricingStructureFactory.CreatePricingStructure(Logger, Cache, NameSpace, null, null, properties, values, algorithm);
            return structure;
        }

        /// <summary>
        /// Create a pricing structure
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="values"></param>
        /// <param name="additional"></param>
        /// <returns></returns>
        public IPricingStructure CreatePricingStructure(NamedValueSet properties, object[,] values, object[,] additional)
        {
            var structure = PricingStructureFactory.CreatePricingStructure(Logger, Cache, NameSpace, null, null, properties, values, additional);
            return structure;
        }

        /// <summary>
        /// Create a pricing structure
        /// </summary>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="properties"></param>
        /// <param name="values">A range object that contains the instruments and quotes.</param>
        /// <returns></returns>
        public IPricingStructure CreatePricingStructure(IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, 
            NamedValueSet properties, object[,] values)
        {
            var structure = PricingStructureFactory.CreatePricingStructure(Logger, Cache, NameSpace, fixingCalendar, rollCalendar, properties, values);
            return structure;
        }

        /// <summary>
        /// Create a pricing structure
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="values">A range object that contains the instruments and quotes.</param>
        /// <returns></returns>
        public IPricingStructure CreateGenericPricingStructure(NamedValueSet properties, object[,] values)
        {
            var structure = PricingStructureFactory.CreateGenericPricingStructure(Logger, Cache, NameSpace, null, null, properties, values);
            return structure;
        }

        /// <summary>
        /// Creates the curve.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="values">The adjusted rates.</param>
        /// <param name="measures">The measures.</param>
        /// <param name="priceQuoteUnits">The priceQuoteUnits.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <returns></returns>
        public IPricingStructure CreateCurve(NamedValueSet properties, string[] instruments,
                            Decimal[] values, string[] measures, string[] priceQuoteUnits, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            var quotedAssetSet = AssetHelper.Parse(instruments, values, measures, priceQuoteUnits);
            var structure = PricingStructureFactory.CreateCurve(Logger, Cache, NameSpace, fixingCalendar, paymentCalendar, properties, quotedAssetSet);
            return structure;
        }

        /// <summary>
        /// Creates the curve.
        /// </summary>
        /// <param name="referenceCurve">The referenceCurve.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="quotedAssetSet">The QuotedAssetSet.</param>
        /// <returns></returns>
        public RateBasisCurve CreateRateBasisCurve(IRateCurve referenceCurve, NamedValueSet properties, QuotedAssetSet quotedAssetSet)
        {
            var structure = new RateBasisCurve(Logger, Cache, NameSpace, referenceCurve, quotedAssetSet, properties, null, null);
            return structure;
        }

        /// <summary>
        /// Creates the curve.
        /// </summary>
        /// <param name="referenceDiscountingCurve">The reference Curve. Normally, the base OIS curve</param>
        /// <param name="properties">The properties.</param>
        /// <param name="quotedAssetSet">The QuotedAssetSet.</param>
        /// <returns></returns>
        public ClearedRateCurve CreateClearedRateBasisCurve(IRateCurve referenceDiscountingCurve, NamedValueSet properties, QuotedAssetSet quotedAssetSet)
        {
            var structure = new ClearedRateCurve(Logger, Cache, NameSpace, referenceDiscountingCurve, quotedAssetSet, properties, null, null);
            return structure;
        }

        /// <summary>
        /// Creates the curve.
        /// </summary>
        /// <param name="referenceCurve">The referenceCurve.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="quotedAssetSet">The QuotedAssetSet.</param>
        ///  <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <returns></returns>
        public RateSpreadCurve CreateRateSpreadCurve(IRateCurve referenceCurve, NamedValueSet properties, QuotedAssetSet quotedAssetSet, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            var structure = new RateSpreadCurve(Logger, Cache, NameSpace, referenceCurve, quotedAssetSet, properties, fixingCalendar, paymentCalendar);
            return structure;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateSpreadCurve"/> class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="refCurve">The reference parent curve id.</param>
        /// <param name="value">The values.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateSpreadCurve CreateRateSpreadCurve(NamedValueSet properties, IPricingStructure refCurve, Decimal value,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var structure = new RateSpreadCurve(Logger, Cache, NameSpace, properties, refCurve, value, fixingCalendar, rollCalendar);
            return structure;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateSpreadCurve"/> class.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="refCurve">The reference parent curve id.</param>
        /// <param name="values">The values.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateSpreadCurve CreateRateSpreadCurve(NamedValueSet properties, IPricingStructure refCurve, Decimal[] values,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var structure = new RateSpreadCurve(Logger, Cache, NameSpace, properties, refCurve, values, fixingCalendar, rollCalendar);
            return structure;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XccySpreadCurve"/> class, 
        /// by applying spreads to an existing RateCurve. Using FX Curve to create synthetic swaps
        /// for the period under 1Y.
        /// </summary>
        /// <param name="properties">The properties of the new curve.</param>
        /// <param name="baseCurve">The base zero curve.</param>
        /// <param name="quoteCurve">The quote zero curve.</param>
        /// <param name="fxCurve">The FX curve, used for constructing synthetic deposits</param>
        /// <param name="instruments">The spread instruments.</param>
        /// <param name="values">The spread values.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public XccySpreadCurve CreateXccySpreadCurve(NamedValueSet properties, IRateCurve baseCurve, IRateCurve quoteCurve, FxCurve fxCurve,
                               string[] instruments, decimal[] values, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var structure = new XccySpreadCurve(Logger, Cache, NameSpace, properties, baseCurve, quoteCurve, fxCurve, instruments, values, fixingCalendar, rollCalendar);
            return structure;
        }

        /// <summary>
        /// Creates synthetic swaps from FX curve for period under 1 year
        /// </summary>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="baseCurve"></param>
        /// <param name="basisAdjustedDiscountCurve"></param>
        /// <param name="currency"></param>
        /// <param name="baseDate"></param>
        /// <param name="swapsRequired">Array of the names of the swaps required</param>
        /// <returns></returns>
        public List<IPriceableRateAssetController> CreateSyntheticSwaps(IRateCurve baseCurve, RateCurve basisAdjustedDiscountCurve, 
            string currency, DateTime baseDate, string[] swapsRequired, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var structure = XccySpreadCurve.CreateSyntheticSwaps(Logger, Cache, NameSpace, baseCurve, basisAdjustedDiscountCurve, currency, baseDate, swapsRequired, fixingCalendar, rollCalendar);
            return structure;
        }

        /// <summary>
        /// Creates a new Discount Curve by adjusting an existing Rate Curve.
        /// </summary>
        /// <param name="baseCurve">The original rate curve the new curve is based on.</param>
        /// <param name="properties">The properties of the new curve.</param>
        /// <param name="spreads">The adjustments required.</param>
        /// <returns>The newly created curve.</returns>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="rollCalendar">The payment Calendar.</param>
        /// <remarks>
        /// Create a new extended curve (Ca) from the base curve with extra points where the spreads are
        /// Solve to find the zero rate spreads (Sa) required, interpolating between points
        /// Retrieve the zero rates (Za) from curve Ca 
        /// Add on the zero rate spreads Sa  onto Za, creating a new set of zero rates (Zb)
        /// Convert the zero rates Zb into discount factors (Db)
        /// Create a new rate curve from Db
        /// Return this curve
        /// </remarks>
        public RateCurve CreateAdjustedRateCurve(RateCurve baseCurve, NamedValueSet properties,
            IList<Pair<string, double>> spreads, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var structure = RateCurve.CreateAdjustedRateCurve(Logger, Cache, NameSpace, baseCurve, properties, spreads, fixingCalendar, rollCalendar);
            return structure;
        }

        /// <summary>
        /// Creates the curve.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="quotedAssetSet">The QuotedAssetSet.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <returns></returns>
        public IPricingStructure CreateCurve(NamedValueSet properties, QuotedAssetSet quotedAssetSet, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            var structure = PricingStructureFactory.CreateCurve(Logger, Cache, NameSpace, fixingCalendar, paymentCalendar, properties, quotedAssetSet);
            return structure;
        }

        /// <summary>
        /// Creates the curve.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="instruments">The instruments.</param>
        /// <param name="adjustedRates">The adjusted rates.</param>
        /// <param name="additional">The additional.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <returns></returns>
        public IPricingStructure CreateCurve(NamedValueSet properties, string[] instruments, Decimal[] adjustedRates, Decimal[] additional, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            var structure = PricingStructureFactory.CreateCurve(Logger, Cache, NameSpace, fixingCalendar, paymentCalendar, properties, instruments, adjustedRates, additional);
            return structure;
        }

        /// <summary>
        /// Creates the volatility surface.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="expiryTerms">The expiry terms.</param>
        /// <param name="strikes">The strikes.</param>
        /// <param name="volatilities">The volatilities.</param>
        /// <returns></returns>
        public IPricingStructure CreateVolatilitySurface(NamedValueSet properties, String[] expiryTerms, double[] strikes, Double[,] volatilities)
        {
            var pricingStructureType = PricingStructureFactory.CreateVolatilitySurface(Logger, Cache, NameSpace, properties, expiryTerms, strikes, volatilities);
            return pricingStructureType;
        }

        /// <summary>
        /// Creates the volatility surface.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="expiryTerms">The expiry terms.</param>
        /// <param name="strikesOrTenors">The strikes or tenor.</param>
        /// <param name="volatilities">The volatilities.</param>
        /// <returns></returns>
        public IPricingStructure CreateVolatilitySurface(NamedValueSet properties, String[] expiryTerms, String[] strikesOrTenors, Double[,] volatilities)
        {
            var pricingStructureType = PricingStructureFactory.CreateVolatilitySurface(Logger, Cache, NameSpace, properties, expiryTerms, strikesOrTenors, volatilities);
            return pricingStructureType;
        }
        
        /// <summary>
        /// Creates the specified vol curve.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="expiryTerms">An array of expiry tenors..</param>
        /// <param name="strikesOrTenor">An array of strikes or tenors.</param>
        /// <param name="volatilities">A range of volatilities of the correct dimension.</param>
        /// <param name="forwards">An array of forwards to the expiry dates. Used for calibration of the wing model.</param>
        /// <returns></returns>
        public IPricingStructure CreateVolatilitySurface(NamedValueSet properties, String[] expiryTerms, String[] strikesOrTenor, Double[,] volatilities, Double[] forwards)
        {
            var pricingStructureType = PropertyHelper.ExtractPricingStructureType(properties);
            var dimension2 = new double[strikesOrTenor.Length];
            if (pricingStructureType != PricingStructureTypeEnum.RateATMVolatilityMatrix)
            {
                dimension2 = ConvertStringArrayToDoubleArray(strikesOrTenor);
            }
            PricingStructureBase pricingStructure;
            switch (pricingStructureType)
            {
                case PricingStructureTypeEnum.EquityWingVolatilityMatrix:
                    pricingStructure = new ExtendedEquityVolatilitySurface(Logger, Cache, NameSpace, properties, expiryTerms, dimension2, forwards, volatilities);
                    break;
                default:
                    var message = $"Specified PricingStructureType: '{pricingStructureType}' is not supported";
                    throw new ApplicationException(message);
            }
            return pricingStructure;
        }

        private static double[] ConvertStringArrayToDoubleArray(string[] strikes)
        {
            var result = new double[strikes.Length];
            for (var i = 0; i < strikes.Length; i++)
            {
                result[i] = Convert.ToDouble(strikes[i]);
            }
            return result;
        }

        /// <summary>
        /// Construct a VolatilityCube
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="expiryTerms"></param>
        /// <param name="tenors"></param>
        /// <param name="volatilities"></param>
        /// <param name="strikes"></param>
        public IPricingStructure CreateVolatilityCube(NamedValueSet properties, String[] expiryTerms, String[] tenors, decimal[,] volatilities, decimal[] strikes)
        {
            var id = CreateVolatilityCubeInMarketAndReturnId(properties, expiryTerms, tenors, volatilities,
                                                             strikes);
            return id;
        }

        /// <summary>
        /// Creates the specified vol curve.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="expiryTerms">An array of expiry tenors..</param>
        /// <param name="tenors">An array of tenors.</param>
        /// <param name="volatilities">A range of volatilities of the correct dimension.</param>
        /// <param name="strikes">The strike array.</param>
        /// <returns></returns>
        public IPricingStructure CreateVolatilityCubeInMarketAndReturnId(NamedValueSet properties, String[] expiryTerms, String[] tenors, decimal[,] volatilities, decimal[] strikes)
        {
            var pricingStructure = new VolatilityCube(properties, expiryTerms, tenors, volatilities, strikes);
            return pricingStructure;
        }

        /// <summary>
        /// Returns properties from the pricing structure. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties.</param>
        public IRateCurve BuildRateCurve(Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties)
        {
            //  Pricing structure type
            if (PricingStructureFactory.Create(Logger, Cache, NameSpace, null, null, fpmlData, properties) is RateCurve curve)
            {
                curve.Build(Logger, Cache, NameSpace, null, null);
                return curve;
            }
            return null;
        }

        /// <summary>
        /// Returns properties from the pricing structure. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        public IPricingStructure CreateCurve(Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
        {
            //  Pricing structure type
            var curve = PricingStructureFactory.Create(Logger, Cache, NameSpace, fixingCalendar, paymentCalendar, fpmlData, properties);
            return curve;
        }

        /// <summary>
        /// Returns an pricing structure. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="referenceCurveData">The reference curve data.</param>//TODO need to add a collection of IBusinessCalendars - one for each curve!
        /// <param name="spreadCurveData">The spread curve data.</param>
        public IPricingStructure CreateCurve(Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> referenceCurveData,
        Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> spreadCurveData)
        {
            //  Pricing structure type
            var curve = PricingStructureFactory.Create(Logger, Cache, NameSpace, null, null, referenceCurveData, spreadCurveData);
            return curve;
        }

        /// <summary>
        /// Returns an pricing structure. <see cref="NamedValueSet"/> class.
        /// </summary>
        /// <param name="referenceCurveData">The reference curve data.</param>
        /// <param name="referenceFxCurveData">The Fx reference curve.</param>
        /// <param name="currency2CurveData">The currency2 data.</param>
        /// <param name="spreadCurveData">The spread curve data.</param>
        public IPricingStructure CreateCurve(Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> referenceCurveData,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> referenceFxCurveData,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> currency2CurveData,
        Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> spreadCurveData)
        {
            //  Pricing structure type
            var curve = PricingStructureFactory.Create(Logger, Cache, NameSpace, null, null, referenceCurveData, referenceFxCurveData, currency2CurveData, spreadCurveData);
            return curve;
        }

        /// <summary>
        /// Creates the basic rate curve risk set.
        /// This function takes a curves, creates a rate curve for each instrument and applying 
        /// supplied basis point perturbation/spread to the underlying instrument in the spread curve
        /// </summary>
        /// <param name="baseCurve">The base curve.</param>
        /// <param name="basisPointPerturbation">The basis point perturbation.</param>
        /// <returns>A list of perturbed rate curves</returns>
        public List<IPricingStructure> CreateRateCurveRiskSet(RateCurve baseCurve, decimal basisPointPerturbation)
        {
            var curveRiskSet = baseCurve.CreateCurveRiskSet(basisPointPerturbation);
            return curveRiskSet;
        }

        #endregion

        #region Market Data Interface

        /// <summary>
        /// Updates the curve references.
        /// </summary>
        /// <param name="pricingStructureIdentifier"></param>
        /// <param name="marketData"></param>
        /// <returns></returns>
        public string RefreshPricingStructure(string pricingStructureIdentifier, QuotedAssetSet marketData)
        {
            var result = RefreshPricingStructure(Logger, Cache, NameSpace, pricingStructureIdentifier, marketData);    
            return result;
        }

        /// <summary>
        /// Updates the curve
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="pricingStructureIdentifier"></param>
        /// <param name="marketData"></param>
        /// <returns></returns>
        public static string RefreshPricingStructure(ILogger logger, ICoreCache cache, string nameSpace, string pricingStructureIdentifier, QuotedAssetSet marketData)
        {
            var result = "Curve not found or recalculated.";
            //1) Find the curve
            var ps = cache.LoadItem<Market>(nameSpace + "." + pricingStructureIdentifier);
            //2) Replace the marketData
            if (ps?.Data is Market market)
            {
                var curve = market.Items1[0] as YieldCurveValuation;
                if (curve != null)
                {
                    curve.inputs = marketData;
                }
                //3) Rebuild the curve
                var rateCurve = PricingStructureFactory.Create(logger, cache, nameSpace, null, null, new Pair<PricingStructure, PricingStructureValuation>(market.Items[0], curve), ps.AppProps);
                //4) Save the pricing structure.
                SaveCurve(cache, nameSpace, rateCurve);
                logger.LogDebug("Updated the curve:" + pricingStructureIdentifier);
                result = pricingStructureIdentifier + " has been refreshed.";
            }
            //5) Return the result           
            return result;
        }

        /// <summary>
        /// Updates the curve
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="configIdentifier"></param>
        /// <param name="pricingStructureIdentifier"></param>
        /// <param name="marketData"></param>
        /// <param name="baseDate"></param>
        /// <param name="buildDateTime"></param>
        /// <returns></returns>
        public static string RefreshPricingStructureFromConfiguration(ILogger logger, ICoreCache cache, string nameSpace, string configIdentifier, 
            string pricingStructureIdentifier, QuotedAssetSet marketData, DateTime baseDate, DateTime buildDateTime)
        {
            var result = "Curve not found or recalculated.";
            //1) Find the curve
            var ps = cache.LoadItem<Market>(nameSpace + "." + configIdentifier);
            //2) Replace the marketData
            if (ps?.Data is Market market)
            {
                var curve = market.Items1[0] as YieldCurveValuation;
                if (curve != null)
                {
                    curve.inputs = marketData;
                }
                //3) Rebuild the curve
                var properties = ps.AppProps.Clone();
                properties.Set(EnvironmentProp.Function, FunctionProp.Market.ToString());
                properties.Set(CurveProp.UniqueIdentifier, null);
                properties.Set(CurveProp.BaseDate, baseDate);
                properties.Set(CurveProp.BuildDateTime, buildDateTime);
                var rateCurve = PricingStructureFactory.Create(logger, cache, nameSpace, null, null, new Pair<PricingStructure, PricingStructureValuation>(market.Items[0], curve), properties);
                //4) Save the pricing structure.
                SaveCurve(cache, nameSpace, rateCurve);
                logger.LogDebug("Updated the curve:" + pricingStructureIdentifier);
                result = pricingStructureIdentifier + " has been refreshed.";
            }
            //5) Return the result           
            return result;
        }

        #endregion

        #region Metrics

        /// <summary>
        /// 
        /// </summary>
        /// <param name="swapMaturity"></param>
        /// <param name="baseDate"></param>
        /// <param name="curveId"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public object[,] GetATMSwapRate(int swapMaturity,
                                             DateTime baseDate,
                                             string curveId,
                                             string currency)
        {
            string assetIdentifier = currency.ToUpper() + "-IRSwap-" + swapMaturity.ToString(CultureInfo.InvariantCulture) + "Y";
            var assetIds = new List<string> { assetIdentifier };
            var metricsArray = new List<string> { "ImpliedQuote" };
            QuotedAssetSet valuations = EvaluateMetricsForAssetSet(metricsArray, curveId, assetIds, baseDate);
            // EvaluateMetricsForAssetSet(metricsArray, curveId, assetIds,  baseDate);
            return MetricsHelper.BuildEvaluationResultsClean(metricsArray, assetIds, valuations.assetQuote, assetIds.Count);
        }

        /// <summary>
        /// Gets the value assuming the curve base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public Double GetValue(string pricingStructureId, DateTime targetDate)
        {
            var pricingStructure = GetCurve(pricingStructureId, false);
            var baseDate = pricingStructure.GetBaseDate();
            IPoint point = new DateTimePoint1D(baseDate, targetDate);
            var result = (double)pricingStructure.GetValue(point).Value;
            return result;
        }

        /// <summary>
        /// Gets the value assuming the curve base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="targetDates">The target dates.</param>
        /// <returns></returns>
        public List<Double> GetValues(string pricingStructureId, DateTime[] targetDates)
        {
            var pricingStructure = GetCurve(pricingStructureId, false);
            var baseDate = pricingStructure.GetBaseDate();
            var result = targetDates.Select(date => new DateTimePoint1D(baseDate, date)).Select(point => (double)pricingStructure.GetValue(point).Value).ToList();
            return result;
        }

        /// <summary>
        /// Gets the value from a base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public Double GetValue(string pricingStructureId, DateTime baseDate,
                                      DateTime targetDate)
        {
            var pricingStructure = GetCurve(pricingStructureId, false);
            IPoint point = new DateTimePoint1D(baseDate, targetDate);
            var result = (double)pricingStructure.GetValue(point).Value;
            return result;
        }

        /// <summary>
        /// Gets the value from a base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDates">The target dates.</param>
        /// <returns></returns>
        public List<Double> GetValues(string pricingStructureId, DateTime baseDate,
                                      DateTime[] targetDates)
        {
            var pricingStructure = GetCurve(pricingStructureId, false);
            var result = targetDates.Select(date => new DateTimePoint1D(baseDate, date)).Select(point => (double)pricingStructure.GetValue(point).Value).ToList();
            return result;
        }

        /// <summary>
        /// Gets the value from a base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="valuationDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public Double GetHorizonValue(string pricingStructureId, DateTime valuationDate,
                                      DateTime targetDate)
        {
            var pricingStructure = GetCurve(pricingStructureId, false);
            var baseDate = pricingStructure.GetBaseDate();
            IPoint point = new DateTimePoint1D(baseDate, valuationDate);
            var df = (double)pricingStructure.GetValue(point).Value;
            IPoint point2 = new DateTimePoint1D(baseDate, targetDate);
            var df1 = (double)pricingStructure.GetValue(point2).Value;
            return df1 / df;
        }

        /// <summary>
        /// Gets the value from a base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public Double GetValue2(string pricingStructureId, DateTime baseDate,
                                       DateTime targetDate)
        {
            var pricingStructure = GetCurve(pricingStructureId, false);
            IPoint point = new DateTimePoint1D(baseDate, targetDate);
            return (double)pricingStructure.GetValue(point).Value;
        }

        /// <summary>
        /// Gets the volatility for the base date and strike provided.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <param name="date">The base date.</param>
        /// <param name="strike">The strike.</param>
        /// <returns></returns>
        public Double GetSurfaceValue(string curveId, DateTime date, Double strike)
        {
            return GetTenorStrikeValue(curveId, date, strike);
        }

        /// <summary>
        /// Gets the volatility for the base date, strike and tenor provided.
        /// </summary>
        /// <param name="volCurveId">The vol curve Id.</param>
        /// <returns></returns>
        public object[,] GetSurfaceValues(string volCurveId)
        {
            var structure = (IVolatilitySurface)GetCurve(volCurveId, false);
            if (structure == null)
            {
                throw new ArgumentException("No surfaces found for the supplied properties");
            }
            var result = structure.GetSurface();
            return result;
        }

        /// <summary>
        /// Gets the volatility for the base date, strike and tenor provided.
        /// </summary>
        /// <param name="curveId">The curveId.</param>
        /// <param name="expiryTerm">The expiry date, as a tenor.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="tenor">The tenor.</param>
        /// <returns></returns>
        public Double GetCubeValue(string curveId, string expiryTerm, Double strike, string tenor)
        {
            var structure = (IVolatilityCube)GetCurve(curveId, false);
            if (structure == null)
            {
                throw new ArgumentException("No cubes found for the supplied properties");
            }
            return structure.GetValue(expiryTerm, tenor, strike);
        }

        /// <summary>
        /// Gets the value assuming the vol surface base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="strike">The strike value required.</param>
        /// <returns></returns>
        public Double GetTenorStrikeValue(string pricingStructureId, DateTime targetDate, Double strike)
        {
            var pricingStructure = GetCurve(pricingStructureId, false);
            var baseDate = pricingStructure.GetBaseDate();
            IPoint point = new DateTimePoint1D(baseDate, targetDate);
            IPoint point2 = new Point2D((double)point.Coords[0], strike);
            return (double)pricingStructure.GetValue(point2).Value;
        }

        /// <summary>
        /// Gets the value assuming the vol surface base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <param name="term">The term of the underlying required.</param>
        /// <returns>The interpolated value.</returns>

        public Double GetExpiryDateTenorValue(string pricingStructureId, DateTime baseDate, DateTime targetDate, String term)
        {
            var pricingStructure = (ISwaptionATMVolatilitySurface)GetCurve(pricingStructureId, false);
            return pricingStructure.GetValueByExpiryDateAndTenor(baseDate, targetDate, term);
        }

        /// <summary>
        /// Gets the value assuming the vol surface base date.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="expiryTerm">The expiry Term.</param>
        /// <param name="tenorTerm">The term of the underlying required.</param>
        /// <returns>The interpolated value.</returns>
        public Double GetExpiryTermTenorValue(string pricingStructureId, String expiryTerm, String tenorTerm)
        {
            var pricingStructure = (ISwaptionATMVolatilitySurface)GetCurve(pricingStructureId, false);
            return pricingStructure.GetValueByExpiryTermAndTenor(expiryTerm, tenorTerm);
        }

        /// <summary>
        /// Gets the interpolated value from a strike volatility surface.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="expiryDate">The expiry date.</param>
        /// <param name="strike">The strike.</param>
        /// <returns>The interpolated value.</returns>
        public Double GetExpiryDateStrikeValue(string pricingStructureId, DateTime baseDate, DateTime expiryDate, Double strike)
        {
            var pricingStructure = (IStrikeVolatilitySurface)GetCurve(pricingStructureId, false);
            return pricingStructure.GetValueByExpiryDateAndStrike(baseDate, expiryDate, strike);
        }

        /// <summary>
        /// Gets the interpolated value from a strike volatility surface.
        /// </summary>
        /// <param name="pricingStructureId"></param>
        /// <param name="expiryTerm">The expiry term.</param>
        /// <param name="strike">The strike.</param>
        /// <returns>The interpolated value.</returns>
        public Double GetExpiryTermStrikeValue(string pricingStructureId, String expiryTerm, Double strike)
        {
            var pricingStructure = (IStrikeVolatilitySurface)GetCurve(pricingStructureId, false);
            return pricingStructure.GetValueByExpiryTermAndStrike(expiryTerm, strike);
        }

        /// <summary>
        /// Gets the interpolated value from a strike volatility surface.
        /// </summary>
        /// <param name="pricingStructureId">The pricing structure identifier.</param>
        /// <param name="expiryTermsAsList">The expiry terms.</param>
        /// <param name="strikesAsList">The strikes.</param>
        /// <returns>The interpolated value.</returns>
        public object[,] GetExpiryTermStrikeValues(string pricingStructureId, List<string> expiryTermsAsList, List<double> strikesAsList)
        {
            //Get the curve.
            var pricingStructure = (IStrikeVolatilitySurface)GetCurve(pricingStructureId, false);
            var rows = expiryTermsAsList.Count;
            var width = strikesAsList.Count;
            //populate the result matrix.
            var result = new object[rows, width];
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    result[i, j] = pricingStructure.GetValueByExpiryTermAndStrike(expiryTermsAsList[i], strikesAsList[j]);
                }
            }
            return result;
        }

        #endregion

        #region Cloning and Perturbing

        /// <summary>
        /// Recalculates the curve.
        /// </summary>
        /// <returns></returns>
        public IPricingStructure CreatePerturbedCopy(IPricingStructure baseCurve, Decimal[] values)
        {
            var marketPair = baseCurve.GetPerturbedCopy(values);
            return TranslateToPricingStructure(marketPair.First, marketPair.Second, out string id);
        }

        #endregion

        #region Evaluation and Solving

        /// <summary>
        /// public constructs, Fra Rate class acts as a
        /// container which holds all the necessary information
        /// </summary>
        /// <param name="fixingCalendar">fixingCalendar</param>
        /// <param name="rollCalendar">rollCalendar</param>
        /// <param name="properties">curve properties</param>
        /// <param name="instruments">list of instruments</param>
        /// <param name="values">value of each instrument</param>
        /// <param name="initialFraRates">initial guesses for fra rate</param>
        /// <param name="shockedInstrumentIndices">array of shocked instrument indices</param>
        /// <param name="initialRates"></param>
        public List<decimal> CalculateEquivalentFraValues(NamedValueSet properties, 
            List<string> instruments,
            IEnumerable<decimal> values,
            IEnumerable<decimal> initialFraRates,
            ICollection<int> shockedInstrumentIndices, 
            List<decimal> initialRates,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var fraSolver = new FraSolver(Logger, Cache, NameSpace, fixingCalendar, rollCalendar, properties, instruments, values, initialFraRates, shockedInstrumentIndices, initialRates);
            return fraSolver.CalculateEquivalentFraValues(Logger, Cache, NameSpace);
        }

        #endregion

        #region Pedersen Calibration

        //#region Set the curves

        ///// <summary>
        ///// Sets the discount factors to use
        ///// </summary>
        ///// <param name="rateCurveId"></param>
        ///// <returns></returns>
        //public string PedersenSetDiscountFactors(String rateCurveId)
        //{
        //    return GetCurve(rateCurveId, false) is IRateCurve rateCurve ? Pedersen.SetDiscountFactors(rateCurve) : String.Format("Discount factors were not set.");
        //}

        //public string PedersenSetCapletImpliedVolatility(Double strike, String volSurfaceIdentifier)
        //{
        //    return GetCurve(volSurfaceIdentifier, false) is IStrikeVolatilitySurface volSurface ? Pedersen.SetCapletImpliedVolatility(strike, volSurface) : null;
        //}

        //public string PedersenSetSwaptionImpliedVolatility(String volSurfaceIdentifier)
        //{
        //    if (GetCurve(volSurfaceIdentifier, false) is IVolatilitySurface volSurface)
        //    {
        //        Pair<PricingStructure, PricingStructureValuation> fpMLPair = volSurface.GetFpMLData();
        //        var volObj = PricingStructureHelper.FpMLPairTo2DArray(fpMLPair);
        //        return Pedersen.SetSwaptionImpliedVolatility(volObj);
        //    }
        //    return null;
        //}

        //#endregion

        ///// <summary>
        ///// WIP stub. Still using dummy vol and correlation.
        ///// </summary>
        ///// <param name="curveId">RateCurve Id</param>
        ///// <returns></returns>
        //public object[,] PedersenCalibration(string curveId)
        //{
        //    var discounts = CreateQuarterlyDiscount(curveId);

        //    #region Correlation

        //    var correlation = new DenseMatrix(16, 16, new[] {
        //                                                                           1.000000000000000,0.755977958030068,0.390753301173931,0.141216505072534,-0.104876511208397,-0.210627197463773,-0.261885066327489,-0.283855022953850,-0.278785040189639,-0.217616581561314,-0.131679872097756,-0.047119056221972,0.035536790425668,0.035012148706578,-0.056755438761266,-0.147067841427471,
        //                                                                           0.755977958030068,1.000000000000000,0.897859194192688,0.754250412789863,0.568584771816748,0.471717652445483,0.414331524248440,0.376892759651739,0.325307014928591,0.295739233811822,0.262631528920056,0.258819816282469,0.223110623543883,0.224277418597115,0.197179794618843,0.214946385346135,
        //                                                                           0.390753301173931,0.897859194192688,1.000000000000000,0.966206548235119,0.871487281927208,0.807411735395667,0.762317308365949,0.725676219350727,0.652204548346068,0.571439499258483,0.467957645337466,0.405073258382088,0.296785508080043,0.295698168204210,0.315008587168980,0.399744811477594,
        //                                                                           0.141216505072534,0.754250412789863,0.966206548235119,1.000000000000000,0.968097165525376,0.930197657405308,0.897664527480700,0.866199451465545,0.789464234299992,0.688558329945124,0.554848012298554,0.462969701618092,0.320322999574078,0.315132123632584,0.355543542028662,0.470322650948144,
        //                                                                           -0.104876511208397,0.568584771816748,0.871487281927208,0.968097165525376,1.000000000000000,0.991870609745447,0.976144403192061,0.954694443933486,0.886260319324773,0.778379798672846,0.628107300555259,0.514474740129681,0.344648740689955,0.329912317723686,0.378645793200518,0.511589751077290,
        //                                                                           -0.210627197463773,0.471717652445483,0.807411735395667,0.930197657405308,0.991870609745447,1.000000000000000,0.995420463837200,0.982645964956045,0.927529471279287,0.826459103514222,0.677480265864820,0.557530885234864,0.376387183783690,0.351182092624649,0.391830263736922,0.525549743054232,
        //                                                                           -0.261885066327489,0.414331524248440,0.762317308365949,0.897664527480700,0.976144403192061,0.995420463837200,1.000000000000000,0.995653842253138,0.956423399222447,0.868102357495867,0.727656184223354,0.607494148494469,0.420864536558254,0.384765297686029,0.410542083201468,0.538065091248112,
        //                                                                           -0.283855022953850,0.376892759651739,0.725676219350727,0.866199451465545,0.954694443933486,0.982645964956045,0.995653842253138,1.000000000000000,0.978998288038717,0.907731550317703,0.780823302873087,0.664381765576934,0.476000948658687,0.428541137695596,0.434866141266280,0.551343586803336,
        //                                                                           -0.278785040189639,0.325307014928591,0.652204548346068,0.789464234299992,0.886260319324773,0.927529471279287,0.956423399222447,0.978998288038717,1.000000000000000,0.973112904777601,0.886257099989342,0.788017307311586,0.608878175546937,0.540668413612633,0.501605787039908,0.584142099062795,
        //                                                                           -0.217616581561314,0.295739233811822,0.571439499258483,0.688558329945124,0.778379798672846,0.826459103514222,0.868102357495867,0.907731550317703,0.973112904777601,1.000000000000000,0.968305181763602,0.903436609813577,0.755190977445885,0.674824032661566,0.590245774772147,0.624331666978794,
        //                                                                           -0.131679872097756,0.262631528920056,0.467957645337466,0.554848012298554,0.628107300555259,0.677480265864820,0.727656184223354,0.780823302873087,0.886257099989342,0.968305181763602,1.000000000000000,0.980402212079355,0.884840913945599,0.806372358629370,0.688651955757898,0.667641768862761,
        //                                                                           -0.047119056221972,0.258819816282469,0.405073258382088,0.462969701618092,0.514474740129681,0.557530885234864,0.607494148494469,0.664381765576934,0.788017307311586,0.903436609813577,0.980402212079355,1.000000000000000,0.957995763948430,0.896533849250595,0.773428357979744,0.713936269134348,
        //                                                                           0.035536790425668,0.223110623543883,0.296785508080043,0.320322999574078,0.344648740689955,0.376387183783690,0.420864536558254,0.476000948658687,0.608878175546937,0.755190977445885,0.884840913945599,0.957995763948430,1.000000000000000,0.979255559959103,0.877721716091762,0.776614810578805,
        //                                                                           0.035012148706578,0.224277418597115,0.295698168204210,0.315132123632584,0.329912317723686,0.351182092624649,0.384765297686029,0.428541137695596,0.540668413612633,0.674824032661566,0.806372358629370,0.896533849250595,0.979255559959103,1.000000000000000,0.951797469031466,0.859107491550573,
        //                                                                           -0.056755438761266,0.197179794618843,0.315008587168980,0.355543542028662,0.378645793200518,0.391830263736922,0.410542083201468,0.434866141266280,0.501605787039908,0.590245774772147,0.688651955757898,0.773428357979744,0.877721716091762,0.951797469031466,1.000000000000000,0.961857372722260,
        //                                                                           -0.147067841427471,0.214946385346135,0.399744811477594,0.470322650948144,0.511589751077290,0.525549743054232,0.538065091248112,0.551343586803336,0.584142099062795,0.624331666978794,0.667641768862761,0.713936269134348,0.776614810578805,0.859107491550573,0.961857372722260,1.000000000000000
        //                                                                       });
        //    #endregion

        //    var timeGrid = new PedersenTimeGrid(
        //        new[] { 0, 1, 2, 3, 4, 6, 8, 10, 12, 16, 20, 24, 28, 34, 40, 50, 60 },
        //        new[] { 0, 1, 2, 3, 4, 6, 8, 10, 12, 16, 20, 24, 28, 34, 40, 50, 60 }
        //        );

        //    #region Swaptions

        //    var swaptionDataRaw = new[,]
        //    {
        //        {0.1290,0.1540,0.1550,0.1545,0.1535,0.1495,0.1465,0.1430,0.1400,0.1385,0.1340,0.1320,0.1305,0.1275},
        //        {0.1465,0.1630,0.1630,0.1615,0.1595,0.1560,0.1530,0.1495,0.1460,0.1440,0.1390,0.1360,0.1340,0.1330},
        //        {0.1755,0.1770,0.1740,0.1710,0.1680,0.1650,0.1615,0.1590,0.1560,0.1540,0.1480,0.1440,0.1425,0.1410},
        //        {0.1875,0.1855,0.1820,0.1790,0.1750,0.1730,0.1700,0.1670,0.1645,0.1625,0.1555,0.1510,0.1475,0.1470},
        //        {0.1895,0.1860,0.1840,0.1805,0.1780,0.1750,0.1730,0.1700,0.1680,0.1660,0.1580,0.1530,0.1505,0.1490},
        //        {0.1880,0.1860,0.1830,0.1805,0.1775,0.1745,0.1725,0.1705,0.1680,0.1660,0.1575,0.1520,0.1500,0.1480},
        //        {0.1830,0.1810,0.1790,0.1760,0.1735,0.1705,0.1685,0.1660,0.1640,0.1620,0.1525,0.1480,0.1455,0.1435},
        //        {0.1795,0.1760,0.1730,0.1710,0.1685,0.1660,0.1635,0.1610,0.1590,0.1570,0.1470,0.1420,0.1395,0.1375},
        //        {0.1670,0.1640,0.1615,0.1585,0.1570,0.1545,0.1525,0.1510,0.1490,0.1470,0.1355,0.1305,0.1285,0.1265},
        //        {0.1505,0.1480,0.1460,0.1445,0.1425,0.1410,0.1395,0.1380,0.1360,0.1350,0.1250,0.1195,0.1175,0.1160},
        //        {0.1380,0.1355,0.1340,0.1315,0.1305,0.1300,0.1290,0.1275,0.1260,0.1250,0.1170,0.1110,0.1090,0.1060},
        //        {0.1290,0.1270,0.1245,0.1215,0.1200,0.1195,0.1180,0.1175,0.1160,0.1150,0.1080,0.1025,0.1005,0.0985},
        //        {0.1195,0.1185,0.1165,0.1135,0.1135,0.1125,0.1110,0.1095,0.1085,0.1070,0.1005,0.0960,0.0935,0.0920}
        //    };
        //    var swaptionExpiry = new[] { 1, 2, 4, 8, 12, 16, 20, 28, 40, 60, 80, 100, 120 };
        //    var swaptionTenor = new[] { 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 60, 80, 100, 120 };
        //    var swaptionData = new double[swaptionExpiry.Length * swaptionTenor.Length, 3];
        //    for (int i = 0; i < swaptionExpiry.Length; i++)
        //    {
        //        for (int j = 0; j < swaptionTenor.Length; j++)
        //        {
        //            int index = i * swaptionTenor.Length + j;
        //            swaptionData[index, 0] = swaptionExpiry[i];
        //            swaptionData[index, 1] = swaptionTenor[j];
        //            swaptionData[index, 2] = swaptionDataRaw[i, j];
        //        }
        //    }

        //    #endregion

        //    #region Caplets

        //    var capletDataRaw = new[]
        //    {
        //        0.11906,0.11967,0.12298,0.12287,0.14967,0.17235,0.19190,0.19664,0.19211,0.18947,
        //        0.18688,0.19040,0.19305,0.18922,0.18837,0.18918,0.18956,0.18647,0.18588,0.18702,
        //        0.18840,0.18758,0.18789,0.18336,0.18731,0.18205,0.17772,0.18107,0.18185,0.18163,
        //        0.18125,0.17984,0.17829,0.17664,0.17450,0.17447,0.17354,0.17144,0.17014,0.17049,
        //        0.17071,0.16730,0.16657,0.16679,0.16687,0.16312,0.16309,0.15872,0.16374,0.15930,
        //        0.15564,0.15711,0.15583,0.15489,0.15341,0.15328,0.15306,0.15132,0.15106,0.15130,
        //        0.15118,0.14823,0.14690,0.14670,0.14636,0.14373,0.14281,0.14286,0.14251,0.14202,
        //        0.13941,0.14167,0.14173,0.14197,0.14286,0.14363,0.14460,0.14581,0.14651,0.14845
        //    };
        //    var capletData = new double[capletDataRaw.Length, 2];
        //    for (int i = 0; i < capletDataRaw.Length; i++)
        //    {
        //        capletData[i, 0] = i + 1;
        //        capletData[i, 1] = capletDataRaw[i];
        //    }

        //    #endregion

        //    var targets = new CalibrationTargets(timeGrid, capletData, swaptionData);
        //    var shifts = new QuarterlyShifts(0);
        //    var settings = new CalibrationSettings();
        //    string outputString = "";
        //    var vol = Analytics.Stochastics.Pedersen.PedersenCalibration.Calibrate(timeGrid, discounts, shifts, correlation, targets, settings, 3, true, ref outputString);
        //    var result = new object[1, 1];
        //    for (int i = 0; i < 60; i++)
        //    {
        //        for (int j = 0; j < 60 - i; j++)
        //        {
        //            result[i + 1, j] = vol.VolNorm(i + 1, j + 1);
        //        }
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// Retrieves quarterly discounter factors from curveId
        ///// </summary>
        ///// <param name="curveId"></param>
        ///// <returns></returns>
        //private QuarterlyDiscounts CreateQuarterlyDiscount(string curveId)
        //{
        //    DateTime current = DateTime.Today;
        //    var discountFactors = new double[121];
        //    for (int i = 0; i <= 120; i++)
        //    {
        //        discountFactors[i] = GetValue(curveId, current);
        //        current = current.AddMonths(3);
        //    }
        //    return new QuarterlyDiscounts(discountFactors);
        //}

        ///// <summary>
        ///// Builds Calibration targets from implied volatility data
        ///// </summary>
        ///// <param name="timeGrid"></param>
        ///// <param name="curveIdCaplet"></param>
        ///// <param name="curveIdSwaption"></param>
        ///// <returns></returns>
        //private CalibrationTargets CreateTargetImpliedVolatilities(PedersenTimeGrid timeGrid, string curveIdCaplet, string curveIdSwaption)
        //{
        //    DateTime current = DateTime.Today;
        //    var capletData = new double[80, 2];
        //    for (int i = 0; i < 80; i++)
        //    {
        //        capletData[i, 0] = i + 1;
        //        capletData[i, 1] = GetExpiryDateStrikeValue(curveIdCaplet, current, current.AddMonths((i + 1) * 3), 0);
        //    }
        //    var swaptionExpiry = new[] { 1, 2, 4, 8, 12, 16, 20, 28, 40, 60, 80, 100, 120 };
        //    var swaptionTenor = new[] { 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 60, 80, 100, 120 };
        //    var swaptionData = new double[swaptionExpiry.Length * swaptionTenor.Length, 3];
        //    for (int i = 0; i < swaptionExpiry.Length; i++)
        //    {
        //        for (int j = 0; j < swaptionTenor.Length; j++)
        //        {
        //            int index = i * swaptionTenor.Length + j;
        //            swaptionData[index, 0] = swaptionExpiry[i];
        //            swaptionData[index, 1] = swaptionTenor[j];
        //            string term = (swaptionTenor[j] * 3).ToString(CultureInfo.InvariantCulture) + "M";
        //            swaptionData[index, 2] = GetExpiryDateTenorValue(curveIdSwaption, current, current.AddMonths(swaptionExpiry[i] * 3), term);
        //        }
        //    }
        //    return new CalibrationTargets(timeGrid, capletData, swaptionData);
        //}

        //#region Pedersen Algorithm

        //#endregion

        //#region Investigation Functions

        ///// <summary>
        ///// Show the correlation data.
        ///// </summary>
        ///// <returns></returns>
        //public object PedersenShowCorrelation()
        //{
        //    return Pedersen.ShowCorrelation();
        //}

        ///// <summary>
        ///// Displays post-Calibration result summary.
        ///// </summary>
        ///// <returns></returns>
        //public object PedersenCalSummary()
        //{
        //    return Pedersen.CalSummary();
        //}

        ///// <summary>
        ///// Displays post-Calibration vol surface (multi-factored)
        ///// </summary>
        ///// <returns></returns>
        //public object PedersenCalVol()
        //{
        //    return Pedersen.CalVol();
        //}

        ///// <summary>
        ///// Displays post-Calibration vol surface (vol sizes)
        ///// </summary>
        ///// <returns></returns>
        //public object PedersenCalVolNorm()
        //{
        //    return Pedersen.CalVolNorm();
        //}

        //#endregion

        //#region Simulation

        ///// <summary>
        ///// Settings range.
        ///// </summary>
        ///// <param name="r1"></param>
        ///// <returns></returns>
        //public object PedersenSimulation(object[,] r1)
        //{
        //    return Pedersen.Simulation(r1);
        //}

        ///// <summary>
        ///// Returns details about the simulation.
        ///// </summary>
        ///// <returns></returns>
        //public object PedersenSimSummary()
        //{
        //    return Pedersen.SimSummary();
        //}

        ///// <summary>
        ///// Gets the debug information 
        ///// </summary>
        ///// <param name="s"></param>
        //public void PedersenDebug(string s)
        //{
        //    Pedersen.Debug(s);
        //}

        ///// <summary>
        ///// Gets the debug information
        ///// </summary>
        ///// <returns></returns>
        //public object PedersenDebugOutput()
        //{
        //    return Pedersen.DebugOutput();
        //}

        //#endregion

        #endregion

        #region Utility Functions

        ///  <summary>
        ///  Examples of values are:
        /// <Property name = "Tolerance" > 1E-10</Property >
        /// < Property name="Bootstrapper">SimpleRateBootstrapper</Property>
        /// <Property name = "BootstrapperInterpolation" > LinearRateInterpolation</Property >
        /// < Property name="CurveInterpolation">LinearInterpolation</Property>
        /// <Property name = "UnderlyingCurve" > ZeroCurve</Property >
        /// < Property name="CompoundingFrequency">Continuous</Property>
        /// <Property name = "ExtrapolationPermitted" > true</Property >
        /// < Property name="DayCounter">ACT/365.FIXED</Property>
        ///  </summary>
        ///  <param name="algorithmName">The name of the algorithm E.g Cubic</param>
        ///  <param name="pricingStructureType">RateCurve, RateSpreadCurve etc.</param>
        ///  <param name="tolerance">A decimal value.</param>
        ///  <param name="bootstrapper">E.g. FastBootstrapper</param>
        ///  <param name="bootstrapperInterpolation">LogLinearInterpolation</param>
        ///  <param name="curveInterpolation">LogLinearInterpolation</param>
        ///  <param name="underlyingCurve">DiscountFactorCurve or ZeroCurve</param>
        ///  <param name="compoundingFrequency">Continuous</param>
        ///  <param name="extrapolation">true</param>
        ///  <param name="dayCounter">Typically ACT/365.FIXED</param>
        ///  <returns></returns>
        public string CreateAlgorithm(string algorithmName, string pricingStructureType, string tolerance,
            string bootstrapper, string bootstrapperInterpolation, string curveInterpolation,
            string underlyingCurve, string compoundingFrequency, string extrapolation, string dayCounter)
        {
            var name = AlgorithmsProp.GenericName + "." + pricingStructureType + "." +
                       algorithmName;
            var uniqueName = NameSpace + "." + name;
            var itemProps = new NamedValueSet();
            itemProps.Set(EnvironmentProp.DataGroup, "Orion.V5r3.Reporting.Configuration.Algorithm.");
            itemProps.Set(EnvironmentProp.SourceSystem, "Orion");
            itemProps.Set(EnvironmentProp.Function, FunctionProp.Configuration.ToString());
            itemProps.Set(EnvironmentProp.Type, "Algorithm");
            itemProps.Set(EnvironmentProp.Schema, "V5r3.Reporting");
            itemProps.Set(EnvironmentProp.NameSpace, NameSpace);
            var algorithm = AlgorithmHelper.CreateAlgorithm(tolerance, bootstrapper, bootstrapperInterpolation, 
                curveInterpolation, underlyingCurve, compoundingFrequency, extrapolation, dayCounter);
            Cache.SaveObject(algorithm, uniqueName, itemProps);
            return name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pricingStructureType"></param>
        /// <param name="algorithmName"></param>
        /// <returns></returns>
        public Algorithm GetAlgorithm(String pricingStructureType, String algorithmName)
        {
            var uniqueName = NameSpace + "." + AlgorithmsProp.GenericName + "." + pricingStructureType + "." +
                             algorithmName;
            var item = Cache.LoadItem<Algorithm>(uniqueName);
            var deserializedAlgorithm = (Algorithm) item?.Data;
            return deserializedAlgorithm;
        }

        private static  NamedValueSet AddPricingStructureProperties(IPricingStructure pricingStructure, NamedValueSet properties)
        {
            var extraProps = pricingStructure.PricingStructureData;
            properties.Set("CurveType", extraProps.CurveType.ToString());
            properties.Set("AssetClass", extraProps.AssetClass.ToString());
            var pricingStructureRiskSetType = properties.GetValue<string>("PricingStructureRiskSetType", false);
            if (pricingStructureRiskSetType == null)
            {
                properties.Set("PricingStructureRiskSetType", extraProps.PricingStructureRiskSetType.ToString());
            }
            return properties;
        }

        /// <summary>
        /// Translates to pricing structure.
        /// </summary>
        /// <returns></returns>
        protected List<IPricingStructure> TranslateToPricingStructure(IEnumerable<Pair<NamedValueSet, Market>> markets)
        {
            var pricingStructures = new List<IPricingStructure>();
            foreach (var market in markets)
            {
                IPricingStructure pricingStructure = TranslateToPricingStructure(market.First, market.Second, out string id);
                pricingStructures.Add(pricingStructure);
            }
            return pricingStructures;
        }

        /// <summary>
        /// Translates to pricing structure.
        /// </summary>
        private IPricingStructure TranslateToPricingStructure(NamedValueSet properties, Market market, out string curveId)
        {
            // cache the curve
            IPricingStructure pricingStructure = null;
            curveId = market.id;
            if (market.Items != null)
            {              
                if (String.IsNullOrEmpty(curveId))
                {
                    curveId = market.Items[0].id;//use yieldCurve.id, CurveGen 1.X compatible
                }
                var marketData = new Pair<PricingStructure, PricingStructureValuation>(market.Items[0], market.Items1[0]);
                pricingStructure = CreateCurve(marketData, properties, null, null);
            }
            return pricingStructure;
        }

        /// <summary>
        /// Constructor for the class <see cref="SwapRate"/>.</summary>
        /// <param name="businessCalendar">Four letter (uppercase) code for
        /// the business calendar that will be used to generate all dates in
        /// the swap schedule.
        /// Example: AUSY.</param>
        /// <param name="calculationDate">Base date.</param>
        /// <param name="dayCount">Three letter (uppercase) code for the day
        /// count convention that will be used to compute the accrual factors.
        /// Example: ACT/365.</param>
        /// <param name="discountFactors">Array of known discount factors.
        /// </param>
        /// <param name="offsets">The number of days from the Calculation
        /// Date to each known discount factor.</param>
        /// <param name="fixedSideFrequency">Roll frequency (number of
        /// rolls/payments per year) on the fixed side of the swap.
        /// Precondition: must be a divisor of 12.
        /// Example: Quarterly corresponds to a frequency of 4; Monthly
        /// corresponds to a frequency of 12.</param>
        /// <param name="rollConvention">Roll convention used to generate
        /// the swap schedule.
        /// Example: MODFOLLOWING.</param>
        public SwapRate GetSwapRate(string businessCalendar,
                        DateTime calculationDate,
                        string dayCount,
                        double[] discountFactors,
                        int[] offsets,
                        int fixedSideFrequency,
                        BusinessDayConventionEnum rollConvention)
        {
            return new SwapRate(Logger, Cache, NameSpace, businessCalendar, calculationDate, dayCount, discountFactors, offsets, fixedSideFrequency, rollConvention);
        }

        #endregion
    }
}
