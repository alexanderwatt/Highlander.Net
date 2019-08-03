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
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting;
using Metadata.Common;
using Orion.CurveEngine.Assets.Helpers;
using Orion.MDAS.Client;
using Orion.Util.Compression;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using Exception = System.Exception;

#endregion

namespace Orion.MDAS.Server
{
    public partial class MarketDataServer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="v221Provider"></param>
        /// <param name="requestId"></param>
        /// <param name="requestParams"></param>
        /// <param name="zsQuotedAssetSet"></param>
        /// <returns></returns>
        public V221OutputQuotedAssetSet GetMarketQuotesV221(V221Header header, V221ProviderId v221Provider, Guid requestId, string requestParams, byte[] zsQuotedAssetSet)
        {
            IModuleInfo connection = null;
            _connectionIndex.Locked(connections =>
            {
                if (!connections.TryGetValue(header.SessionId, out connection))
                    throw new ApplicationException("Ignoring request from unknown client!");
            });

            //var errors = new List<V221ErrorDetail>();
            var result = new QuotedAssetSet();
            string step = "GetMarketQuotesV221: unknown";
            try
            {
                step = "GetMarketQuotesV221: decompressing";
                // deserialise inputs
                var receivedRequest =
                    XmlSerializerHelper.DeserializeFromString<QuotedAssetSet>(CompressionHelper.DecompressToString(zsQuotedAssetSet));

                //debug dump request
                //this.Logger.LogDebug("Received: {0}", XmlSerializerHelper.SerializeToString<QuotedAssetSet>(receivedRequest));
                // end debug

                step = "GetMarketQuotesV221: splitting";
                // split request into provider-specific requests
                // build unique instrumentSet
                var instrumentMap = new Dictionary<string, Asset>();
                foreach (Asset asset in receivedRequest.instrumentSet.Items)
                {
                    string assetId = asset.id;
                    instrumentMap[assetId.ToLower()] = asset;
                }
                // now split quotes based on provider preferences
                MDSProviderId defaultProvider = V221Helpers.ToProviderId(v221Provider);
                int providerCount = Enum.GetValues(typeof(MDSProviderId)).Length;
                var providerRequests = new RequestContainer[providerCount];
                for (int i = 0; i < providerRequests.Length; i++)
                    providerRequests[i] = new RequestContainer();
                int requestedQuoteCount = 0;
                foreach (BasicAssetValuation valuation in receivedRequest.assetQuote)
                {
                    string assetId = valuation.objectReference.href;
                    if (!instrumentMap.TryGetValue(assetId.ToLower(), out var asset))
                        throw new ApplicationException($"Cannot find asset '{assetId}' in instrument set");
                    foreach (BasicQuotation quote in valuation.quote)
                    {
                        if (!quote.valueSpecified)
                        {
                            requestedQuoteCount++;
                            MDSProviderId quoteProvider = ChooseProvider(quote.informationSource, _activeProviders, defaultProvider);
                            RequestContainer requestContainer = providerRequests[(int)quoteProvider];
                            requestContainer.InstrumentMap[assetId.ToLower()] = asset;
                            // merge the quotes
                            if (!requestContainer.ValuationMap.TryGetValue(assetId.ToLower(), out var bav))
                            {
                                // missing - create
                                bav = new BasicAssetValuation { objectReference = new AnyAssetReference { href = assetId } };
                                requestContainer.ValuationMap[assetId.ToLower()] = bav;
                            }
                            // append the asset quotes
                            var quotes = new List<BasicQuotation>();
                            if (bav.quote != null)
                                quotes.AddRange(bav.quote);
                            quotes.Add(quote);
                            bav.quote = quotes.ToArray();
                        }
                    }
                }
                if (requestedQuoteCount == 0)
                    throw new ApplicationException("No quotes requested!");

                step = "GetMarketQuotesV221: calling providers";
                // run each provider request
                foreach (MDSProviderId activeProvider in _activeProviders)
                {
                    RequestContainer requestContainer = providerRequests[(int)activeProvider];
                    if (requestContainer.InstrumentMap.Count > 0)
                    {
                        // request is not empty - call the MDS provider
                        var types = new List<ItemsChoiceType19>();
                        foreach (var asset in requestContainer.InstrumentMap.Values)
                        {
                            var assetTypeFpML = AssetTypeConvertor.ParseEnumStringToFpML(asset.id);//TODO The id must contain the asset descriptor.
                            types.Add(assetTypeFpML);
                        }
                        var instrumentSet = new InstrumentSet { Items = requestContainer.InstrumentMap.Values.ToArray(), ItemsElementName = types.ToArray() };
                        var providerRequest = new QuotedAssetSet
                                                  {
                                                      instrumentSet = instrumentSet,
                            assetQuote = requestContainer.ValuationMap.Values.ToArray()
                        };
                        step = "GetMarketQuotesV221: calling " + activeProvider.ToString();
                        QuotedAssetSet providerResult = _providers[(int)activeProvider].GetMarketQuotes(
                            activeProvider, connection, requestId, true,
                            new NamedValueSet(requestParams),
                            providerRequest).Result;
                        // combine provider-specific results
                        result = result.Merge(providerResult, false, true, true);
                    }
                }
                step = "GetMarketQuotesV221: compressing";
                return new V221OutputQuotedAssetSet(
                    CompressionHelper.CompressToBuffer(XmlSerializerHelper.SerializeToString(result)),
                    null);
            }
            catch (Exception excp)
            {
                Logger.LogError("Exception: step='{0}': {1}", step, excp);
                return new V221OutputQuotedAssetSet(null, new V221ErrorDetail(excp));
            }
        }

