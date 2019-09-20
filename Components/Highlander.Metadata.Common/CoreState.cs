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
using Highlander.Utilities;

namespace Highlander.Metadata.Common
{
    // helper classes
    public class CacheChangeData
    {
        public readonly Guid CacheId;
        public readonly CacheChange Change;
        public readonly ICoreItem OldItem;
        public readonly ICoreItem NewItem;
        public CacheChangeData(Guid cacheId, CacheChange change, ICoreItem oldItem, ICoreItem newItem)
        {
            CacheId = cacheId;
            Change = change;
            OldItem = oldItem;
            NewItem = newItem;
        }
    }

    public delegate void CacheChangeHandler(CacheChangeData update);

    /// <summary>
    /// Core modes
    /// </summary>
    public enum CoreModeEnum
    {
        /// <summary>
        /// Default mode designed for transient use (eg. within web service request handlers).
        /// - All exceptions are propagated to the caller.
        /// - Subscriptions are not supported.
        /// Automatic recovery mode. Transient mode plus:
        /// - communication exceptions are managed;
        /// - subscriptions supported;
        /// - offline state not allowed.
        /// </summary>
        Standard,
        /// <summary>
        /// Not supported yet.
        /// Offline/reliable mode. Auto-recovery mode where Offline state is allowed. Uses a reliable
        /// transport (MSMQ) to communicate with servers.
        /// </summary>
        Reliable
    }

    public enum CoreStateEnum
    {
        Initial,
        Connecting,
        Connected,
        Offline,
        Disposed,
        Faulted
    }

    public class CoreStateChange
    {
        public readonly CoreStateEnum OldState;
        public readonly CoreStateEnum NewState;
        public CoreStateChange(CoreStateEnum oldState, CoreStateEnum newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }

    public delegate void CoreStateHandler(CoreStateChange update);
}
