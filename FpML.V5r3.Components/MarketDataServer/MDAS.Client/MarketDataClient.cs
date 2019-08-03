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
using System.Reflection;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using Core.Common;
using FpML.V5r3.Reporting;
using Metadata.Common;
using Orion.Build;
using Orion.Util.Compression;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.RefCounting;
using Orion.Util.Serialisation;
using Orion.Util.Threading;
using Exception = System.Exception;

#endregion

namespace Orion.MDAS.Client
{
    public class MarketDataFactory
    {
        /// <summary>
        /// Creates a market data client for the default environment, connecting to specific servers.
        /// </summary>
        /// <param name="loggerRef">The logger.</param>
        /// <param name="authorisedClient">The authorised client.</param>
        /// <param name="hostList">The host list.</param>
        /// <returns></returns>
        public static IMarketDataClient Create(Reference<ILogger> loggerRef, Assembly authorisedClient, string hostList)
        {
            return new MarketDataClient(loggerRef, null, authorisedClient, BuildConst.BuildEnv, hostList);
        }
    }

    internal class RequestBase
    {
        public readonly Guid RequestId;
        public readonly bool DebugRequest;
        public readonly AsyncResult<List<string>> AsyncResult;
        public readonly Type DataTypeType;
        public readonly DateTimeOffset ExpiryTime;
        public readonly List<string> PartialResults;
        public RequestBase(AsyncResult<List<string>> asyncResult, Type dataType, TimeSpan expiryTimeout, bool debugRequest)
        {
            RequestId = Guid.NewGuid();
            DataTypeType = dataType;
            AsyncResult = asyncResult;
            ExpiryTime = DateTimeOffset.Now + expiryTimeout;
            PartialResults = new List<string>();
            DebugRequest = debugRequest;
        }
    }

    internal class V131ClientInfoWrapper : IModuleInfo
    {
        private readonly V131ClientInfo _clientInfo;
        public V131ClientInfoWrapper(V131ClientInfo clientInfo) { _clientInfo = clientInfo; }

        #region IModuleInfo Members

        public Guid NodeGuid => _clientInfo.NodeGuid;
        public EnvId BuildEnv => CoreHelper.ToEnvId(_clientInfo.BuildEnv);
        public EnvId ConfigEnv => CoreHelper.ToEnvId(_clientInfo.ConfigEnv);
        public string HostName => _clientInfo.HostName;
        public string HostIpV4 => _clientInfo.HostIpV4;
        public string[] NetAddrs => _clientInfo.NetAddrs;
        public string UserName => _clientInfo.UserInfo.UserIdentityName.Split('\\')[1];
        public string UserWDom => _clientInfo.UserInfo.UserIdentityName.Split('\\')[0];
        public string ApplName => _clientInfo.ApplInfo.AssmName;
        public string ApplNVer => _clientInfo.ApplInfo.AssmNVer;
        public string ApplFVer => _clientInfo.ApplInfo.AssmFVer;
        public string ApplPTok => _clientInfo.ApplInfo.AssmPTok;
        public int ApplHash => _clientInfo.ApplInfo.AssmHash;
        public string CoreName => _clientInfo.CompInfo.AssmName;
        public string CoreNVer => _clientInfo.CompInfo.AssmNVer;
        public string CoreFVer => _clientInfo.CompInfo.AssmFVer;
        public string CorePTok => _clientInfo.CompInfo.AssmPTok;
        public int CoreHash => _clientInfo.CompInfo.AssmHash;
        public string UserFullName => null;

        #endregion

        // IIdentity methods
        public string Name => _clientInfo.UserInfo.Name;
        public string AuthenticationType => throw new NotImplementedException();
        public bool IsAuthenticated => throw new NotImplementedException();
    }

    internal class MarketDataClient : IMarketDataClient
    {
        // readonly state
        private readonly Reference<ILogger> _loggerRef;
        public ILogger Logger => _loggerRef.Target;
        private readonly AddressBinding _sessCtrlAddressBinding;
        private readonly AddressBinding _transferAddressBinding;
        private readonly V131ClientInfo _clientInfoV131;
        public IModuleInfo ClientInfo { get; }
        public EnvId ClientEnv => ClientInfo.ConfigEnv;

        // managed state
        private Exception _initialExcp;
        private Guid _sessionId = Guid.Empty;
        private MrktDataSenderV221 _clientBase;
        // default timeout
        private readonly TimeSpan _minTimeout = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _maxTimeout = TimeSpan.FromSeconds(3600);
        private TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);
        public TimeSpan RequestTimeout
        {
            get => _defaultTimeout;
            set
            {
                if ((value < _minTimeout) || (value > _maxTimeout))
                    throw new ArgumentOutOfRangeException($"RequestTimeout",
                        $"Must be between {_minTimeout} and {_maxTimeout}");
                _defaultTimeout = value;
            }
        }

