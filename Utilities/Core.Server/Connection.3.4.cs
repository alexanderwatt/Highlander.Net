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

#region usings

using System;
using System.ServiceModel;
using System.Threading;
using Core.Common;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.Threading;

#endregion

namespace Core.Server
{
    internal class ConnectionV34 : IConnection
    {
        public string ContractName => typeof(ITransferV341).FullName;

        // readonly state
        private readonly ILogger _logger;
        private readonly CacheEngine _cacheEngine;
        private readonly ServerCfg _thisServerCfg;
        public Guid ClientId { get; }

        // managed state
        private NodeType _peerNodeType;
        public NodeType PeerNodeType => _peerNodeType;
        private AddressBinding _peerAddressBinding;
        private void SetAddressBinding(string replyAddress, NodeType peerNodeType)
        {
            _peerNodeType = peerNodeType;
            _peerAddressBinding = WcfHelper.CreateAddressBinding(
                replyAddress, WcfConst.LimitMultiplier, WcfConst.ExpressMsgLifetime);
            // dispose any existing client base
            DisposeHelper.SafeDispose(ref _clientBase);
        }
        public string ReplyAddress
        {
            get => _peerAddressBinding.Address.ToString();
            set => SetAddressBinding(value, _peerNodeType);
        }
        // - connection expiry
        private DateTimeOffset _expiryTime = DateTimeOffset.Now.Add(ServerCfg.CommsConnectionExtension);
        public bool HasExpired => (DateTimeOffset.Now > _expiryTime);

        public void ExtendExpiry()
        {
            // cannot extend a faulted connection
            if (_faulted)
                throw new InvalidOperationException("Connection has faulted!");
            _expiryTime = DateTimeOffset.Now.Add(ServerCfg.CommsConnectionExtension);
        }
        // - connection faulted
        private bool _faulted;
        public bool HasFaulted => _faulted;
        private TransferSenderV341 _clientBase;
        private readonly AsyncThreadQueue _incomingThreadQueue;
        public void DispatchToIncomingThreadQueue<T>(T data, AsyncQueueCallback<T> callback)
        {
            _incomingThreadQueue.Dispatch(data, callback);
        }
        private readonly AsyncThreadQueue _outgoingThreadQueue;
        public ConnectionV34(ILogger logger, CacheEngine cacheEngine, ServerCfg thisServerCfg,
            Guid clientId, string replyAddress, NodeType peerNodeType)
        {
            _logger = logger;
            _cacheEngine = cacheEngine;
            _thisServerCfg = thisServerCfg;
            ClientId = clientId;
            SetAddressBinding(replyAddress, peerNodeType);
            _incomingThreadQueue = new AsyncThreadQueue(_logger);
            _outgoingThreadQueue = new AsyncThreadQueue(_logger);
        }
        // sender methods
        private delegate void CodeSection();

        private void BackoffRetryAlgorithm(Exception excp, DateTime expiryTime, int attempt)
        {
            DisposeHelper.SafeDispose(ref _clientBase);
            if (attempt == 1)
            {
                _logger.LogWarning("Connection: '{0}' first send attempt ({1}) to client at '{2}' failed: {3}",
                    ClientId, attempt, _peerAddressBinding.Address.Uri.AbsoluteUri, excp.GetType().Name);
            }
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
                _faulted = true;
                _logger.LogWarning("Connection: '{0}' final send attempt ({1}) to client at '{2}' failed: {3}",
                    ClientId, attempt, _peerAddressBinding.Address.Uri.AbsoluteUri, excp.GetType().Name);
            }
        }

