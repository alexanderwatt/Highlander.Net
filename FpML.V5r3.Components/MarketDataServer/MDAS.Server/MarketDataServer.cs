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
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using Core.Common;
using FpML.V5r3.Reporting;
using Metadata.Common;
using Orion.MDAS.Client;
using Orion.MDAS.Provider;
using Orion.Provider;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.RefCounting;
using Orion.Util.Threading;
using Exception = System.Exception;

#endregion

namespace Orion.MDAS.Server
{
    internal enum MdsSubsState
    {
        Unknown,
        Active,
        Complete,
        Cancelled,
        Failed
    }

    public class MarketDataRealtimeClient : IMarketDataClient
    {
        public MDSProviderId ProviderId { get; }

        public AsyncThreadQueue ClientThreadQueue { get; }

        private readonly Reference<ILogger> _loggerRef;
        private MarketDataRealtimeServer _mds;

        public MarketDataRealtimeClient(
            Reference<ILogger> loggerRef,
            AsyncThreadQueue clientThreadQueue,
            ICoreClient client,
            MDSProviderId dataProvider)
        {
            if (dataProvider == MDSProviderId.Undefined)
                throw new ArgumentNullException(nameof(dataProvider));
            ProviderId = dataProvider;
            _loggerRef = loggerRef?.Clone() ?? throw new ArgumentNullException(nameof(loggerRef));
            ClientThreadQueue = clientThreadQueue ?? new AsyncThreadQueue(_loggerRef.Target);
            _mds = new MarketDataRealtimeServer(_loggerRef, clientThreadQueue, client);
            var serverSettings = new NamedValueSet();
            serverSettings.Set(MdpConfigName.ProviderId, (int)dataProvider);
            _mds.ApplySettings(serverSettings);
            _mds.Start();
        }

        public void Dispose()
        {
            DisposeHelper.SafeDispose(ref _mds);
            _loggerRef.Dispose();
        }

        public void CancelRealtimeFeed(
            IModuleInfo clientInfo,
            Guid subscriptionId,
            AsyncQueueCallback<QuotedAssetSet> callback)
        {
            _mds.CancelRealtimeFeed(clientInfo, subscriptionId, callback);
        }

        public void StartRealtimeFeed(
            IModuleInfo clientInfo,
            Guid subscriptionId,
            NamedValueSet requestParams,
            QuotedAssetSet instruments,
            TimeSpan subsLifetime,
            AsyncQueueCallback<QuotedAssetSet> callback)
        {
            _mds.StartRealtimeFeed(
                clientInfo, subscriptionId, requestParams, instruments, subsLifetime, callback);
        }

        public MDSResult<QuotedAssetSet> GetMarketQuotes(
            MDSProviderId provider,
            IModuleInfo clientInfo,
            Guid requestId,
            bool throwOnError,
            NamedValueSet requestParams,
            QuotedAssetSet instruments)
        {
            // construct market data request
            return _mds.GetMarketQuotes(
                provider, clientInfo, requestId, throwOnError, requestParams, instruments);
        }
        public MDSResult<QuotedAssetSet> GetPricingStructure(
            MDSProviderId provider,
            IModuleInfo clientInfo,
            Guid requestId,
            bool throwOnError,
            NamedValueSet requestParams,
            NamedValueSet structureParams)
        {
            return _mds.GetPricingStructure(
                provider, clientInfo, requestId, throwOnError, requestParams, structureParams);
        }
    }

    internal class SubscriptionDetail
    {
        // readonly properties
        public Guid SubsId { get; }
        public MDSRequestType RequestType { get; }
        //private readonly int _DataLifetime;
        //public int DataLifetime { get { return _DataLifetime; } }
        public AsyncQueueCallback<QuotedAssetSet> ClientCallback { get; }
        // modifiable properties
        public MdsSubsState SubsState { get; set; }
        public DateTimeOffset SubsExpires { get; set; }

        public SubscriptionDetail(
            Guid subscriptionId,
            TimeSpan subsLifetime,
            MdsSubsState initialState,
            MDSRequestType requestType,
            AsyncQueueCallback<QuotedAssetSet> clientCallback)
        {
            SubsId = subscriptionId;
            SubsExpires = DateTimeOffset.Now + subsLifetime;
            //_DataLifetime = dataLifetime;
            SubsState = initialState;
            RequestType = requestType;
            ClientCallback = clientCallback;
        }
    }