        public MarketDataClient(Reference<ILogger> loggerRef, string applName, Assembly authorisedClient, string env, string hostList)
        {
            if (env == null)
                throw new ArgumentNullException(nameof(env));
            if (loggerRef == null)
                throw new ArgumentNullException(nameof(loggerRef));
            _loggerRef = loggerRef.Clone();
            EnvId envId = EnvHelper.CheckEnv(EnvHelper.ParseEnvName(env));
            Guid clientId = Guid.NewGuid();
            // get user identity and full name
            string userLoginName;
            string userFullName = null;
            using (WindowsIdentity winIdent = WindowsIdentity.GetCurrent())
            {
                userLoginName = winIdent.Name;
            }
            try
            {
                using (var principalContext = new PrincipalContext(ContextType.Domain))
                    using (UserPrincipal principal = UserPrincipal.FindByIdentity(principalContext, IdentityType.SamAccountName, userLoginName))
                    {
                        if (principal != null)
                            userFullName = principal.GivenName + " " + principal.Surname;
                    }
            }
            catch (PrincipalException principalException)
            {
                // swallow - can occur on machines not connected to domain controller
                _loggerRef.Target.LogWarning("UserPrincipal.FindByIdentity failed: {0}: {1}", principalException.GetType().Name, principalException.Message);
            }
            Assembly coreAssembly = Assembly.GetExecutingAssembly();
            ClientInfo = new ModuleInfo(env, clientId, userLoginName, userFullName, authorisedClient, coreAssembly);
            _clientInfoV131 = new V131ClientInfo(
                clientId,
                CoreHelper.ToV131EnvId(envId),
                coreAssembly,
                authorisedClient,
                ClientInfo.Name,
                ClientInfo.UserFullName);
            //if (applName == null) applName = _clientInfoV131.ApplInfo.AssmName;

            // create server connections
            const SvcId svc = SvcId.MarketData;
            string svcName = EnvHelper.SvcPrefix(svc);
            string[] serviceAddrs = EnvHelper.GetServiceAddrs(envId, svc, false);
            if (hostList != null)
                serviceAddrs = hostList.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int defaultPort = EnvHelper.SvcPort(envId, svc);
            ServiceAddress resolvedServer = V111Helpers.ResolveServer(_loggerRef.Target, svcName, new ServiceAddresses(WcfConst.AllProtocols, serviceAddrs, defaultPort),
                new[] { typeof(ISessCtrlV131).FullName, typeof(IMrktDataV221).FullName });
            _sessCtrlAddressBinding = WcfHelper.CreateAddressBinding(
                WcfConst.NetTcp, resolvedServer.Host, resolvedServer.Port, svcName, typeof(ISessCtrlV131).Name);
            _transferAddressBinding = WcfHelper.CreateAddressBinding(
                WcfConst.NetTcp, resolvedServer.Host, resolvedServer.Port, EnvHelper.SvcPrefix(svc), typeof(IMrktDataV221).Name);
            // intialise session
            CallServiceWithRetry(true, false, () => { });
        }

        public void Disconnect()
        {
            // close connection
            try
            {
                using (var session = new SessCtrlSenderV131(_sessCtrlAddressBinding))
                {
                    var header = new V131SessionHeader(_sessionId, Guid.NewGuid(), false, false, null, null, true);
                    session.CloseSessionV131(header);
                }
            }
            catch (CommunicationException wcfExcp)
            {
                _loggerRef.Target.LogWarning("Close session attempt failed: {0}: {1}", wcfExcp.GetType().Name, wcfExcp.Message);
            }
            _sessionId = Guid.Empty;
        }

        public void Dispose()
        {
            Disconnect();
            DisposeHelper.SafeDispose(ref _clientBase);
            //_LoggerRef.Target.LogDebug("Closed");
            _loggerRef.Dispose();
        }

        private delegate void CodeSection();

