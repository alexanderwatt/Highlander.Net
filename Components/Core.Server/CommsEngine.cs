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

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Highlander.Core.Common;
using Highlander.Utilities.Compression;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.Serialisation;
using Highlander.Utilities.Threading;

#endregion

namespace Highlander.Core.Server
{
    internal interface IConnection
    {
        Guid ClientId { get; }
        string ContractName { get; }
        string ReplyAddress { get; set; }
        NodeType PeerNodeType { get; }
        bool HasFaulted { get; }
        bool HasExpired { get; }
        void ExtendExpiry();
        void DispatchToIncomingThreadQueue<T>(T data, AsyncQueueCallback<T> callback);
        // recv
        void DispatchAsyncRecvSelectMultipleItems(PackageSelectMultipleItems package);
        void DispatchAsyncRecvAnswerMultipleItems(PackageAnswerMultipleItems package);
        void DispatchAsyncRecvCancelSubscription(PackageCancelSubscription package);
        void DispatchAsyncRecvCompletionResult(PackageCompletionResult package);
        void DispatchAsyncRecvCreateSubscription(PackageCreateSubscription package);
        void DispatchAsyncRecvExtendSubscription(PackageExtendSubscription package);
        void DispatchAsyncRecvNotifyMultipleItems(PackageNotifyMultipleItems package);
        // send
        void DispatchAsyncSendAnswerMultipleItems(PackageAnswerMultipleItems package);
        void DispatchAsyncSendCompletionResult(PackageCompletionResult package);
        void DispatchAsyncSendNotifyMultipleItems(PackageNotifyMultipleItems package);
    }

    internal class CommsEngine : ServerPart,
        IDiscoverV111,
        ISessCtrlV131, ITransferV341    // V3.4 contracts
    {
        // readonly state
        private readonly ServerCfg _serverCfg;
        private readonly AsyncThreadQueue _mainCommsDispatcher;

        // start-time state
        private CacheEngine _cacheEngine;
        private IStoreEngine _storeEngine;
        private Timer _housekeepTimer;

        // latest contracts
        private CustomServiceHost<IDiscoverV111, DiscoverRecverV111> _discoverV111ServerHost;
        private CustomServiceHost<ISessCtrlV131, SessCtrlRecverV131> _sessCtrlV131ServerHost;
        private CustomServiceHost<ITransferV341, TransferRecverV341> _transferV341ServerHost;

        // connections
        private readonly GuardedDictionary<Guid, IConnection> _connectionIndex = new GuardedDictionary<Guid, IConnection>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serverCfg"></param>
        public CommsEngine(ILogger logger, ServerCfg serverCfg)
            : base(PartNames.Comms, logger)
        {
            _serverCfg = serverCfg;
            _mainCommsDispatcher = new AsyncThreadQueue(Logger);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // final housekeep cycle
                DisposeHelper.SafeDispose(ref _housekeepTimer);
                DispatchHousekeepTimeout(null);
                _mainCommsDispatcher.WaitUntilEmpty(ServerCfg.ShutdownDisposeDelay);
                // clean up managed resources
                DisposeHelper.SafeDispose(ref _discoverV111ServerHost);
                // V3.4 service hosts
                DisposeHelper.SafeDispose(ref _sessCtrlV131ServerHost);
                DisposeHelper.SafeDispose(ref _transferV341ServerHost);
            }
            // no unmanaged resources to clean up
            base.Dispose(disposing);
        }

        protected override void OnAttached(string name, IServerPart part)
        {
            switch (name)
            {
                case PartNames.Cache:
                    _cacheEngine = (CacheEngine)part;
                    break;
                case PartNames.Store:
                    _storeEngine = (IStoreEngine)part;
                    break;
                default:
                    throw new NotSupportedException($"Unknown part: '{name}'");
            }
        }

