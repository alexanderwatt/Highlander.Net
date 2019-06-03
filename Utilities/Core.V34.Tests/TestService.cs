using System;
using System.Threading;
using System.ServiceModel;
using Core.Common;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.Threading;

namespace Core.V34.Tests
{
    [ServiceContract]
    public interface ITestService
    {
        [OperationContract]
        void SetValue(Guid value);
        [OperationContract]
        Guid GetValue();
        [OperationContract]
        int GetThreadCount(int msTimeout);
    }

    public class TestClient : CustomClientBase<ITestService>, ITestService
    {
        public TestClient(string host, int port)
            : base(WcfHelper.CreateAddressBinding(
                WcfConst.NetTcp, host, port, "Test", typeof(ITestService).Name)) { }
        public void SetValue(Guid value) { Channel.SetValue(value); }
        public Guid GetValue() { return Channel.GetValue(); }
        public int GetThreadCount(int msTimeout) { return Channel.GetThreadCount(msTimeout); }
    }
    
    public class TestServer : IDisposable, ITestService
    {
        private CustomServiceHost<ITestService, TestServer> _ServerHost;
        public TestServer(ILogger logger, int port)
        {
            _ServerHost = new CustomServiceHost<ITestService, TestServer>(
                logger, this, ServiceHelper.FormatEndpoint(WcfConst.NetTcp, port),
                "Test", typeof(ITestService).Name, true);
        }
        public void Dispose()
        {
            DisposeHelper.SafeDispose<CustomServiceHost<ITestService, TestServer>>(ref _ServerHost);
        }

        //---------- service implementation ----------
        private class StateContainer
        {
            public Guid Value;
        }
        private Guarded<StateContainer> _State = new Guarded<StateContainer>(new StateContainer());
        public void SetValue(Guid value)
        {
            _State.Locked((state) => state.Value = value);
        }

        public Guid GetValue()
        {
            Guid result = Guid.Empty;
            _State.Locked((state) => result = state.Value);
            return result;
        }

        private int _ThreadCount = 0;
        public int GetThreadCount(int msTimeout)
        {
            int result = Interlocked.Increment(ref _ThreadCount);
            try
            {
                Thread.Sleep(msTimeout); // ms
            }
            finally
            {
                Interlocked.Decrement(ref _ThreadCount);
            }
            return result;
        }
    }
}
