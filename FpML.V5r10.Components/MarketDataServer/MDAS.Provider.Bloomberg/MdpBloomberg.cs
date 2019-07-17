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
using Bloomberg.Api;
using Core.Common;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using Metadata.Common;
using Orion.MDAS.Client;
using Orion.Provider;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Exception = System.Exception;

namespace Orion.MDAS.Provider
{
    public class MdpFactoryBloomberg
    {
        public static IQRMarketDataProvider Create(ILogger logger, ICoreClient client, ConsumerDelegate consumer)
        {
            return new MdpBloomberg(logger, client, consumer);
        }
    }

    internal class BloombergRequestProps
    {
        public string CustomerAlias { get; set; }
        public int UUID { get; set; }
        public int SID { get; set; }
        public int SIDInstance { get; set; }
        public int TerminalSID { get; set; }
        public int TerminalSIDInstance { get; set; }
        public string HostName { get; set; }
        public string SourceIp { get; set; }
        public string UserWNId { get; set; }
        public string AssmName { get; set; }
        public int AssmHash { get; set; }
    }

    //internal class BloombergRequestMap
    //{
    //    private readonly Guid _SubscriptionId;
    //    public Guid SubscriptionId { get { return _SubscriptionId; } }
    //    private readonly DateTimeOffset _SubsExpires;
    //    public DateTimeOffset SubsExpires { get { return _SubsExpires; } }
    //    private readonly Guid _BloombergId;
    //    public Guid BloombergId { get { return _BloombergId; } }
    //    private readonly NamedValueSet _RequestParams;
    //    public NamedValueSet RequestProps { get { return _RequestParams; } }

    //    public BloombergRequestMap(
    //        Guid subscriptionId, 
    //        Guid bloombergId,
    //        NamedValueSet requestParams,
    //        DateTimeOffset subsExpires)
    //    {
    //        _SubscriptionId = subscriptionId;
    //        _BloombergId = bloombergId;
    //        _RequestParams = requestParams;
    //        _SubsExpires = subsExpires;
    //    }
    //}

    internal class MdpBloomberg : MdpBaseProvider
    {
        //private readonly IDictionary<Guid, BloombergRequestMap> _SubsMapExternal;

        public MdpBloomberg(ILogger logger, ICoreClient client, ConsumerDelegate consumer)
            : base(logger, client, MDSProviderId.Bloomberg, consumer) { }

        //public void ApplySettings(NamedValueSet settings)
        //{
        //    if (settings != null)
        //    {
        //        ICollection<NamedValue> coll = settings.ToColl();
        //        foreach (NamedValue nv in coll)
        //        {
        //            try
        //            {
        //                _Logger.LogDebug("{0}: Configuration value: {1}='{2}'",
        //                    "MdpBloomberg", nv.ShortName, nv.ValueString);
        //                {
        //                    switch (nv.ShortName.ToLower())
        //                    {
        //                        //case MdpConfigName.Bloomberg_UseServerAPI:
        //                        //    _UseServerAPI = Boolean.Parse(nv.ValueString);
        //                        //    break;
        //                        default:
        //                            _Logger.LogError("{0}: Configuration value '{1}' not supported",
        //                                "MdpBloomberg", nv.ShortName);
        //                            break;
        //                    }
        //                }
        //            }
        //            catch (Exception excp)
        //            {
        //                _Logger.Log(excp);
        //            }
        //        }
        //    }
        //}

        protected override void OnStart()
        {
            // server API startup
            MarketDataAdapter.StatusEvent += MarketDataAdapterStatusEvent;
            MarketDataAdapter.ReplyEvent += MarketDataAdapterReplyEvent;
            MarketDataAdapter.IsServerApi = true;
            MarketDataAdapter.Startup();

            // Get a reference to the FieldTable factory object that has already
            // been loaded during the MarketDataAdapter.Startup method
            //m_ftbl = MarketDataAdapter.FieldTable;
        }

        protected override void OnStop()
        {
            MarketDataAdapter.Shutdown();
        }