        public V221OutputQuotedAssetSet GetPricingStructureV221(V221Header header, V221ProviderId provider, Guid requestId, string requestParams, string structureProperties)
        {
            IModuleInfo connection = null;
            _connectionIndex.Locked(connections =>
            {
                if (!connections.TryGetValue(header.SessionId, out connection))
                    throw new ApplicationException("Ignoring request from unknown client!");
            });
            //var errors = new List<V221ErrorDetail>();
            string step = "GetPricingStructureV221: unknown";
            try
            {
                step = "GetPricingStructureV221: calling GetPricingStructure";
                // run each provider request
                MDSProviderId defaultProvider = V221Helpers.ToProviderId(provider);
                MDSResult<QuotedAssetSet> result = _providers[(int)defaultProvider].GetPricingStructure(
                    defaultProvider, connection, requestId, true,
                    new NamedValueSet(requestParams),
                    new NamedValueSet(structureProperties));
                step = "GetPricingStructureV221: compressing";
                return new V221OutputQuotedAssetSet(
                    CompressionHelper.CompressToBuffer(XmlSerializerHelper.SerializeToString(result)),
                    null);
            }
            catch (Exception excp)
            {
                Logger.LogError("Exception: step='{0}': {1}", step, excp);
                return new V221OutputQuotedAssetSet(null, new V221ErrorDetail(excp));
            }
        }

        #region ISessCtrlV131 Members

        public V131SessionReply BeginSessionV131(V131SessionHeader header, V131ClientInfo clientInfo)
        {
            // validate new client
            // - ensure configured client/server environments are the same
            if (CoreHelper.ToEnvId(clientInfo.ConfigEnv) != _serverCfg.ModuleInfo.ConfigEnv)
            {
                // not valid
                string msg =
                    $"Client environment ({clientInfo.ConfigEnv}) <> server environment ({_serverCfg.ModuleInfo.ConfigEnv})!";
                Logger.LogWarning(msg);
                return new V131SessionReply(msg);
            }
            // - ensure build environment is backward compatible
            if (CoreHelper.ToEnvId(clientInfo.BuildEnv) < _serverCfg.ModuleInfo.BuildEnv)
            {
                // not valid
                string msg =
                    $"Client build environment ({clientInfo.BuildEnv}) < server build environment ({_serverCfg.ModuleInfo.BuildEnv})!";
                Logger.LogWarning(msg);
                return new V131SessionReply(msg);
            }
            // - ensure STG/PRD environment servers only accessed by valid clients
            if ((_serverCfg.ModuleInfo.ConfigEnv >= EnvId.Stg_StagingLive) && (clientInfo.CompInfo.AssmPTok != _serverCfg.ModuleInfo.CorePTok))
            {
                Logger.LogDebug("Client signature ({0}) <> server signature ({1})!",
                    clientInfo.CompInfo.AssmPTok, _serverCfg.ModuleInfo.CorePTok);
            }
            // check client version
            const string requiredVersion = "2.2.1815.1";
            if (!V131Helpers.CheckRequiredFileVersion(Logger, requiredVersion, clientInfo.CompInfo.AssmFVer))
            {
                // not valid
                string msg = $"Client version ({clientInfo.CompInfo.AssmFVer}) < required version ({requiredVersion})!";
                Logger.LogWarning(msg);
                return new V131SessionReply(msg);
            }
            // valid client
            Guid sessionId = Guid.NewGuid();
            _connectionIndex.Locked(connections =>
            {
                // - update the connection
                connections[sessionId] = new V131ModuleInfo(clientInfo);
                if (header.DebugRequest)
                {
                    Logger.LogDebug("Connection (auto)");
                    Logger.LogDebug("  Identity   : {0} ({1})", clientInfo.UserInfo.UserIdentityName, clientInfo.UserInfo.UserFullName);
                    Logger.LogDebug("  Application: {0} V{1}/{2} ({3}/{4})", clientInfo.ApplInfo.AssmName, clientInfo.ApplInfo.AssmNVer, clientInfo.ApplInfo.AssmFVer, clientInfo.ApplInfo.AssmPTok, clientInfo.ApplInfo.AssmHash);
                    Logger.LogDebug("  Component  : {0} V{1}/{2} ({3}/{4})", clientInfo.CompInfo.AssmName, clientInfo.CompInfo.AssmNVer, clientInfo.CompInfo.AssmFVer, clientInfo.CompInfo.AssmPTok, clientInfo.CompInfo.AssmHash);
                    Logger.LogDebug("  Client Env.: {0} ({1} build)", clientInfo.ConfigEnv, clientInfo.BuildEnv);
                    Logger.LogDebug("  Other Addrs: {0} ({1},{2})", clientInfo.HostName, clientInfo.HostIpV4, String.Join(",", clientInfo.NetAddrs.ToArray()));
                    Logger.LogDebug("  Session Id.: {0}/{1}", sessionId, clientInfo.NodeGuid);
                }
            });
            return new V131SessionReply(sessionId, null);
        }

        public void CloseSessionV131(V131SessionHeader header)
        {
            _connectionIndex.Locked(connections =>
            {
                // - remove the old connection (if any)
                if (connections.Remove(header.SessionId))
                {
                    if (header.DebugRequest)
                    {
                        Logger.LogDebug("Disconnected");
                        Logger.LogDebug("  Session Id.: {0}", header.SessionId);
                    }
                }
            });
        }

        #endregion
    }
}