        protected override void OnStart()
        {
            // restore un-expired connections
            Logger.LogDebug("Restoring connections...");
            DateTimeOffset dtNow = DateTimeOffset.Now;
            List<CommonItem> connItems = _cacheEngine.GetCacheItems(
                null, ItemKind.Local, typeof(ClientConnectionState).FullName, null, 0, dtNow, true, false);
            foreach (CommonItem item in connItems)
            {
                try
                {
                    var oldConn = XmlSerializerHelper.DeserializeFromString<ClientConnectionState>(
                        CompressionHelper.DecompressToString(item.YData));
                    IConnection connection = null;
                    if (oldConn.Contract == typeof(ITransferV341).FullName)
                    {
                        var clientId = new Guid(oldConn.SourceId);
                        connection = new ConnectionV34(
                            Logger, _cacheEngine, _serverCfg,
                            clientId, oldConn.ReplyAddress, NodeType.Client);
                        connection.ExtendExpiry();
                        _connectionIndex.Set(clientId, connection);
                    }
                    if (connection != null)
                    {
                        Logger.LogDebug("Restored connection:");
                        Logger.LogDebug("  Client Id. : {0}", connection.ClientId);
                        Logger.LogDebug("  Client Addr: {0}", connection.ReplyAddress);
                    }
                    else
                        Logger.LogDebug("Ignoring unsupported connection: '{0}'", oldConn.ReplyAddress);
                }
                catch (Exception e)
                {
                    // failed, however the show must go on
                    Logger.Log(e);
                }
            }
            // restore subscriptions
            Logger.LogDebug("Restoring subscriptions...");
            List<CommonItem> subsItems = _cacheEngine.GetCacheItems(
                null, ItemKind.Local, typeof(ClientSubscriptionState).FullName, null, 0, dtNow, true, false);
            foreach (CommonItem item in subsItems)
            {
                try
                {
                    var oldSubscription = XmlSerializerHelper.DeserializeFromString<ClientSubscriptionState>(
                        CompressionHelper.DecompressToString(item.YData));
                    var subscription = new ClientSubscription(oldSubscription);
                    var clientId = new Guid(oldSubscription.ConnectionId);
                    IConnection connection = GetValidConnection(clientId);
                    if (connection != null)
                    {
                        _cacheEngine.RestoreSubscription(subscription);
                        Logger.LogDebug("Restored subscription:");
                        Logger.LogDebug("  Client Id: {0}", connection.ClientId);
                        Logger.LogDebug("  Address  : {0}", connection.ReplyAddress);
                        Logger.LogDebug("  Subs. Id : {0}", subscription.SubscriptionId);
                        Logger.LogDebug("  AppScopes: {0}", (subscription.AppScopes == null) ? "*" : String.Join(",", subscription.AppScopes));
                        Logger.LogDebug("  ItemKind : {0}", (subscription.ItemKind == ItemKind.Undefined) ? "(any)" : subscription.ItemKind.ToString());
                        Logger.LogDebug("  DataType : {0}", subscription.DataTypeName ?? "(any)");
                        Logger.LogDebug("  Query    : {0}", subscription.Expression.DisplayString());
                        Logger.LogDebug("  MinimumUSN > : {0}", subscription.MinimumUSN);
                        Logger.LogDebug("  Excl.Deleted?: {0}", (subscription.ExcludeDeleted));
                        Logger.LogDebug("  Excl.DataBody: {0}", subscription.ExcludeDataBody);
                    }
                    else
                    {
                        _cacheEngine.DeleteSubscriptionState(subscription.SubscriptionId);
                        Logger.LogDebug("Ignoring expired subscription id: {0}", oldSubscription.SubscriptionId);
                    }
                }
                catch (Exception e)
                {
                    // failed, however the show must go on
                    Logger.Log(e);
                }
            }
            string svcName = EnvHelper.SvcPrefix(SvcId.CoreServer);
            // discovery service
            _discoverV111ServerHost = new CustomServiceHost<IDiscoverV111, DiscoverRecverV111>(
                Logger, new DiscoverRecverV111(this), _serverCfg.V31DiscoEndpoints,
                svcName, typeof(IDiscoverV111).Name, true);
            // V3.4 services
            _sessCtrlV131ServerHost = new CustomServiceHost<ISessCtrlV131, SessCtrlRecverV131>(
                Logger, new SessCtrlRecverV131(this), _serverCfg.V31DiscoEndpoints,
                svcName, typeof(ISessCtrlV131).Name, true);
            _transferV341ServerHost = new CustomServiceHost<ITransferV341, TransferRecverV341>(
                Logger, new TransferRecverV341(this), _serverCfg.V31AsyncEndpoints,
                svcName, typeof(ITransferV341).Name, true);
            // start housekeeping timer
            _housekeepTimer = new Timer(DispatchHousekeepTimeout, null, ServerCfg.CommsHousekeepInterval, ServerCfg.CommsHousekeepInterval);
        }

