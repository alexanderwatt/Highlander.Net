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
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using Highlander.Build;
using Highlander.Core.Common;
using Highlander.GrpcService.Data;
using Highlander.Utilities.Encryption;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Highlander.Utilities.Servers;

#endregion

namespace Highlander.Core.Server
{
        // to do:
        // - autogen build constants
        // - self connection check
        // - unit test full topology
        #region Internal Classes

    public static class CfgPropName
    {
        public const string NodeType = "NodeType";
        public const string EnvName = "EnvName";
        public const string Endpoints = "Endpoints";
        public const string Port = "Port";
        public const string DbServer = "DbServer";
        public const string DbPrefix = "DbPrefix";
    }

    internal class PartNames
    {
        internal const string Cache = "Cache";
        internal const string Store = "Store";
        internal const string Comms = "Comms";
    }

    internal enum ItemSource
    {
        Undefined,
        Client,
        LocalStore,
        PeerServer
    }

    internal class InnerItemRef
    {
        public DateTimeOffset Changed { get; private set; }

        public CommonItem Item { get; private set; }

        public bool Persisted { get; private set; }

        public InnerItemRef(CommonItem item, bool persisted)
        {
            Changed = DateTimeOffset.Now;
            Item = item;
            Persisted = persisted;
        }

        public void Persist()
        {
            if (Persisted) return;
            Changed = DateTimeOffset.Now;
            Persisted = true;
        }

        public void Delete()
        {
            Changed = DateTimeOffset.Now;
            Item = null;
        }
    }

    internal class ClientSubscription
    {
        //public readonly IConnection Connection;
        public readonly Guid ClientId;
        public readonly Guid SubscriptionId;
        //public readonly DateTimeOffset ExpiryTime;
        public readonly string DataTypeName;
        public readonly ItemKind ItemKind;
        public readonly IExpression Expression;
        public readonly string[] AppScopes;
        public readonly long MinimumUSN;
        public readonly bool ExcludeExisting;
        public readonly bool ExcludeDeleted;
        public readonly DateTimeOffset AsAtTime;
        public readonly bool ExcludeDataBody;
        public readonly bool DebugRequest;

        // constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="subscription"></param>
        /// <param name="debugRequest"></param>
        public ClientSubscription(Guid clientId, PackageCreateSubscription subscription, bool debugRequest)
        {
            ClientId = clientId;
            SubscriptionId = subscription.Query.SubscriptionId;
            //ExpiryTime = subscription.ExpiryTime; // was DateTimeOffset.Now.AddMinutes(5);
            DataTypeName = subscription.Query.DataType;
            ItemKind = subscription.Query.ItemKind;
            Expression = (subscription.Query.QueryExpr != null) ? Expr.Create(subscription.Query.QueryExpr) : Expr.ALL;
            AppScopes = subscription.Query.AppScopes;
            MinimumUSN = subscription.Query.MinimumUSN;
            ExcludeExisting = subscription.Query.ExcludeExisting;
            ExcludeDeleted = subscription.Query.ExcludeDeleted;
            AsAtTime = subscription.Query.AsAtTime;
            ExcludeDataBody = subscription.Query.ExcludeDataBody;
            DebugRequest = debugRequest;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscription"></param>
        public ClientSubscription(ClientSubscriptionState subscription)
        {
            ClientId = new Guid(subscription.ConnectionId);
            SubscriptionId = new Guid(subscription.SubscriptionId);
            //if (resetExpiry)
            //    ExpiryTime = DateTimeOffset.Now.Add(ServerCfg.CommsSubscriptionExtension);
            //else
            //    ExpiryTime = DateTimeOffset.Parse(subscription.ExpiryTime);
            DataTypeName = subscription.DataTypeName;
            ItemKind = (ItemKind)subscription.ItemKind;
            Expression = (subscription.Expression != null) ? Expr.Create(subscription.Expression) : Expr.ALL;
            AppScopes = subscription.AppScopes;
            MinimumUSN = subscription.MinimumUSN;
            ExcludeExisting = subscription.ExcludeExisting;
            ExcludeDeleted = !subscription.IncludeDeleted;
            AsAtTime = DateTimeOffset.Parse(subscription.AsAtTime);
            ExcludeDataBody = subscription.ExcludeDataBody;
            DebugRequest = subscription.DebugRequest;
        }
    }