        private Dictionary<string, BasicQuotation> BuildProviderResultsIndex(Reply reply)
        {
            // build the provider result set
            DateTime dtNow = DateTime.Now;
            //InformationSource informationSource = ;
            // key = provider instrument id / field name
            var results = new Dictionary<string, BasicQuotation>();
            SecuritiesDataCollection securityColl = reply.GetSecurityDataItems();
            foreach (SecurityDataItem security in securityColl)
            {
                string instrName = security.Security.Name;
                //int fieldCount = security.FieldsData.Count;
                //BasicAssetValuation quote = new BasicAssetValuation() { objectReference = new AnyAssetReference() { href = instrName } };
                //quote.quote = new BasicQuotation[fieldCount];
                int fieldNum = 0;
                foreach (FieldDataItem dataNodeList in security.FieldsData)
                {
                    string fieldName = dataNodeList.Field.Mnemonic;
                    string providerQuoteKey = FormatProviderQuoteKey(instrName, fieldName);
                    var providerQuote = new BasicQuotation();
                    // field value
                    object value = null;
                    string measureTypeAsString = AssetMeasureEnum.Undefined.ToString();
                    string quoteUnitsAsString = PriceQuoteUnitsEnum.Undefined.ToString();
                    try
                    {
                        DataPoint point = dataNodeList.DataPoints[0];
                        value = point.Value;
                        if(point.IsError)
                        {
                            // bloomberg error
                            ReplyError replyError = point.ReplyError;
                            Logger.LogDebug("ReplyError: {0}/{1}={2}/{3}/{4}",
                                instrName, fieldName, replyError.Code, replyError.DisplayName, replyError.Description);
                            quoteUnitsAsString = String.Format("{0}:{1}", replyError.Code, replyError.DisplayName);
                        }
                        else if (value == null)
                        {
                            // value mia
                            Logger.LogDebug("DataNullMissing: {0}/{1}='{2}'", instrName, fieldName);
                            quoteUnitsAsString = "DataNullMissing";
                        }
                        else if (value.GetType() == typeof(ReplyError))
                        {
                            // bloomberg error
                            var replyError = (ReplyError)value;
                            Logger.LogDebug("ReplyError: {0}/{1}={2}/{3}/{4}",
                                instrName, fieldName, replyError.Code, replyError.DisplayName, replyError.Description);
                            quoteUnitsAsString = $"{replyError.Code}:{replyError.DisplayName}";
                        }
                        else if ((value is string) && (value.ToString().ToLower() == "n.a."))
                        {
                            // not available?
                            Logger.LogDebug("DataNotAvailable: {0}/{1}='{2}'", instrName, fieldName, value.ToString());
                            quoteUnitsAsString = "DataNotAvailable";
                        }
                        else
                        {
                            providerQuote.value = Convert.ToDecimal(value);
                            providerQuote.valueSpecified = true;
                            // When the quote was computed (FpML definition) i.e. when the provider published it
                            providerQuote.valuationDateSpecified = true;
                            providerQuote.valuationDate = point.Time;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogDebug("Exception: {0}/{1}='{2}' {3}", instrName, fieldName, value, e);
                        quoteUnitsAsString = $"{e.GetType().Name}:{e.Message}";
                    }
                    providerQuote.measureType = new AssetMeasureType { Value = measureTypeAsString };
                    providerQuote.quoteUnits = new PriceQuoteUnits { Value = quoteUnitsAsString };
                    providerQuote.informationSource = new[]
                    {
                        new InformationSource
                            {
                            rateSource = new InformationProvider { Value = ProviderId.ToString() },
                            rateSourcePage = new RateSourcePage { Value = instrName + "/" + fieldName }
                        }
                    };
                    // When the quote was observed or derived (FpML definition) i.e. now.
                    providerQuote.timeSpecified = true;
                    providerQuote.time = dtNow;
                    results[providerQuoteKey] = providerQuote;
                    // next field
                    fieldNum++;
                } // foreach field
            } // foreach instr
            return results;
        }

        protected override void OnCancelRequest(Guid subscriptionId)
        {
            // scan Bloomberg requestid map looking for our subs id
            RealtimeRequestMap subsMap = null;
            lock (SubsMapExternal)
            {
                foreach (RealtimeRequestMap tempSubsMap in SubsMapExternal.Values)
                {
                    if (tempSubsMap.InternalRequestId == subscriptionId)
                    {
                        subsMap = tempSubsMap;
                        break;
                    }
                }
            }
            if (subsMap != null)
            {
                // bloomberg request found - cancel it
                MarketDataAdapter.CancelRequest(new Guid(subsMap.ExternalRequestId));
                Logger.LogDebug("[Cancelled][ReqId={1}][SubsId={0}]", subscriptionId, subsMap.ExternalRequestId);
            }
            else
            {
                // unknown
                Logger.LogDebug("[ReqIdUnknown][SubsId={0}]", subscriptionId);
            }
        }

        //private BloombergRequestProps ProcessRequestParams(NamedValueSet requestParams)
        //{
        //    BloombergRequestProps result = new BloombergRequestProps();
        //    result.CustomerAlias = null;

        //    //NamedValueSet requestSettings = new NamedValueSet(requestParams);
        //    if (requestParams != null)
        //    {
        //        ICollection<NamedValue> coll = requestParams.ToColl();
        //        foreach (NamedValue nv in coll)
        //        {
        //            try
        //            {
        //                switch (nv.ShortName.ToLower())
        //                {
        //                    case MdpConfigName.Bloomberg_CustName:
        //                        result.CustomerAlias = nv.ValueString;
        //                        break;
        //                    case MdpConfigName.Bloomberg_UUID:
        //                        result.UUID = Int32.Parse(nv.ValueString);
        //                        break;
        //                    case MdpConfigName.Bloomberg_SID:
        //                        result.SID = Int32.Parse(nv.ValueString);
        //                        break;
        //                    case MdpConfigName.Bloomberg_SidN:
        //                        result.SIDInstance = Int32.Parse(nv.ValueString);
        //                        break;
        //                    case MdpConfigName.Bloomberg_TSID:
        //                        result.TerminalSID = Int32.Parse(nv.ValueString);
        //                        break;
        //                    case MdpConfigName.Bloomberg_TSidN:
        //                        result.TerminalSIDInstance = Int32.Parse(nv.ValueString);
        //                        break;
        //                    case MdpConfigName.SourceIp:
        //                        result.SourceIp = nv.ValueString;
        //                        break;
        //                    case MdpConfigName.HostName:
        //                        result.HostName = nv.ValueString;
        //                        break;
        //                    case MdpConfigName.UserWNId:
        //                        result.UserWNId = nv.ValueString;
        //                        break;
        //                    case MdpConfigName.AssmName:
        //                        result.AssmName = nv.ValueString;
        //                        break;
        //                    case MdpConfigName.AssmHash:
        //                        result.AssmHash = Int32.Parse(nv.ValueString);
        //                        break;
        //                    default:
        //                        _Logger.LogDebug(
        //                            "{0}: Parameter {1}='{2}' ignored",
        //                            "MdpBloomberg", nv.ShortName, nv.ValueString));
        //                        break;
        //                }
        //            }
        //            catch (Exception excp)
        //            {
        //                _Logger.Log(excp);
        //            }
        //        }
        //    }
        //    return result;
        //}

        //public CurveSet RequestCurveData(
        //    Guid requestId,
        //    NamedValueSet requestParams,
        //    MDSRequestType requestType,
        //    DateTimeOffset subsExpires,
        //    string[] standardCurveIds,
        //    DateTimeOffset baseDate)
        //{
        //    throw new NotSupportedException();
        //}

        private abstract class ExemptClient
        {
            public readonly EnvId ConfigEnv;

            protected ExemptClient(EnvId configEnv, string assmName, string assmNVer, string assmFVer, string assmPTok, int assmHash, string hostName, string hostIpV4, string userWDom, string userName)
            {
                ConfigEnv = configEnv;
                UserName = userName;
                UserWDom = userWDom;
                HostIpV4 = hostIpV4;
                HostName = hostName;
                AssmHash = assmHash;
                AssmPTok = assmPTok;
                AssmFVer = assmFVer;
                AssmNVer = assmNVer;
                AssmName = assmName;
            }

            public string AssmName { get; }
            public string AssmNVer { get; }
            public string AssmFVer { get; }
            public string AssmPTok { get; }
            public int AssmHash { get; }
            public string HostName { get; }
            public string HostIpV4 { get; }
            public string UserWDom { get; }
            public string UserName { get; }
        }

        private bool IsExempt(IModuleInfo candidate, IEnumerable<ExemptClient> exemptions)
        {
            foreach (ExemptClient exemption in exemptions)
            {
                if (
                       ((exemption.AssmName == null) || (String.Compare(exemption.AssmName, candidate.ApplName, StringComparison.OrdinalIgnoreCase) == 0))
                    && ((exemption.AssmNVer == null) || (String.Compare(exemption.AssmNVer, candidate.ApplNVer, StringComparison.OrdinalIgnoreCase) == 0))
                    && ((exemption.AssmFVer == null) || (String.Compare(exemption.AssmFVer, candidate.ApplFVer, StringComparison.OrdinalIgnoreCase) == 0))
                    && ((exemption.AssmPTok == null) || (String.Compare(exemption.AssmPTok, candidate.ApplPTok, StringComparison.OrdinalIgnoreCase) == 0))
                    && ((exemption.AssmHash == 0) || (exemption.AssmHash == candidate.ApplHash))
                    && ((exemption.HostName == null) || (String.Compare(exemption.HostName, candidate.HostName, StringComparison.OrdinalIgnoreCase) == 0))
                    && ((exemption.HostIpV4 == null) || (String.Compare(exemption.HostIpV4, candidate.HostIpV4, StringComparison.OrdinalIgnoreCase) == 0))
                    && ((exemption.UserWDom == null) || (String.Compare(exemption.UserWDom, candidate.UserWDom, StringComparison.OrdinalIgnoreCase) == 0))
                    && ((exemption.UserName == null) || (String.Compare(exemption.UserName, candidate.UserName, StringComparison.OrdinalIgnoreCase) == 0))
                    && ((exemption.ConfigEnv == EnvId.Undefined) || (exemption.ConfigEnv == candidate.ConfigEnv))
                    )
                    return true;
            }
            return false;
        }

        private MDSResult<QuotedAssetSet> RunBloombergRequest(
            IModuleInfo clientInfo,
            Guid requestId,
            NamedValueSet requestParams,
            MDSRequestType requestType,
            DateTimeOffset subsExpires,
            QuotedAssetSet standardQuotedAssetSet)
		{
            // process request parameters
            //BloombergRequestProps requestProps = ProcessRequestParams(requestParams);

            // process the asset/quote lists to produce 1 or more instrument/field matrices
            RequestContext requestContext = ConvertStandardAssetQuotesToProviderInstrFieldCodes(requestType, standardQuotedAssetSet);

            // process the instr/field code sets
            var results = new List<BasicAssetValuation>();
            foreach (ProviderInstrFieldCodeSet instrFieldCodeSet in requestContext.ProviderInstrFieldCodeSets)
            {
                Request req;
                switch (requestType)
                {
                    case MDSRequestType.Current:
                        req = new RequestForStatic {SubscriptionMode = SubscriptionMode.ByRequest};
                        break;
                    case MDSRequestType.History:
                        req = new RequestForHistory {SubscriptionMode = SubscriptionMode.ByRequest};
                        throw new NotImplementedException("dataRequestType=History");
                    case MDSRequestType.Realtime:
                        req = new RequestForRealtime {SubscriptionMode = SubscriptionMode.ByField};
                        break;
                    default:
                        throw new NotSupportedException("dataRequestType");
                }

                // process request parameters
                //BloombergRequestProps requestProps = ProcessRequestParams(requestParams);

                // Multi-user mode logon check required. Note: some clients have exemptions:
                // - unit test clients
                // - server mode apps (eg. CurveGenerator)
                // Exempted clients must never forward any market data, only derived data.

                Logger.LogDebug("  Identity   : {0} ({1})", clientInfo.Name, clientInfo.UserFullName);
                Logger.LogDebug("  Application: {0} V{1}/{2} ({3}/{4})", clientInfo.ApplName, clientInfo.ApplNVer, clientInfo.ApplFVer, clientInfo.ApplPTok, clientInfo.ApplHash);
                Logger.LogDebug("  Core Client: {0} V{1}/{2} ({3}/{4})", clientInfo.CoreName, clientInfo.CoreNVer, clientInfo.CoreFVer, clientInfo.CorePTok, clientInfo.CoreHash);
                Logger.LogDebug("  Client Env.: {0} ({1} build)", clientInfo.ConfigEnv, clientInfo.BuildEnv);
                Logger.LogDebug("  Addresses  : {0} ({1},{2})", clientInfo.HostName, clientInfo.HostIpV4, String.Join(",", clientInfo.NetAddrs));

                // exemption list - todo - move to database
                //var exemptions = new List<ExemptClient>
                //                     {
                //                         new ExemptClient() {ConfigEnv = EnvId.DEV_Development, AssmName = "TestMds"},
                //                         new ExemptClient() {ConfigEnv = EnvId.DEV_Development, AssmName = "TestWebMdc"},
                //                         new ExemptClient() {AssmName = "nab.QDS.Workflow.CurveGeneration"}
                //                     };
                // - dev apps
                // - all environments

                //bool serverMode = IsExempt(clientInfo, exemptions.ToArray());
                var sapiLicense = new ServerApiLicense();
                if (true) // (serverMode)
                {
                    Logger.LogDebug("Server-mode: client logon check skipped.");
                    sapiLicense.CustomerAlias = "";
                    sapiLicense.UUID = 0;
                    sapiLicense.SID = 0;
                    sapiLicense.SIDInstance = 0;
                    sapiLicense.TerminalSID = 0;
                    sapiLicense.TerminalSIDInstance = 0;
                }
                //else
                //{
                //    _Logger.LogDebug("Multi-user-mode: logon check required.");
                //    sapiLicense.CustomerAlias = requestParams.GetValue<string>(MdpConfigName.Bloomberg_CustName, "unknown");
                //    sapiLicense.UUID = requestParams.GetValue<int>(MdpConfigName.Bloomberg_UUID, 0);
                //    sapiLicense.SID = requestParams.GetValue<int>(MdpConfigName.Bloomberg_SID, 0);
                //    sapiLicense.SIDInstance = requestParams.GetValue<int>(MdpConfigName.Bloomberg_SidN, 0);
                //    sapiLicense.TerminalSID = requestParams.GetValue<int>(MdpConfigName.Bloomberg_TSID, 0);
                //    sapiLicense.TerminalSIDInstance = requestParams.GetValue<int>(MdpConfigName.Bloomberg_TSidN, 0);
                //    IPAddress ipAddress = IPAddress.Parse(clientInfo.HostIpV4);
                //    TerminalMonitor.LogonStatus logonStatus
                //        = TerminalMonitor.Instance.GetLogonStatus(sapiLicense, ipAddress, 5000);
                //    if (logonStatus != TerminalMonitor.LogonStatus.LoggedOn)
                //        throw new ApplicationException(String.Format(
                //            "Bloomberg user (uuid={0}) is not logged on at terminal ({1})", sapiLicense.UUID, ipAddress));
                //}
                req.License = sapiLicense;

                // load instrument ids
                foreach (string instrId in instrFieldCodeSet.InstrumentIds)
                    req.Securities.Add(instrId);

                // load field names
                foreach (string fieldName in instrFieldCodeSet.FieldNames)
                    req.Fields.Add(MarketDataAdapter.FieldTable[fieldName]);

                // call Bloomberg
                if (requestType == MDSRequestType.Realtime)
                {
                    // realtime requests
                    MarketDataAdapter.SendRequest(req);
                    // save subsmap
                    lock (SubsMapExternal)
                    {
                        var subsMap = new RealtimeRequestMap(
                            requestId, req.RequestId.ToString(), requestParams, subsExpires, requestContext);
                        SubsMapExternal.Add(req.RequestId, subsMap);
                    }
                    Logger.LogDebug("[Sent][ReqId={0}][SubsId={1}]", req.RequestId, requestId);
                    return null;
                }
                // snapshot (non-realtime) requests
                Reply reply = MarketDataAdapter.SynchronousRequest(req, 30000);
                // check for errors
                if (reply == null)
                {
                    throw new ApplicationException(
                        String.Format("SynchronousRequest() returned null!"));
                }
                if (reply.ReplyError != null)
                {
                    throw new ApplicationException(
                        $"SynchronousRequest() failed: {reply.ReplyError.DisplayName}: {reply.ReplyError.Description}");
                }
                // process provider results
                Dictionary<string, BasicQuotation> providerResults = BuildProviderResultsIndex(reply);
                results.AddRange(ConvertProviderResultsToStandardValuations(providerResults, requestContext));
            } // foreach request page
            return new MDSResult<QuotedAssetSet>
                       {
                Result = new QuotedAssetSet { assetQuote = results.ToArray() }
            };
        }

        protected override MDSResult<QuotedAssetSet> OnRequestMarketData(
            IModuleInfo clientInfo, 
            Guid requestId, 
            MDSRequestType requestType, 
            NamedValueSet requestParams, 
            DateTimeOffset subsExpires,
            QuotedAssetSet standardQuotedAssetSet)
        {
            return RunBloombergRequest(clientInfo, requestId, requestParams, MDSRequestType.Current, DateTimeOffset.Now, standardQuotedAssetSet);
        }

        protected override void OnPublishMarketData(Guid requestId, NamedValueSet requestParams, TimeSpan dataLifetime, QuotedAssetSet marketDataSet)
        {
            throw new NotSupportedException();
        }

        protected void MarketDataAdapterReplyEvent(Reply reply)
		{
            // process asynchronous reply
            try
            {
                // check for errors first
                if (reply == null)
                {
                    throw new ApplicationException(
                        String.Format("MarketDataAdapter_ReplyEvent() received null!"));
                }
                if (reply.ReplyError != null)
                {
                    throw new ApplicationException(
                        $"MarketDataAdapter_ReplyEvent() failed: {reply.ReplyError.DisplayName}: {reply.ReplyError.Description}");
                }
                string debugMsg = String.Format("[Received]");
                try
                {
                    debugMsg = debugMsg + $"[ReplyType={reply.ReplyType}]";
                    if (reply.Request != null)
                    {
                        debugMsg = debugMsg + $"[ReqId={reply.Request.RequestId}]";
                        SecuritiesDataCollection securityColl = reply.GetSecurityDataItems();
                        foreach (SecurityDataItem security in securityColl)
                        {
                            debugMsg = debugMsg + $"[{security.Security.Name}]";
                            debugMsg = security.FieldsData.Cast<FieldDataItem>().Aggregate(debugMsg, (current, dataNodeList) => current +
                                                                                                                                $"[{dataNodeList.Field.Mnemonic}]");
                        }

                        // map Bloomberg requestid to our subsId
                        RealtimeRequestMap subsMap;
                        lock (SubsMapExternal)
                        {
                            SubsMapExternal.TryGetValue(reply.Request.RequestId, out subsMap);
                        }
                        if (subsMap != null)
                        {

                            if (DateTimeOffset.Now > subsMap.SubsExpires)
                            {
                                MarketDataAdapter.CancelRequest(reply.Request.RequestId);
                                Logger.LogDebug("[Cancelled][ReqId={1}][SubsId={0}]", subsMap.InternalRequestId, reply.Request.RequestId);
                            }
                            Dictionary<string, BasicQuotation> providerResults = BuildProviderResultsIndex(reply);
                            ConsumerCallback(subsMap.InternalRequestId, new QuotedAssetSet { assetQuote = Enumerable.ToArray(ConvertProviderResultsToStandardValuations(providerResults, subsMap.RequestContext)) });
                        }
                    }
                    else
                    {
                        debugMsg = debugMsg + "[RequestIdUnknown]";
                    }
                }
                catch (Exception e)
                {
                    debugMsg = debugMsg + Environment.NewLine + "!!!EXCEPTION!!! " + e.Message;
                }
                finally
                {
                    Logger.LogDebug(debugMsg);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                // don't rethrow
            }
		}

		protected void MarketDataAdapterStatusEvent(StatusCode status, string description)
		{
            Logger.LogInfo(status.ToString() + ": " + description);
		}

    }
}
