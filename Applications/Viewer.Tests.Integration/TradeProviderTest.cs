using Orion.UnitTestEnv;
using Orion.WebViewer.Trade.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Viewer.Tests.Integration
{
    /// <summary>
    /// This is a test class for TradeProviderTest and is intended
    /// to contain all TradeProviderTest Unit Tests
    /// </summary>
    [TestClass]
    public class TradeProviderTest
    {
        private static TradeProvider TradeProvider { get; set; }
        private static UnitTestEnvironment UTE { get; set; }

        #region Test Initialize and Cleanup

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            UTE = new UnitTestEnvironment();
            TradeProvider = new TradeProvider(UTE.Cache);
        }

        [ClassCleanup]
        public static void Teardown()
        {
            UTE.Dispose();
        }

        #endregion

        //public TradeProviderTest()
        //{
        //    TradeProvider = new TradeProvider();
        //}

        /// <summary>
        /// A test for GetTrades
        /// </summary>
        [TestMethod]
        public void GetTradesTest()
        {
            //var startDate = DateTime.MinValue;
            //var endDate = new DateTime(2012, 8, 30);
            const string productType = "All";
            const string tradeId = "t0001";
            const int maximumRows = 30;
            //const int startRowIndex = 0;
            IEnumerable<Trade> actual
                = TradeProvider.GetTrades(null, null, productType, tradeId, maximumRows);
            Assert.AreEqual(1, actual.Count());
        }

        /// <summary>
        /// A test for GetTrades
        /// </summary>
        [TestMethod]
        public void GetSomeTradesTest()
        {
            //var startDate = DateTime.MinValue; //new DateTime(2012, 8, 30);
            //var endDate = DateTime.MaxValue; //new DateTime(2012, 8, 30);
            const string productType = "BulletPayment";
            const string tradeId = "";
            const int maximumRows = 30;
            //const int startRowIndex = 0;
            IEnumerable<Trade> actual
                = TradeProvider.GetTrades(null, null, productType, tradeId, maximumRows);
            Assert.AreEqual(1, actual.Count());
        }

        /// <summary>
        /// A test for GetTrades
        /// </summary>
        [TestMethod]
        public void GetTradesCountTest()
        {
            var startDate = DateTime.MinValue;//new DateTime(1998, 1, 1);
            var endDate = DateTime.MaxValue;//new DateTime(2012, 8, 30);
            const string productType = "All";
            const string tradeId = "";
            const int maximumRows = 30;
            //const int startRowIndex = 0;
            int actual
                = TradeProvider.GetTradesCount(startDate, endDate, productType, tradeId, maximumRows);
            Assert.AreNotEqual(1, actual);
        }

        /// <summary>
        /// A test for GetTrade
        /// </summary>
        [TestMethod]
        public void GetTradeTest()
        {
            // first load some trades so that we can get an id
            var startDate = DateTime.MinValue;//new DateTime(1998, 1, 1);
            var endDate = DateTime.MaxValue;//new DateTime(2012, 8, 30);
            const string productType = "All";
            const int maximumRows = 30;
            //const int startRowIndex = 0;
            IEnumerable<Trade> trades
                = TradeProvider.GetTrades(startDate, endDate, productType, string.Empty, maximumRows);
            string tradeId = trades.First().TradeId;
            // Directly
            Trade trade = TradeProvider.GetTrade(tradeId);
            Assert.AreEqual(tradeId, trade.TradeId);
            // Indirectly
            trades = TradeProvider.GetTrades(DateTime.MinValue, DateTime.MaxValue, string.Empty, tradeId, 30);
            Assert.AreEqual(tradeId, trades.Single().TradeId);
        }
    }
}