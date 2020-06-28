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
using Highlander.Build;
using Highlander.Core.Common;
using Highlander.Utilities.Threading;

namespace Highlander.Core.Bridge
{
    public class ServerBridge : ServerBase2
    {
        public static EnvId BuildEnv { get; } = EnvHelper.ParseEnvName(BuildConst.BuildEnv);

        private ICoreClient _targetClient;

        public ICoreClient TargetClient
        {
            get => _targetClient;
            set
            {
                CheckNotStarted();
                _targetClient = value;
            }
        }

        private LoggedCounter _itemTotal;
        private LoggedCounter _excpTotal;
        private ISubscription _objSubs;
        private ISubscription _dbgSubs;
        private ISubscription _sysSubs;

        protected override void OnServerStarted()
        {
            if (_targetClient == null)
                throw new InvalidOperationException("Cannot start when TargetClient is not assigned!");
            //_RecvQueue = new LoggedCounter(Logger, "RecvQueue", 0, 500);
            _itemTotal = new LoggedCounter(Logger, "ItemTotal", 0, 1000);
            _excpTotal = new LoggedCounter(Logger, "ExcpTotal", 0, 1);
        }

        protected override void OnFirstCallback()
        {
            // subscribe to all application objects
            _objSubs = IntClient.Target.StartUntypedSubscription(null, null, EnqueueItemCallback, null);
            // subscribe to all debug messages
            _dbgSubs = IntClient.Target.CreateUntypedSubscription(null, null);
            _dbgSubs.ItemKind = ItemKind.Debug;
            _dbgSubs.UserCallback = EnqueueItemCallback;
            _dbgSubs.Start();
            // subscribe to all system data
            _sysSubs = IntClient.Target.CreateUntypedSubscription(null, null);
            _sysSubs.ItemKind = ItemKind.System;
            _sysSubs.UserCallback = EnqueueItemCallback;
            _sysSubs.Start();
        }

        private void CancelSubscriptions()
        {
            _objSubs.Cancel();
            _dbgSubs.Cancel();
            _sysSubs.Cancel();
        }

        protected override void OnServerStopping()
        {
            CancelSubscriptions();
            MainThreadQueue.WaitUntilEmpty(TimeSpan.FromSeconds(30));
        }

        private void EnqueueItemCallback(ISubscription subs, ICoreItem item)
        {
            // ignore when in faulted state
            if (_excpTotal.Counter > 0)
                return;
            MainThreadQueue.Dispatch(item, DequeueItemCallback);
        }
        private void DequeueItemCallback(ICoreItem item)
        {
            try
            {
                var rawItem = new RawItem(item);
                // synchronous save
                _targetClient.SaveRawItem(rawItem);
                _itemTotal.Increment();
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Logger.LogError("Server item processing failed: '{0}' ({1}) {2} {3}",
                    item.Name, item.ItemKind, item.Id, e.GetType().Name);
                _excpTotal.Increment();
                // cancel subscriptions
                CancelSubscriptions();
            }
        }
    }
}
