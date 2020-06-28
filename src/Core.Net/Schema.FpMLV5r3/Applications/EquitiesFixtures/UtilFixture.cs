/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Highlander.Utilities.Exception;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Equities.Tests
{
    [TestClass]
    public class UtilFixture
    {
        private static readonly DateTime[] KnownDates = new DateTime[24];
        private static readonly double[] KnownRates = new double[24];
        private static readonly DateTime[] UnsortedDates = new DateTime[24];
        private static readonly DateTime[] UnequalLengthDates = new DateTime[25];

        #region Test Initialize and Cleanup

        [ClassInitialize]
        public static void Setup(TestContext context)
        {                  
            KnownDates[0]=new DateTime(2008,1,1);
            KnownRates[0] = 0.048969;
            for (int i = 1; i < 24; i++)
            {
                int j = i + 1;
                KnownDates[i] = KnownDates[i - 1].AddMonths(1);
                KnownRates[i] = 0.0472 + 0.0018 * j - 3.00E-5 * j * j - 1.00E-6 * j * j * j;
            }
            KnownDates.CopyTo(UnsortedDates, 0);
            UnsortedDates[12]=UnsortedDates[12].AddMonths(3); //this will cause the dates to be unsorted
            KnownDates.CopyTo(UnequalLengthDates, 1);
            UnequalLengthDates[0] = new DateTime(2007, 12, 1);
        }

        [ClassCleanup]
        public static void Teardown()
        {
            // Release resources
        }

        #endregion

        [TestMethod]
        public void FlatRates()
        {
            Assert.AreEqual(Utilities.InterpolateDates(new DateTime(2008, 01, 16), KnownDates, KnownRates), 0.049793, 0.00001, "Not Match");
            Assert.AreEqual(Utilities.InterpolateDates(new DateTime(2008, 01, 4), KnownDates, KnownRates), 0.049134, 0.00001, "Not Match");
            Assert.AreEqual(Utilities.InterpolateDates(new DateTime(2009, 10, 18), KnownDates, KnownRates), 0.061046, 0.00001, "Not Match");
            for (int i = 1; i < KnownRates.Length - 1; i++)
            {
                //check each of the midpoints
                DateTime midDate = KnownDates[i - 1].AddDays((KnownDates[i] - KnownDates[i - 1]).TotalDays / 2);
                double midRate = (KnownRates[i] + KnownRates[i - 1]) / 2;
                Assert.AreEqual(Utilities.InterpolateDates(midDate, KnownDates, KnownRates), midRate, 0.00001, "Not Equal");
                //check all of the three-quarter points
                DateTime threeQDate = KnownDates[i - 1].AddDays(0.75 * (KnownDates[i] - KnownDates[i - 1]).TotalDays);
                double threeQRate = (0.75 * KnownRates[i] + 0.25 * KnownRates[i - 1]);
                Assert.AreEqual(Utilities.InterpolateDates(threeQDate, KnownDates, KnownRates), threeQRate, 0.00001, "Not Equal");
                //check all of the quarter points
                DateTime qDate = KnownDates[i - 1].AddDays(0.25 * (KnownDates[i] - KnownDates[i - 1]).TotalDays);
                double qRate = (0.25 * KnownRates[i] + 0.75 * KnownRates[i - 1]);
                Assert.AreEqual(Utilities.InterpolateDates(qDate, KnownDates, KnownRates), qRate, 0.00001, "Not Equal");
                //check all of the actual data points
                Assert.AreEqual(Utilities.InterpolateDates(KnownDates[i], KnownDates, KnownRates), KnownRates[i], 0.00001);
            }
            //The final data point will not be reached in above iterative loop, so test it here
            Assert.AreEqual(Utilities.InterpolateDates(KnownDates[23], KnownDates, KnownRates), KnownRates[23], 0.00001);
        }

        [TestMethod]
        public void TestMidpoints()
        {
            Assert.AreEqual(Utilities.InterpolateDates(new DateTime(2008, 01, 16), KnownDates, KnownRates), 0.049793, 0.00001, "Not Match");
            Assert.AreEqual(Utilities.InterpolateDates(new DateTime(2008, 01, 4), KnownDates, KnownRates), 0.049134, 0.00001, "Not Match");
            Assert.AreEqual(Utilities.InterpolateDates(new DateTime(2009, 10, 18), KnownDates, KnownRates), 0.061046, 0.00001, "Not Match");
            for (int i = 1; i < KnownRates.Length - 1; i++)
            {
                //check each of the midpoints
                DateTime midDate = KnownDates[i - 1].AddDays((KnownDates[i] - KnownDates[i - 1]).TotalDays / 2);
                double midRate = (KnownRates[i] + KnownRates[i - 1]) / 2;
                Assert.AreEqual(Utilities.InterpolateDates(midDate, KnownDates, KnownRates), midRate, 0.00001, "Not Equal");
                //check all of the three-quarter points
                DateTime threeQDate = KnownDates[i - 1].AddDays(0.75 * (KnownDates[i] - KnownDates[i - 1]).TotalDays);
                double threeQRate = (0.75 * KnownRates[i] + 0.25 * KnownRates[i - 1]);
                Assert.AreEqual(Utilities.InterpolateDates(threeQDate, KnownDates, KnownRates), threeQRate, 0.00001, "Not Equal");
                //check all of the quarter points
                DateTime qDate = KnownDates[i - 1].AddDays(0.25 * (KnownDates[i] - KnownDates[i - 1]).TotalDays);
                double qRate = (0.25 * KnownRates[i] + 0.75 * KnownRates[i - 1]);
                Assert.AreEqual(Utilities.InterpolateDates(qDate, KnownDates, KnownRates), qRate, 0.00001, "Not Equal");
                //check all of the actual data points
                Assert.AreEqual(Utilities.InterpolateDates(KnownDates[i], KnownDates, KnownRates), KnownRates[i], 0.00001);
            }
            //The final data point will not be reached in above iterative loop, so test it here
            Assert.AreEqual(Utilities.InterpolateDates(KnownDates[23], KnownDates, KnownRates), KnownRates[23], 0.00001);
        }

        [TestMethod]
        public void TestOutOfRange()
        {
            //OutOfRange values return 0.0 at this stage
            bool thrown1 = false;
            try
            {   //This is the date too small exception
                Utilities.InterpolateDates(new DateTime(2007, 12, 31), KnownDates, KnownRates);
            }
            catch (DateToSmallException e)
            {
                thrown1 = true; //checking that the exception was correctly thrown.
            }
            finally
            {
                Assert.IsTrue(thrown1);
            }
            thrown1 = false; //resetting
            try
            {   //This is the date too large exception
                Utilities.InterpolateDates(new DateTime(2009, 12, 2), KnownDates, KnownRates);
            }
            catch (DateToLargeException e)
            {
                thrown1 = true; //checking that the exception was correctly thrown.
            }
            finally
            {
                Assert.IsTrue(thrown1);
            }
            //Assert.AreEqual(Utilities.Interpolate(new DateTime(2009, 12, 2), knownDates, knownRates), 0.0);
        }

        [TestMethod]
        public void TestCompoundConversion()
        {
            //Semi Annual
            double testRate = 0.06;
            double testRateConv = 0.059118;
            Assert.AreEqual(Utilities.ConvToContinuousRate(testRate, CompoundingFrequency.SemiAnnual), testRateConv, 0.00001);
            testRateConv = 0.058269;
            Assert.AreEqual(Utilities.ConvToContinuousRate(testRate, CompoundingFrequency.Annual), testRateConv, 0.00001);
            testRateConv = 0.059554;
            Assert.AreEqual(Utilities.ConvToContinuousRate(testRate, CompoundingFrequency.Quarterly), testRateConv, 0.00001);
            testRateConv = 0.059850;
            Assert.AreEqual(Utilities.ConvToContinuousRate(testRate, "Monthly"), testRateConv, 0.00001);
        }

        [TestMethod]
        public void TestUnsortedExceptions()
        {
            bool bool1 = false;
            try
            {
                Utilities.InterpolateDates(new DateTime(2008, 01, 16), UnsortedDates, KnownRates);
            }
            catch (UnsortedDatesException e)
            {
                bool1 = true;
            }
            finally
            {
                Assert.IsTrue(bool1);
            }
        }

        [TestMethod]
        public void TestUnequalLengthExceptions()
        {
            bool bool1 = false;
            try
            {
                Utilities.InterpolateDates(new DateTime(2008, 01, 16), UnequalLengthDates, KnownRates);
            }
            catch (UnequalArrayLengthsException e)
            {
                bool1 = true;
            }
            finally
            {
                Assert.IsTrue(bool1);
            }
        }
    }
}