    public class ServerCfg
    {
        // tunable (not configurable) constants
        public static readonly TimeSpan ShutdownDisposeDelay = TimeSpan.FromSeconds(30);
        //public static readonly TimeSpan CacheHeartbeatInterval = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan CacheHousekeepInterval = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan CacheDeletedItemsRetention = TimeSpan.FromMinutes(1);
        public static readonly TimeSpan CacheAncientGuidsRetention = TimeSpan.FromMinutes(2);
        public static readonly TimeSpan CommsHousekeepInterval = TimeSpan.FromSeconds(30);
        //public static readonly TimeSpan CommsSubscriptionExtension = TimeSpan.FromMinutes(60);
        public static readonly TimeSpan CommsConnectionExtension = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan CommsClientSendTimeout = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan StatsReportIntervalDeltas = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan StatsReportIntervalTotals = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan StoreDatabaseRetryInterval = TimeSpan.FromSeconds(60);

        public IModuleInfo ModuleInfo { get; }

        public NodeType ServerMode { get; }

        public string DbServer { get; }

        public string DbPrefix { get; }

        //public string V31AsyncEndpoints { get; }

        //public string V31DiscoEndpoints { get; }

        // constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="moduleInfo"></param>
        /// <param name="serverMode"></param>
        /// <param name="dbServer"></param>
        /// <param name="dbPrefix"></param>
        /// <param name="v31AsyncEndpoints"></param>
        /// <param name="v31DiscoEndpoints"></param>
        public ServerCfg(
            IModuleInfo moduleInfo, NodeType serverMode, 
            string dbServer, string dbPrefix
            //string v31AsyncEndpoints, string v31DiscoEndpoints
            )
        {
            ModuleInfo = moduleInfo;
            ServerMode = serverMode;
            DbServer = dbServer;
            DbPrefix = dbPrefix;
            //V31AsyncEndpoints = v31AsyncEndpoints;
            //V31DiscoEndpoints = v31DiscoEndpoints;
        }
    }

    #endregion

        #region Constructors

    public class CoreServer : BasicServer
    {
        private readonly ServerCfg _serverCfg;
        private readonly HighlanderContext _dbContext;

        // security
        public ICryptoManager CryptoManager { get; } = new DefaultCryptoManager();

        internal CommsEngine CommsEngine { get; set; }

        private IStoreEngine _storeEngine;

        public IStoreEngine StoreEngine { get => _storeEngine;
            set => _storeEngine = value;
        }

        private CacheEngine _cacheEngine;

        // 3.1 service endpoints
        public List<string> GetServerAddresses(string scheme)
        {
            return CommsEngine.GetServerAddresses(scheme);
        }

        // main constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerRef"></param>
        /// <param name="settings"></param>
        public CoreServer(Reference<ILogger> loggerRef, NamedValueSet settings, HighlanderContext dbContext)
        {
            // default configuration
            LoggerRef = loggerRef;
            NodeType serverMode = NodeType.Router;
            var envName = BuildConst.BuildEnv;
            var dbServer = @"localhost\sqlite";
            var dbPrefix = "Core";
            // custom configuration
            if (settings != null)
            {
                serverMode = (NodeType)settings.GetValue(CfgPropName.NodeType, (int)serverMode);
                envName = settings.GetValue(CfgPropName.EnvName, envName);
                dbServer = settings.GetValue(CfgPropName.DbServer, dbServer);
                dbPrefix = settings.GetValue(CfgPropName.DbPrefix, dbPrefix);
            }
            if (serverMode == NodeType.Undefined)
                throw new ArgumentNullException(CfgPropName.NodeType);
            var envId = EnvHelper.CheckEnv(EnvHelper.ParseEnvName(envName));
            // derived configuration
            int port = EnvHelper.SvcPort(envId, SvcId.CoreServer);
            if (settings != null)
            {
                port = settings.GetValue(CfgPropName.Port, port);
            }
            // service endpoints
            //var v31AsyncEndpoints = string.Format("{1}:{0};{2}:{0};{3}:{0}", port, WcfConst.NetMsmq, WcfConst.NetTcp, WcfConst.NetPipe);
            //var v31DiscoEndpoints = string.Format("{1}:{0}", port, WcfConst.NetTcp);
            //if (settings != null)
            //{
            //    v31AsyncEndpoints = settings.GetValue(CfgPropName.Endpoints, v31AsyncEndpoints);
            //    // add default port to endpoints if required
            //    List<string> tempEndpoints = new List<string>();
            //    foreach (var ep in v31AsyncEndpoints.Split(';'))
            //    {
            //        var epParts = ep.Split(':');
            //        var scheme = epParts[0];
            //        if (epParts.Length > 1)
            //            port = int.Parse(epParts[1]);
            //        tempEndpoints.Add($"{scheme}:{port}"); 
            //    }
            //    v31AsyncEndpoints = string.Join(";", tempEndpoints.ToArray());
            //}

            // get user identity and full name
            var os = Environment.OSVersion;
            string userFullName = null;
            string claimsIdentityName = null;
            if(os.Platform == PlatformID.Win32NT)
            {
                var winIdent = WindowsIdentity.GetCurrent();
                claimsIdentityName = winIdent.Name;
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
                if (principal != null)
                {
                    userFullName = principal.GivenName + " " + principal.Surname;
                }
            }
            else if(os.Platform == PlatformID.Unix)
            {
                userFullName = "user";
                claimsIdentityName = "Unix\\user";
            }

            var serverId = Guid.NewGuid();
            _serverCfg = new ServerCfg(
                new ModuleInfo(envName, serverId, claimsIdentityName, userFullName, null, null),
                serverMode, 
                dbServer, dbPrefix
                //v31AsyncEndpoints, v31DiscoEndpoints
                );
            _dbContext = dbContext;
            CommsEngine = new CommsEngine(Logger, _serverCfg);
        }

