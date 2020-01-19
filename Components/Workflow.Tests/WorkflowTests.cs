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
using System.Globalization;
using System.Threading;
using Highlander.Core.Common;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Workflow.Tests
{
    public class UnitTestEnvironment : IDisposable
    {
        private ICoreClient _client;
        private readonly ICoreCache _cache;
        public UnitTestEnvironment(ILogger logger)
        {
            _client = new PrivateCore(logger);
            _cache = _client.CreateCache();
        }
        public ICoreClient Proxy { get { return _client; } }
        public ICoreCache Cache { get { return _cache; } }

        public void Dispose()
        {
            DisposeHelper.SafeDispose(ref _client);
        }
    }

    /// <summary>
    /// Summary description for WorkflowTests
    /// </summary>
    [TestClass]
    public class WorkflowTests
    {
        public WorkflowTests()
        {
            //
            // Add constructor logic here
            //
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private static ILogger _logger;
        private static UnitTestEnvironment _testEnvironment;
        private static IWorkContext _workContext;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            // create test env and load data
            _logger = new TraceLogger(true);
            _testEnvironment = new UnitTestEnvironment(_logger);
            _workContext = new WorkContext(_logger, _testEnvironment.Cache, "UnitTest");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _testEnvironment.Dispose();
            _logger.Dispose();
        }


        [TestMethod]
        public void TestWorkstepDelegation()
        {
            // defines and executes a single workstep via delegate methods
            var w = new WorkstepBase<T1, T2>(
                delegate(T1 input)
                {
                    T2 result;
                    result.b = input.a;
                    return result;
                }, null);
            w.Initialise(_workContext);
            var t1 = new T1 {a = 42};
            var t2 = w.Execute(t1).Result;
            Assert.AreEqual(42, t2.b);
        }

        [TestMethod]
        public void TestWorkstepOverride()
        {
            // defines and executes a single workstep via virtual (override) methods 
            var w = new SequentialSubstep1();
            w.Initialise(_workContext);
            var t1 = new T1 {a = 42};
            T2 t2 = w.Execute(t1).Result;
            Assert.AreEqual(42, t2.b);
        }

        [TestMethod]
        public void TestWorkstepSequentialDelegate()
        {
            // defines and executes a basic sequential workflow using inline delegate methods
            var w = new WorkstepBaseSeq<T1, T3>(
                new IWorkstep[] {
                    // step 1
                    new WorkstepBase<T1, T2>(
                        delegate(T1 input)
                        {
                            T2 result;
                            result.b = input.a;
                            return result;
                        }, null), 
                    // step 2
                    new WorkstepBase<T2, T3>(
                        delegate(T2 input)
                        {
                            T3 result;
                            result.c = input.b;
                            return result;
                        }, null) 
                });
            w.Initialise(_workContext);
            var t1 = new T1 {a = 42};
            T3 t3 = w.Execute(t1).Result;
            Assert.AreEqual(42, t3.c);
        }

        [TestMethod]
        public void TestWorkstepSequentialOverride()
        {
            // defines and executes a basic sequential workflow using classes
            var w = new SequentialStep();
            w.Initialise(_workContext);
            var t1 = new T1 {a = 42};
            T3 t3 = w.Execute(t1).Result;
            Assert.AreEqual(42, t3.c);
        }

        [TestMethod]
        public void TestWorkstepBranchingDelegate()
        {
            // defines and executes a branching workflow
            var w = new WorkstepBaseAlt<T1, T2>(
                // chooser
                input => input.a,
                // steps
                new IWorkstep[] {
                    // step 1
                    new WorkstepBase<T1, T2>(
                        delegate(T1 input)
                        {
                            T2 result;
                            result.b = input.a * -5;
                            return result;
                        }, null), 
                    // step 2
                    new WorkstepBase<T1, T2>(
                        delegate(T1 input)
                        {
                            T2 result;
                            result.b = input.a * +5;
                            return result;
                        }, null) 
                });
            w.Initialise(_workContext);
            var t1 = new T1 {a = 1};
            T2 t2 = w.Execute(t1).Result;
            Assert.AreEqual(5, t2.b);
        }

        [TestMethod]
        public void TestWorkstepIfThenElse()
        {
            // defines and executes an if-then-else workstep
            var w = new WorkstepIfThenElse<T1, T2>(
                // if-condition
                input => (input.a == 0),
                // then-step
                new WorkstepBase<T1, T2>(delegate
                    {
                    T2 result;
                    result.b = -5;
                    return result;
                }, null),
                // else-step
                new WorkstepBase<T1, T2>(delegate
                    {
                    T2 result;
                    result.b = +5;
                    return result;
                }, null));
            w.Initialise(_workContext);
            {
                var t1 = new T1 {a = 0};
                T2 t2 = w.Execute(t1).Result;
                Assert.AreEqual(-5, t2.b);
            }
            {
                var t1 = new T1 {a = 1};
                T2 t2 = w.Execute(t1).Result;
                Assert.AreEqual(5, t2.b);
            }
        }

        [TestMethod]
        public void TestWorkstepCompare()
        {
            // defines and executes a comparison workstep
            var w = new WorkstepCompare<T1, T2>(
                // compare method
                delegate(T1 input)
                    {
                        if (input.a == 0)
                            return 0;
                        if (input.a < 0)
                            return -1;
                        return 1;
                    },
                // less-than-step
                new WorkstepBase<T1, T2>(delegate(T1 input)
                {
                    T2 result;
                    result.b = input.a * -4;
                    return result;
                }, null),
                // equal-to-step
                new WorkstepBase<T1, T2>(delegate
                    {
                    T2 result;
                    result.b = 0;
                    return result;
                }, null),
                // greater-than-step
                new WorkstepBase<T1, T2>(delegate(T1 input)
                {
                    T2 result;
                    result.b = input.a * 5;
                    return result;
                }, null));
            w.Initialise(_workContext);
            {
                var t1 = new T1 {a = -3};
                T2 t2 = w.Execute(t1).Result;
                Assert.AreEqual(12, t2.b);
            }
            {
                var t1 = new T1 {a = 0};
                T2 t2 = w.Execute(t1).Result;
                Assert.AreEqual(0, t2.b);
            }
            {
                var t1 = new T1 {a = 4};
                T2 t2 = w.Execute(t1).Result;
                Assert.AreEqual(20, t2.b);
            }
        }

        [TestMethod]
        public void TestWorkstepBranchingOverride()
        {
            // defines and executes a branching workflow
            var w = new BranchingStep(10);
            w.Initialise(_workContext);
            var t1 = new T1 {a = 5};
            T2 t2 = w.Execute(t1).Result;
            Assert.AreEqual(25, t2.b);
        }

        [TestMethod]
        public void TestWorkstepParallelDelegate()
        {
            // defines and executes a distributed workflow
            var w = new WorkstepBasePar<P1, P2, P8, P9>(
                // splitter-method
                delegate(P1 input)
                {
                    var result = new List<P2>();
                    int lower = input.from;
                    int upper = input.from + input.slice - 1;
                    while (upper < input.to)
                    {
                        var p2 = new P2 {p = lower, q = upper};
                        result.Add(p2);
                        lower = upper + 1;
                        upper += input.slice;
                    }
                    var pLast = new P2 {p = lower, q = input.to};
                    result.Add(pLast);
                    return result.ToArray();
                },
                // parallel-worker-method
                delegate(P2 input)
                {
                    P8 result;
                    result.subtotal = 0;
                    for (int i = input.p; i <= input.q; i++)
                        result.subtotal += i;
                    Thread.Sleep(500);
                    return result;
                },
                // combiner-method
                delegate(P8[] outputs)
                {
                    var result = new P9 {total = 0};
                    foreach (var subOutput in outputs)
                        result.total += subOutput.subtotal;
                    return result;
                });
            w.Initialise(_workContext);
            var p1 = new P1 {@from = 0, to = 2000, slice = 100};
            // 20 parallel slices
            P9 p9 = w.Execute(p1).Result;
            Assert.AreEqual(2001000, p9.total);
        }

        [TestMethod]
        public void TestWorkstepParallelOverride()
        {
            // defines and executes a distributed workflow
            var w = new ParallelStep();
            w.Initialise(_workContext);
            var p1 = new P1 {@from = 0, to = 2000, slice = 100};
            // 20 parallel slices
            P9 p9 = w.Execute(p1).Result;
            Assert.AreEqual(2001000, p9.total);
        }

        [TestMethod]
        public void TestWorkstepLoopingDelegate()
        {
            // defines and executes a cyclic (looping) workflow
            var w = new WorkstepBaseLoop<LoopInput, LoopData, LoopResult>(
                // initialiser
                delegate(LoopInput input)
                {
                    LoopData loopData;
                    loopData.index = input.begin;
                    loopData.end = input.end;
                    loopData.total = 0;
                    return loopData;
                },
                // condition
                loopData => (loopData.index <= loopData.end),
                // loop body
                delegate(LoopData loopData) { loopData.total += loopData.index; return loopData; },
                // loop iterator
                delegate(ref LoopData loopData) { loopData.index++; },
                // finaliser
                delegate(LoopData loopData) { LoopResult temp; temp.total = loopData.total; return temp; }
            );
            w.Initialise(_workContext);
            var inp = new LoopInput {begin = 0, end = 2000};
            // loops until condition is true
            LoopResult outp = w.Execute(inp).Result;
            Assert.AreEqual(2001000, outp.total);
        }

        [TestMethod]
        public void TestWorkstepLoopingOverride()
        {
            // defines and executes a cyclic (looping) workflow
            var w = new LoopingStep();
            w.Initialise(_workContext);
            var inp = new LoopInput {begin = 0, end = 2000};
            // loops until condition is true
            LoopResult result = w.Execute(inp).Result;
            Assert.AreEqual(2001000, result.total);
        }

        [TestMethod]
        public void TestWorkstepAsyncExecution()
        {
            // defines and executes a workstep synchronously and asynchronously
            var s1A = new SequentialSubstep1();
            s1A.Initialise(_workContext);

            // synchronous execution
            var t1 = new T1 {a = 31};
            T2 t2A = s1A.Execute(t1).Result;
            Assert.AreEqual(31, t2A.b);

            // asynchronous execution
            // note: there are 3 ways to complete an async operation
            // 1. wait method
            t1.a = 42;
            AsyncResult<WorkflowOutput<T2>> ar1 = s1A.BeginExecute(t1, null);
            T2 t2B1 = ar1.EndInvoke().Result;
            Assert.AreEqual(42, t2B1.b);

            // 2. poll method
            t1.a = 53;
            AsyncResult<WorkflowOutput<T2>> ar2 = s1A.BeginExecute(t1, null);
            while (!ar2.IsCompleted)
                Thread.Sleep(100);
            T2 t2B2 = ar2.EndInvoke().Result;
            Assert.AreEqual(53, t2B2.b);

            // 3. callback method
            t1.a = 64;
            bool ar3Callback = false;
            T2 t2B3; t2B3.b = 0;
            s1A.BeginExecute(
                t1,
                delegate(IAsyncResult ar)
                    {
                        ar3Callback = true;
                        var ar3Temp = (AsyncResult<WorkflowOutput<T2>>)ar;
                        t2B3 = ar3Temp.EndInvoke().Result;
                    });
            while (!ar3Callback)
                Thread.Sleep(100);
            Assert.AreEqual(64, t2B3.b);
        }

        [TestMethod]
        public void TestWorkstepErrorHandlingSimple()
        {
            //IWorkContext context = new WorkContext(null, TestEnvironment.Client);
            _workContext.Logger.LogDebug("starting...");
            var w = new WorkstepBase<int, string>(
                // step body
                delegate(int input)
                    {
                        if (input < 0)
                        throw new ArgumentOutOfRangeException("input", input, "Must be > 0");
                        return input.ToString(CultureInfo.InvariantCulture);
                    },
                // exception handler
                (input, e) => e.GetType().Name);
            w.Initialise(_workContext);
            string noFailResult = w.Execute(1).Result;
            Assert.AreEqual("1", noFailResult);
            string failedResult = w.Execute(-1).Result;
            Assert.AreEqual("ArgumentOutOfRangeException", failedResult);
            _workContext.Logger.LogDebug("complete.");
        }

        //private string GetStatusString(IWorkstep step)
        //{
        //    WorkStatus status = step.Status;
        //    return (status == WorkStatus.Running) ? step.Progress : status.ToString();
        //}
        //private void LogProgress1(string stage, IWorkContext context, IWorkstep s0)
        //{
        //    context.Logger.LogDebug("[{0}] {1}", stage, GetStatusString(s0));
        //}
        //private void LogProgress3(string stage, IWorkContext context, IWorkstep s0, IWorkstep s1, IWorkstep s2)
        //{
        //    context.Logger.LogDebug("[{0}] Main:{1} Sub1:{2} Sub2:{3}", stage,
        //        GetStatusString(s0), GetStatusString(s1), GetStatusString(s2));
        //}

        [TestMethod]
        public void TestWorkflowProgressParallel()
        {
            //IWorkContext context = new WorkContext(null, TestEnvironment.Client);
            // defines and executes a distributed workflow
            var w = new ParallelStep();
            w.Initialise(_workContext);
            var p1 = new P1 {@from = 0, to = 2000, slice = 100};
            // 20 parallel slices
            // start and monitor
            _workContext.Logger.LogDebug("Starting...");
            _workContext.Logger.LogDebug("[{0}] {1}", "I", w.Progress);
            //P9 p9 = w.Execute(p1, TimeSpan.FromSeconds(10));
            AsyncResult<WorkflowOutput<P9>> ar = w.BeginExecute(p1, null);
            int loop = 0;
            while (!ar.IsCompleted)
            {
                Thread.Sleep(100);
                _workContext.Logger.LogDebug("[{0}] {1}", loop, w.Progress);
                loop++;
            }
            P9 p9 = ar.EndInvoke().Result;
            _workContext.Logger.LogDebug("[{0}] {1}", "Z", w.Progress);
            Assert.AreEqual(2001000, p9.total);
            _workContext.Logger.LogDebug("Complete.");

        }

        [TestMethod]
        public void TestWorkflowProgressLoopingSimple()
        {
            //IWorkContext context = new WorkContext(null, TestEnvironment.Client);
            // defines and executes a cyclic (looping) workflow
            var w = new LoopingStep();
            w.Initialise(_workContext);
            var inp = new LoopInput {begin = 0, end = 2000};
            // loops until condition is true
            // start and monitor
            _workContext.Logger.LogDebug("Starting...");
            _workContext.Logger.LogDebug("[{0}] {1}", "I", w.Progress);
            //LoopResult result = w.Execute(inp, TimeSpan.FromSeconds(10));
            AsyncResult<WorkflowOutput<LoopResult>> ar = w.BeginExecute(inp, null);
            int loop = 0;
            while (!ar.IsCompleted)
            {
                Thread.Sleep(1000);
                _workContext.Logger.LogDebug("[{0}] {1}", loop, w.Progress);
                loop++;
            }
            LoopResult result = ar.EndInvoke().Result;
            _workContext.Logger.LogDebug("[{0}] {1}", "Z", w.Progress);
            Assert.AreEqual(2001000, result.total);
            _workContext.Logger.LogDebug("Complete.");

        }

    }
}
