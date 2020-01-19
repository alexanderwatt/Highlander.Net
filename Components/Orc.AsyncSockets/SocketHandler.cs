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
using System.Net.Sockets;
using System.Text;

namespace Highlander.Orc.AsyncSockets
{
    public class SocketHandler : AsyncClass, IIORecvServer, IIOSendServer
    {
        const int CBufferSize = 16384;
        const int CBufferPool = 16;
        private readonly Socket _socket;
        private readonly IList<ArraySegment<byte>> _recvBuffers;

        public SocketHandler(Socket socket) 
            : base(null)
        {
            Debug.Assert(socket != null);
            _socket = socket;
            // build the initial receive buffers
            _recvBuffers = new List<ArraySegment<byte>>();
            RefillRecvBuffers();
        }
        private void RefillRecvBuffers()
        {
            // replace used buffers
            // note - performance could be improved by getting pre-allocated buffers
            //        from an asynchronously maintained free list
            while (_recvBuffers.Count < CBufferPool)
                _recvBuffers.Add(new ArraySegment<byte>(new byte[CBufferSize]));
        }

        public IIORecvClient RecvClient { get; set; }

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
                    if (RecvClient == null)
                        throw new ApplicationException("RecvClient not set!");
                    if (SendClient == null)
                        throw new ApplicationException("SendClient not set!");
                    if (!_socket.Connected)
                        throw new ApplicationException("Not connected!");
                    //_Socket.BeginReceive(_RecvBuffers, SocketFlags.None, out errorCode, RecvCallback, null);
                    byte[] header = new byte[10];
                    _socket.BeginReceive(header, 0, 10, SocketFlags.Peek, out _, RecvCallback, header);
                    break;
                case AsyncClassState.Active:
                    break;
                case AsyncClassState.Stopping:
                    break;
                case AsyncClassState.Stopped:
                    if (_socket.Connected)
                        _socket.Disconnect(false);
                    _socket.Close();
                    RecvClient.OnRecverStop(AsyncStopReason);
                    break;
                default:
                    throw new ApplicationException("Unknown state: " + newState);
            }
        }

        public void Send(byte[] buffer)
        {
            Send(buffer, 0, buffer.Length);
        }
        public void Send(byte[] buffer, int offset, int count)
        {
            IList<ArraySegment<byte>> buffers = new List<ArraySegment<byte>>(1);
            buffers.Add(new ArraySegment<byte>(buffer, offset, count));
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
                if (_socket.Connected)
                    _socket.Send(buffers, SocketFlags.None);
            }
            catch (SocketException se)
            {
                if (_socket.Connected)
                    _socket.Disconnect(false);
                _socket.Close();
                AsyncStop(se.ToString());
                SendClient.OnSenderStop(se.ToString());
            }
            // other exceptions caught by base AsyncClass
        }

        private void RecvCallback(IAsyncResult ar)
        {
            try
            {
                // complete the receive
                int bytesRead = 0;
                if (_socket.Connected)
                    bytesRead = _socket.EndReceive(ar);
                byte[] header = (byte[])ar.AsyncState;
                if (bytesRead > 0)
                {
                    Debug.Assert(bytesRead == 10);
                    //Debug.Print("Header" + Encoding.UTF8.GetString(header));
                    //now read the rest of the message
                    string headerStr = Encoding.ASCII.GetString(header);
                    int bodySize = int.Parse(headerStr) + 10 ;
                    byte[] body = new byte[bodySize];
                    var bytesread = _socket.Receive(body, bodySize , SocketFlags.None);
                    int bytes = bytesread;
                    string msg1 = Encoding.UTF8.GetString(body);
                    //Debug.Print("-----msg1----" + msg1 + "------ end msg1 ----  ");
                    while (bytesread < bodySize)
                    {
                        bytes = _socket.Receive(body, bytesread , bodySize-bytesread, SocketFlags.None);
                        bytesread += bytes;
                        string msg2 = Encoding.UTF8.GetString(body);
                        //Debug.Print("----msg2-----" + msg2 + "----- end msg2 ------");
                    }
                    //Debug.Print("Header" + Encoding.UTF8.GetString(header));
                    IList<ArraySegment<byte>> buffers = new List<ArraySegment<byte>>();
                    buffers.Add(new ArraySegment<byte>(body));
                    //// delete this debug line - AB
                    string msg = Encoding.UTF8.GetString(body);
                    //Debug.Print(msg);
                    Debug.Assert(msg[msg.Length - 2] == '}');
                    RecvClient.OnRecverData(buffers);
                }
                // orig code
                //// received some data
                //if (bytesRead > 0)
                //{
                //    // get the data
                //    IList<ArraySegment<byte>> tempBuffers = new List<ArraySegment<byte>>();
                //    int buffersRead = 0;
                //    int bytesCopied = 0;
                //    string msg = "";
                //    while (bytesCopied < bytesRead)
                //    {
                //        ArraySegment<byte> tempBuffer = _RecvBuffers[0];
                //        _RecvBuffers.RemoveAt(0);
                //        bytesCopied += tempBuffer.Count;
                //        // truncate unused part of last buffer
                //        int bytesUnused = bytesCopied - bytesRead;
                //        if (bytesUnused > 0)
                //        {
                //            tempBuffer = new ArraySegment<byte>(
                //                tempBuffer.Array, tempBuffer.Offset, tempBuffer.Count - bytesUnused);
                //            bytesCopied = bytesRead;
                //        }
                //        tempBuffers.Add(tempBuffer);
                //        buffersRead++;             
                //    }
                //    // replace used buffers
                //    RefillRecvBuffers();
                //    // call the client to process the buffers
                //    // - this will usually be a packet protocol handler
                //    // - for max throughput the client should asynchronously process the data
                //    //   and return immediately so we can begin another socket receive                                    
                //    _RecvClient.OnRecverData(tempBuffers);
                //}
                // start another receive
                if (_socket.Connected && AsyncStateIsActive)
                {
                    //byte[] newheader = new byte[10];
                    _socket.BeginReceive(header, 0, 10, SocketFlags.Peek, out _, RecvCallback, header);
                    //_Socket.BeginReceive(_RecvBuffers, SocketFlags.None,  RecvCallback, null);
                }
            }
            catch (SocketException se)
            {
                if (_socket.Connected)
                    _socket.Disconnect(false);
                _socket.Close();
                AsyncStop(se.ToString());
            }
            catch (Exception e)
            {
                // other exceptions
                OnAsyncException(e);
            }
        }
    }
}