        // other constructors
        // - router mode
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerRef"></param>
        /// <param name="env"></param>
        /// <param name="nodeType"></param>
        public CoreServer(Reference<ILogger> loggerRef, string env, NodeType nodeType, HighlanderContext dbContext)
            : this(loggerRef, new NamedValueSet(
                new[] { CfgPropName.NodeType, CfgPropName.EnvName },
                new object[] { (int)nodeType, env }), dbContext)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerRef"></param>
        /// <param name="env"></param>
        /// <param name="nodeType"></param>
        /// <param name="port"></param>
        public CoreServer(Reference<ILogger> loggerRef, string env, NodeType nodeType, int port, HighlanderContext dbContext)
            : this(loggerRef, new NamedValueSet(
                new[] { CfgPropName.NodeType, CfgPropName.EnvName, CfgPropName.Port },
                new object[] { (int)nodeType, env, port }), dbContext)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerRef"></param>
        /// <param name="env"></param>
        /// <param name="nodeType"></param>
        /// <param name="port"></param>
        /// <param name="endpoints"></param>
        public CoreServer(Reference<ILogger> loggerRef, string env, NodeType nodeType, int port, string endpoints, HighlanderContext dbContext)
            : this(loggerRef, new NamedValueSet(
                new[] { CfgPropName.NodeType, CfgPropName.EnvName, CfgPropName.Port, CfgPropName.Endpoints },
                new object[] { (int)nodeType, env, port, endpoints }), dbContext)
        { }

        #endregion
        
        #region Static Methods

        // virtual overrides
        protected sealed override void OnBasicSyncStart()
        {
            // create, attach, and start the server parts
            // - store engine
            // - comms manager
            // - cache engine
            if (_serverCfg.ServerMode == NodeType.Server)
            {
                // server mode requires an SQL store
                if (_storeEngine == null)
                    _storeEngine = new StoreEngine(Logger, _serverCfg, _dbContext);
            }
            _cacheEngine = new CacheEngine(Logger, _serverCfg, CryptoManager);
            // connect server parts
            _cacheEngine.Attach(PartNames.Store, _storeEngine);
            _cacheEngine.Attach(PartNames.Comms, CommsEngine);
            CommsEngine.Attach(PartNames.Cache, _cacheEngine);
            _storeEngine?.Attach(PartNames.Cache, _cacheEngine);
            // start server parts
            _storeEngine?.Start();
            _cacheEngine.Start();
            CommsEngine.Start();
        }

        protected sealed override void OnBasicSyncStop()
        {
            CleanUp();
        }

        private void CleanUp()
        {
            DisposeHelper.SafeDispose(CommsEngine);
            DisposeHelper.SafeDispose(ref _cacheEngine);
            DisposeHelper.SafeDispose(ref _storeEngine);
        }

        #endregion
    }
}