        private void CallServiceWithRetry(bool throwIfFaulted, bool keepClientOpen, CodeSection codeSection)
        {
            try
            {
                // sanitise retry timeout - 5 mins max
                TimeSpan maxTimeout = _defaultTimeout;
                if (maxTimeout < TimeSpan.Zero)
                    maxTimeout = TimeSpan.Zero;
                if (maxTimeout > TimeSpan.FromSeconds(300))
                    maxTimeout = TimeSpan.FromSeconds(300);
                DateTime expiryTime = DateTime.Now.Add(maxTimeout);
                // attempt to send
                int attempt = 0;
                bool sent = false;
                while (!sent)
                {
                    attempt++;
                    try
                    {
                        if (_clientBase == null)
                        {
                            if (attempt > 1)
                                _loggerRef.Target.LogDebug("Reconnect attempt ({0}) to server at: {1}", attempt, _sessCtrlAddressBinding.Address.Uri.AbsoluteUri);
                            if (_sessionId == Guid.Empty)
                            {
                                using (var session = new SessCtrlSenderV131(_sessCtrlAddressBinding))
                                {
                                    var header = new V131SessionHeader(Guid.Empty, Guid.NewGuid(), false, false, null, null, true);
                                    V131SessionReply sessionReply = session.BeginSessionV131(header, _clientInfoV131);
                                    if (!sessionReply.Success)
                                    {
                                        throw new ApplicationException("Connection rejected: " + sessionReply.Message);
                                    }
                                    _sessionId = sessionReply.SessionId;
                                }
                            }
                            _clientBase = new MrktDataSenderV221(_transferAddressBinding);
                        }
                        codeSection();
                        sent = true;
                        if (attempt > 1)
                            _loggerRef.Target.LogDebug("Transfer attempt ({0}) succeeded.", attempt);
                        _initialExcp = null;
                    }
                    catch (CommunicationException wcfExcp)
                    {
                        // only handle wcf exceptions - others are not our problem
                        if (attempt == 1)
                            _initialExcp = wcfExcp;
                        DisposeHelper.SafeDispose(ref _clientBase);
                        _loggerRef.Target.LogWarning("Transfer attempt ({0}) to server at '{1}' failed: {2}",
                            attempt, _transferAddressBinding.Address.Uri.AbsoluteUri, wcfExcp.GetType().Name);
                        // backoff (between 0 and 10 seconds) if max timeout not reached
                        double backoff = 0.01 * (attempt - 1) * (attempt - 1) + 0.1 * (attempt - 1);
                        if (backoff < 0.0)
                            backoff = 0.0;
                        if (backoff > 10.0)
                            backoff = 10.0;
                        if (DateTime.Now < expiryTime)
                            Thread.Sleep(TimeSpan.FromSeconds(backoff));
                        else
                        {
                            _loggerRef.Target.LogWarning(
                                "Aborting transfer - send timeout ({0}) reached. First exception: {1}. Final exception: {2}.",
                                maxTimeout, _initialExcp.GetType().Name, wcfExcp.GetType().Name);
                            throw new CommunicationException(
                                $"Aborting transfer - send timeout ({maxTimeout}) reached!", _initialExcp);
                        }
                    }
                } // while
            }
            finally
            {
                if (!keepClientOpen)
                    DisposeHelper.SafeDispose(ref _clientBase);
            }
        }

        // IMarketDataClient methods
        public void StartRealtimeFeed(
            IModuleInfo clientInfo,
            Guid subscriptionId,
            NamedValueSet requestParams,
            QuotedAssetSet standardQuotedAssetSet,
            TimeSpan subsLifetime,
            AsyncQueueCallback<QuotedAssetSet> callback)
        {
            //SetRequestParams(requestParams);
            throw new NotSupportedException();
        }
        public void CancelRealtimeFeed(
            IModuleInfo clientInfo,
            Guid subscriptionId,
            AsyncQueueCallback<QuotedAssetSet> callback)
        {
            throw new NotSupportedException();
        }

        public MDSResult<QuotedAssetSet> GetMarketQuotes(
            MDSProviderId provider,
            IModuleInfo clientInfoNotUsed,
            Guid requestId,
            bool throwOnError,
            NamedValueSet requestParams,
            QuotedAssetSet quoteAssetSet)
        {
            // serialise inputs
            byte[] serialisedInputSet = CompressionHelper.CompressToBuffer(XmlSerializerHelper.SerializeToString(quoteAssetSet));

            // call the WCF service
            V221OutputQuotedAssetSet result = null;
            CallServiceWithRetry(false, false, () =>
            {
                result = _clientBase.GetMarketQuotesV221(
                    new V221Header(_sessionId),
                    V221Helpers.ToV221ProviderId(provider), 
                    requestId, 
                    requestParams?.Serialise(),
                    serialisedInputSet);
            });

            var error = (result.Error != null) ? new MDSException(result.Error) : null;
            if (error != null)
            {
                if (throwOnError)
                    throw error;
            }

            // deserialise outputs
            string buffer = CompressionHelper.DecompressToString(result.QuotedAssetSet);
            var result2 = XmlSerializerHelper.DeserializeFromString<QuotedAssetSet>(buffer);
            return new MDSResult<QuotedAssetSet>
                       {
                Result = result2,
                Error = error
            };
        }

        public MDSResult<QuotedAssetSet> GetPricingStructure(
            MDSProviderId provider,
            IModuleInfo clientInfoNotUsed,
            Guid requestId,
            bool throwOnError,
            NamedValueSet requestParams,
            NamedValueSet structureParams)
        {
            // call the WCF service
            V221OutputQuotedAssetSet result = null;
            CallServiceWithRetry(false, false, () =>
            {
                result = _clientBase.GetPricingStructureV221(
                    new V221Header(_sessionId),
                    V221Helpers.ToV221ProviderId(provider),
                    requestId,
                    requestParams?.Serialise(),
                    structureParams?.Serialise());
            });
            if (result.Error != null && throwOnError)
            {
                throw new MDSException(result.Error);
            }
            // deserialise outputs
            return new MDSResult<QuotedAssetSet>
                       {
                Result = XmlSerializerHelper.DeserializeFromString<QuotedAssetSet>(CompressionHelper.DecompressToString(result.QuotedAssetSet)),
                Error = (result.Error != null) ? new MDSException(result.Error) : null
            };
        }
    }
}
