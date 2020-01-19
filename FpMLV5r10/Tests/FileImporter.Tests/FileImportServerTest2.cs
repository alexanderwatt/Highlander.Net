using System;
using System.Collections.Generic;
using System.Threading;
using Core.Common;
using Core.Server;
using Core.V34;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using Orion.V5r3.Configuration;
using Server.FileImporter;

namespace FileImporter.Tests
{
    
    
    /// <summary>
    ///This is a test class for FileImportServerTest and is intended
    ///to contain all FileImportServerTest Unit Tests
    ///</summary>
    [TestClass]
    public class FileImportServerTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for FileImportServer Constructor
        ///</summary>
        [TestMethod]
        public void TestFileImportServer()
        {
            // start the server, connect a client1, and shutdown
            using (ILogger logger = new TraceLogger(true))
            {
                const EnvId env = EnvId.Utt_UnitTest;
                var logRef = Reference<ILogger>.Create(logger);
                var random = new Random(Environment.TickCount);
                int port = random.Next(8000, 8099);
                using (var server = new CoreServer(logRef, env.ToString(), NodeType.Router, port, WcfConst.NetTcp))
                {
                    // start server
                    server.Start();
                    // connect client
                    using (ICoreClient client = new CoreClientFactory(logRef).SetEnv(env.ToString()).Create())
                    {
                        // create test import rule
                        var rule = new FileImportRule
                            {
                            RuleName = "UnitTest",
                            SourceLocation = @"C:\windows\system32",
                            TargetLocation = @"c:\temp",
                            CopyFilePatterns = "notep*.exe"
                        };
                        var name = rule.RuleName;
                        // start the file import server
                        //DateTimeOffset waitCompleted;
                        var target = new FileImportServer {Client = Reference<ICoreClient>.Create(client)};
                        client.SaveObject(rule, "UnitTest", null, true, TimeSpan.MaxValue);
                        using (target)//logger, env
                        {
                            target.Start();
                            Thread.Sleep(TimeSpan.FromSeconds(5));
                            //waitCompleted = DateTimeOffset.Now;
                            target.Stop();
                        }
                        {                       
                            var results1 = client.LoadItem<FileImportRule>(name);
                            Assert.IsNotNull(results1);
                            List<ImportRuleResult> results = client.LoadObjects<ImportRuleResult>(Expr.ALL);
                            Assert.AreEqual(1, results.Count);
                            ImportRuleResult result = results[0];
                            Assert.AreEqual("UnitTest", result.RuleName);
                            Assert.AreEqual("Completed", result.ImportResult);
                            Assert.AreEqual(1, result.FileNames.Length);
                            Assert.AreEqual("notepad.exe", result.FileNames[0].ToLower());
                        }
                        {
                            List<ImportFileResult> results = client.LoadObjects<ImportFileResult>(Expr.ALL);
                            Assert.AreEqual(1, results.Count);
                            ImportFileResult result = results[0];
                            Assert.AreEqual("UnitTest", result.RuleName);
                            Assert.AreEqual("Completed", result.ImportResult);
                            Assert.AreEqual("notepad.exe", result.FileName.ToLower());
                        }
                    }
                    // explicit shutdown
                    // - not necessary in a "using" block but run it anyway
                    server.Stop();
                }
            }
        }
    }
}
