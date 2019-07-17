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
using System.Threading;
using Core.Common;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.Threading;

#endregion

namespace Core.V34
{
    /// <summary>
    /// A manager class for subscriptions.
    /// </summary>
    internal class Subscription : ISubscription
    {
        // readonly state
        private readonly Guid _id = Guid.NewGuid();
        public Guid Id => _id;
        private readonly CoreClient _client;
        private readonly ILogger _logger;
        private readonly AsyncThreadQueue _userThreadQueue;

        // pre-start state
        private IExpression _queryExpr = Expr.ALL;

        /// <summary>
        /// 
        /// </summary>
        public IExpression WhereExpr { get => _queryExpr;
            set { CheckNotStarted(); _queryExpr = value ?? Expr.ALL; } }

        private Type _dataType;

        /// <summary>
        /// 
        /// </summary>
        public Type DataType
        {
            get => _dataType;
            set
            {
                CheckNotStarted();
                _dataType = value;
                _dataTypeName = (_dataType != null) ? _dataType.FullName : null;
            }
        }

        private string _dataTypeName;

        /// <summary>
        /// 
        /// </summary>
        public string DataTypeName
        {
            get => _dataTypeName;
            set
            {
                CheckNotStarted();
                _dataType = null;
                _dataTypeName = value;
            }
        }

        private bool _excludeDeleted;

        /// <summary>
        /// 
        /// </summary>
        public bool ExcludeDeleted { get => _excludeDeleted;
            set { CheckNotStarted(); _excludeDeleted = value; } }

        private bool _excludeExisting;

        /// <summary>
        /// 
        /// </summary>
        public bool ExcludeExisting { get => _excludeExisting;
            set { CheckNotStarted(); _excludeExisting = value; } }

        private bool _waitForExisting;

        /// <summary>
        /// 
        /// </summary>
        public bool WaitForExisting { get => _waitForExisting;
            set { CheckNotStarted(); _waitForExisting = value; } }

        private bool _excludeDataBody;

        /// <summary>
        /// 
        /// </summary>
        public bool ExcludeDataBody { get => _excludeDataBody;
            set { CheckNotStarted(); _excludeDataBody = value; } }

        private DateTimeOffset _asAtTime = DateTimeOffset.Now;

        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset AsAtTime { get => _asAtTime;
            set { CheckNotStarted(); _asAtTime = value; } }

        private long _minimumUSN;

        /// <summary>
        /// 
        /// </summary>
        public long MinimumUSN { get => _minimumUSN;
            set { CheckNotStarted(); _minimumUSN = value; } }

        private ItemKind _itemKind = ItemKind.Object;

        /// <summary>
        /// 
        /// </summary>
        public ItemKind ItemKind { get => _itemKind;
            set { CheckNotStarted(); _itemKind = value; } }

        private SubscriptionCallback _userCallback;

        /// <summary>
        /// 
        /// </summary>
        public SubscriptionCallback UserCallback { get => _userCallback;
            set => _userCallback = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public object UserContext { get; set; }

        // events
        /// <summary>
        /// 
        /// </summary>
        public event CoreStateHandler OnStateChange;

        // app scopes
        private string[] _appScopes;

        /// <summary>
        /// 
        /// </summary>
        public string[] AppScopes
        {
            get => _appScopes;
            set
            {
                CheckNotStarted();
                _appScopes = value ?? new[] { AppScopeNames.Legacy };
            }
        }

        // managed state
        private long _startCount;

        /// <summary>
        /// 
        /// </summary>
        public bool Started => (Interlocked.Add(ref _startCount, 0) > 0);

        private void CheckNotStarted()
        {
            if (Interlocked.Add(ref _startCount, 0) > 0)
                throw new ApplicationException("Subscription has started!");
        }

        // constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="client"></param>
        public Subscription(ILogger logger, CoreClient client)
        {
            _logger = logger;
            _client = client;
            _appScopes = _client.DefaultAppScopes;
            _userThreadQueue = new AsyncThreadQueue(_logger);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Cancel();
            _userThreadQueue.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            CheckNotStarted();
            if (_itemKind == ItemKind.Undefined)
                throw new ArgumentNullException("ItemKind");
            Interlocked.Exchange(ref _startCount, 1);
            _client.OnStateChange += NotifyUserStateChange;
            _client.StartSubscription(_id);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Cancel()
        {
            IAsyncResult ar = CancelBegin();
            CancelEnd(ar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IAsyncResult CancelBegin()
        {
            if (Interlocked.Add(ref _startCount, 0) == 0)
                return null;
            IAsyncResult ar = _client.CancelSubscriptionBegin(_id);
            Interlocked.Exchange(ref _startCount, 0);
            return ar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public void CancelEnd(IAsyncResult ar)
        {
            _client.CancelSubscriptionEnd(ar);
        }

        internal void ProcessUpdate(ICoreItem item)
        {
            if (Interlocked.Add(ref _startCount, 0) == 0)
            {
                _logger.LogWarning("Subscription '{0}' inactive - ignoring data", _id);
                return;
            }
            // call the client callback (on the client callback 'thread')
            NotifyUser(item);
        }

        private void NotifyUser(object state)
        {
            ICoreItem item = (ICoreItem)state;
            try
            {
                _userCallback?.Invoke(this, item);
            }
            catch (Exception e)
            {
                _logger.LogError("Subscription '{0}' UserCallback() failed: {1}", _id, e);
            }
        }

        // core mode change notifications
        private void NotifyUserStateChange(CoreStateChange update)
        {
            _userThreadQueue.Dispatch(update, UserCallbackStateChange);
        }

        private void UserCallbackStateChange(CoreStateChange update)
        {
            try
            {
                OnStateChange?.Invoke(update);
            }
            catch (Exception e)
            {
                _logger.Log(e);
            }
        }
    }
}