        private int _housekeepCallsPart1;
        private void DispatchHousekeepTimeout(object notUsed)
        {
            Interlocked.Increment(ref _housekeepCallsPart1);
            _mainCommsDispatcher.Dispatch<object>(null, HousekeepConnections);
        }

        private void HousekeepConnections(object notUsed)
        {
            // calls are accumulative, not discrete
            if (Interlocked.Decrement(ref _housekeepCallsPart1) > 0) return;

            try
            {
                DateTimeOffset dtCommenced = DateTimeOffset.Now;
                int activeCount = 0;
                var expiredConns = new List<IConnection>();
                foreach (IConnection connection in _connectionIndex.GetValues())
                {
                    bool removeConnection = false;
                    if (connection.HasFaulted)
                    {
                        removeConnection = true;
                        Logger.LogDebug("Connection: '{0}' faulted ({1})", connection.ClientId, connection.ReplyAddress);
                    }
                    if (connection.HasExpired)
                    {
                        removeConnection = true;
                        Logger.LogDebug("Connection: '{0}' expired ({1})", connection.ClientId, connection.ReplyAddress);
                    }
                    if (removeConnection)
                    {
                        // client not found or expired
                        expiredConns.Add(connection);
                    }
                    else
                    {
                        activeCount++;
                    }
                }
                foreach (IConnection connection in expiredConns)
                {
                    _connectionIndex.Remove(connection.ClientId);
                    _cacheEngine.DeleteConnectionState(connection.ClientId);
                }
                DateTimeOffset dtCompleted = DateTimeOffset.Now;
                TimeSpan duration = dtCompleted - dtCommenced;
                Logger.LogDebug("---------- Housekeep Connections ----------");
                Logger.LogDebug("Connections");
                Logger.LogDebug("  Active    : {0}", activeCount);
                Logger.LogDebug("  Expired   : {0}", expiredConns.Count);
                Logger.LogDebug("Duration    : {0}s", duration.TotalSeconds);
                Logger.LogDebug("---------- Housekeep Connections ----------");
            }
            catch (Exception e)
            {
                Logger.LogError("HousekeepConnections failed: {0}", e);
            }

            // ---------------------------------------- next part ----------------------------------------
            //Interlocked.Increment(ref _HousekeepCallsPart2);
            //_MainCommsDispatcher.Dispatch<object>(null, HousekeeperPart2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scheme"></param>
        /// <returns></returns>
        public List<string> GetServerAddresses(string scheme)
        {
            var results = new List<string>();
            if (_transferV341ServerHost != null)
                results.AddRange(_transferV341ServerHost.GetIpV4Addresses(scheme));
            return results;
        }

        #region IDiscoverV111 Members

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public V111DiscoverReply DiscoverServiceV111()
        {
            return new V111DiscoverReply
                       {
                SupportedContracts = new[]
                {
                    typeof(IDiscoverV111).FullName,
                    typeof(ISessCtrlV131).FullName,
                    typeof(ITransferV341).FullName
                }
            };
        }

        #endregion

        private bool TryGetValidConnection(Guid clientId, out IConnection connection)
        {
            connection = _connectionIndex.Get(clientId);
            return connection != null && !connection.HasExpired && !connection.HasFaulted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public IConnection GetValidConnection(Guid clientId)
        {
            return TryGetValidConnection(clientId, out var connection) ? connection : null;
        }

        // send
        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void DispatchAsyncSendAnswerMultipleItems(PackageAnswerMultipleItems package)
        {
            if (TryGetValidConnection(package.ClientId, out var connection))
            {
                connection.DispatchAsyncSendAnswerMultipleItems(package);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void DispatchAsyncSendCompletionResult(PackageCompletionResult package)
        {
            if (TryGetValidConnection(package.ClientId, out var connection))
            {
                connection.DispatchAsyncSendCompletionResult(package);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void DispatchAsyncSendNotifyMultipleItems(PackageNotifyMultipleItems package)
        {
            if (TryGetValidConnection(package.ClientId, out var connection))
            {
                connection.DispatchAsyncSendNotifyMultipleItems(package);
            }
        }

        // Core V3.4 server implementation

        #region ISessCtrlV131 Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="clientInfo"></param>
        /// <returns></returns>
        public V131SessionReply BeginSessionV131(V131SessionHeader header, V131ClientInfo clientInfo)
        {
            // validate new client
            Guid clientId = clientInfo.NodeGuid;
            // - ensure configured client/server envs are the same
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
            // - check client version
            const string minimumVersion = "3.4.1723.1"; // 1.1.1501.1  March 01, 2019
            const string optimalVersion = "3.4.1723.1";
            if (!V131Helpers.CheckRequiredFileVersion(Logger, minimumVersion, clientInfo.CompInfo.AssmFVer))
            {
                // older than minimum - reject connection
                string msg = $"Client version ({clientInfo.CompInfo.AssmFVer}) < minimum version ({minimumVersion})!";
                Logger.LogError(msg);
                Logger.LogDebug("Connection: '{0}' rejected ({1})", clientId, header.ReplyAddress);
                return new V131SessionReply(msg);
            }
            if (!V131Helpers.CheckRequiredFileVersion(Logger, optimalVersion, clientInfo.CompInfo.AssmFVer))
            {
                // older than optimal - log warning
                string msg = $"Client version ({clientInfo.CompInfo.AssmFVer}) < optimal version ({optimalVersion})!";
                Logger.LogWarning(msg);
            }
            // - ensure STG/PRD envs servers only accessed by valid clients
            if (_serverCfg.ModuleInfo.ConfigEnv >= EnvId.Stg_StagingLive && (clientInfo.CompInfo.AssmPTok != _serverCfg.ModuleInfo.CorePTok))
            {
                string msg =
                    $"Client signature ({clientInfo.CompInfo.AssmPTok}) <> server signature ({_serverCfg.ModuleInfo.CorePTok})!";
                Logger.LogWarning(msg);
                return new V131SessionReply(msg);
            }
            // - ensure automated unit tests are not running integration tests
            if (_serverCfg.ModuleInfo.ConfigEnv >= EnvId.Sit_SystemTest
                && clientInfo.ApplInfo.AssmName.Equals("QTAgent32", StringComparison.OrdinalIgnoreCase))
            {
                string msg =
                    $"Unauthorised client: {clientInfo.UserInfo.UserIdentityName} {clientInfo.HostName} {clientInfo.ApplInfo.AssmName}";
                Logger.LogWarning(msg);
                return new V131SessionReply(msg);
            }
            IConnection newConnection;
            // build correct connection version
            if (header.ReplyContract == typeof(ITransferV341).FullName)
            {
                newConnection = new ConnectionV34(
                    Logger, _cacheEngine, _serverCfg, clientId, header.ReplyAddress, NodeType.Client);
            }
            else
            {
                // reply contract not supported
                string msg = $"ReplyContract not supported: {header.ReplyContract}";
                Logger.LogWarning(msg);
                return new V131SessionReply(msg);
            }
            // success - grant access
            IConnection connection = _connectionIndex.GetOrSet(clientId, () => newConnection);
            connection.ReplyAddress = header.ReplyAddress;
            _cacheEngine.UpdateConnectionState(connection.ClientId, connection.ContractName, connection.ReplyAddress);
            // update who stats: domain/name/host/app
            StatsCountersDelta.AddToHierarchy(
                $"ConnUser.{clientInfo.UserInfo.UserIdentityName}.{clientInfo.HostName}.{clientInfo.ApplInfo.AssmName}");
            // update where stats: ip/host/app
            StatsCountersDelta.AddToHierarchy(
                $"ConnHost.{clientInfo.HostIpV4}.{clientInfo.HostName}.{clientInfo.ApplInfo.AssmName}");
            // update version stats:
            StatsCountersDelta.AddToHierarchy(
                $"CVersion.{clientInfo.BuildEnv}.{string.Join(".", clientInfo.CompInfo.AssmFVer.Split('.'), 0, 2).Replace('.', '_')}.{String.Join(".", clientInfo.CompInfo.AssmFVer.Split('.'), 2, 2).Replace('.', '_')}.{clientInfo.HostName}.{clientInfo.ApplInfo.AssmName}");
            Logger.LogDebug("Connection: '{0}' created ({1})", clientId, header.ReplyAddress);
            if (header.DebugRequest)
            {
                Logger.LogDebug("  Identity   : {0} ({1})", clientInfo.UserInfo.UserIdentityName, clientInfo.UserInfo.UserFullName);
                Logger.LogDebug("  Application: {0} V{1}/{2} ({3}/{4})", clientInfo.ApplInfo.AssmName, clientInfo.ApplInfo.AssmNVer, clientInfo.ApplInfo.AssmFVer, clientInfo.ApplInfo.AssmPTok, clientInfo.ApplInfo.AssmHash);
                Logger.LogDebug("  Component  : {0} V{1}/{2} ({3}/{4})", clientInfo.CompInfo.AssmName, clientInfo.CompInfo.AssmNVer, clientInfo.CompInfo.AssmFVer, clientInfo.CompInfo.AssmPTok, clientInfo.CompInfo.AssmHash);
                Logger.LogDebug("  Client Env.: {0} ({1} build)", clientInfo.ConfigEnv, clientInfo.BuildEnv);
                Logger.LogDebug("  Client Intf: {0}", header.ReplyContract);
                Logger.LogDebug("  Other Addrs: {0} ({1},{2})", clientInfo.HostName, clientInfo.HostIpV4, String.Join(",", clientInfo.NetAddrs.ToArray()));
            }
            return new V131SessionReply(clientId, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        public void CloseSessionV131(V131SessionHeader header)
        {
            // - remove the old connection (if any)
            IConnection connection = _connectionIndex.Remove(header.SessionId);
            if (connection != null)
            {
                _cacheEngine.DeleteConnectionState(connection.ClientId);
                Logger.LogDebug("Connection: '{0}' removed ({1})", connection.ClientId, connection.ReplyAddress);
            }
        }

        #endregion

        #region ITransferV341 Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        public void TransferV341SelectMultipleItems(V131SessionHeader header, V341SelectMultipleItems body)
        {
            if (TryGetValidConnection(header.SessionId, out var connection))
            {
                connection.DispatchAsyncRecvSelectMultipleItems(
                    new PackageSelectMultipleItems(connection.ClientId, new PackageHeader(header), new PackageSelectItemsQuery(body)));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        public void TransferV341CompletionResult(V131SessionHeader header, V341CompletionResult body)
        {
            if (TryGetValidConnection(header.SessionId, out var connection))
            {
                connection.DispatchAsyncRecvCompletionResult(
                    new PackageCompletionResult(connection.ClientId, new PackageHeader(header), body.Success, body.Result, body.Message));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        public void TransferV341CreateSubscription(V131SessionHeader header, V341CreateSubscription body)
        {
            if (TryGetValidConnection(header.SessionId, out var connection))
            {
                connection.DispatchAsyncRecvCreateSubscription(
                    new PackageCreateSubscription(
                        connection.ClientId, new PackageHeader(header), new PackageSubscriptionQuery(body), body.ExpiryTime));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        public void TransferV341ExtendSubscription(V131SessionHeader header, V341ExtendSubscription body)
        {
            if (TryGetValidConnection(header.SessionId, out var connection))
            {
                connection.ExtendExpiry();
                _cacheEngine.UpdateConnectionState(connection.ClientId, connection.ContractName, connection.ReplyAddress);
                connection.DispatchAsyncRecvExtendSubscription(
                    new PackageExtendSubscription(
                        connection.ClientId, new PackageHeader(header), body.SubscriptionId, body.ExpiryTime));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        public void TransferV341CancelSubscription(V131SessionHeader header, V341CancelSubscription body)
        {
            if (TryGetValidConnection(header.SessionId, out var connection))
            {
                connection.DispatchAsyncRecvCancelSubscription(
                    new PackageCancelSubscription(connection.ClientId, new PackageHeader(header), body.SubscriptionId));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        public void TransferV341AnswerMultipleItems(V131SessionHeader header, V341AnswerMultipleItems body)
        {
            if (TryGetValidConnection(header.SessionId, out var connection))
            {
                connection.DispatchAsyncRecvAnswerMultipleItems(new PackageAnswerMultipleItems(connection.ClientId, header, body));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        public void TransferV341NotifyMultipleItems(V131SessionHeader header, V341NotifyMultipleItems body)
        {
            if (TryGetValidConnection(header.SessionId, out var connection))
            {
                connection.DispatchAsyncRecvNotifyMultipleItems(new PackageNotifyMultipleItems(connection.ClientId, header, body));
            }
        }

        #endregion

    }
}
