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

namespace Highlander.Orc.AsyncSockets
{
    public interface IIORecvClient
    {
        void OnRecverData(IList<ArraySegment<byte>> buffers);
        void OnRecverStop(string reason);
    }

    public interface IIOSendClient
    {
        void OnSenderStop(string reason);
    }

    public interface IIORecvServer
    {
        void Start();
        void Stop(string reason);
    }

    public interface IIOSendServer
    {
        void Start();
        void Stop(string reason);
        void Send(byte[] buffer);
        void Send(byte[] buffer, int offset, int size);
        void Send(IList<ArraySegment<byte>> buffers);
    }
}