        private void PerformSyncSendWithRetry(bool keepClientOpen, CodeSection codeSection)
        {

            if (_faulted)
                return;

            try
            {
                // sanitise retry limit - 5 mins max
                TimeSpan maxTimeout = ServerCfg.CommsClientSendTimeout;
                DateTime expiryTime = DateTime.Now.Add(maxTimeout);
                // attempt to send
                int attempt = 0;
                bool sent = false;
                while (!sent && !_faulted)
                {
                    attempt++;
                    try
                    {
                        if (_clientBase == null)
                        {
                            _clientBase = new TransferSenderV341(_peerAddressBinding);
                        }
                        codeSection();
                        // success
                        sent = true;
                        if (attempt > 1)
                            _logger.LogInfo("Connection: '{0}' send attempt ({1}) succeeded.", ClientId, attempt);
                    }
                    catch (CommunicationException commsExcp)
                    {
                        // expected - retry
                        BackoffRetryAlgorithm(commsExcp, expiryTime, attempt);
                    }
                    catch (TimeoutException timeoutExcp)
                    {
                        // expected - retry
                        BackoffRetryAlgorithm(timeoutExcp, expiryTime, attempt);
                    }
                    catch (Exception unexpectedExcp)
                    {
                        // unexpected - fault immediately
                        _faulted = true;
                        _logger.Log(unexpectedExcp);
                    }
                } // while
            }
            finally
            {
                if (_faulted || !keepClientOpen)
                    DisposeHelper.SafeDispose(ref _clientBase);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package1"></param>
        public void DispatchAsyncSendCompletionResult(PackageCompletionResult package1)
        {
            _outgoingThreadQueue.Dispatch(package1, (package2) =>
            {
                PerformSyncSendWithRetry(false, () => _clientBase.TransferV341CompletionResult(
                    package2.Header.ToV131Header(), package2.ToV341CompletionResult()));
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package1"></param>
        public void DispatchAsyncSendAnswerMultipleItems(PackageAnswerMultipleItems package1)
        {
            _outgoingThreadQueue.Dispatch(package1, (package2) =>
            {
                PerformSyncSendWithRetry(true, () => _clientBase.TransferV341AnswerMultipleItems(
                    package2.Header.ToV131Header(), package2.ToV341AnswerMultipleItems()));
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package1"></param>
        public void DispatchAsyncSendNotifyMultipleItems(PackageNotifyMultipleItems package1)
        {
            _outgoingThreadQueue.Dispatch(package1, (package2) =>
            {
                PerformSyncSendWithRetry(true, () => _clientBase.TransferV341NotifyMultipleItems(
                    package2.Header.ToV131Header(), package2.ToV341NotifyMultipleItems()));
            });
        }

        // recver methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void DispatchAsyncRecvAnswerMultipleItems(PackageAnswerMultipleItems package)
        {
            _incomingThreadQueue.Dispatch(package, _cacheEngine.PerformSyncRecvAnswerMultipleItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void DispatchAsyncRecvNotifyMultipleItems(PackageNotifyMultipleItems package)
        {
            _incomingThreadQueue.Dispatch(package, _cacheEngine.PerformSyncRecvNotifyMultipleItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void DispatchAsyncRecvSelectMultipleItems(PackageSelectMultipleItems package)
        {
            _incomingThreadQueue.Dispatch(package, _cacheEngine.PerformSyncRecvSelectMultipleItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void DispatchAsyncRecvCompletionResult(PackageCompletionResult package)
        {
            _incomingThreadQueue.Dispatch(package, _cacheEngine.PerformSyncRecvCompletionResult);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void DispatchAsyncRecvCreateSubscription(PackageCreateSubscription package)
        {
            _incomingThreadQueue.Dispatch(package, _cacheEngine.PerformSyncRecvCreateSubscription);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void DispatchAsyncRecvExtendSubscription(PackageExtendSubscription package)
        {
            _incomingThreadQueue.Dispatch(package, _cacheEngine.PerformSyncRecvExtendSubscription);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        public void DispatchAsyncRecvCancelSubscription(PackageCancelSubscription package)
        {
            _incomingThreadQueue.Dispatch(package, _cacheEngine.PerformSyncRecvCancelSubscription);
        }
    }
}