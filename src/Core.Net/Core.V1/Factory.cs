/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

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
using System.Reflection;
using Highlander.Core.Common;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using Highlander.Utilities.Threading;

#endregion

namespace Highlander.Core.V1
{
    internal class ClientSettings
    {
        public string Env;
        public ServiceAddress ServerAddress;
        public bool DebugRequests;
        public TimeSpan RequestTimeout;
        public TimeSpan OfflineTimeout;
        public CoreModeEnum CoreMode;
        public Assembly ApplAssembly;
    }

    internal class FactoryState
    {
        public readonly Dictionary<string, ClientSettings> SettingsCache = new Dictionary<string, ClientSettings>();
        public bool CacheEnabled = true; //is this used?
        public bool UseFallbackServers;
        public string Env;
        public string Servers;
        public string Protocols;
        public bool DebugRequests;
        public TimeSpan RequestTimeout = TimeSpan.Zero;
        public TimeSpan OfflineTimeout = TimeSpan.Zero;
        public CoreModeEnum CoreMode = CoreModeEnum.Standard;
        public Assembly ApplAssembly;
    }

    /// <summary>
    /// Factory class for creating a core (client) client.
    /// </summary>
    public class CoreClientFactory : IDisposable
    {
        // settings cache
        private readonly Guarded<FactoryState> _factoryState = new Guarded<FactoryState>(new FactoryState());

        // default settings
        // - non-cached settings
        private readonly Reference<ILogger> _loggerRef;

        /// <summary>
        /// 
        /// </summary>
        public bool UseFallbackServers
        {
            get
            {
                bool result = false;
                _factoryState.Locked(state => result = state.UseFallbackServers);
                return result;
            }
            set
            {
                _factoryState.Locked(state =>
                {
                    state.UseFallbackServers = value;
                    state.SettingsCache.Clear();
                });
            }
        }

        public bool CacheEnabled
        {
            get
            {
                bool result = false;
                _factoryState.Locked((state) => result = state.CacheEnabled);
                return result;
            }
            set
            {
                _factoryState.Locked((state) =>
                {
                    state.CacheEnabled = value;
                    if (!value)
                        state.SettingsCache.Clear();
                });
            }
        }

        // - cached settings
        /// <summary>
        /// </summary>
        public TimeSpan RequestTimeout
        {
            get
            {
                TimeSpan result = TimeSpan.Zero;
                _factoryState.Locked(state => result = state.RequestTimeout);
                return result;
            }
            set
            {
                _factoryState.Locked(state => state.RequestTimeout = value);
            }
        }

        /// <summary>
        /// </summary>
        public TimeSpan OfflineTimeout
        {
            get
            {
                TimeSpan result = TimeSpan.Zero;
                _factoryState.Locked(state => result = state.OfflineTimeout);
                return result;
            }
            set
            {
                _factoryState.Locked(state => state.OfflineTimeout = value);
            }
        }

        /// <summary>
        /// </summary>
        public CoreModeEnum CoreMode
        {
            get
            {
                CoreModeEnum result = CoreModeEnum.Standard;
                _factoryState.Locked(state => result = state.CoreMode);
                return result;
            }
            set
            {
                _factoryState.Locked(state => state.CoreMode = value);
            }
        }

        /// <summary>
        /// </summary>
        public Assembly ApplAssembly
        {
            get
            {
                Assembly result = null;
                _factoryState.Locked(state => result = state.ApplAssembly);
                return result;
            }
            set
            {
                _factoryState.Locked(state => state.ApplAssembly = value);
            }
        }

        /// <summary>
        /// </summary>
        public bool DebugRequests
        {
            get
            {
                bool result = false;
                _factoryState.Locked(state => result = state.DebugRequests);
                return result;
            }
            set
            {
                _factoryState.Locked(state => state.DebugRequests = value);
            }
        }

        /// <summary>
        /// </summary>
        public string Env
        {
            get
            {
                string result = null;
                _factoryState.Locked(state => result = state.Env);
                return result;
            }
            set
            {
                _factoryState.Locked(state => state.Env = value);
            }
        }

        /// <summary>
        /// </summary>
        public string Servers
        {
            get
            {
                string result = null;
                _factoryState.Locked(state => result = state.Servers);
                return result;
            }
            set
            {
                _factoryState.Locked(state => state.Servers = value);
            }
        }

