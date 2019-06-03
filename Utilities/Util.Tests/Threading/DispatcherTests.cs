using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using Orion.Util.Threading;

namespace Util.Tests.Threading
{
    /// <summary>
    /// Summary description for CorePerfTests
    /// </summary>
    [TestClass]
    public class DispatcherTests
    {
        public DispatcherTests()
        {
            //
            // Add constructor logic here
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

        private class TestMsg
        {
            public readonly int L;
            public readonly int A = -1;
            public readonly int B = -1;
            public readonly int C = -1;
            public readonly int D = -1;
            public readonly int N;
            public TestMsg(int seqnum)
            {
                L = 0;
                N = seqnum;
            }
            public TestMsg(int seqnum, int a)
            {
                L = 1;
                A = a;
                N = seqnum;
            }
            public TestMsg(int seqnum, int a, int b)
            {
                L = 2;
                A = a;
                B = b;
                N = seqnum;
            }
            public TestMsg(int seqnum, int a, int b, int c)
            {
                L = 3;
                A = a;
                B = b;
                C = c;
                N = seqnum;
            }
            public TestMsg(int seqnum, int a, int b, int c, int d)
            {
                L = 4;
                A = a;
                B = b;
                C = c;
                D = d;
                N = seqnum;
            }
            private string NStr(int n)
            {
                if ((n >= 0) && (n <= 9))
                    return " " + n.ToString();
                else
                    return n.ToString();
            }
            public string Title { get { return String.Format("L{0}[{1},{2},{3}]", L, NStr(A), NStr(B), NStr(C)); } }
        }

        private delegate void TestMsgDelegate(TestMsg thisMsg, TestMsg prevMsg);

        [TestMethod]
        public void TestDispatcherSequencing()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                const int InitSeqNum = 1000000;
                const int scaleFactor = 50;

                long seqErrors = 0;
                int dispatchCount = 0;
                int callbackCount = 0;
                Dispatcher seq = new Dispatcher(loggerRef.Target, "TestCount");
                try
                {
                    // setup Dispatcher handles
                    DispatcherHandle handle0 = seq.GetDispatcherHandle("");
                    DispatcherHandle[] handle1 = new DispatcherHandle[scaleFactor];
                    DispatcherHandle[,] handle2 = new DispatcherHandle[scaleFactor, scaleFactor];
                    DispatcherHandle[, ,] handle3 = new DispatcherHandle[scaleFactor, scaleFactor, scaleFactor];
                    TestMsg prev_msg0 = null;
                    TestMsg[] prev_msg1 = new TestMsg[scaleFactor];
                    TestMsg[,] prev_msg2 = new TestMsg[scaleFactor, scaleFactor];
                    TestMsg[, ,] prev_msg3 = new TestMsg[scaleFactor, scaleFactor, scaleFactor];
                    loggerRef.Target.LogDebug("building handles...");
                    for (int a = 0; a < scaleFactor; a++)
                    {
                        string aKey = "a" + a.ToString();
                        handle1[a] = seq.GetDispatcherHandle(aKey);
                        for (int b = 0; b < scaleFactor; b++)
                        {
                            string bKey = aKey + "/" + "b" + b.ToString();
                            handle2[a, b] = seq.GetDispatcherHandle(bKey);
                            for (int c = 0; c < scaleFactor; c++)
                            {
                                string cKey = bKey + "/" + "c" + c.ToString();
                                handle3[a, b, c] = seq.GetDispatcherHandle(cKey);
                            }
                        }
                    }

                    // dispatch some events and check sequence
                    TestMsgDelegate testSeqNum = delegate(TestMsg thisMsg, TestMsg prevMsg)
                                                     {
                                                         if (prevMsg == null)
                                                             return;
                                                         if (thisMsg.N > prevMsg.N)
                                                             return;
                                                         // failed
                                                         Interlocked.Increment(ref seqErrors);
                                                         loggerRef.Target.LogError("{0} SeqNum ({1}) is <= PrevMsg {2} SeqNum ({3})",
                                                                         thisMsg.Title, thisMsg.N, prevMsg.Title, prevMsg.N);
                                                     };

                    SendOrPostCallback callback = delegate(object state)
                                                      {
                                                          Interlocked.Increment(ref callbackCount);
                                                          TestMsg msg = (TestMsg)state;
                                                          //loggerRef.Target.LogDebug("{0} SeqNum ({1}) callback commenced.", msg.Title, msg.N);
                                                          Thread.Sleep(3 - (msg.L * 1));
                                                          // test sequence
                                                          // - seqnum must be greater than parents and all children
                                                          if (msg.L == 0)
                                                          {
                                                              testSeqNum(msg, prev_msg0);
                                                              for (int a = 0; a < scaleFactor; a++)
                                                              {
                                                                  testSeqNum(msg, prev_msg1[a]);
                                                                  for (int b = 0; b < scaleFactor; b++)
                                                                  {
                                                                      testSeqNum(msg, prev_msg2[a, b]);
                                                                      for (int c = 0; c < scaleFactor; c++)
                                                                      {
                                                                          testSeqNum(msg, prev_msg3[a, b, c]);
                                                                      }
                                                                  }
                                                              }
                                                              prev_msg0 = msg;
                                                          }
                                                          if (msg.L == 1)
                                                          {
                                                              testSeqNum(msg, prev_msg0);
                                                              testSeqNum(msg, prev_msg1[msg.A]);
                                                              for (int b = 0; b < scaleFactor; b++)
                                                              {
                                                                  testSeqNum(msg, prev_msg2[msg.A, b]);
                                                                  for (int c = 0; c < scaleFactor; c++)
                                                                  {
                                                                      testSeqNum(msg, prev_msg3[msg.A, b, c]);
                                                                  }
                                                              }
                                                              prev_msg1[msg.A] = msg;
                                                          }
                                                          if (msg.L == 2)
                                                          {
                                                              testSeqNum(msg, prev_msg0);
                                                              testSeqNum(msg, prev_msg1[msg.A]);
                                                              testSeqNum(msg, prev_msg2[msg.A, msg.B]);
                                                              for (int c = 0; c < scaleFactor; c++)
                                                              {
                                                                  testSeqNum(msg, prev_msg3[msg.A, msg.B, c]);
                                                              }
                                                              prev_msg2[msg.A, msg.B] = msg;
                                                          }
                                                          if (msg.L == 3)
                                                          {
                                                              testSeqNum(msg, prev_msg0);
                                                              testSeqNum(msg, prev_msg1[msg.A]);
                                                              testSeqNum(msg, prev_msg2[msg.A, msg.B]);
                                                              testSeqNum(msg, prev_msg3[msg.A, msg.B, msg.C]);
                                                              prev_msg3[msg.A, msg.B, msg.C] = msg;
                                                          }
                                                          //loggerRef.Target.LogDebug("{0} SeqNum ({1}) callback completed.", msg.Title, msg.N);
                                                      };
                    loggerRef.Target.LogDebug("dispatching tasks...");
                    int seqnum = InitSeqNum;
                    handle0.DispatchObject(callback, new TestMsg(seqnum++));
                    for (int a = 0; a < scaleFactor; a++)
                    {
                        handle1[a].DispatchObject(callback, new TestMsg(seqnum++, a));
                        for (int b = 0; b < scaleFactor; b++)
                        {
                            handle2[a, b].DispatchObject(callback, new TestMsg(seqnum++, a, b));
                            for (int c = 0; c < scaleFactor; c++)
                            {
                                handle3[a, b, c].DispatchObject(callback, new TestMsg(seqnum++, a, b, c));
                            }
                            handle2[a, b].DispatchObject(callback, new TestMsg(seqnum++, a, b));
                        }
                        handle1[a].DispatchObject(callback, new TestMsg(seqnum++, a));
                    }
                    handle0.DispatchObject(callback, new TestMsg(seqnum++));
                    dispatchCount = (seqnum - InitSeqNum);
                    loggerRef.Target.LogDebug("dispatched {0} tasks.", dispatchCount);
                }
                finally
                {
                    long n = seq.Wait(TimeSpan.FromSeconds(0));
                    while (n > 0)
                    {
                        loggerRef.Target.LogDebug("waiting for {0} tasks", n);
                        n = seq.Wait(TimeSpan.FromSeconds(5));
                        Assert.AreEqual<long>(0, Interlocked.Add(ref seqErrors, 0));
                    }
                    seq = null;
                    loggerRef.Target.LogDebug("{0} tasks completed.", callbackCount);
                    // test
                    Assert.AreEqual<int>(dispatchCount, callbackCount);
                    Assert.AreEqual<long>(0, Interlocked.Add(ref seqErrors, 0));
                }
            }
        }

