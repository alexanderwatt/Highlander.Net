using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using Orion.Util.Threading;

namespace Util.Tests.Threading
{
    /// <summary>
    /// Summary description for LockPerfTests
    /// </summary>
    [TestClass]
    public class LockPerfTests
    {
        public LockPerfTests()
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

        private const int TestIterations = 1000000; // yes 1 million

        [TestMethod]
        public void TestObjectLockPerformance()
        {
            bool _IsSingleCore = (Environment.ProcessorCount == 1);

            //---------------------------------------------------------------
            {
                object target = new object();
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int lockedCount = 0;
                for (int i = 0; i < TestIterations; i++)
                {
                    lock (target)
                    {
                        lockedCount++;
                    }
                }
                sw.Stop();
                Debug.Print("BenchmarkC#LockPattern : {0} seconds, {1} locks/ms",
                    sw.Elapsed.TotalSeconds, lockedCount / sw.ElapsedMilliseconds);
            }
            //---------------------------------------------------------------
            // ----------- this is the lowest overhead technique ------------
            //---------------------------------------------------------------
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int lockedCount = 0;
                int _Spinlock = 0;
                for (int i = 0; i < TestIterations; i++)
                {
                    while (Interlocked.CompareExchange(ref _Spinlock, 1, 0) != 0)
                    {
                        if (_IsSingleCore)
                            Thread.Sleep(0);
                    }
                    try
                    {
                        lockedCount++;
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _Spinlock, 0);
                    }
                }
                sw.Stop();
                Debug.Print("InlineInterlockedInt32 : {0} seconds, {1} locks/ms",
                    sw.Elapsed.TotalSeconds, lockedCount / sw.ElapsedMilliseconds);
            }

            //---------------------------------------------------------------
            {
                object target = new object();
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int lockedCount = 0;
                for (int i = 0; i < TestIterations; i++)
                {
                    Monitor.Enter(target);
                    try
                    {
                        lockedCount++;
                    }
                    finally
                    {
                        Monitor.Exit(target);
                    }
                }
                sw.Stop();
                Debug.Print("SystemMonitorEnterExit : {0} seconds, {1} locks/ms",
                    sw.Elapsed.TotalSeconds, lockedCount / sw.ElapsedMilliseconds);
            }
            //---------------------------------------------------------------
            {
                object target = new object();
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int lockedCount = 0;
                for (int i = 0; i < TestIterations; i++)
                {
                    int spinCount = 0;
                    object lockedObject = null;
                    while ((lockedObject = Interlocked.Exchange<object>(ref target, null)) == null)
                    {
                        if (_IsSingleCore)
                            Thread.Sleep(0);
                        else
                        {
                            spinCount++;
                            if (spinCount % 4000 == 0)
                                Thread.Sleep(0);
                        }
                    }
                    try
                    {
                        lockedCount++;
                    }
                    finally
                    {
                        Interlocked.Exchange<object>(ref target, lockedObject);
                    }
                }
                sw.Stop();
                Debug.Print("InlineInterlockedObject: {0} seconds, {1} locks/ms",
                    sw.Elapsed.TotalSeconds, lockedCount / sw.ElapsedMilliseconds);
            }
            //---------------------------------------------------------------
            {
                SpinLock spinlockFast = new SpinLock(false);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int lockedCount = 0;
                for (int i = 0; i < TestIterations; i++)
                {
                    bool lockTaken = false;
                    while (!lockTaken)
                    {
                        spinlockFast.Enter(ref lockTaken);
                        try
                        {
                            if (lockTaken)
                            {
                                lockedCount++;
                            }
                        }
                        finally
                        {
                            if (lockTaken)
                                spinlockFast.Exit();
                        }
                    }
                }
                sw.Stop();
                Debug.Print("Net4SpinlockPatternFast: {0} seconds, {1} locks/ms",
                    sw.Elapsed.TotalSeconds, lockedCount / sw.ElapsedMilliseconds);
            }
            //---------------------------------------------------------------
            {
                SpinLock spinlockSlow = new SpinLock(true);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int lockedCount = 0;
                for (int i = 0; i < TestIterations; i++)
                {
                    bool lockTaken = false;
                    while (!lockTaken)
                    {
                        spinlockSlow.Enter(ref lockTaken);
                        try
                        {
                            if (lockTaken)
                            {
                                lockedCount++;
                            }
                        }
                        finally
                        {
                            if (lockTaken)
                                spinlockSlow.Exit();
                        }
                    }
                }
                sw.Stop();
                Debug.Print("Net4SpinlockPatternSlow: {0} seconds, {1} locks/ms",
                    sw.Elapsed.TotalSeconds, lockedCount / sw.ElapsedMilliseconds);
            }
            //---------------------------------------------------------------
            {
                object target = new object();
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int lockedCount = 0;
                for (int i = 0; i < TestIterations; i++)
                {
                    object lockedObject = ObjectLock<object>.Enter(ref target);
                    try
                    {
                        lockedCount++;
                    }
                    finally
                    {
                        ObjectLock<object>.Leave(ref target, lockedObject);
                    }
                }
                sw.Stop();
                Debug.Print("EnterLeaveCallsPattern : {0} seconds, {1} locks/ms",
                    sw.Elapsed.TotalSeconds, lockedCount / sw.ElapsedMilliseconds);
            }
            //---------------------------------------------------------------
            {
                object target = new object();
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int lockedCount = 0;
                for (int i = 0; i < TestIterations; i++)
                {
                    ObjectLock<object>.Protect(ref target, delegate(object lockedObject)
                    {
                        lockedCount++;
                    });
                }
                sw.Stop();
                Debug.Print("LockedDelegatePattern  : {0} seconds, {1} locks/ms",
                    sw.Elapsed.TotalSeconds, lockedCount / sw.ElapsedMilliseconds);
            }
            //---------------------------------------------------------------
            {
                object target = new object();
                Guarded<object> wrapper = new Guarded<object>(target);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int lockedCount = 0;
                for (int i = 0; i < TestIterations; i++)
                {
                    //int n = (i % TargetObjects);
                    wrapper.Locked((lockedObject) =>
                    {
                        lockedCount++;
                    });
                }
                sw.Stop();
                Debug.Print("GuardedWrapperPattern  : {0} seconds, {1} locks/ms",
                    sw.Elapsed.TotalSeconds, lockedCount / sw.ElapsedMilliseconds);
            }
        }
    }
}
