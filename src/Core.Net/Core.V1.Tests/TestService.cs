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
using System.Threading;
using Highlander.Core.Common;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.Threading;

namespace Highlander.Core.V1.Tests
{
   // [ServiceContract]
    public interface ITestService
    {
        //[OperationContract]
        void SetValue(Guid value);
        //[OperationContract]
        Guid GetValue();
        //[OperationContract]
        int GetThreadCount(int msTimeout);
    }

    public class TestClient //: CustomClientBase<ITestService>, ITestService
    {
        public TestClient(string host, int port) {}
            //: base(WcfHelper.CreateAddressBinding(
             //   WcfConst.NetTcp, host, port, "Test", typeof(ITestService).Name)) { }
        //public void SetValue(Guid value) { Channel.SetValue(value); }
        //public Guid GetValue() { return Channel.GetValue(); }
        //public int GetThreadCount(int msTimeout) { return Channel.GetThreadCount(msTimeout); }
    }
    
    public class TestServer : IDisposable//, ITestService
    {
        //private CustomServiceHost<ITestService, TestServer> _serverHost;
        public TestServer(ILogger logger, int port)
        {
            //_serverHost = new CustomServiceHost<ITestService, TestServer>(
            //    logger, this, ServiceHelper.FormatEndpoint(WcfConst.NetTcp, port),
            //    "Test", typeof(ITestService).Name, true);
        }
        public void Dispose()
        {
            //DisposeHelper.SafeDispose<CustomServiceHost<ITestService, TestServer>>(ref _serverHost);
        }

        //---------- service implementation ----------
        private class StateContainer
        {
            public Guid Value;
        }
        private readonly Guarded<StateContainer> _state = new Guarded<StateContainer>(new StateContainer());
        public void SetValue(Guid value)
        {
            _state.Locked((state) => state.Value = value);
        }

        public Guid GetValue()
        {
            Guid result = Guid.Empty;
            _state.Locked((state) => result = state.Value);
            return result;
        }

        private int _threadCount = 0;
        public int GetThreadCount(int msTimeout)
        {
            int result = Interlocked.Increment(ref _threadCount);
            try
            {
                Thread.Sleep(msTimeout); // ms
            }
            finally
            {
                Interlocked.Decrement(ref _threadCount);
            }
            return result;
        }
    }
}
