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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Common;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using Metadata.Common;
using Orion.MDAS.Client;
using Orion.Util.Caching;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Threading;
using Exception = System.Exception;

namespace Orion.Provider
{
    public enum ConvertFailMode
    {
        PassThrough,
        ReturnNull,
        ReturnUnknown,
        ThrowException
    }

    public delegate void ConsumerDelegate(Guid subscriptionId, QuotedAssetSet resultSet);

    public interface IQRMarketDataProvider
    {
        void ApplySettings(NamedValueSet settings);
        void Start();
        void Stop();
        MDSResult<QuotedAssetSet> RequestMarketQuotes(
            IModuleInfo clientInfo,
            Guid requestId,
            NamedValueSet requestParams,
            MDSRequestType requestType,
            DateTimeOffset subsExpires,
            QuotedAssetSet standardQuotedAssetSet);
        MDSResult<QuotedAssetSet> RequestPricingStructure(
            IModuleInfo clientInfo,
            Guid requestId,
            NamedValueSet requestParams,
            MDSRequestType requestType,
            DateTimeOffset subsExpires,
            NamedValueSet structureParams);
        void CancelRequest(Guid subscriptionId);
        void PublishMarketData(
            IModuleInfo clientInfo,
            Guid requestId,
            NamedValueSet requestParams,
            TimeSpan dataLifetime,
            QuotedAssetSet marketDataSet);
    }

    public interface IMarketDataMap
    {
        //void ClearRules(MDSProviderId provider);
        //void AddInstrMap(int priority, string sourcePattern, string targetPattern);
        //void AddFieldMap(int priority, MDSRequestType requestType, string sourcePattern, string targetPattern);
        //void AddUnitsMap(int priority, MDSRequestType requestType, string sourcePattern, PriceQuoteUnitsEnum targetUnits);
        //void SortRules();
        string Convert(
            MDSDictionaryType dictType, MDSRequestType requestType,
            MDSProviderId sourceProvider, MDSProviderId targetProvider,
            string sourceValue, ConvertFailMode failMode);
    }

    //public class MarketDataMapFactory
    //{
    //    public static IMarketDataMap Create(ILogger logger, MDSProviderId provider, ChangeCallback callback)
    //    {
    //        if (logger == null)
    //            throw new ArgumentNullException("logger");
    //        return new MarketDataMap(logger, provider, callback);
    //    }
    //}

    //public delegate void ChangeCallback(MapRule oldMap, MapRule newMap);