    internal class ServerCfg
    {
        public ModuleInfo ModuleInfo { get; }
        public string TransEndpoints { get; }
        public string DiscoEndpoints { get; }
        // constructor
        public ServerCfg(
            ModuleInfo moduleInfo,
            string transEndpoints,
            string discoEndpoints)
        {
            ModuleInfo = moduleInfo;
            TransEndpoints = transEndpoints;
            DiscoEndpoints = discoEndpoints;
        }
    }

    public partial class MarketDataServer : ServerBase2, IDiscoverV111, ISessCtrlV131, IMrktDataV221
    {
        private ServerCfg _serverCfg;
        private MDSProviderId[] _activeProviders = { MDSProviderId.Bloomberg };
        private readonly IMarketDataClient[] _providers = new IMarketDataClient[Enum.GetValues(typeof(MDSProviderId)).Length];
        private readonly Guarded<Dictionary<Guid, IModuleInfo>> _connectionIndex =
            new Guarded<Dictionary<Guid, IModuleInfo>>(new Dictionary<Guid, IModuleInfo>());
        // V2.2
        private CustomServiceHost<IDiscoverV111, DiscoverRecverV111> _discoServerHostV111;
        private CustomServiceHost<ISessCtrlV131, SessCtrlRecverV131> _sessCtrlV131ServerHost;
        private CustomServiceHost<IMrktDataV221, MrktDataRecverV221> _mrktDataV221ServerHost;

        protected override void OnServerStarted()
        {
            // default configuration
            _activeProviders = new[] { MDSProviderId.Bloomberg };
            // custom configuration
            string[] enabledProviders = OtherSettings.GetArray<string>(MdsPropName.EnabledProviders);
            if (enabledProviders != null && enabledProviders.Length > 0)
            {
                _activeProviders = enabledProviders.Select(providerName => EnumHelper.Parse<MDSProviderId>(providerName, true)).ToArray();
            }
            // derived configuration
            EnvId envId = IntClient.Target.ClientInfo.ConfigEnv;
            string envName = EnvHelper.EnvName(envId);
            var port = OtherSettings.GetValue(MdsPropName.Port, EnvHelper.SvcPort(envId, SvcId.MarketData));
            // service endpoints
            string transEndpoints = ServiceHelper.FormatEndpoint(WcfConst.NetTcp, port);
            string discoEndpoints = ServiceHelper.FormatEndpoint(WcfConst.NetTcp, port);
            transEndpoints = OtherSettings.GetValue(MdsPropName.Endpoints, transEndpoints);
            // add default port to endpoints if required
            var tempEndpoints = new List<string>();
            foreach (string ep in transEndpoints.Split(';'))
            {
                var epParts = ep.Split(':');
                var scheme = epParts[0];
                var tport = port;
                if (epParts.Length > 1)
                    tport = Int32.Parse(epParts[1]);
                tempEndpoints.Add($"{scheme}:{tport}");
            }
            transEndpoints = String.Join(";", tempEndpoints.ToArray());
            // get user identity and full name
            WindowsIdentity winIdent = WindowsIdentity.GetCurrent();
            UserPrincipal principal = null;
            try
            {
                var principalContext = new PrincipalContext(ContextType.Domain);
                principal = UserPrincipal.FindByIdentity(principalContext, IdentityType.SamAccountName, winIdent.Name);
            }
            catch (PrincipalException principalException)
            {
                // swallow - can occur on machines not connected to domain controller
                Logger.LogWarning("UserPrincipal.FindByIdentity failed: {0}: {1}", principalException.GetType().Name, principalException.Message);
            }
            string userFullName = null;
            if (principal != null)
            {
                userFullName = principal.GivenName + " " + principal.Surname;
            }
            _serverCfg = new ServerCfg(
                new ModuleInfo(envName, Guid.NewGuid(), winIdent.Name, userFullName, null, null),
                transEndpoints, discoEndpoints);
            foreach (MDSProviderId providerId in _activeProviders)
            {
                _providers[(int)providerId] = new MarketDataRealtimeClient(LoggerRef, MainThreadQueue, IntClient.Target, providerId);
            }
            string svcName = EnvHelper.SvcPrefix(SvcId.MarketData);
            // V2.2
            _mrktDataV221ServerHost = new CustomServiceHost<IMrktDataV221, MrktDataRecverV221>(
                Logger, new MrktDataRecverV221(this), _serverCfg.TransEndpoints,
                svcName, typeof(IMrktDataV221).Name, true);
            _sessCtrlV131ServerHost = new CustomServiceHost<ISessCtrlV131, SessCtrlRecverV131>(
                Logger, new SessCtrlRecverV131(this), _serverCfg.DiscoEndpoints,
                svcName, typeof(ISessCtrlV131).Name, true);
            _discoServerHostV111 = new CustomServiceHost<IDiscoverV111, DiscoverRecverV111>(
                Logger, new DiscoverRecverV111(this), _serverCfg.DiscoEndpoints,
                svcName, typeof(IDiscoverV111).Name, true);
        }

