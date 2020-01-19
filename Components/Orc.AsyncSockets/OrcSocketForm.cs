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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Highlander.Orc.AsyncSockets
{
    public partial class OrcSocketForm : Form, IIORecvClient, IIOSendClient
    {
        private readonly SynchronizationContext _syncContext;
        // socket stuff
        private Socket _clientSocket;
        private Socket _serverSocket;
        // generic io stuff
        private IIOSendServer _sendServer;
        private IList<IIORecvServer> _recvServerList;
        // stats
        private DateTime _serverStart = DateTime.MinValue;
        private DateTime _serverStop = DateTime.MaxValue;
        private int _serverBytes;

        public OrcSocketForm()
        {
            InitializeComponent();
            _syncContext = SynchronizationContext.Current;
        }

        private void AsyncLogMessageHandler(object state)
        {
            string msg = (string)state;
            SyncLogMessage(msg);
        }

        private void AsyncLogExceptionHandler(object state)
        {
            Exception e = (Exception)state;
            SyncLogMessage(e.ToString());
        }

        private void SyncLogMessage(string msg)
        {
            txtLog.AppendText(msg + Environment.NewLine);
        }
        private void AsyncLogMessage(string msg)
        {
            _syncContext.Post(AsyncLogMessageHandler, msg);
        }

        private void AsyncLogException(Exception excp)
        {
            _syncContext.Post(AsyncLogExceptionHandler, excp);
        }

        private void Form1Load(object sender, EventArgs e)
        {
            _sendServer = null;
            _recvServerList = new List<IIORecvServer>();
        }

        private void Button1Click(object sender, EventArgs e)
        {
            // open client socket
            if (rbClientPort.Checked)
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.Connect(txtClientHost.Text, Convert.ToInt32(txtClientPort.Text));
                SocketHandler socketHandler = new SocketHandler(_clientSocket);
                PacketSender packetSender = new PacketSender();
                PacketRecver packetRecver = new PacketRecver();
                socketHandler.ErrorCallback = IOHandlerErrorCallback;
                packetSender.ErrorCallback = IOHandlerErrorCallback;
                packetRecver.ErrorCallback = IOHandlerErrorCallback;
                packetSender.SendClient = this;
                packetSender.SendServer = socketHandler;
                packetRecver.RecvClient = this;
                socketHandler.RecvClient = packetRecver;
                socketHandler.SendClient = packetSender;
                packetSender.Start();
                packetRecver.Start();
                socketHandler.Start();
                _sendServer = packetSender;
            }
            // open send queue
            if (rbClientQueue.Checked)
            {
                QueueWriter queueWriter = new QueueWriter(txtClientHost.Text, txtSendQueue.Text);
                PacketSender packetSender = new PacketSender {SendClient = this, SendServer = queueWriter};
                queueWriter.SendClient = packetSender;
                packetSender.ErrorCallback = IOHandlerErrorCallback;
                queueWriter.ErrorCallback = IOHandlerErrorCallback;
                packetSender.Start();
                queueWriter.Start();
                _sendServer = packetSender;
            }
        }

        // recv methods called by lower layer
        public void OnRecverData(IList<ArraySegment<byte>> buffers)
        {
            // client has received some data
            // - do something with the data
            int totalSize = 0;
            foreach (ArraySegment<byte> buffer in buffers)
            {
                totalSize += buffer.Count;
            }
            AsyncLogMessage($"Client received {totalSize} bytes in {buffers.Count} buffers");
            DateTime dtNow = DateTime.Now;
            if (_serverStart == DateTime.MinValue)
                _serverStart = dtNow;
            _serverStop = dtNow;
            _serverBytes += totalSize;
        }

        public void OnRecverStop(string reason)
        {
            AsyncLogMessage($"Recv stop: {reason}");
        }

        public void OnSenderStop(string reason)
        {
            AsyncLogMessage($"Send stop: {reason}");
        }

        public void SendExcp(Exception excp)
        {
            // client has received an error
            // - do something with the data
            AsyncLogException(excp);
        }

        private void IOHandlerErrorCallback(object context, Exception excp)
        {
            AsyncLogException(excp);
        }

        private void Button2Click(object sender, EventArgs e)
        {
            if (_clientSocket != null)
            {
                _clientSocket.Disconnect(false);
                _clientSocket = null;
            }
            if (_sendServer != null)
            {
                _sendServer.Stop("User request");
                _sendServer = null;
            }
        }

        private void Button5Click(object sender, EventArgs e)
        {
            // send text
            _sendServer.Send(Encoding.ASCII.GetBytes(txtRequestMsg.Text));
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket socket = listener.EndAccept(ar);
            AsyncLogMessage("Accepted connection from " + socket.RemoteEndPoint.ToString());
            SocketHandler socketHandler = new SocketHandler(socket);
            PacketRecver packetRecver = new PacketRecver();
            PacketSender packetSender = new PacketSender();
            _recvServerList.Add(socketHandler);
            packetRecver.RecvClient = this;
            socketHandler.RecvClient = packetRecver;
            socketHandler.SendClient = packetSender;
            packetRecver.ErrorCallback = IOHandlerErrorCallback;
            socketHandler.ErrorCallback = IOHandlerErrorCallback;
            packetRecver.Start();
            socketHandler.Start();
        }

        private void Button6Click(object sender, EventArgs e)
        {
            // create and send huge message
            int msgSize = Convert.ToInt32(txtMsgSize.Text);
            int msgCount = Convert.ToInt32(txtMsgCount.Text);
            StringBuilder sb = new StringBuilder(msgSize);
            for (int i = 0; i < msgSize; i++)
            {
                char ch = Convert.ToChar(i % 26 + 65);
                sb.Append(ch); 
            }
            string hugeMsg = sb.ToString();
            for (int i = 0; i < msgCount; i++)
                _sendServer.Send(Encoding.ASCII.GetBytes(hugeMsg));
        }

        private void BtnServerStartClick(object sender, EventArgs e)
        {
            // open main listener socket
            if (rbPort.Checked)
            {
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress[] ipAddresses = Dns.GetHostAddresses(txtClientHost.Text);
                foreach (IPAddress ipAddress in ipAddresses)
                {
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        EndPoint localEp = new IPEndPoint(ipAddress, Convert.ToInt32(txtListenPort.Text));
                        _serverSocket.Bind(localEp);
                        _serverSocket.Listen(100);
                        _serverSocket.BeginAccept(AcceptCallback, _serverSocket);
                        break;
                    }
                }
            }
            // open receive queue
            if (rbQueue.Checked)
            {
                PacketRecver packetRecver = new PacketRecver();
                QueueReader queueReader = new QueueReader(txtRecvQueue.Text);
                _recvServerList.Add(queueReader);
                packetRecver.RecvClient = this;
                queueReader.RecvClient = packetRecver;
                packetRecver.ErrorCallback = IOHandlerErrorCallback;
                queueReader.ErrorCallback = IOHandlerErrorCallback;
                packetRecver.Start();
                queueReader.Start();
            }
            // init stats
            _serverStart = DateTime.MinValue;
            _serverStop = DateTime.MaxValue;
            _serverBytes = 0;

        }

        private void BtnServerStopClick(object sender, EventArgs e)
        {
            // close main listener socket
            if (_serverSocket != null)
            {
                _serverSocket.Close();
                _serverSocket = null;
            }
            // stop client sending
            if (_sendServer != null)
            {
                _sendServer.Stop("User request");
                _sendServer = null;
            }
            // close all receiver servers
            foreach (IIORecvServer handler in _recvServerList)
            {
                handler.Stop("User request");
            }
        }

        private void Timer1Tick(object sender, EventArgs e)
        {
            // update stats
            txtServerConns.Text = _recvServerList.Count.ToString();
            TimeSpan duration = _serverStop - _serverStart;
            txtServerDuration.Text = duration.ToString();
            txtServerVolume.Text = _serverBytes.ToString();
        }
    }
}