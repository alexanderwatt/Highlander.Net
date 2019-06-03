using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using Orion.Util.Threading;

namespace Util.Tests.Threading
{
    /// <summary>
    /// Summary description for ThreadQueueTests
    /// </summary>
    [TestClass]
    public class ThreadQueueTests
    {
        public ThreadQueueTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestQueueOrdering()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                const int iterations = 500000; // yes 500,000 - takes about 1.0 seconds
                int sendCount = 0;
                int recvCount = 0;
                int totalErrors = 0;
                int threadCount = 0;

                // dispatches a series of tasks
                using (AsyncThreadQueue queue = new AsyncThreadQueue(loggerRef.Target))
                {
                    AsyncQueueCallback<int> callback = delegate(int data)
                    {
                        int count = Interlocked.Increment(ref recvCount);
                        int threads = Interlocked.Increment(ref threadCount);
                        try
                        {
                            if (threads > 1)
                            {
                                // multithreading issue!
                                int errors = Interlocked.Increment(ref totalErrors);
                                if (errors <= 50)
                                    loggerRef.Target.LogDebug("Thread count = {0}!", threads);
                            }
                            if (count != (data + 1))
                            {
                                int errors = Interlocked.Increment(ref totalErrors);
                                if (errors <= 50)
                                    loggerRef.Target.LogDebug("Recv count = {0} but (data+1) = {1}", count, data + 1);
                            }
                        }
                        finally
                        {
                            Interlocked.Decrement(ref threadCount);
                        }
                    };
                    for (int i = 0; i < iterations; i++)
                    {
                        Interlocked.Increment(ref sendCount);
                        queue.Dispatch<int>(i, callback);
                    }
                    Assert.AreEqual<int>(iterations, sendCount);
                    long itemsRemaining = queue.WaitUntilEmpty(TimeSpan.FromSeconds(30));
                    Assert.AreEqual<long>(0, itemsRemaining);
                    Assert.AreEqual<int>(iterations, recvCount);
                    Assert.AreEqual<int>(0, totalErrors);
                }
            }
        }

        [TestMethod]
        public void TestQueueConcurrency()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                const int iterations = 10;
                int sendCount = 0;
                int totalErrors = 0;
                int threadCount = 0;
                int callbacksCommenced = 0;
                int callbacksCompleted = 0;

                // dispatches a series of tasks
                using (AsyncThreadQueue queue = new AsyncThreadQueue(loggerRef.Target))
                {
                    AsyncQueueCallback<int> callback = delegate(int data)
                    {
                        Interlocked.Increment(ref callbacksCommenced);
                        int threads = Interlocked.Increment(ref threadCount);
                        try
                        {
                            if (threads > 1)
                            {
                                // multithreading issue!
                                int errors = Interlocked.Increment(ref totalErrors);
                                if (errors <= 50)
                                    loggerRef.Target.LogDebug("Thread count = {0}!", threads);
                            }
                            Thread.Sleep(TimeSpan.FromSeconds(0.5));
                        }
                        finally
                        {
                            Interlocked.Decrement(ref threadCount);
                        }
                        Interlocked.Increment(ref callbacksCompleted);
                    };
                    for (int i = 0; i < iterations; i++)
                    {
                        Interlocked.Increment(ref sendCount);
                        queue.Dispatch<int>(i, callback);
                    }
                    Assert.AreEqual<int>(iterations, sendCount);
                    long itemsRemaining = queue.WaitUntilEmpty(TimeSpan.FromSeconds(30));
                    Assert.AreEqual<long>(0, itemsRemaining);
                    Assert.AreEqual<int>(0, totalErrors);
                    Assert.AreEqual<int>(iterations, callbacksCommenced);
                    Assert.AreEqual<int>(iterations, callbacksCompleted);
                }
            }
        }

        [TestMethod]
        public void TestPriorityQueue()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                const int iterations = 100000;
                int sendCount = 0;
                int recvCount = 0;
                int excpCount = 0;

                // dispatches a series of tasks with various priorities
                int nPriorities = Enum.GetValues(typeof(AsyncQueuePriority)).Length;
                int[] _LastRecv = new int[nPriorities];
                for (int l = 0; l < nPriorities; l++)
                {
                    _LastRecv[l] = -1;
                }
                using (AsyncPriorityQueue queue = new AsyncPriorityQueue(loggerRef.Target))
                {
                    AsyncQueueCallback<int> callback = delegate(int value)
                    {
                        int count = Interlocked.Increment(ref recvCount);
                        // check values are recevied in priority order
                        int i = value % 1000000;
                        int priority = (value - i) / 1000000;
                        int last = _LastRecv[priority];
                        if (i <= last)
                        {
                            int errors = Interlocked.Increment(ref excpCount);
                            if (errors <= 50)
                                loggerRef.Target.LogDebug("Priority={0}: i ({1}) not greater than previous ({2})!",
                                    priority, i, last);
                        }
                        _LastRecv[priority] = i;
                    };
                    for (int i = 0; i < iterations; i++)
                    {
                        Interlocked.Increment(ref sendCount);
                        AsyncQueuePriority priority = (AsyncQueuePriority)(i % nPriorities);
                        int value = (int)priority * 1000000 + i;
                        queue.Dispatch<int>(value, callback, priority);
                    }
                    Assert.AreEqual<int>(iterations, sendCount);
                    long itemsRemaining = queue.WaitUntilEmpty(TimeSpan.FromSeconds(30));
                    Assert.AreEqual<long>(0, itemsRemaining);
                    Assert.AreEqual<int>(iterations, recvCount);
                    Assert.AreEqual<int>(0, excpCount);
                }
            }
        }

        [TestMethod]
        public void TestPriorityStack()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                const int iterations = 10000;
                int sendCount = 0;
                //int recvCount = 0;
                //int excpCount = 0;

                // dispatches a series of tasks with various priorities
                int nPriorities = Enum.GetValues(typeof(AsyncQueuePriority)).Length;
                int[] _LastRecv = new int[nPriorities];
                for (int l = 0; l < nPriorities; l++)
                {
                    _LastRecv[l] = -1;
                }
                using (AsyncPriorityStack stack = new AsyncPriorityStack(loggerRef.Target))
                {
                    AsyncQueueCallback<int> callback = delegate(int value)
                    {
                        Thread.Sleep(0);
                        //int count = Interlocked.Increment(ref recvCount);
                        // check values are recevied in priority order
                        //int i = value % 1000000;
                        //int priority = (value - i) / 1000000;
                        //int last = _LastRecv[priority];
                        //if (i <= last)
                        //{
                        //    int errors = Interlocked.Increment(ref excpCount);
                        //    if (errors <= 50)
                        //        loggerRef.Target.LogDebug("Priority={0}: i ({1}) not greater than previous ({2})!",
                        //            priority, i, last);
                        //}
                        //_LastRecv[priority] = i;
                    };
                    for (int i = 0; i < iterations; i++)
                    {
                        Interlocked.Increment(ref sendCount);
                        AsyncQueuePriority priority = (AsyncQueuePriority)(i % nPriorities);
                        int value = (int)priority * 1000000 + i;
                        stack.Dispatch<int>(value, callback, priority, (i % 1000).ToString());
                    }
                    Assert.AreEqual<int>(iterations, sendCount);
                    long itemsRemaining = stack.WaitUntilEmpty(TimeSpan.FromSeconds(30));
                    Assert.AreEqual<long>(0, itemsRemaining);
                    //Assert.AreEqual<int>(iterations, recvCount);
                    //Assert.AreEqual<int>(0, excpCount);
                }
            }
        }

    }
}