        protected override void OnServerStopping()
        {
            // V2.2
            DisposeHelper.SafeDispose(ref _discoServerHostV111);
            DisposeHelper.SafeDispose(ref _sessCtrlV131ServerHost);
            DisposeHelper.SafeDispose(ref _mrktDataV221ServerHost);
            for (int i = 0; i < _providers.Length; i++)
                DisposeHelper.SafeDispose(ref _providers[i]);
        }

        //#region IDiscoverV101 Members

        //public V101SessionReply DiscoverService(V101ClientInfo clientInfo)
        //{
        //    // validate new client
        //    // - ensure configured client/server envs are the same
        //    if (CoreHelper.ToEnvId(clientInfo.ConfigEnv) != _ServerCfg.ModuleInfo.ConfigEnv)
        //    {
        //        // not valid
        //        string msg = String.Format("Client environment ({0}) <> server environment ({1})!",
        //            clientInfo.ConfigEnv, _ServerCfg.ModuleInfo.ConfigEnv);
        //        Logger.LogWarning(msg);
        //        return new V101SessionReply(msg);
        //    }
        //    // - ensure build environment is backward compatible
        //    if (CoreHelper.ToEnvId(clientInfo.BuildEnv) < BuildConst.BuildEnv)
        //    {
        //        // not valid
        //        string msg = String.Format("Client build environment ({0}) < server build environment ({1})!",
        //            clientInfo.BuildEnv, BuildConst.BuildEnv);
        //        Logger.LogWarning(msg);
        //        return new V101SessionReply(msg);
        //    }
        //    // - ensure STG/PRD envs servers only accessed by valid clients
        //    if ((_ServerCfg.ModuleInfo.ConfigEnv >= EnvId.STG_StagingLive) && (clientInfo.CompPTok != _ServerCfg.ModuleInfo.CompPTok))
        //    {
        //        Logger.LogDebug("Client signature ({0}) <> server signature ({1})!",
        //            clientInfo.CompPTok, _ServerCfg.ModuleInfo.CompPTok);
        //    }
        //    // grant access
        //    Guid token = Guid.NewGuid();
        //    _GrantedSessions.Locked((tokens) => tokens.Add(new GrantedSession(token, clientInfo)));
        //    return new V101SessionReply(token, null);
        //}

        //#endregion

        private MDSProviderId ChooseProvider(IEnumerable<InformationSource> sources, MDSProviderId[] activeProviders, MDSProviderId defaultProvider)
        {
            if (sources != null)
            {
                foreach (InformationSource source in sources)
                {
                    if (source.rateSource?.Value != null)
                    {
                        if (EnumHelper.TryParse(source.rateSource.Value, true, out MDSProviderId result))
                        {
                            // check preferred provider is active
                            if (activeProviders.Any(enabledProvider => result == enabledProvider))
                            {
                                return result;
                            }
                        }
                        else
                        {
                            // log warning
                            Logger.LogWarning("ChooseProvider: Unknown MDSProviderId: '{0}'", source.rateSource.Value);
                        }
                    }
                }
                // log warning
                Logger.LogWarning("ChooseProvider: Using default MDSProviderId: '{0}'", defaultProvider);
            }
            return defaultProvider;
        }

        private class RequestContainer
        {
            public readonly Dictionary<string, Asset> InstrumentMap = new Dictionary<string, Asset>();
            public readonly Dictionary<string, BasicAssetValuation> ValuationMap = new Dictionary<string, BasicAssetValuation>();
        }

        #region IDiscoverV111 Members

