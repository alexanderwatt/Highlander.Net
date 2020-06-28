using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.EquityCollarPricer.Tests
{
    [TestClass]
    public class ZeroRateCurveTest
    {
        static DateTime[] cTenorDates = { DateTime.Today, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1) };
        static Double[] cRates = { 10, 13, 12 };
        static DateTime cCurveDate = DateTime.Today.AddDays(20);

        [TestMethod]
        public void Create()
        {
            List<DateTime> tenorDates = new List<DateTime>(cTenorDates);
            List<Double> rates = new List<Double>(cRates);
            ZeroRateCurveTest.CreateCurve(cCurveDate, tenorDates, rates);
        }

        /// <summary>
        /// Creates the strike.
        /// </summary>
        /// <param name="style">The style.</param>
        /// <param name="price">The price.</param>
        /// <returns></returns>
        static public ZeroAUDCurve CreateCurve(DateTime curveDate, List<DateTime> tenorDates, List<Double> rates)
        {
            ZeroAUDCurve curve = new ZeroAUDCurve(curveDate, tenorDates, rates);
            Assert.AreEqual(curve.CurveDate, curveDate);
            Assert.AreEqual(curve.Tenors.Length, tenorDates.Count);
            Assert.AreEqual(curve.Rates.Length, rates.Count);
            return curve;
        }


    }
}
