using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.EquityCollarPricer.Tests
{
    [TestClass]
    public class StrikeTest
    {
        static OptionType cType = OptionType.Call;
        static Double cPrice = 10;

        [TestMethod]
        public void Create()
        {
            StrikeTest.CreateStrike(cType, cPrice);

        }

        /// <summary>
        /// Creates the strike.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="price">The price.</param>
        /// <returns></returns>
        static public Strike CreateStrike(OptionType style, Double price)
        {
            Strike strike = new Strike(style, price);
            Assert.AreEqual(strike.Style, style);
            Assert.AreEqual(strike.StrikePrice, price);

            return strike;
        }

    }
}
