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
using System.Runtime.Serialization;
using Highlander.Utilities.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Workflow.Tests
{
    /// <summary>
    /// Summary description for WorkgridTests
    /// </summary>
    [TestClass]
    public class WorkgridTests
    {
        public WorkgridTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [DataContract]
        public class GridData1
        {
            [DataMember]
            public string S;
        }

        [DataContract]
        public class GridData2
        {
            [DataMember]
            public double D;
        }

        public class GridSeqStep : WorkstepBaseSeq<GridData1, GridData2>
        {
            public GridSeqStep()
            {
                AddSubstep(new GridSeqSubstep1());
                AddSubstep(new GridSeqSubstep2());
                AddSubstep(new GridSeqSubstep3());
            }
        }

        public class GridSeqSubstep1 : WorkstepBase<GridData1, GridData2>
        {
            protected override GridData2 OnExecute(GridData1 input)
            {
                return new GridData2 { D = Convert.ToDouble(input.S) };
            }
        }
        public class GridSeqSubstep2 : WorkstepBase<GridData2, GridData2>
        {
            protected override GridData2 OnExecute(GridData2 input)
            {
                double result = (1.0 / input.D);
                if (double.IsInfinity(result))
                    throw new DivideByZeroException();
                return new GridData2 { D = result };
            }
        }
        public class GridSeqSubstep3 : WorkstepBase<GridData2, GridData2>
        {
            protected override GridData2 OnExecute(GridData2 input)
            {
                double result = Math.Sqrt(input.D);
                if (double.IsNaN(result))
                    throw new InvalidOperationException();
                return new GridData2 { D = result };
            }
        }

        [TestMethod]
        public void TestWorkflowErrorHandling()
        {
            using (ILogger logger = new TraceLogger(true))
            {
                IWorkContext context = new WorkContext(logger, null, "UnitTest");
                const int routerPort = 9991;
                using (var router = new GridSeqStep())
                {
                    router.Initialise(context);
                    router.EnableGrid(GridLevel.Router, Guid.NewGuid(), routerPort, null);
                    using (var worker = new GridSeqStep())
                    {
                        worker.Initialise(context);
                        worker.EnableGrid(GridLevel.Worker, Guid.NewGuid(), routerPort, null);
                        // succeeds
                        {
                            WorkflowOutput<GridData2> output = worker.Execute(new GridData1 { S = "1.0" });
                            Assert.IsNotNull(output.Errors);
                            Assert.AreEqual(0, output.Errors.Length);
                            Assert.AreEqual(1.0, output.Result.D);
                        }
                        // fails in 1st substep
                        {
                            WorkflowOutput<GridData2> output = worker.Execute(new GridData1 { S = "not_a_number" });
                            Assert.IsNotNull(output.Errors);
                            Assert.AreEqual(1, output.Errors.Length);
                            Assert.AreEqual(typeof(FormatException).FullName, output.Errors[0].FullName);
                            Assert.AreEqual(default, output.Result);
                        }
                        // fails in 2nd substep
                        {
                            WorkflowOutput<GridData2> output = worker.Execute(new GridData1 { S = "0.0" });
                            Assert.IsNotNull(output.Errors);
                            Assert.AreEqual(1, output.Errors.Length);
                            Assert.AreEqual(typeof(DivideByZeroException).FullName, output.Errors[0].FullName);
                            Assert.AreEqual(default, output.Result);
                        }
                        // fails in 3rd substep
                        {
                            WorkflowOutput<GridData2> output = worker.Execute(new GridData1 { S = "-1.0" });
                            Assert.IsNotNull(output.Errors);
                            Assert.AreEqual(1, output.Errors.Length);
                            Assert.AreEqual(typeof(InvalidOperationException).FullName, output.Errors[0].FullName);
                            Assert.AreEqual(default, output.Result);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestSelfHostedGrid()
        {
            using (ILogger logger = new TraceLogger(true))
            {
                IWorkContext context = new WorkContext(logger, null, "UnitTest");
                // defines and executes a basic sequential workflow on a private 2-worker grid
                const int routerPort = 9992;
                // create nodes
                using (var router = new GridSeqStep())
                using (var worker1 = new GridSeqStep())
                using (var worker2 = new GridSeqStep())
                using (var client = new GridSeqStep())
                {
                    // start nodes
                    router.Initialise(context);
                    router.EnableGrid(GridLevel.Router, Guid.NewGuid(), routerPort, null);
                    worker1.Initialise(context);
                    worker1.EnableGrid(GridLevel.Worker, Guid.NewGuid(), routerPort, null);
                    worker2.Initialise(context);
                    worker2.EnableGrid(GridLevel.Worker, Guid.NewGuid(), routerPort, null);
                    client.Initialise(context);
                    client.EnableGrid(GridLevel.Client, Guid.NewGuid(), routerPort, null);
                    // execute
                    {
                        WorkflowOutput<GridData2> output = client.Execute(new GridData1 { S = "4.0" });
                        // should return sqrt(1/4.0) = 0.5
                        Assert.AreEqual(0.5, output.Result.D);
                        Assert.IsNotNull(output.Errors);
                        Assert.AreEqual(0, output.Errors.Length);
                    }
                    // and again
                    {
                        WorkflowOutput<GridData2> output = client.Execute(new GridData1 { S = "16.0" });
                        // should return sqrt(1/16.0) = 0.25
                        Assert.AreEqual(0.25, output.Result.D);
                        Assert.IsNotNull(output.Errors);
                        Assert.AreEqual(0, output.Errors.Length);
                    }
                }
            }
        }
    }
}

