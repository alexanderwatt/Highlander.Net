using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.EquityCollarPricer.Tests
{
    [TestClass]
    public class DividendTest
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
            DividendTest.CreateDividend(cExDivDate, cExDivDate.AddDays(10), cPayAmt, cCcy);
        }

        static public Dividend CreateDividend(DateTime exDivDate, DateTime paymentDate, Double paymentAmt, String currencyCode)
        {
            Dividend dividend = new Dividend(exDivDate, paymentDate, paymentAmt, currencyCode);
            Assert.AreEqual(dividend.ExDivDate, exDivDate);
            Assert.AreEqual(dividend.PaymentDate, paymentDate);
            Assert.AreEqual(dividend.PaymentAmountInCents, paymentAmt);
            Assert.AreEqual(dividend.CurrencyCode, currencyCode);
            return dividend;
        }
    }
}
