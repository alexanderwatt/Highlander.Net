using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.EquityCollarPricer.Tests
{
    [TestClass]
    public class TransactionDetailTest
    {
        private const string CStockId = "BHP";
        static DateTime _cTradeDate = DateTime.Today;
        private const Double CSpot = 2000;
        private const PayStyleType CPayStyle = PayStyleType.American;

        /// <summary>
        /// Creates the basic.
        /// </summary>
        [TestMethod]
        public void CreateTransaction()
        {
            TransactionDetail transaction = Create();
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns></returns>
        static public TransactionDetail Create()
        {
            Strike strike =  StrikeTest.CreateStrike(OptionType.Call, 25);
            return CreateTransaction(CStockId, _cTradeDate, _cTradeDate.AddDays(10), CSpot, CPayStyle, strike);
        }

        /// <summary>
        /// Creates the transaction.
        /// </summary>
        /// <param name="stockId">The stock id.</param>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="expiryDate">The expiry date.</param>
        /// <param name="spot">The spot.</param>
        /// <param name="payStyle">The pay style.</param>
        /// <param name="strike">The strike.</param>
        /// <returns></returns>
        static public TransactionDetail CreateTransaction(string stockId, DateTime tradeDate, DateTime expiryDate, Double spot, PayStyleType payStyle, Strike strike)
        {
            var transaction = new TransactionDetail(stockId)
                {
                    TradeDate = tradeDate,
                    ExpiryDate = expiryDate,
                    CurrentSpot = spot,
                    PayStyle = payStyle
                };
            transaction.SetStrike(strike);

            Assert.AreEqual(transaction.StockId, stockId);
            Assert.AreEqual(transaction.TradeDate, tradeDate);
            //Assert.AreEqual(transaction.ExpiryDate, Is.GreaterThanOrEqualTo(expiryDate);
            Assert.AreEqual(transaction.PayStyle, payStyle);
            Assert.AreEqual(transaction.CurrentSpot, spot);
            //Assert.AreEqual(transaction.Strike, !Is.Null);
            return transaction;
        }
    }
}