        public V111DiscoverReply DiscoverServiceV111()
        {
            return new V111DiscoverReply
                       {
                SupportedContracts = new[]
                {
                    //typeof(IDiscoverV101).FullName
                    //,typeof(IMarketDataV212).FullName
                    typeof(IDiscoverV111).FullName,
                    typeof(ISessCtrlV131).FullName,
                    typeof(IMrktDataV221).FullName
                }
            };
        }

        #endregion

    }

    public class MarketDataRealtimeServer : IMarketDataClient
    {
        private readonly Reference<ILogger> _loggerRef;
        private readonly AsyncThreadQueue _threadQueue;
        private readonly ICoreClient _client;

        // todo - housekeeping - expire old subscriptions
        private readonly IDictionary<Guid, SubscriptionDetail> _subsDetailIndex;

        private MDSProviderId _providerId;
        private NamedValueSet _providerSettings;
        // client properties
        private bool _started;
        // configurable settings
        //private MDSProviderId _ProviderId = MDSProviderId.Undefined;
        // variable state
        private IQRMarketDataProvider _marketDataProvider;

        public MarketDataRealtimeServer(
            Reference<ILogger> loggerRef,
            AsyncThreadQueue threadQueue,
            ICoreClient client)
        {
            _loggerRef = loggerRef?.Clone() ?? throw new ArgumentNullException(nameof(loggerRef));
            _threadQueue = threadQueue;
            _client = client;
            _subsDetailIndex = new Dictionary<Guid, SubscriptionDetail>();
        }

        public void Dispose()
        {
            Stop();
            _loggerRef.Dispose();
        }

        public MDSProviderId ProviderId
        {
            get => _providerId;
            set
            {
                if (_started)
                    throw new ApplicationException("Already started!");
                _providerId = value;
            }
        }

        //public IRuntime Runtime
        //{
        //    get { return _Client; }
        //    set
        //    {
        //        if (_Started)
        //            throw new ApplicationException("Already started!");
        //        _Client = value;
        //    }
        //}

        public void ApplySettings(NamedValueSet settings)
        {
            if (_started)
                throw new ApplicationException("Already started!");
            if (settings != null)
            {
                if (_providerSettings == null)
                    _providerSettings = new NamedValueSet();
                ICollection<NamedValue> coll = settings.ToColl();
                foreach (NamedValue nv in coll)
                {
                    try
                    {
                        _loggerRef.Target.LogDebug(
                            "{0}: Configuration value: {1}='{2}'",
                            "MarketDataServerComponent", nv.Name, nv.ValueString);
                        if (nv.Name.StartsWith(MdpConfigName.ProviderPrefix,
                            StringComparison.InvariantCultureIgnoreCase))
                        {
                            // provider settings
                            _providerSettings.Set(nv.Name.Substring(MdpConfigName.ProviderPrefix.Length), nv.Value);
                        }
                        else
                        {
                            switch (nv.Name.ToLower())
                            {
                                case MdpConfigName.ProviderId:
                                    _providerId = (MDSProviderId)Int32.Parse(nv.ValueString);
                                    break;
                                default:
                                    _loggerRef.Target.LogDebug(
                                        "{0}: Configuration value '{1}' not supported",
                                        "MarketDataServerComponent", nv.Name);
                                    break;
                            }
                        }
                    }
                    catch (Exception excp)
                    {
                        _loggerRef.Target.Log(excp);
                    }
                }
            }
        }

        public void Start()
        {
            if (_started)
                throw new ApplicationException("Already started!");
            //if (_Client == null)
            //    throw new ArgumentNullException("client");
            // MDS startup
            switch (_providerId)
            {
                case MDSProviderId.Bloomberg:
                    _marketDataProvider = MdpFactoryBloomberg.Create(_loggerRef.Target, _client, OnProviderDataCallback);
                    break;
                case MDSProviderId.Simulator:
                    _marketDataProvider = MdpFactorySimulator.Create(_loggerRef.Target, _client, OnProviderDataCallback);
                    break;
                //case MDSProviderId.ReutersIDN:
                //case MDSProviderId.ReutersDTS:
                //    _marketDataProvider = MdpFactoryReuters.Create(_loggerRef.Target, _client, _ProviderId, OnProviderDataCallback);
                //    break;
                case MDSProviderId.GlobalIB:
                    _marketDataProvider = MdpFactoryGlobalIB.Create(_loggerRef.Target, _client, OnProviderDataCallback);
                    break;
                case MDSProviderId.nabCurveDb:
                    _marketDataProvider = MdpFactoryNabCurveDb.Create(_loggerRef.Target, _client, OnProviderDataCallback);
                    break;
                default:
                    throw new ArgumentException("Unknown MarketDataProviderId: " + _providerId);
            }

            _marketDataProvider.ApplySettings(_providerSettings);
            _marketDataProvider.Start();
            _started = true;
        }

