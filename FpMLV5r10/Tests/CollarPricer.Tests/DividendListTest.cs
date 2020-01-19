using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.EquityCollarPricer.Tests
{
    [TestClass]
    public class DividendListTest
    {
        static DateTime cExDivDate = DateTime.Today;
        static Double cPayAmt = 300;
        static String cCcy = "AUD";

        /// <summary>
        /// Creates this instance.
        /// </summary>
        [TestMethod]
        public void Create()
        {
            DividendListTest.CreateDividendList();
        }

        static public DividendList CreateDividendList()
        {
            Dividend div1 = DividendTest.CreateDividend(cExDivDate, cExDivDate.AddDays(10), cPayAmt, cCcy);
            Dividend div2 = DividendTest.CreateDividend(cExDivDate.AddDays(10), cExDivDate.AddDays(20), cPayAmt + 10, cCcy);
            List<Dividend> dividends = new List<Dividend>();
            dividends.Add(div1);
            dividends.Add(div2);
            DividendList dividendList = DividendListTest.CreateDividendList(dividends);
            return dividendList;
        }

        /// <summary>
        /// Creates the dividend list.
        /// </summary>
        /// <param name="exDivDate">The ex div date.</param>
        /// <param name="paymentDate">The payment date.</param>
        /// <param name="paymentAmt">The payment amt.</param>
        /// <param name="currencyCode">The currency code.</param>
        /// <returns></returns>
        static public DividendList CreateDividendList(List<Dividend> dividends)
        {
            DividendList dividendList = new DividendList();
            foreach (Dividend div in dividends)
            {
                dividendList.Add(div);
            }
            Assert.AreEqual(dividendList.Dividends.Length, dividends.Count);
            Assert.AreEqual(dividendList.ExDivDates.Length, dividends.Count);
            Assert.AreEqual(dividendList.PaymentAmountsInCents.Length, dividends.Count);
            Assert.AreEqual(dividendList.PaymentDates.Length, dividends.Count);
            return dividendList;
        }
    }
}