        /// <summary>
        /// </summary>
        /// <summary>
        /// Gets or sets the list of protocols. e.g. "net.tcp;net.msmq". Default is "net.tcp".
        /// </summary>
        /// <value>The protocols.</value>
        public string Protocols
        {
            get
            {
                string result = null;
                _factoryState.Locked(state => result = state.Protocols);
                return result;
            }
            set
            {
                _factoryState.Locked(state => state.Protocols = value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreClientFactory"/> class.
        /// </summary>
        public CoreClientFactory(Reference<ILogger> loggerRef) { _loggerRef = loggerRef.Clone(); }

        /// <summary>
        /// </summary>
        public void Dispose() { }

        // custom settings
        /// <summary>
        /// Sets the environment (useful for utilities and testing).
        /// </summary>
        /// <param name="env">The environment.</param>
        /// <returns></returns>
        public CoreClientFactory SetEnv(string env)
        {
            Env = env ?? throw new ArgumentNullException(nameof(env));
            return this;
        }

        /// <summary>
        /// Sets the server address list (useful for utilities and testing).
        /// </summary>
        /// <param name="servers">The (semicolon separated) server address list. Eg. "localhost:9001;sydwadqds01"</param>
        /// <returns></returns>
        public CoreClientFactory SetServers(string servers)
        {
            Servers = servers;
            return this;
        }

        /// <summary>
        /// Sets the debug requests flag for clients created by the factory.
        /// </summary>
        /// <param name="debugRequests">if set to <c>true</c> [debug requests].</param>
        /// <returns></returns>
        public CoreClientFactory SetDebugRequests(bool debugRequests)
        {
            DebugRequests = debugRequests;
            return this;
        }

        /// <summary>
        /// Sets the default request timeout for clients created by the factory.
        /// </summary>
        /// <param name="requestTimeout">The request timeout.</param>
        /// <returns></returns>
        public CoreClientFactory SetRequestTimeout(TimeSpan requestTimeout)
        {
            RequestTimeout = requestTimeout;
            return this;
        }

        /// <summary>
        /// Sets the default offline timeout for clients created by the factory.
        /// </summary>
        /// <param name="offlineTimeout">The offline timeout.</param>
        /// <returns></returns>
        public CoreClientFactory SetOfflineTimeout(TimeSpan offlineTimeout)
        {
            OfflineTimeout = offlineTimeout;
            return this;
        }

        /// <summary>
        /// Sets the default request timeout for clients created by the factory.
        /// </summary>
        /// <param name="coreMode">The coreMode.</param>
        /// <returns></returns>
        public CoreClientFactory SetCoreMode(CoreModeEnum coreMode)
        {
            CoreMode = coreMode;
            return this;
        }

        /// <summary>
        /// Sets the application assembly to be used for identification and authorization. If null is given, the entry assembly is assumed.
        /// </summary>
        /// <param name="applAssembly">The application assembly.</param>
        /// <returns></returns>
        public CoreClientFactory SetApplication(Assembly applAssembly)
        {
            ApplAssembly = applAssembly;
            return this;
        }

        /// <summary>
        /// Sets the transport scheme used by WCF (default is net.tcp).
        /// </summary>
        /// <param name="protocols">The protocol.</param>
        /// <returns></returns>
        public CoreClientFactory SetProtocols(string protocols)
        {
            Protocols = protocols;
            return this;
        }

        /// <summary>
        /// Enables or disables fail over to fallback servers if connection to all primary servers fails.
        /// This setting is ignored if servers are specified.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public CoreClientFactory SetFailOver(bool value)
        {
            UseFallbackServers = value;
            return this;
        }

        // constructors
        /// <summary>
        /// </summary>
        public ICoreClient Create() { return Create(null); }

        /// <summary>
        /// </summary>
        public ICoreClient Create(string instanceName)
        {
            string cacheKey = null;
            string env = null;
            string servers = null;
            string protocols = null;
            bool debugRequests = false;
            TimeSpan requestTimeout = TimeSpan.Zero;
            TimeSpan offlineTimeout = TimeSpan.Zero;
            CoreModeEnum coreMode = CoreModeEnum.Standard;
            Assembly applAssembly = null;
            bool useFallbackServers = true;
            //bool cacheEnabled = false;
            _factoryState.Locked(state =>
            {
                useFallbackServers = state.UseFallbackServers;
                //cacheEnabled = state.CacheEnabled;
                env = state.Env;
                servers = state.Servers;
                protocols = state.Protocols;
                debugRequests = state.DebugRequests;
                requestTimeout = state.RequestTimeout;
                offlineTimeout = state.OfflineTimeout;
                coreMode = state.CoreMode;
                applAssembly = state.ApplAssembly;
                cacheKey = $"[{env}][{(protocols ?? "").ToLower()}][{(servers ?? "").ToLower()}]";
                if (state.SettingsCache.TryGetValue(cacheKey, out var cachedSettings))
                {
                    env = cachedSettings.Env;
                    servers = $"{cachedSettings.ServerAddress.Host}:{cachedSettings.ServerAddress.Port}";
                    protocols = cachedSettings.ServerAddress.Protocol;
                    useFallbackServers = false;
                    debugRequests = cachedSettings.DebugRequests;
                    requestTimeout = cachedSettings.RequestTimeout;
                    offlineTimeout = cachedSettings.OfflineTimeout;
                    coreMode = cachedSettings.CoreMode;
                    applAssembly = cachedSettings.ApplAssembly;
                    //_logger.LogDebug("Using cached settings: {0} --> {1}://{2}", cacheKey, scheme, servers);
                }
            });
            ICoreClient client = new CoreClient(
                _loggerRef, instanceName, env, servers, protocols, useFallbackServers, debugRequests, 
                requestTimeout, offlineTimeout, coreMode, applAssembly);
            // save settings
            _factoryState.Locked(state =>
            {
                state.SettingsCache[cacheKey] = new ClientSettings
                                                    {
                    Env = EnvHelper.EnvName(client.ClientInfo.ConfigEnv),
                    ServerAddress = client.ServerAddress,
                    DebugRequests = debugRequests,
                    RequestTimeout = requestTimeout,
                    OfflineTimeout = offlineTimeout,
                    CoreMode = coreMode,
                    ApplAssembly = applAssembly
                };
            });
            return client;
        }
    }
}
