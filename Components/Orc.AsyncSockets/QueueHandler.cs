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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Messaging;

namespace Highlander.Orc.AsyncSockets
{
    internal class QueueReader : AsyncClass, IIORecvServer
    {
        private readonly string _recvQueueName;
        private MessageQueue _recvQueue;

        public QueueReader(string localQueueName) 
            : base(null)
        {
            _recvQueueName = $@".\Private$\{localQueueName}";
        }

        public IIORecvClient RecvClient { get; set; }

        public void Start()
        {
            AsyncStart();
        }
        public void Stop(string reason)
        {
            this.AsyncStop(reason);
        }

        protected override void OnAsyncStateChange(AsyncClassState oldState, AsyncClassState newState)
        {
            switch (newState)
            {
                case AsyncClassState.Initial:
                    break;
                case AsyncClassState.Starting:
                    if (RecvClient == null)
                        throw new ApplicationException("RecvClient not set!");
                    if (!MessageQueue.Exists(_recvQueueName))
                        MessageQueue.Create(_recvQueueName);
                    _recvQueue = new MessageQueue(_recvQueueName, QueueAccessMode.Receive);
                    _recvQueue.BeginReceive(TimeSpan.FromSeconds(15), null, RecvCallback);
                    break;
                case AsyncClassState.Active:
                    break;
                case AsyncClassState.Stopping:
                    break;
                case AsyncClassState.Stopped:
                    _recvQueue.Close();
                    RecvClient.OnRecverStop("Normal stop");
                    break;
                default:
                    throw new ApplicationException("Unknown state: " + newState.ToString());
            }
        }
        private void RecvCallback(IAsyncResult ar)
        {
            try
            {
                // complete the receive
                Message recdMessage = null;
                try
                {
                    recdMessage = _recvQueue.EndReceive(ar);
                }
                catch (MessageQueueException msmqExcp)
                {
                    // receive timeout is ok
                    if (msmqExcp.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                        throw;
                }
                if (recdMessage != null)
                {
                    // get the data
                    int bytesToRead = Convert.ToInt32(recdMessage.BodyStream.Length);
                    byte[] buffer = new byte[bytesToRead];
                    int bytesRead = recdMessage.BodyStream.Read(buffer, 0, bytesToRead);
                    IList<ArraySegment<byte>> tempBuffers = new List<ArraySegment<byte>>();
                    tempBuffers.Add(new ArraySegment<byte>(buffer));
                    // call the client to process the buffers
                    // - this will usually be a packet protocol handler
                    // - for max throughput the client should asynchronously process the data
                    //   and return immediately so we can begin another socket receive
                    RecvClient.OnRecverData(tempBuffers);
                }
                // start another receive
                if (AsyncStateIsActive)
                    _recvQueue.BeginReceive(TimeSpan.FromSeconds(15), null, RecvCallback);
            }
            catch (MessageQueueException me)
            {
                _recvQueue.Close();
                AsyncStop(me.ToString());
            }
            catch (Exception e)
            {
                // other exceptions
                OnAsyncException(e);
            }
        }
    }

    internal class QueueWriter : AsyncClass, IIOSendServer
    {
        private readonly string _sendQueueName;
        private MessageQueue _sendQueue;

        public QueueWriter(string remoteHostName, string remoteQueueName)
            : base(null)
        {
            _sendQueueName = remoteHostName.Trim().ToLower() == "localhost" ? $@"{"."}\Private$\{remoteQueueName}" 
                : $@"{remoteHostName}\Private$\{remoteQueueName}";
        }

        public IIOSendClient SendClient { get; set; }

        public void Start()
        {
            AsyncStart();
        }

        public void Stop(string reason)
        {
            AsyncStop(reason);
        }

        protected override void OnAsyncStateChange(AsyncClassState oldState, AsyncClassState newState)
        {
            switch (newState)
            {
                case AsyncClassState.Initial:
                    break;
                case AsyncClassState.Starting:
                    if (SendClient == null)
                        throw new ApplicationException("SendClient not set!");
                    _sendQueue = new MessageQueue(_sendQueueName, QueueAccessMode.Send);
                    break;
                case AsyncClassState.Active:
                    break;
                case AsyncClassState.Stopping:
                    break;
                case AsyncClassState.Stopped:
                    _sendQueue.Close();
                    SendClient.OnSenderStop(this.AsyncStopReason);
                    break;
                default:
                    throw new ApplicationException("Unknown state: " + newState.ToString());
            }
        }

        public void Send(byte[] buffer)
        {
            Send(buffer, 0, buffer.Length);
        }

        public void Send(byte[] buffer, int offset, int size)
        {
            IList<ArraySegment<byte>> buffers = new List<ArraySegment<byte>>(3);
            buffers.Add(new ArraySegment<byte>(buffer, offset, size));
            Send(buffers);
        }
        public void Send(IList<ArraySegment<byte>> buffers)
        {
            AsyncPost(buffers);
        }

        protected override void OnAsyncProcessData(object data)
        {
            IList<ArraySegment<byte>> buffers = (IList<ArraySegment<byte>>)data;
            try
            {
                Message m = new Message();
                long bytesWritten = 0;
                foreach (ArraySegment<byte> buffer in buffers)
                {
                    m.BodyStream.Write(buffer.Array, buffer.Offset, buffer.Count);
                    bytesWritten += buffer.Count;
                }
                Debug.Assert(bytesWritten == m.BodyStream.Length);
                _sendQueue.Send(m);
            }
            catch (MessageQueueException me)
            {
                _sendQueue.Close();
                AsyncStop(me.ToString());
                SendClient.OnSenderStop(me.ToString());
            }
            // other exceptions caught by base AsyncClass
        }
    }
}
