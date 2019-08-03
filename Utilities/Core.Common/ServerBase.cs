/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Orion.Util.Helpers;
using Orion.Util.RefCounting;
using Orion.Util.Servers;

namespace Core.Common
{
    public interface IServerBase2 : IBasicServer
    {
        Reference<ICoreClient> Client { get; set; }
        string HostInstance { get; set; }
        int ServerInstance { get; set; }
        int ServerFarmSize { get; set; }
    }

    public class ServerBase2 : BasicServer, IServerBase2
    {
        protected Reference<ICoreClient> IntClient;
        private Reference<ICoreClient> _extClient;
        public Reference<ICoreClient> Client
        {
            get => _extClient;
            set
            {
                CheckNotStarted();
                _extClient = value;
            }
        }

        private string _hostInstance;
        public string HostInstance
        {
            get => _hostInstance;
            set
            {
                CheckNotStarted();
                _hostInstance = value;
            }
        }

        private int _serverInstance;
        public int ServerInstance
        {
            get => _serverInstance;
            set
            {
                CheckNotStarted();
                _serverInstance = value;
            }
        }

        private int _serverFarmSize = 1;
        public int ServerFarmSize
        {
            get => _serverFarmSize;
            set
            {
                CheckNotStarted();
                _serverFarmSize = value;
            }
        }

        private void Cleanup()
        {
            DisposeHelper.SafeDispose(ref IntClient);
            _extClient = null;
        }

        protected virtual void OnServerStarted() { }
        protected sealed override void OnBasicSyncStart()
        {
            IntClient = _extClient?.Clone() ?? throw new InvalidOperationException("Cannot start when Client is not assigned!");
            Logger.LogInfo("Starting");
            OnServerStarted();
        }

        protected virtual void OnServerStopping() { }
        protected sealed override void OnBasicSyncStop()
        {
            Logger.LogInfo("Stopping");
            OnServerStopping();
            Cleanup();
        }
    }
}
