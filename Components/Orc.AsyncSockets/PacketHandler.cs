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
using System.Globalization;
using System.Text;

namespace Highlander.Orc.AsyncSockets
{
    public class PacketRecver : AsyncClass, IIORecvClient, IIORecvServer
    {
        private IIORecvClient _recvClient;

        private readonly IList<ArraySegment<byte>> _recvQueue = new List<ArraySegment<byte>>();
        private int _recvQueueBytesEnqueued;
        private int _recvQueueBytesRequired;
        private bool _recvSeenHeader;

        public PacketRecver()
            : base(null)
        {
        }

        public IIORecvClient RecvClient
        {
            get => _recvClient;
            set { Debug.Assert(AsyncState == AsyncClassState.Initial); _recvClient = value; }
        }

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
                    if (_recvClient == null)
                        throw new ApplicationException("RecvClient not set!");
                    break;
                case AsyncClassState.Active:
                    break;
                case AsyncClassState.Stopping:
                    break;
                case AsyncClassState.Stopped:
                    _recvClient.OnRecverStop(this.AsyncStopReason);
                    break;
                default:
                    throw new ApplicationException("Unknown state: " + newState.ToString());
            }
        }

        // recv methods called by lower layer
        public void OnRecverData(IList<ArraySegment<byte>> inputBuffers)
        {
            AsyncPost(inputBuffers);
        }
        protected override void OnAsyncProcessData(object data)
        {
            IList<ArraySegment<byte>> inputBuffers = (IList<ArraySegment<byte>>)data;
            // packet receive states:
            // new - processing header
            // filling - waiting for more data
            // scanning - resyncing todo
            const int cHeadLength = 10;
            const int cTailLength = 0;
            Debug.Assert(_recvQueueBytesEnqueued >= 0);
            //debug
            int dbg1BytesToCopy = 0;
            foreach (ArraySegment<byte> buffer in inputBuffers)
                dbg1BytesToCopy += buffer.Count;
            byte[] dbg1Buffer = new byte[dbg1BytesToCopy];
            int dbg1BytesCopied = 0;
            foreach (ArraySegment<byte> buffer in inputBuffers)
            {
                Array.Copy(buffer.Array, buffer.Offset, dbg1Buffer, dbg1BytesCopied, buffer.Count);
                dbg1BytesCopied += buffer.Count;
            }
            string dbg1Text = Encoding.UTF8.GetString(dbg1Buffer, 0, dbg1BytesCopied);
            //Debug.Print(dbg1Text);
            //end debug
            // add buffers to input queue
            foreach (ArraySegment<byte> buffer in inputBuffers)
            {
                _recvQueue.Add(buffer);
                _recvQueueBytesEnqueued += buffer.Count;
            }
            Debug.Assert(_recvQueueBytesEnqueued >= dbg1BytesCopied);
            // we have enough data to do something
            while (_recvQueueBytesEnqueued >= _recvQueueBytesRequired)
            {
                // we have enough data to do something
                if (!_recvSeenHeader)
                {
                    // read the header
                    byte[] headBuffer = new byte[cHeadLength];
                    int headBytesRead = 0;
                    int headBufferIndex = 0;
                    while (headBytesRead < cHeadLength)
                    {
                        Debug.Assert(_recvQueue.Count > headBufferIndex);
                        ArraySegment<byte> tempBuffer = _recvQueue[headBufferIndex];
                        if (tempBuffer.Count >= (cHeadLength - headBytesRead))
                        {
                            // enough bytes to read header
                            Array.Copy(tempBuffer.Array, tempBuffer.Offset, headBuffer, headBytesRead, (cHeadLength - headBytesRead));                                             
                            headBytesRead = cHeadLength;
                        }
                        else
                        {
                            // not enough bytes to read header
                            Array.Copy(tempBuffer.Array, tempBuffer.Offset, headBuffer, headBytesRead, tempBuffer.Count);
                            headBytesRead += tempBuffer.Count;
                        }
                        headBufferIndex++;
                    }
                    string headString = Encoding.UTF8.GetString(headBuffer, 0, cHeadLength);
                    Debug.Assert(headString.StartsWith("0"));
                    //Debug.Assert(headString.EndsWith("_"));
                    int bodyLength = Int32.Parse(
                       headString.Substring(0, cHeadLength), NumberStyles.Number);
                    _recvQueueBytesRequired = cHeadLength + bodyLength + cTailLength;
                    _recvSeenHeader = true;
                }
                else
                {
                    // extract the packet bytes from the receive queue
                    IList<ArraySegment<byte>> buffers = new List<ArraySegment<byte>>();
                    int bytesExtracted = 0;
                    while (bytesExtracted < _recvQueueBytesRequired)
                    {
                        ArraySegment<byte> buffer = _recvQueue[0];
                        bytesExtracted += buffer.Count;
                        if (bytesExtracted <= _recvQueueBytesRequired)
                        {
                            // extract full buffer
                            buffers.Add(buffer);
                            _recvQueue.RemoveAt(0);
                        }
                        else
                        {
                            // extract partial buffer
                            int partialLength = buffer.Count - (bytesExtracted - _recvQueueBytesRequired);
                            ArraySegment<byte> lastBuffer = new ArraySegment<byte>(
                                buffer.Array, buffer.Offset, partialLength);
                            buffers.Add(lastBuffer);
                            //Debug.Assert(lastBuffer.Array[lastBuffer.Offset + lastBuffer.Count - 1] == 95); // '_'
                            // leave unextracted part in the queue
                            ArraySegment<byte> nextBuffer = new ArraySegment<byte>(
                                buffer.Array, buffer.Offset + partialLength, buffer.Count - partialLength);
                            //Debug.Assert(nextBuffer.Array[nextBuffer.Offset] == 80); //'P'
                            _recvQueue[0] = nextBuffer;
                            // done
                            bytesExtracted = _recvQueueBytesRequired;
                        }
                    }
                    _recvQueueBytesEnqueued -= _recvQueueBytesRequired;
                    //debug
                    int dbg2BytesToCopy = 0;
                    foreach (ArraySegment<byte> buffer in buffers)
                        dbg2BytesToCopy += buffer.Count;
                    byte[] dbg2Buffer = new byte[dbg2BytesToCopy];
                    int dbg2BytesCopied = 0;
                    foreach (ArraySegment<byte> buffer in buffers)
                    {
                        Array.Copy(buffer.Array, buffer.Offset, dbg2Buffer, dbg2BytesCopied, buffer.Count);
                        dbg2BytesCopied += buffer.Count;
                    }
                    string dbg2Text = Encoding.UTF8.GetString(dbg2Buffer, 0, dbg2BytesCopied);
                    //Debug.Print(dbg2Text);
                    int dbg3BytesToCopy = 0;
                    foreach (ArraySegment<byte> buffer in _recvQueue)
                        dbg3BytesToCopy += buffer.Count;
                    byte[] dbg3Buffer = new byte[dbg3BytesToCopy];
                    int dbg3BytesCopied = 0;
                    foreach (ArraySegment<byte> buffer in _recvQueue)
                    {
                        Array.Copy(buffer.Array, buffer.Offset, dbg3Buffer, dbg3BytesCopied, buffer.Count);
                        dbg3BytesCopied += buffer.Count;
                    }
                    string dbg3Text = Encoding.UTF8.GetString(dbg3Buffer, 0, dbg3BytesCopied);
                    //end debug

                    // check/strip packet tail
                    byte[] tailBuffer = new byte[cTailLength];
                    int tailBytesToRead = cTailLength;
                    int tailBufferIndex = buffers.Count - 1;
                    while (tailBytesToRead > 0)
                    {
                        Debug.Assert(buffers.Count > tailBufferIndex);
                        ArraySegment<byte> tempBuffer = buffers[tailBufferIndex];
                        if (tempBuffer.Count >= tailBytesToRead)
                        {
                            // enough bytes to read tail
                            Array.Copy(
                                tempBuffer.Array,
                                tempBuffer.Offset + tempBuffer.Count - tailBytesToRead,
                                tailBuffer,
                                0,
                                tailBytesToRead);
                            tailBytesToRead = 0;
                        }
                        else
                        {
                            // not enough bytes to read tail
                            Array.Copy(
                                tempBuffer.Array,
                                tempBuffer.Offset,
                                tailBuffer,
                                tailBytesToRead - tempBuffer.Count,
                                tempBuffer.Count);
                            tailBytesToRead -= tempBuffer.Count;
                        }
                        tailBufferIndex--;
                    }
                    string tailString = Encoding.UTF8.GetString(tailBuffer, 0, cTailLength);
                    //Debug.Assert(tailString.StartsWith("_"));
                    //Debug.Assert(tailString.EndsWith("___"));
                    int checkSum = 0;
                    //if (Int32.TryParse(tailString.Substring(1, 8), NumberStyles.HexNumber, null, out checkSum))
                    //{
                    // todo
                    //}
                    // shorten/remove head buffer
                    int headBytesToRemove = cHeadLength;
                    while (headBytesToRemove > 0)
                    {
                        Debug.Assert(buffers.Count > 0);
                        ArraySegment<byte> tempBuffer = buffers[0];
                        if (tempBuffer.Count > headBytesToRemove)
                        {
                            buffers[0] = new ArraySegment<byte>(
                                tempBuffer.Array,
                                tempBuffer.Offset + headBytesToRemove,
                                tempBuffer.Count - headBytesToRemove);
                            headBytesToRemove = 0;
                        }
                        else
                        {
                            buffers.RemoveAt(0);
                            headBytesToRemove -= tempBuffer.Count;
                        }
                    }
                    // shorten/remove tail buffer
                    int tailBytesToRemove = cTailLength;
                    while (tailBytesToRemove > 0)
                    {
                        Debug.Assert(buffers.Count > 0);
                        ArraySegment<byte> tempBuffer = buffers[buffers.Count - 1];
                        if (tempBuffer.Count > tailBytesToRemove)
                        {
                            buffers[buffers.Count - 1] = new ArraySegment<byte>(
                                tempBuffer.Array,
                                tempBuffer.Offset,
                                tempBuffer.Count - tailBytesToRemove);
                            tailBytesToRemove = 0;
                        }
                        else
                        {
                            buffers.RemoveAt(buffers.Count - 1);
                            tailBytesToRemove -= tempBuffer.Count;
                        }
                    }

                    // pass to client
                    _recvClient.OnRecverData(buffers);
                    // next header
                    _recvQueueBytesRequired = cHeadLength;
                    _recvSeenHeader = false;
                }
            }
            // pass to client
            //_RecvClient.OnRecverData(inputBuffers); // pass to client            
            // other exceptions caught by base AsyncClass
            // next header
            //_RecvQueueBytesRequired = cHeadLength;
            //_RecvSeenHeader = false;
            //_RecvClient.OnRecverData(inputBuffers); // pass to client   
        }

        public void OnRecverStop(string reason)
        {
            AsyncStop(reason);
        }
    }

    public class PacketSender : AsyncClass, IIOSendClient, IIOSendServer
    {
        private IIOSendServer _sendServer;
        private IIOSendClient _sendClient;

        public PacketSender()
            : base(null)
        {
        }

        public IIOSendClient SendClient
        {
            get => _sendClient;
            set { Debug.Assert(AsyncState == AsyncClassState.Initial); _sendClient = value; }
        }
        public IIOSendServer SendServer
        {
            get => _sendServer;
            set { Debug.Assert(AsyncState == AsyncClassState.Initial); _sendServer = value; }
        }

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
                    if (_sendClient == null)
                        throw new ApplicationException("SendClient not set!");
                    if (_sendServer == null)
                        throw new ApplicationException("SendServer not set!");
                    break;
                case AsyncClassState.Active:
                    break;
                case AsyncClassState.Stopping:
                    break;
                case AsyncClassState.Stopped:
                    _sendServer.Stop(AsyncStopReason);
                    break;
                default:
                    throw new ApplicationException("Unknown state: " + newState);
            }
        }

        // send methods called by higher layer (client)
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
            int bodyLength = 0;
            foreach (ArraySegment<byte> buffer in buffers)
                bodyLength += buffer.Count;

            //// add packet head and tail to buffer list
            //ArraySegment<byte> head = new ArraySegment<byte>(
            //    Encoding.UTF8.GetBytes("P0_" + bodyLength.ToString("X8") + "_"));
            //buffers.Insert(0, head);
            //int checkSum = -1; // todo
            //ArraySegment<byte> tail = new ArraySegment<byte>(
            //    Encoding.UTF8.GetBytes("_" + checkSum.ToString("X8") + "___"));
            //buffers.Add(tail);
            // send
            ArraySegment<byte> head = new ArraySegment<byte>(
                Encoding.UTF8.GetBytes(bodyLength.ToString().PadLeft(10,'0')));
            buffers.Insert(0, head);
            _sendServer.Send(buffers);
            // other exceptions caught by base AsyncClass
        }

        public void OnSenderStop(string reason)
        {
            AsyncStop(reason);
            _sendClient.OnSenderStop(reason);
        }
    }
}