        public void Stop()
        {
            if (_started)
            {
                _marketDataProvider.Stop();
            }
            _marketDataProvider = null;
            _started = false;
        }

        public MDSResult<QuotedAssetSet> GetMarketQuotes(
            MDSProviderId provider,
            IModuleInfo clientInfo,
            Guid requestId,
            bool throwOnError,
            NamedValueSet requestParams,
            QuotedAssetSet standardQuotedAssetSet)
        {
            return _marketDataProvider.RequestMarketQuotes(
                clientInfo,
                requestId,
                requestParams,
                MDSRequestType.Current,
                DateTimeOffset.MinValue,
                standardQuotedAssetSet);
        }

        public MDSResult<QuotedAssetSet> GetPricingStructure(
            MDSProviderId provider,
            IModuleInfo clientInfo, 
            Guid requestId,
            bool throwOnError,
            NamedValueSet requestParams, 
            NamedValueSet structureParams)
        {
            return _marketDataProvider.RequestPricingStructure(
                clientInfo,
                requestId,
                requestParams,
                MDSRequestType.Current,
                DateTimeOffset.MinValue,
                structureParams);
        }

        public void StartRealtimeFeed(
            IModuleInfo clientInfo,
            Guid subscriptionId,
            NamedValueSet requestParams,
            QuotedAssetSet standardQuotedAssetSet,
            TimeSpan subsLifetime,
            AsyncQueueCallback<QuotedAssetSet> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            // check if subscription is old or expired - todo
            var subsDetail = new SubscriptionDetail(
                    subscriptionId, subsLifetime, MdsSubsState.Unknown, MDSRequestType.Realtime, callback);
            lock (_subsDetailIndex)
            {
                _subsDetailIndex.Add(subscriptionId, subsDetail);
            }

            try
            {
                // exist if request not for us
                //if (request.dataProvider != _ProviderId)
                //    return;

                // check if subscription is too old
                if (subsDetail.SubsExpires < DateTimeOffset.Now)
                    return;

                // call market data provider plugin
                QuotedAssetSet results = _marketDataProvider.RequestMarketQuotes(
                    clientInfo,
                    subsDetail.SubsId,
                    requestParams,
                    MDSRequestType.Realtime,
                    subsDetail.SubsExpires,
                    standardQuotedAssetSet).Result;

                // update subscription status
                subsDetail.SubsState = MdsSubsState.Active;

                // publish market data reply
                // convert provider ids to internal ids
                //QuotedAssetSet internalResults = ConvertResults(instrConversionMap, providerResults, MDSRequestType.Realtime);
                 _threadQueue.Dispatch(results, callback);
                subsDetail.SubsState = MdsSubsState.Complete;
            }
            catch (Exception)
            {
                subsDetail.SubsState = MdsSubsState.Failed;
            }
        }

        public void CancelRealtimeFeed(
            IModuleInfo clientInfo,
            Guid subscriptionId,
            AsyncQueueCallback<QuotedAssetSet> callback)
        {
            // check if subscription exists
            lock (_subsDetailIndex)
            {
                if (_subsDetailIndex.TryGetValue(subscriptionId, out var subsDetail))
                {
                    // found - cancel it
                    _marketDataProvider.CancelRequest(subscriptionId);
                    subsDetail.SubsState = MdsSubsState.Cancelled;
                    subsDetail.SubsExpires = DateTimeOffset.Now;
                    //_SubsDetailIndex.Remove(subscriptionId);
                }
            }
        }

        public void OnProviderDataCallback(Guid subscriptionId, QuotedAssetSet results)
        {
            // callback for processing realtime data
            lock (_subsDetailIndex)
            {
                if (_subsDetailIndex.TryGetValue(subscriptionId, out var subsDetail))
                {
                    _threadQueue.Dispatch(results, subsDetail.ClientCallback);
                }
            }
        }
    }

}
