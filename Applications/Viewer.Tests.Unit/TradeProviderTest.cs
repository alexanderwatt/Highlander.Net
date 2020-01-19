using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.WebViewer.Trade.Business;
using nab.QDS.FpML.Codes;
using Trade = nab.QDS.FpML.V47.Trade;

namespace Viewer.Tests.Unit
{
    /// <summary>
    ///This is a test class for TradeProviderTest and is intended
    ///to contain all TradeProviderTest Unit Tests
    ///</summary>
    [TestClass]
    public class TradeProviderTest
    {
        readonly TradeProvider _tradeProvider;

        public TradeProviderTest()
        {
            ICoreClient store = new PrivateCore(new TraceLogger(false));

            #region Add fra
            string tradeId = "Trade1";
            var trade1 = new Trade();
            var properties
                = new Dictionary<string, object>
                      {
                          {TradeProp.TradeDate, DateTime.Today},
                          {TradeProp.ProductType, "fra"},
                          {TradeProp.TradeSource, "Calypso"},
                          {TradeProp.TradeId, tradeId},
                          {TradeProp.OriginatingPartyId, "MyOriginatingPartyId"},
                          {"OriginatingPartyName", "MyOriginatingPartyName"},
                          {"CounterpartyId", "MyCounterpartyId"},
                          {"CounterpartyName", "MyCounterpartyName"},
                          {TradeProp.TradingBookId, "MyTradingBookId"},
                          {"TradingBookName", "MyTradingBookName"},
                      };
            store.SaveObject(trade1, tradeId, new NamedValueSet(properties), TimeSpan.MaxValue);
            #endregion

            #region Add swap
            tradeId = "Trade2";
            var trade2 = new Trade();
            properties
                = new Dictionary<string, object>
                      {
                          {TradeProp.TradeDate, DateTime.Today},
                          {TradeProp.ProductType, "swap"},
                          {TradeProp.TradeSource, "Calypso"},
                          {TradeProp.TradeId, tradeId},
                          {TradeProp.OriginatingPartyId, "MyOriginatingPartyId"},
                          {"OriginatingPartyName", "MyOriginatingPartyName"},
                          {"CounterpartyId", "MyCounterpartyId"},
                          {"CounterpartyName", "MyCounterpartyName"},
                          {TradeProp.TradingBookId, "MyTradingBookId"},
                          {"TradingBookName", "MyTradingBookName"},
                      };
            store.SaveObject(trade2, tradeId, new NamedValueSet(properties), TimeSpan.MaxValue);
            #endregion

            _tradeProvider = new TradeProvider(store);
        }

        /// <summary>
        ///A test for GetTrades
        ///</summary>
        [TestMethod]
        public void GetTradesTest()
        {
            DateTime startDate = DateTime.Today.AddMonths(-1);
            DateTime endDate = DateTime.Today;
            const string productType = "All";
            const string tradeId = "";
            const int maximumRows = 30;
            //const int startRowIndex = 0;

            var actual = (IEnumerable<Trade>) _tradeProvider.GetTrades(startDate, endDate, productType, tradeId,
                                                                                                maximumRows);

            Assert.AreEqual(2, actual.Count());
        }

        /// <summary>
        ///A test for GetTrades
        ///</summary>
        [TestMethod]
        public void GetSomeTradesTest()
        {
            DateTime startDate = DateTime.Today.AddMonths(-1);
            DateTime endDate = DateTime.Today;
            const string productType = "fra";
            const string tradeId = "";
            const int maximumRows = 30;
            //const int startRowIndex = 0;

            var actual = _tradeProvider.GetTrades(startDate, endDate, productType, tradeId,
                                                                maximumRows);

            Assert.AreEqual(1, actual.Count());
        }

        /// <summary>
        ///A test for GetTrades
        ///</summary>
        [TestMethod]
        public void GetTradesCountTest()
        {
            DateTime startDate = DateTime.Today.AddMonths(-1);
            DateTime endDate = DateTime.Today;
            string productType = string.Empty;
            string tradeId = string.Empty;
            const int maximumRows = 30;
            //const int startRowIndex = 0;

            int actual = _tradeProvider.GetTradesCount(startDate, endDate, productType, tradeId,
                                                      maximumRows);

            Assert.AreEqual(2, actual);
        }

        /// <summary>
        ///A test for GetTrade
        ///</summary>
        [TestMethod]
        public void GetTradeTest()
        {
            // first load some trades so that we can get an id
            DateTime startDate = DateTime.Today.AddMonths(-1);
            DateTime endDate = DateTime.Today;
            string productType = string.Empty;
            const int maximumRows = 30;
            //const int startRowIndex = 0;

            var trades = _tradeProvider.GetTrades(startDate, endDate, productType, 
                                                                "", maximumRows);

            string tradeId = trades.First().TradeId;

            // Directly
            var trade = _tradeProvider.GetTrade(tradeId);
            Assert.AreEqual(tradeId, trade.TradeId);

            // Indirectly
            trades = _tradeProvider.GetTrades(DateTime.MinValue, DateTime.MaxValue, "", tradeId, 30);
            Assert.AreEqual(tradeId, trades.Single().TradeId);
        }

        /// <summary>
        ///A test for GetTrade
        ///</summary>
        [TestMethod]
        public void DontGetTradeTest()
        {
            // Directly
            var trade = _tradeProvider.GetTrade("");
            Assert.IsNull(trade);
        }

        /// <summary>
        ///A test for SupportedProductTypes
        ///</summary>
        [TestMethod]
        public void SupportedProductTypesTest()
        {
            IList<string> structures = TradeProvider.SupportedProductTypes;
            Assert.AreNotEqual(0, structures.Count);
        }
    }
}