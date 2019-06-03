using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Logging;
using Orion.Util.RefCounting;

namespace Core.V34.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
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
        public void TestThatServiceIsASingleton()
        {
            const int nClients = 10;
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                using (TestServer server = new TestServer(loggerRef.Target, 9001))
                {
                    Guid value0 = Guid.NewGuid();
                    // single publishing client
                    using (TestClient client = new TestClient("localhost", 9001))
                    {
                        client.SetValue(value0);
                    }
                    // multiple concurrent subscribing clients
                    long excpCount = 0;
                    long threadCount = 0;
                    for (int i = 0; i < nClients; i++)
                    {
                        Interlocked.Increment(ref threadCount);
                        ThreadPool.QueueUserWorkItem((state) =>
                        {
                            try
                            {
                                using (TestClient client = new TestClient("localhost", 9001))
                                {
                                    Assert.AreEqual<Guid>(value0, client.GetValue());
                                }
                            }
                            catch (Exception)
                            {
                                Interlocked.Increment(ref excpCount);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref threadCount);
                            }
                        });
                    }
                    while ((Interlocked.Add(ref threadCount, 0) > 0) && (Interlocked.Add(ref excpCount, 0) == 0))
                    {
                        Thread.Sleep(100);
                    }
                    Assert.AreEqual<long>(0, Interlocked.Add(ref excpCount, 0));
                }
            }
        }
        [TestMethod]
        public void TestThatServiceIsMultithreaded()
        {
            const int nClients = 100;
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                using (TestServer server = new TestServer(loggerRef.Target, 9001))
                {
                    const int msThreadDelay = 250;
                    bool multiThreaded = false;
                    // multiple concurrent clients
                    long excpCount = 0;
                    long startedCount = 0;
                    SpinLock spinlock = new SpinLock();
                    int maxServerThreadCount = 0;
                    //int maxSpinlockAttempts = 0;
                    //long runningCount = 0;
                    for (int i = 0; i < nClients; i++)
                    {
                        Interlocked.Increment(ref startedCount);
                        ThreadPool.QueueUserWorkItem((state) =>
                        {
                            try
                            {
                                using (TestClient client = new TestClient("localhost", 9001))
                                {
                                    // wait until all client threads are running
                                    //Interlocked.Increment(ref runningCount);
                                    //while ((Interlocked.Add(ref runningCount, 0) < nClients) && (Interlocked.Add(ref excpCount, 0) == 0))
                                    //{
                                    //    Thread.Sleep(100);
                                    //}
                                    // now call the service
                                    int serverThreadCount = client.GetThreadCount(msThreadDelay);
                                    if (serverThreadCount > 1)
                                    {
                                        multiThreaded = true;
                                        bool spinlocked = false;
                                        //int attempts = 0;
                                        //int failures = 0;
                                        while (!spinlocked)
                                        {
                                            //attempts++;
                                            spinlock.Enter(ref spinlocked);
                                            try
                                            {
                                                if (spinlocked)
                                                {
                                                    if (serverThreadCount > maxServerThreadCount)
                                                        maxServerThreadCount = serverThreadCount;
                                                    //if (attempts > maxSpinlockAttempts)
                                                    //    maxSpinlockAttempts = attempts;
                                                    //Thread.Sleep(100);
                                                }
                                                //else
                                                //    failures++;
                                            }
                                            finally
                                            {
                                                if (spinlocked)
                                                    spinlock.Exit();
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                Interlocked.Increment(ref excpCount);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref startedCount);
                            }
                        });
                    }
                    while ((Interlocked.Add(ref startedCount, 0) > 0) && (Interlocked.Add(ref excpCount, 0) == 0))
                    {
                        Thread.Sleep(1000);
                    }
                    Assert.AreEqual<long>(0, Interlocked.Add(ref excpCount, 0));
                    Assert.IsTrue(maxServerThreadCount > 1);
                    //Assert.IsTrue(maxSpinlockAttempts > 0);
                    Assert.IsTrue(multiThreaded);
                }
            }
        }
    }
}