        [TestMethod]
        public void TestDispatcherScalability()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                const int InitSeqNum = 1000000;
                const int scaleFactor = 20; // => 20^4 (or 160,000) tasks

                int dispatchCount = 0;
                int callbackCount = 0;
                Dispatcher seq = new Dispatcher(loggerRef.Target, "TestCount");
                try
                {
                    // setup Dispatcher handles
                    DispatcherHandle handle0 = seq.GetDispatcherHandle("");
                    DispatcherHandle[] handle1 = new DispatcherHandle[scaleFactor];
                    DispatcherHandle[,] handle2 = new DispatcherHandle[scaleFactor, scaleFactor];
                    DispatcherHandle[, ,] handle3 = new DispatcherHandle[scaleFactor, scaleFactor, scaleFactor];
                    DispatcherHandle[, , ,] handle4 = new DispatcherHandle[scaleFactor, scaleFactor, scaleFactor, scaleFactor];
                    //TestMsg prev_msg0 = null;
                    //TestMsg[] prev_msg1 = new TestMsg[scaleFactor];
                    //TestMsg[,] prev_msg2 = new TestMsg[scaleFactor, scaleFactor];
                    //TestMsg[, ,] prev_msg3 = new TestMsg[scaleFactor, scaleFactor, scaleFactor];
                    for (int a = 0; a < scaleFactor; a++)
                    {
                        string aKey = "a" + a.ToString();
                        handle1[a] = seq.GetDispatcherHandle(aKey);
                        for (int b = 0; b < scaleFactor; b++)
                        {
                            string bKey = aKey + "/" + "b" + a.ToString();
                            handle2[a, b] = seq.GetDispatcherHandle(bKey);
                            for (int c = 0; c < scaleFactor; c++)
                            {
                                string cKey = bKey + "/" + "c" + a.ToString();
                                handle3[a, b, c] = seq.GetDispatcherHandle(cKey);
                                for (int d = 0; d < scaleFactor; d++)
                                {
                                    string dKey = cKey + "/" + "d" + a.ToString();
                                    handle4[a, b, c, d] = seq.GetDispatcherHandle(dKey);
                                }
                            }
                        }
                    }

                    // dispatch some events and check sequence
                    SendOrPostCallback callback = delegate(object state)
                                                      {
                                                          Interlocked.Increment(ref callbackCount);
                                                          TestMsg msg = (TestMsg)state;
                                                          //loggerRef.Target.LogDebug("{0} SeqNum ({1}) callback commenced.", msg.Title, msg.N);
                                                          //Thread.Sleep(30 - (msg.L * 10));
                                                          //loggerRef.Target.LogDebug("{0} SeqNum ({1}) callback completed.", msg.Title, msg.N);
                                                      };
                    loggerRef.Target.LogDebug("dispatching tasks...");
                    int seqnum = InitSeqNum;
                    handle0.DispatchObject(callback, new TestMsg(seqnum++));
                    for (int a = 0; a < scaleFactor; a++)
                    {
                        handle1[a].DispatchObject(callback, new TestMsg(seqnum++, a));
                        for (int b = 0; b < scaleFactor; b++)
                        {
                            handle2[a, b].DispatchObject(callback, new TestMsg(seqnum++, a, b));
                            for (int c = 0; c < scaleFactor; c++)
                            {
                                handle3[a, b, c].DispatchObject(callback, new TestMsg(seqnum++, a, b, c));
                                for (int d = 0; d < scaleFactor; d++)
                                {
                                    handle4[a, b, c, d].DispatchObject(callback, new TestMsg(seqnum++, a, b, c, d));
                                }
                                handle3[a, b, c].DispatchObject(callback, new TestMsg(seqnum++, a, b, c));
                            }
                            handle2[a, b].DispatchObject(callback, new TestMsg(seqnum++, a, b));
                        }
                        handle1[a].DispatchObject(callback, new TestMsg(seqnum++, a));
                    }
                    handle0.DispatchObject(callback, new TestMsg(seqnum++));
                    dispatchCount = (seqnum - InitSeqNum);
                    loggerRef.Target.LogDebug("dispatched {0} tasks.", dispatchCount);
                }
                finally
                {
                    long n = seq.Wait(TimeSpan.FromSeconds(0));
                    while (n > 0)
                    {
                        loggerRef.Target.LogDebug("waiting for {0} tasks", n);
                        n = seq.Wait(TimeSpan.FromSeconds(5));
                    }
                    seq = null;
                    loggerRef.Target.LogDebug("{0} tasks completed.", callbackCount);
                    // test
                    Assert.AreEqual<int>(dispatchCount, callbackCount);
                }
            }
        }

        [TestMethod]
        public void TestDispatcherParallelism()
        {
            using (Reference<ILogger> loggerRef = Reference<ILogger>.Create(new TraceLogger(true)))
            {
                Int32 counter = 10000;
                // dispatches a series of serial and parallel tasks
                Dispatcher seq = new Dispatcher(loggerRef.Target, "UnitTest");
                try
                {
                    DispatcherHandle key0_root = seq.GetDispatcherHandle("");
                    DispatcherHandle key1_slow = seq.GetDispatcherHandle("slow");
                    DispatcherHandle key1_norm = seq.GetDispatcherHandle("norm");
                    DispatcherHandle key1_fast = seq.GetDispatcherHandle("fast");
                    // start first global task
                    key0_root.DispatchObject(delegate(object state)
                                                   {
                                                       int count1 = Interlocked.Increment(ref counter);
                                                       loggerRef.Target.LogDebug("[{0}]key0_root: (a) running", count1);
                                                       Thread.Sleep(1000);
                                                       int count2 = Interlocked.Increment(ref counter);
                                                       loggerRef.Target.LogDebug("[{0}]key0_root: (a) stopped", count2);
                                                   }, null);
                    Assert.AreEqual<long>(1, seq.Wait(TimeSpan.Zero));
                    // start 1 slow (3 second) task
                    for (int i = 0; i < 1; i++)
                    {
                        key1_slow.DispatchObject(delegate(object state)
                                                       {
                                                           string name = (string)state;
                                                           int count1 = Interlocked.Increment(ref counter);
                                                           loggerRef.Target.LogDebug("[{1}]key1_slow: ({0}) running", name, count1);
                                                           Thread.Sleep(3000); // 3 secs
                                                           int count2 = Interlocked.Increment(ref counter);
                                                           loggerRef.Target.LogDebug("[{1}]key1_slow: ({0}) stopped", name, count2);
                                                       }, i.ToString());
                        Assert.AreEqual<long>(i + 2, seq.Wait(TimeSpan.Zero));
                    }
                    // start 3 normal (1 second) tasks
                    for (int i = 0; i < 3; i++)
                    {
                        key1_norm.DispatchObject(delegate(object state)
                                                       {
                                                           string name = (string)state;
                                                           int count1 = Interlocked.Increment(ref counter);
                                                           loggerRef.Target.LogDebug("[{1}]key1_norm: ({0}) running", name, count1);
                                                           Thread.Sleep(1000); // 1 sec
                                                           int count2 = Interlocked.Increment(ref counter);
                                                           loggerRef.Target.LogDebug("[{1}]key1_norm: ({0}) stopped", name, count2);
                                                       }, i.ToString());
                        Assert.AreEqual<long>(i + 3, seq.Wait(TimeSpan.Zero));
                    }
                    // start 10 fast (0.1 second) tasks
                    for (int i = 0; i < 10; i++)
                    {
                        key1_fast.DispatchObject(delegate(object state)
                                                       {
                                                           string name = (string)state;
                                                           int count1 = Interlocked.Increment(ref counter);
                                                           loggerRef.Target.LogDebug("[{1}]key1_fast: ({0}) running", name, count1);
                                                           Thread.Sleep(100); // 0.1 sec
                                                           int count2 = Interlocked.Increment(ref counter);
                                                           loggerRef.Target.LogDebug("[{1}]key1_fast: ({0}) stopped", name, count2);
                                                       }, i.ToString());
                        Assert.AreEqual<long>(i + 6, seq.Wait(TimeSpan.Zero));
                    }
                    // start last global task
                    key0_root.DispatchObject(delegate(object state)
                                                   {
                                                       //int count = Interlocked.Increment(ref counter);
                                                       //loggerRef.Target.LogDebug("[{0}]key0_root: (b) running", count);
                                                       Thread.Sleep(1000);
                                                       int count2 = Interlocked.Increment(ref counter);
                                                       loggerRef.Target.LogDebug("[{0}]key0_root: (b) stopped", count2);
                                                   }, null);
                    Assert.AreEqual<long>(16, seq.Wait(TimeSpan.Zero));
                }
                finally
                {
                    loggerRef.Target.LogDebug("all tasks dispatched");
                    long n = seq.Wait(TimeSpan.FromSeconds(0));
                    while (n > 0)
                    {
                        loggerRef.Target.LogDebug("waiting for {0} tasks", n);
                        n = seq.Wait(TimeSpan.FromSeconds(5));
                    }
                    seq = null;
                    loggerRef.Target.LogDebug("all tasks completed");
                }
            }
        }
    }
}