    internal class MapRule : IComparable<MapRule>
    {
        private readonly Regex _regex;
        public Regex Regex { get { return _regex; } }
        public bool Disabled { get; set; }
        public int Priority { get; set; }
        public MDSDictionaryType DictType { get; set; }
        public MDSProviderId SourceProvider { get; set; }
        public MDSProviderId TargetProvider { get; set; }
        public MDSRequestType RequestType { get; set; }
        public string SearchValue { get; set; }
        public string OutputValue { get; set; }
        public MapRule(MDSDictionaryType dictType,
            MDSProviderId sourceProvider, MDSProviderId targetProvider,
            MDSRequestType requestType,
            int priority,
            string searchValue, string outputValue)
        {
            Disabled = false;
            DictType = dictType;
            SourceProvider = sourceProvider;
            TargetProvider = targetProvider;
            RequestType = requestType;
            Priority = priority;
            SearchValue = searchValue;
            OutputValue = outputValue;
            _regex = new Regex("^" + searchValue + "$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
        public int CompareTo(MapRule map)
        {
            return (Priority - map.Priority) * (-1); // highest to lowest
        }
    }

    internal class MarketDataMap : IMarketDataMap
    {
        private readonly ILogger _logger;
        private readonly Guarded<List<MapRule>> _rules = new Guarded<List<MapRule>>(new List<MapRule>());
        private readonly MDSProviderId _provider;
        private readonly Dictionary<string, string> _forwardCache;
        private readonly Dictionary<string, string> _reverseCache;
        //private readonly ChangeCallback _Callback;

        public MarketDataMap(ILogger logger, MDSProviderId provider)
        {
            _logger = logger;
            _provider = provider;
            //_Callback = callback;

            // load static maps
            _forwardCache = new Dictionary<string, string>();
            _reverseCache = new Dictionary<string, string>();

        }

        public ProviderRuleSet ExportRules()
        {
            var list = new List<QRMarketDataMap>();
            _rules.Locked(rules =>
            {
                foreach (MapRule rule in rules)
                {
                    list.Add(new QRMarketDataMap
                                 {
                        dictType = rule.DictType,
                        disabled = rule.Disabled,
                        priority = rule.Priority,
                        requestType = rule.RequestType,
                        sourceProvider = rule.SourceProvider,
                        targetProvider = rule.TargetProvider,
                        sourcePattern = rule.SearchValue,
                        outputValue = rule.OutputValue
                    });
                }
            });
            return new ProviderRuleSet { provider = _provider, rules = list.ToArray() };
        }

        public void ImportRules(ProviderRuleSet mapRuleSet)
        {
            ClearRules(mapRuleSet.provider);
            if (mapRuleSet.rules != null)
            {
                foreach (QRMarketDataMap rule in mapRuleSet.rules)
                {
                    MDSDictionaryType dictType = rule.dictType;
                    switch (dictType)
                    {
                        case MDSDictionaryType.FieldName:
                            AddFieldMap(rule.priority, rule.requestType, rule.sourcePattern, rule.outputValue);
                            break;
                        case MDSDictionaryType.Instrument:
                            AddInstrMap(rule.priority, rule.sourcePattern, rule.outputValue);
                            break;
                        case MDSDictionaryType.QuoteUnits:
                            AddUnitsMap(rule.priority, rule.requestType, rule.sourcePattern, PriceQuoteUnitsScheme.ParseEnumString(rule.outputValue));
                            break;
                        default:
                            throw new NotSupportedException("DictionaryType: " + dictType);
                    }
                }
            }
            // import done
            SortRules();
        }

        public void ClearRules(MDSProviderId provider)
        {
            _rules.Locked(rules =>
                {
                    var newRules = rules.Where(rule => rule.SourceProvider != provider && rule.TargetProvider != provider).ToList();
                    rules = newRules;
                });
        }

        public void SortRules()
        {
            _rules.Locked((rules) => rules.Sort());
        }

        private void AddInstrMap(int priority, string sourcePattern, string targetPattern)
        {
            _rules.Locked(rules => rules.Add(new MapRule(
                                                   MDSDictionaryType.Instrument,
                                                   MDSProviderId.GlobalIB, _provider,
                                                   MDSRequestType.Undefined,
                                                   priority,
                                                   sourcePattern, targetPattern)));
        }

        private void AddFieldMap(int priority, MDSRequestType requestType, string sourcePattern, string targetPattern)
        {
            _rules.Locked(rules => rules.Add(new MapRule(
                                                   MDSDictionaryType.FieldName,
                                                   MDSProviderId.GlobalIB, _provider,
                                                   requestType,
                                                   priority,
                                                   sourcePattern, targetPattern)));
        }

        private void AddUnitsMap(int priority, MDSRequestType requestType, string sourcePattern, PriceQuoteUnitsEnum targetUnits)
        {
            _rules.Locked(rules => rules.Add(new MapRule(
                                                   MDSDictionaryType.QuoteUnits,
                                                   MDSProviderId.GlobalIB, _provider,
                                                   requestType,
                                                   priority,
                                                   sourcePattern, targetUnits.ToString())));
        }

        public string GetKey(
            MDSDictionaryType dictType, MDSRequestType requestType,
            MDSProviderId sourceProvider, MDSProviderId targetProvider, string sourceValue)
        {
            //return String.Format("{0};{1};{2}:{3};{4}:{5}",
            return
                $"{dictType.ToString()};{requestType.ToString()}:{sourceProvider.ToString()};{sourceValue.ToUpper()}:{targetProvider.ToString()}";
        }

        //public string GetKey(MapRule map)
        //{
        //    return GetKey(
        //        map.DictType, map.RequestType, //map.AssetIdType, 
        //        map.SourceProvider, map.TargetProvider, map.SearchValue);
        //}

        //private void OnRecvMarketDataMap(ISubscription subscription, ICoreItem item)
        //{
        //    MapRule newMap = (MapRule)item.Data;
        //    string key = GetKey(newMap);
        //    MapRule oldMap = null;
        //    bool changed = false;
        //    lock (_MarketDataMap)
        //    {
        //        if (_MarketDataMap.TryGetValue(key, out oldMap))
        //        {
        //            // found - update
        //            _MarketDataMap[key] = newMap;
        //            changed = (newMap.outputValue != oldMap.outputValue)
        //                || (newMap.Disabled != oldMap.Disabled)
        //                || (newMap.Priority != oldMap.Priority);
        //        }
        //        else
        //        {
        //            // not found - create
        //            _MarketDataMap.Add(key, newMap);
        //            changed = true;
        //        }
        //    }
        //    if (changed && (_Callback != null))
        //    {
        //        _Callback(oldMap, newMap);
        //    }
        //}

        private void UpdateForwardCache(string key, string value)
        {
            lock (_forwardCache)
            {
                // forward
                if (!_forwardCache.ContainsKey(key))
                {
                    _forwardCache[key] = value;
                    //_Logger.LogDebug("Forward cache updated: '{0}' -> {1}", key, value);
                }
            }
        }
        private void UpdateReverseCache(string key, string value)
        {
            lock (_reverseCache)
            {
                // Reverse
                if (!_reverseCache.ContainsKey(key))
                {
                    _reverseCache[key] = value;
                    //_Logger.LogDebug("Reverse cache updated: '{0}' -> {1}", key, value);
                }
            }
        }

        public string Convert(
            MDSDictionaryType dictType, MDSRequestType requestType,
            MDSProviderId sourceProvider, MDSProviderId targetProvider,
            string sourceValue, ConvertFailMode failMode)
        {
            // method:
            // 1. check for cached result
            // 2. find relevant maps (by dicttype, reqtype, assettype, source, target)
            // 3. process maps in priority order until hit, fail if no hit
            // 4. save outbound and inbound results in cache

            if (dictType == MDSDictionaryType.Undefined)
                throw new ArgumentNullException(nameof(dictType));
            if (requestType == MDSRequestType.Undefined)
                throw new ArgumentNullException(nameof(requestType));
            //if (assetType == MDSAssetType.Undefined)
            //    throw new ArgumentNullException("assetType");
            if (sourceProvider == MDSProviderId.Undefined)
                throw new ArgumentNullException(nameof(sourceProvider));
            if (targetProvider == MDSProviderId.Undefined)
                throw new ArgumentNullException(nameof(targetProvider));
            if (sourceValue == null)
                throw new ArgumentNullException(nameof(sourceValue));
            sourceValue = sourceValue.Trim().ToUpper();
            if (sourceValue == "")
                throw new ArgumentNullException(nameof(sourceValue));

            const string cUnknownValue = "[unknown]";

            // check for cached result
            string forwardKey = GetKey(dictType, requestType, //assetType, 
                sourceProvider, targetProvider, sourceValue);
            lock (_forwardCache)
            {
                if (_forwardCache.TryGetValue(forwardKey, out var cachedResult))
                {
                    //Logger.LogDebug("Converted {0} {1} '{2}' to {3} '{4}' (via forward cache)",
                    //    sourceProvider.ToString(), dictType.ToString(), sourceValue, targetProvider.ToString(), cachedResult);
                    return cachedResult;
                }
            }

            // find relevant maps (by dicttype, reqtype, assettype, source, target)
            var maps = new List<MapRule>();
            _rules.Locked((rules) =>
            {
                foreach (var rule in rules)
                {
                    if ((!rule.Disabled)
                        && (rule.DictType == MDSDictionaryType.Undefined || (rule.DictType == dictType))
                        && (rule.SourceProvider == MDSProviderId.Undefined || (rule.SourceProvider == sourceProvider))
                        && (rule.TargetProvider == MDSProviderId.Undefined || (rule.TargetProvider == targetProvider))
                        //&& (rule.AssetIdType == MDSAssetType.Undefined || (rule.AssetIdType == assetType))
                        && (rule.RequestType == MDSRequestType.Undefined || (rule.RequestType == requestType)))
                        maps.Add(rule);
                }
            });

            // process maps in priority order until hit, fail if no hit
            string result = sourceValue;
            bool replaced = false;
            foreach (var map in maps)
            {
                Match match = map.Regex.Match(sourceValue);
                if (match.Success)
                {
                    result = map.Regex.Replace(sourceValue, map.OutputValue);
                    replaced = true;
                    break;
                }
            }

            if (replaced)
            {
                result = result.Trim().ToUpper();
                //_Logger.LogDebug("Converted {0} {1} '{2}' to {3} '{4}' (via mapping rules)",
                //    sourceProvider.ToString(), dictType.ToString(), sourceValue, targetProvider.ToString(), result);
                // update forward and reverse caches
                UpdateForwardCache(forwardKey, result);
                string reverseKey = GetKey(dictType, requestType, //assetType, 
                    targetProvider, sourceProvider, result);
                UpdateReverseCache(reverseKey, sourceValue);
            }
            else
            {
                // no replacement rules - try the reverse cache
                string cachedResult;
                bool foundInReverseCache;
                lock (_reverseCache)
                {
                    foundInReverseCache = _reverseCache.TryGetValue(forwardKey, out cachedResult);
                }
                if (foundInReverseCache)
                {
                    _logger.LogDebug("Converted {0} {1} '{2}' to {3} '{4}' (via reverse cache)",
                        sourceProvider.ToString(), dictType.ToString(), sourceValue, targetProvider.ToString(), cachedResult);
                    // update forward cache
                    UpdateForwardCache(forwardKey, cachedResult);
                    return cachedResult;
                }

                // exhausted all conversion options
                _logger.LogWarning("Cannot convert {0} {1} '{2}' to {3} (no matching map found)",
                    sourceProvider.ToString(), dictType.ToString(), sourceValue, targetProvider.ToString());
                switch (failMode)
                {
                    case ConvertFailMode.PassThrough:
                        result = sourceValue;
                        break;
                    case ConvertFailMode.ReturnNull:
                        result = null;
                        break;
                    case ConvertFailMode.ReturnUnknown:
                        result = cUnknownValue;
                        break;
                    default:
                        throw new ApplicationException(
                            $"Cannot convert {sourceProvider.ToString()} {dictType.ToString()} '{sourceValue}' to {targetProvider.ToString()} ");
                }
            }

            return result;

        }

        public void SaveMaps(string fileName, bool includeDisabled)
        {
            //ProviderRuleSet mapColl = new ProviderRuleSet();
            //MapRule[] selectedMaps = null;
            //int selectedCount = 0;
            //// lock dictionary while getting maps
            //lock (_MarketDataMap)
            //{
            //    selectedMaps = new MapRule[_MarketDataMap.Count];
            //    foreach (MapRule map in _MarketDataMap)
            //    {
            //        if (includeDisabled || (!map.Disabled))
            //        {
            //            selectedMaps[selectedCount] = map;
            //            selectedCount++;
            //        }
            //    }
            //}
            //// now do IO outside the lock
            //mapColl.maps = new MapRule[selectedCount];
            //for (int i = 0; i < selectedCount; i++)
            //    mapColl.maps[i] = selectedMaps[i];
            //using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            //{
            //    XmlSerializer xs = new XmlSerializer(typeof(ProviderRuleSet));
            //    xs.Serialize(fs, mapColl);
            //    fs.Flush();
            //}
        }

        public void LoadMaps(string fileName, bool includeDisabled)
        {
            //// load the map collection
            //ProviderRuleSet mapColl;
            //using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            //{
            //    XmlSerializer xs = new XmlSerializer(typeof(ProviderRuleSet));
            //    mapColl = (ProviderRuleSet)xs.Deserialize(fs);
            //}
            //foreach (MapRule map in mapColl.maps)
            //{
            //    if ((!map.Disabled) || includeDisabled)
            //        Publish(map);
            //}
        }
    }

    public class ProviderInstrFieldCodeSet
    {
        public List<string> InstrumentIds = new List<string>();
        public List<string> FieldNames = new List<string>();
        public ProviderInstrFieldCodeSet(IEnumerable<string> instrumentIds, IEnumerable<string> fieldNames)
        {
            if (instrumentIds != null)
                InstrumentIds.AddRange(instrumentIds);
            if (fieldNames != null)
                FieldNames.AddRange(fieldNames);
        }
    }

    public class RequestItem
    {
        public Asset StandardAsset;
        public BasicQuotation StandardQuote;
        public PriceQuoteUnitsEnum StandardUnits;
        public string ProviderInstrumentId;
        public string ProviderFieldName;
        public PriceQuoteUnitsEnum ProviderUnits;
    }
    public class RequestContext
    {
        public List<RequestItem> RequestItems;
        public Dictionary<string, string> InstrConversionMap;
        public Dictionary<string, string> FieldConversionMap;
        public List<ProviderInstrFieldCodeSet> ProviderInstrFieldCodeSets;
    }

    public class MdpBaseProvider : IQRMarketDataProvider
    {
        protected readonly ILogger Logger;
        protected readonly ICoreClient Client;
        protected readonly MDSProviderId ProviderId;
        private readonly MarketDataMap _marketDataMap;
        protected IMarketDataMap MarketDataMap => _marketDataMap;
        protected readonly ConsumerDelegate ConsumerCallback;
        protected readonly IDictionary<Guid, RealtimeRequestMap> SubsMapExternal;

        // managed state
        protected ICoreCache RuleSet;

        // base constructor
        public MdpBaseProvider(ILogger logger, ICoreClient client, MDSProviderId providerId, ConsumerDelegate consumer)
        {
            Logger = new FilterLogger(logger, $"{providerId}: ");
            Client = client;
            ProviderId = providerId;
            _marketDataMap = new MarketDataMap(logger, providerId);
            ConsumerCallback = consumer;
            SubsMapExternal = new Dictionary<Guid, RealtimeRequestMap>();
        }

        // IQRMarketDataProvider methods
        public void ApplySettings(NamedValueSet settings) { }
        protected virtual void OnStart() { }
        public void Start()
        {
            Logger.LogDebug("Starting...");
            OnStart();
            Logger.LogInfo("Started.");
            // - subscribe to ProviderRuleSet updates
            RuleSet = Client.CreateCache(delegate(CacheChangeData update)
                {
                    try
                    {
                        if (update.Change == CacheChange.ItemCreated || update.Change == CacheChange.ItemUpdated)
                        {
                            var mapRuleSet = (ProviderRuleSet)update.NewItem.Data;
                            if (mapRuleSet.provider == ProviderId)
                            {
                                _marketDataMap.ImportRules(mapRuleSet);
                                Logger.LogDebug("Processed new rule set: {0}", update.NewItem.Name);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e);
                    }
                }, null);
            RuleSet.Subscribe<ProviderRuleSet>(Expr.ALL);
        }

        protected virtual void OnStop() { }
        public void Stop()
        {
            Logger.LogDebug("Stopping...");
            DisposeHelper.SafeDispose(ref RuleSet);
            try
            {
                OnStop();
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
            Logger.LogInfo("Stopped.");
        }

        protected RequestContext ConvertStandardAssetQuotesToProviderInstrFieldCodes(
            MDSRequestType requestType,
            QuotedAssetSet standardQuotedAssetSet)
        {
            // extract assets/quotes that require market quotes
            var standardAssets = new List<Asset>();
            var standardQuotes = new List<BasicQuotation>();
            {
                // build a request/response map (indexed by instrument id)
                var instrumentMap = new Dictionary<string, Asset>();
                //List<Pair<Asset, BasicQuotation>> completeAssetQuotes = new List<Pair<Asset, BasicQuotation>>();

                foreach (Asset asset in standardQuotedAssetSet.instrumentSet.Items)
                {
                    instrumentMap[asset.id.ToLower()] = asset;
                }
                foreach (BasicAssetValuation quoteInstr in standardQuotedAssetSet.assetQuote)
                {
                    string instrId = quoteInstr.objectReference.href;
                    if (!instrumentMap.TryGetValue(instrId.ToLower(), out var asset))
                        throw new ApplicationException(String.Format(
                            "Cannot find instrument '{0}' for assetQuote", instrId));
                    foreach (BasicQuotation quoteField in quoteInstr.quote)
                    {
                        if (quoteField.valueSpecified)
                        {
                            // value provided - dont get from market
                            //completeAssetQuotes.Add(new Pair<Asset, BasicQuotation>(asset, quoteField));
                        }
                        else
                        {
                            // value not supplied - get from market
                            BasicQuotation quote = BasicQuotationHelper.Clone(quoteField);
                            standardAssets.Add(asset);
                            standardQuotes.Add(quote);
                        }
                    }
                }

            }

            var requestItems = new List<RequestItem>();

            var instrConversionMap = new Dictionary<string, string>();
            var instrUniquenessMap = new Dictionary<string, string>();
            var internalInstrIds = new List<string>();
            var fieldConversionMap = new Dictionary<string, string>();
            var fieldUniquenessMap = new Dictionary<string, string>();
            var internalFieldIds = new List<string>();
            Logger.LogDebug("    Mappings    :");
            for (int i = 0; i < standardAssets.Count; i++)
            {
                // map asset to provider instrument id
                Asset standardAsset = standardAssets[i];
                string internalInstrId = standardAsset.id;
                internalInstrIds.Add(internalInstrId);
                string providerInstrId = _marketDataMap.Convert(
                    MDSDictionaryType.Instrument, requestType,
                    MDSProviderId.GlobalIB, ProviderId, internalInstrId,
                    ConvertFailMode.ThrowException);
                // update 1-way map
                instrConversionMap[internalInstrId.ToLower()] = providerInstrId;
                instrUniquenessMap[providerInstrId.ToLower()] = providerInstrId;

                // map quote to provider field name
                BasicQuotation standardQuote = standardQuotes[i];
                string internalFieldId = standardQuote.GetStandardFieldName();
                internalFieldIds.Add(internalFieldId);
                string providerFieldId = _marketDataMap.Convert(
                    MDSDictionaryType.FieldName, requestType,
                    MDSProviderId.GlobalIB, ProviderId, internalFieldId,
                    ConvertFailMode.ThrowException);
                // update 1-way map
                fieldConversionMap[internalFieldId.ToLower()] = providerFieldId;
                fieldUniquenessMap[providerFieldId.ToLower()] = providerFieldId;

                // get provider units
                string providerUnitsId = _marketDataMap.Convert(
                    MDSDictionaryType.QuoteUnits, requestType,
                    MDSProviderId.GlobalIB, ProviderId, $"{internalInstrId}/{internalFieldId}",
                    ConvertFailMode.ThrowException);

                var requestItem = new RequestItem
                                      {
                    StandardAsset = standardAsset,
                    StandardQuote = standardQuote,
                    StandardUnits = PriceQuoteUnitsScheme.ParseEnumString(standardQuote.quoteUnits.Value),
                    ProviderInstrumentId = providerInstrId,
                    ProviderFieldName = providerFieldId,
                    ProviderUnits = PriceQuoteUnitsScheme.ParseEnumString(providerUnitsId)
                };
                requestItems.Add(requestItem);
                // debug
                Logger.LogDebug("      [{0}] '{1}/{2}' ({3}) --> '{4}/{5}' ({6})", i,
                    internalInstrIds[i], internalFieldIds[i], standardQuote.quoteUnits.Value,
                    instrConversionMap[internalInstrIds[i].ToLower()], fieldConversionMap[internalFieldIds[i].ToLower()], providerUnitsId);
                // enddebug
            }
            var providerInstrIds = new List<string>(instrUniquenessMap.Values);
            var providerFieldIds = new List<string>(fieldUniquenessMap.Values);

            // build provider instr/field code sets - todo - for now just build 1
            var results = new List<ProviderInstrFieldCodeSet>();
            var result = new ProviderInstrFieldCodeSet(providerInstrIds, providerFieldIds);
            results.Add(result);
            return new RequestContext
                       {
                RequestItems = requestItems,
                ProviderInstrFieldCodeSets = results,
                InstrConversionMap = instrConversionMap,
                FieldConversionMap = fieldConversionMap
            };
        }

        protected string FormatProviderQuoteKey(string instrumentId, string fieldName)
        {
            return $"[{instrumentId}][{fieldName}]";
        }

        protected List<BasicAssetValuation> ConvertProviderResultsToStandardValuations(
            Dictionary<string, BasicQuotation> providerResults,
            RequestContext requestContext)
        {
            var results = new List<BasicAssetValuation>();
            // build standard results (and convert quote units)
            Logger.LogDebug("    Results     :");
            for (int i = 0; i < requestContext.RequestItems.Count; i++)
            {
                RequestItem requestItem = requestContext.RequestItems[i];
                Asset standardAsset = requestItem.StandardAsset;
                BasicQuotation standardQuote = requestItem.StandardQuote;
                string standardInstrId = standardAsset.id;
                string standardFieldId = standardQuote.GetStandardFieldName();
                PriceQuoteUnitsEnum standardQuoteUnit = requestItem.StandardUnits;
                PriceQuoteUnitsEnum providerQuoteUnit = requestItem.ProviderUnits;
                string providerInstrId = requestContext.InstrConversionMap[standardInstrId.ToLower()];
                string providerFieldId = requestContext.FieldConversionMap[standardFieldId.ToLower()];
                string providerQuoteKey = FormatProviderQuoteKey(providerInstrId, providerFieldId);
                if (providerResults.TryGetValue(providerQuoteKey, out var providerQuote))
                {
                    BasicQuotation convertedQuote;
                    if (providerQuote.valueSpecified)
                    {
                        // valid value returned
                        decimal convertedValue = PriceQuoteUnitsHelper.ConvertPriceQuoteUnitsValue(providerQuoteUnit, standardQuoteUnit, providerQuote.value);
                        convertedQuote = BasicQuotationHelper.Create(standardQuote, convertedValue);
                        // debug
                        Logger.LogDebug("      [{0}] '{1}/{2}' ({3}/{4}) [{5}] --> '{6}/{7}' ({8}/{9}) [{10}]", i,
                            providerInstrId, providerFieldId, AssetMeasureEnum.MarketQuote, providerQuoteUnit, providerQuote.value,
                            standardInstrId, standardFieldId, AssetMeasureEnum.MarketQuote, standardQuoteUnit, convertedQuote.value);
                        // enddebug
                    }
                    else
                    {
                        // no value - copy error details
                        convertedQuote = providerQuote;
                        // debug
                        Logger.LogDebug("      [{0}] '{1}/{2}' ({3}/{4}) [{5}] --> '{6}/{7}' ({8}/{9}) [{10}]", i,
                            providerInstrId, providerFieldId, providerQuote.measureType.Value, providerQuote.quoteUnits.Value, providerQuote.value,
                            standardInstrId, standardFieldId, convertedQuote.measureType.Value, convertedQuote.quoteUnits.Value, convertedQuote.value);
                        // enddebug
                    }
                    // add other provider info
                    convertedQuote.timeSpecified = providerQuote.timeSpecified;
                    convertedQuote.time = providerQuote.time;
                    convertedQuote.valuationDateSpecified = providerQuote.valuationDateSpecified;
                    convertedQuote.valuationDate = providerQuote.valuationDate;
                    convertedQuote.informationSource = providerQuote.informationSource;
                    results.Add(new BasicAssetValuation
                                    {
                        objectReference = new AnyAssetReference { href = standardAsset.id },
                        quote = new[] { convertedQuote }
                    });
                }
            }
            return results;
        }

        protected virtual MDSResult<QuotedAssetSet> OnRequestMarketData(
            IModuleInfo clientInfo,
            Guid requestId,
            MDSRequestType requestType,
            NamedValueSet requestParams,
            DateTimeOffset subsExpires,
            QuotedAssetSet standardQuotedAssetSet)
        {
            throw new NotSupportedException("This provider (" + ProviderId.ToString() + ") does not support the OnRequestMarketData method!");
        }

        public MDSResult<QuotedAssetSet> RequestMarketQuotes(
            IModuleInfo clientInfo,
            Guid requestId,
            NamedValueSet requestParams,
            MDSRequestType requestType,
            DateTimeOffset subsExpires,
            QuotedAssetSet standardQuotedAssetSet)
        {
            Logger.LogDebug("{0}.RequestMarketQuotes", GetType().Name);
            Logger.LogDebug("  Inputs:");
            Logger.LogDebug("    Request Id  : {0}", requestId);
            Logger.LogDebug("    Request Type: {0}", requestType);
            Logger.LogDebug("    Other params: {0}", requestParams.Serialise());

            return OnRequestMarketData(
                clientInfo, requestId, requestType, requestParams, subsExpires, standardQuotedAssetSet);
        }

        protected virtual MDSResult<QuotedAssetSet> OnRequestPricingStructure(
            IModuleInfo clientInfo,
            Guid requestId,
            MDSRequestType requestType,
            NamedValueSet requestParams,
            DateTimeOffset subsExpires,
            NamedValueSet structureParams)
        {
            throw new NotSupportedException("This provider (" + ProviderId.ToString() + ") does not support the OnRequestPricingStructure method!");
        }

        public MDSResult<QuotedAssetSet> RequestPricingStructure(IModuleInfo clientInfo, Guid requestId, NamedValueSet requestParams, MDSRequestType requestType, DateTimeOffset subsExpires, NamedValueSet structureParams)
        {
            Logger.LogDebug("{0}.RequestPricingStructure", GetType().Name);
            Logger.LogDebug("  Inputs:");
            Logger.LogDebug("    Request Id  : {0}", requestId);
            Logger.LogDebug("    Request Type: {0}", requestType);
            Logger.LogDebug("    Curve params: {0}", structureParams.Serialise());
            Logger.LogDebug("    Other params: {0}", requestParams.Serialise());

            return OnRequestPricingStructure(
                clientInfo, requestId, requestType, requestParams, subsExpires, structureParams);
        }

        protected virtual void OnCancelRequest(Guid subscriptionId)
        {
            throw new NotSupportedException("This provider (" + ProviderId.ToString() + ") does not support the OnCancelRequest method!");
        }
        public void CancelRequest(Guid subscriptionId)
        {
            OnCancelRequest(subscriptionId);
        }

        protected virtual void OnPublishMarketData(
            Guid requestId,
            NamedValueSet requestParams,
            TimeSpan dataLifetime,
            QuotedAssetSet marketDataSet)
        {
            throw new NotSupportedException("This provider (" + ProviderId + ") does not support the OnPublishMarketData method!");
        }
        public void PublishMarketData(
            IModuleInfo clientInfo,
            Guid requestId,
            NamedValueSet requestParams,
            TimeSpan dataLifetime,
            QuotedAssetSet marketDataSet)
        {
            OnPublishMarketData(requestId, requestParams, dataLifetime, marketDataSet);
        }

    }

    public class RealtimeRequestMap
    {
        public Guid InternalRequestId { get; }
        public string ExternalRequestId { get; }
        public DateTimeOffset SubsExpires { get; }
        public NamedValueSet RequestProps { get; }
        public RequestContext RequestContext { get; }

        public RealtimeRequestMap(
            Guid internalRequestId,
            string externalRequestId,
            NamedValueSet requestProps,
            DateTimeOffset subsExpires,
            RequestContext requestContext)
        {
            InternalRequestId = internalRequestId;
            ExternalRequestId = externalRequestId;
            RequestProps = requestProps;
            SubsExpires = subsExpires;
            RequestContext = requestContext;
        }
    }

}
