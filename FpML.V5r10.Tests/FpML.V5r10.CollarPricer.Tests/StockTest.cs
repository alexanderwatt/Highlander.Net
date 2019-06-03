using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.EquityCollarPricer.Tests
{
    [TestClass]
    public class StockTest
    {
        private const string CStockId = "122";
        private const string CName = "BHP";

        /// <summary>
        /// Creates this instance.
        /// </summary>
        [TestMethod]
        public void Create()
        {

            var curvatures = new[] { WingCurvatureTest.Create() };
            TransactionDetail transaction = TransactionDetailTest.Create();
            DividendList divList = DividendListTest.CreateDividendList();
            Stock stock = CreateStock(CStockId, CName, divList, curvatures, transaction);
        }

        /// <summary>
        /// Creates the stock.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="name">The name.</param>
        /// <param name="divList">The div list.</param>
        /// <param name="curvature">The curvature.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        static public Stock CreateStock(string id, string name, DividendList divList, WingCurvature[] curvature, TransactionDetail transaction)
        {
            var stock = new Stock(id, name, divList, curvature) {Transaction = transaction};

            Assert.AreEqual(stock.Id, id);
            Assert.AreEqual(stock.Name, name);
            Assert.AreEqual(stock.Dividends.Count, divList.Count);
            Assert.AreEqual(stock.WingCurvature.Length, curvature.Length);
            //Assert.AreEqual(stock.Transaction, !IsNull);

            return stock;
        }
    }
}
