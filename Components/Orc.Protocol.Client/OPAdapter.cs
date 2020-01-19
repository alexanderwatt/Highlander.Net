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
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using Highlander.Orc.AsyncSockets;


namespace Highlander.Orc.Client
{
    public class OPAdapter : IIOSendClient, IIORecvClient
    {
        private DateTime _serverStart = DateTime.MinValue;
        private DateTime _serverStop = DateTime.MaxValue;
        private int _serverBytes;

        public delegate void ConsumerDelegate(string reply);
        protected readonly ConsumerDelegate ConsumerCallback;

        // socket stuff
        private Socket _clientSocket;
 

        // generic io stuff


        public OPAdapter(ConsumerDelegate consumer)
        {
            ConsumerCallback = consumer;
        }

        public IIOSendServer SendServer { get; set; }


        public void Startup(string  host, int port)
        {           
            //string host = "10.16.177.53";
           // int port = 7980;
            //string host = "localhost";
            //int port = 6981;

            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clientSocket.Connect(host, port);
            SocketHandler socketHandler = new SocketHandler(_clientSocket);
            var packetSender = new PacketSender();
            var packetRecver = new PacketRecver();            
            packetSender.SendClient = this;
            packetSender.SendServer = socketHandler;
            packetRecver.RecvClient = this;
            socketHandler.RecvClient = packetRecver;
            socketHandler.SendClient = packetSender;
            socketHandler.ErrorCallback = IOHandlerErrorCallback;
            packetSender.ErrorCallback = IOHandlerErrorCallback;
            packetRecver.ErrorCallback = IOHandlerErrorCallback;
            packetSender.Start();
            packetRecver.Start();
            socketHandler.Start();
            SendServer = packetSender;            
        }

        public void OnRecverStop(string reason)
        {
            ConsumerCallback($"Recv stop: {reason}");
        }
        public void OnSenderStop(string reason)
        {
            ConsumerCallback($"Send stop: {reason}");
        }

        private void IOHandlerErrorCallback(object context, Exception excp)
        {
            ConsumerCallback($"Send stop: {excp.Message}"); 
        }

        // recv methods called by lower layer
        public void OnRecverData(IList<ArraySegment<byte>> buffers)
        {
            // client has received some data
            // - do something with the data
            int totalSize = 0;
            string msg = "";
            foreach (ArraySegment<byte> buffer in buffers)
            {
                totalSize += buffer.Count;                
                msg += Encoding.UTF8.GetString(buffer.Array);
                //Debug.Print("Size:" + totalSize + " Msg:"  +msg);
            }
            //Debug.Print(msg);
            //AsyncLogMessage(String.Format("Client received {0} bytes in {1} buffers", totalSize, buffers.Count));
            DateTime dtNow = DateTime.Now;
            if (_serverStart == DateTime.MinValue)
                _serverStart = dtNow;
            _serverStop = dtNow;
            _serverBytes += totalSize;
            ConsumerCallback(msg);
        }
    }
}

