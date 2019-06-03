using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Maths.Collections;
using Orion.Analytics.Rates;
using Math = System.Math;

namespace Orion.Analytics.Tests.Rates
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class RatesTests 
    {
        private static readonly double[] Rates = { 0.071d, 0.072d, 0.071d, 0.072d, 0.073d, 0.075d, 0.074d, 0.076d, 0.075d };
        private static readonly double[] Times = {0.01d, 0.02d, 0.5d, 0.72d, 1.0d, 2.0d, 3.0d, 5.0d, 10.0d};   

        private readonly decimal[] _compounding = { 0.0m, (decimal)1 / 365, (decimal)1 / 52, (decimal)1 / 12, (decimal)1 / 6, (decimal)1 / 4, (decimal)1 / 2, 1 };

        /// <summary>
        /// Testing the zero rates.
        /// </summary>
        [TestMethod]
        public void DiscountFactorToZeroRateTest()
        {
            var result = new double[Rates.Length];
            for (var i = 0; i < Rates.Length; i++)
            {
                result[i] = Rates[i] * Times[i];
            }
            var vectorRatesTimes = new DoubleVector(result);
            vectorRatesTimes.Multiply(-1.0d);
            vectorRatesTimes.Apply(Math.Exp);
            var index = 0;
            foreach (var pointvalue in vectorRatesTimes)
            {
                var yearFraction = (decimal)Times[index];
                foreach (var comp in _compounding)
                {
                    var result2 = RateAnalytics.DiscountFactorToZeroRate(1.0m, (decimal)pointvalue, yearFraction, comp);
                    Debug.WriteLine($"Rate : {result2} Time: {yearFraction}");
                }
                index++;
            }
        }

        /// <summary>
        /// Testing the discount factors.
        /// </summary>
        [TestMethod]
        public void ZeroRateToDiscountFactor0Test()
        {
            var index = 0;
            foreach (var pointvalue in Rates)
            {
                var yearFraction = (decimal)Times[index];
                foreach (var comp in _compounding)
                {
                    var result2 = RateAnalytics.ZeroRateToDiscountFactor((decimal)pointvalue, yearFraction, comp);
                    Debug.WriteLine($"rate : {result2} Time: {yearFraction}");
                }
                index++;
            }
        }

        /// <summary>
        /// Testing the discount factors.
        /// </summary>
        [TestMethod]
        public void TestSelfConsistency()
        {
            var result = new double[Rates.Length];
            for (var i = 0; i < Rates.Length; i++)
            {
                result[i] = Rates[i] * Times[i];
            }
            var vectorRatesTimes = new DoubleVector(result);
            vectorRatesTimes.Multiply(-1.0d);
            vectorRatesTimes.Apply(Math.Exp);

            var index = 0;
            foreach (var pointvalue in vectorRatesTimes)
            {
                var yearFraction = (decimal)Times[index];
                foreach (var comp in _compounding)
                {
                    var result2 = RateAnalytics.DiscountFactorToZeroRate(1.0m, (decimal)pointvalue, yearFraction, comp);
                    var result1 = RateAnalytics.ZeroRateToDiscountFactor(result2, yearFraction, comp);

                    Debug.WriteLine($"DFIn : {pointvalue} DFOut: {(double) result1}");
                    Assert.AreEqual((double)result1, pointvalue, .000000001);
                }
                index++;
            }
        }

        /// <summary>
        /// Testing the discount factors.
        /// </summary>
        [TestMethod]
        public void ZeroRateToDiscountFactor1Test()
        {
            decimal result = RateAnalytics.ZeroRateToDiscountFactor(0.02m, 0.5m, 0.5m);
            Assert.AreEqual(0.99009900, (double)result, 0.00000001);

            result = RateAnalytics.ZeroRateToDiscountFactor(0.02m, 0.5m, 0);
            Assert.AreEqual(0.99004983, (double)result, 0.00000001);

            result = RateAnalytics.ZeroRateToDiscountFactor(0.02m, 0m, 0.5m);
            Assert.AreEqual(1, result);
        }

        /// <summary>
        /// Testing the discount factors.
        /// </summary>
        [TestMethod]
        public void ZeroRateToDiscountFactor2Test()
        {
            double result = RateAnalytics.ZeroRateToDiscountFactor(0.02, 0.5, "SemiAnnual");
            Assert.AreEqual(0.99009900, result, 0.00000001);

            result = RateAnalytics.ZeroRateToDiscountFactor(0.02, 0.5, "Continuous");
            Assert.AreEqual(0.99004983, result, 0.00000001);

        }

        [TestMethod]
        public void TerminalWealthFromZeroRateTest()
        {
            const decimal rate = 0.05m;
            const decimal yearFraction = 1.5m;
            decimal actual = RateAnalytics.TerminalWealthFromZeroRate(rate, yearFraction, "Quarterly");

            Assert.AreEqual(0.98153875, (double)actual, 0.00000001);
        }

        [TestMethod]
        public void DiscountFactorToZeroRate2Test()
        {
            double result = RateAnalytics.DiscountFactorToZeroRate(0.9, 0.5, "Quarterly");
            Assert.AreEqual(0.21637021, result, 0.00000001);

            result = RateAnalytics.DiscountFactorToZeroRate(0.9, 0.5, "Continuous");
            Assert.AreEqual(0.21072103, result, 0.00000001);

            result = RateAnalytics.DiscountFactorToZeroRate(0.9, 0.5, "Weekly");
            Assert.AreEqual(0.21114856, result, 0.00000001);

            result = RateAnalytics.DiscountFactorToZeroRate(0.9, 0.5, "Daily");
            Assert.AreEqual(0.21078186, result, 0.00000001);

            result = RateAnalytics.DiscountFactorToZeroRate(0.9, 0.5, "Annual");
            Assert.AreEqual(0.23456790, result, 0.00000001);
        }

        [TestMethod]
        public void DiscountFactorToZeroRate3Test()
        {
            double result = RateAnalytics.DiscountFactorToZeroRate(0.9, 0.5, "Quarterly");
            Assert.AreEqual(0.21637021, result, 0.00000001);

            result = RateAnalytics.DiscountFactorToZeroRate(0.9, 0.5, "SemiAnnual");
            Assert.AreEqual(0.22222222, result, 0.00000001);
        }

        //[TestMethod]
        //public void DiscountFactorToZeroRate4Test()
        //{
        //    DateTime baseDate = new DateTime(2010, 01, 01);
        //    DateTime targetDate = new DateTime(2010, 07, 01);
        //    double result = RateAnalytics.DiscountFactorToZeroRate(0.9, baseDate, targetDate, "Quarterly");
        //    Assert.AreEqual(0.21821138, result, 0.00000001);

        //    result = RateAnalytics.DiscountFactorToZeroRate(0.9, baseDate, targetDate, "Monthly");
        //    Assert.AreEqual(0.21435942, result, 0.00000001);
        //}

        [TestMethod]
        public void DiscountFactorToZeroRate5Test()
        {
            double result = (double)RateAnalytics.DiscountFactorToZeroRate(0.9m, 0.8m, 0.5m, 0.25m);
            Assert.AreEqual(0.24264068, result, 0.00000001);

            result = (double)RateAnalytics.DiscountFactorToZeroRate(0.9m, 0.8m, 0.5m, 0);
            Assert.AreEqual(0.23556607, result, 0.00000001);
        }
    }
}