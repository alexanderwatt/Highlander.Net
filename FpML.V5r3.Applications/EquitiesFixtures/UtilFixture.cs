using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using EquitiesAddin;

namespace EquitiesFixtures
{
    [TestFixture]
    public class UtilFixture
    {
        DateTime[] knownDates = new DateTime[24];
        double[] knownRates = new double[24];
        DateTime[] unsortedDates = new DateTime[24];
        DateTime[] unequalLengthDates = new DateTime[25];


        public void Setup()
        {                  
            knownDates[0]=new DateTime(2008,1,1);
            knownRates[0] = 0.048969;
            for (int i = 1; i < 24; i++)
            {
                int j = i + 1;
                knownDates[i] = knownDates[i - 1].AddMonths(1);
                knownRates[i] = 0.0472 + 0.0018 * j - 3.00E-5 * j * j - 1.00E-6 * j * j * j;
            }
            knownDates.CopyTo(unsortedDates, 0);
            
            unsortedDates[12]=unsortedDates[12].AddMonths(3); //this will cause the dates to be unsorted

            knownDates.CopyTo(unequalLengthDates, 1);
            unequalLengthDates[0] = new DateTime(2007, 12, 1);
        }


        [Test]
        public void FlatRates()
        {
            Setup();
            Assert.AreEqual(Utilities.InterpolateDates(new DateTime(2008, 01, 16), knownDates, knownRates), 0.049793, 0.00001, "Not Match");
            Assert.AreEqual(Utilities.InterpolateDates(new DateTime(2008, 01, 4), knownDates, knownRates), 0.049134, 0.00001, "Not Match");
            Assert.AreEqual(Utilities.InterpolateDates(new DateTime(2009, 10, 18), knownDates, knownRates), 0.061046, 0.00001, "Not Match");

            for (int i = 1; i < knownRates.Length - 1; i++)
            {
                //check each of the midpoints
                DateTime MidDate = knownDates[i - 1].AddDays((knownDates[i] - knownDates[i - 1]).TotalDays / 2);
                double MidRate = (knownRates[i] + knownRates[i - 1]) / 2;
                Assert.AreEqual(Utilities.InterpolateDates(MidDate, knownDates, knownRates), MidRate, 0.00001, "Not Equal");

                //check all of the three-quarter points
                DateTime ThreeQDate = knownDates[i - 1].AddDays(0.75 * (knownDates[i] - knownDates[i - 1]).TotalDays);
                double ThreeQRate = (0.75 * knownRates[i] + 0.25 * knownRates[i - 1]);
                Assert.AreEqual(Utilities.InterpolateDates(ThreeQDate, knownDates, knownRates), ThreeQRate, 0.00001, "Not Equal");

                //check all of the quarter points
                DateTime QDate = knownDates[i - 1].AddDays(0.25 * (knownDates[i] - knownDates[i - 1]).TotalDays);
                double QRate = (0.25 * knownRates[i] + 0.75 * knownRates[i - 1]);
                Assert.AreEqual(Utilities.InterpolateDates(QDate, knownDates, knownRates), QRate, 0.00001, "Not Equal");

                //check all of the actual data points
                Assert.AreEqual(Utilities.InterpolateDates(knownDates[i], knownDates, knownRates), knownRates[i], 0.00001);

            }
            //The final data point will not be reached in above iterative loop, so test it here
            Assert.AreEqual(Utilities.InterpolateDates(knownDates[23], knownDates, knownRates), knownRates[23], 0.00001);

        }

        [Test]
        public void TestMidpoints()
        {

            Assert.AreEqual(Utilities.InterpolateDates(new DateTime(2008, 01, 16), knownDates, knownRates), 0.049793, 0.00001, "Not Match");
            Assert.AreEqual(Utilities.InterpolateDates(new DateTime(2008, 01, 4), knownDates, knownRates), 0.049134, 0.00001, "Not Match");
            Assert.AreEqual(Utilities.InterpolateDates(new DateTime(2009, 10, 18), knownDates, knownRates), 0.061046, 0.00001, "Not Match");

            for (int i = 1; i < knownRates.Length - 1; i++)
            {
                //check each of the midpoints
                DateTime MidDate = knownDates[i - 1].AddDays((knownDates[i] - knownDates[i - 1]).TotalDays / 2);
                double MidRate = (knownRates[i] + knownRates[i - 1]) / 2;
                Assert.AreEqual(Utilities.InterpolateDates(MidDate, knownDates, knownRates), MidRate, 0.00001, "Not Equal");

                //check all of the three-quarter points
                DateTime ThreeQDate = knownDates[i - 1].AddDays(0.75 * (knownDates[i] - knownDates[i - 1]).TotalDays);
                double ThreeQRate = (0.75 * knownRates[i] + 0.25 * knownRates[i - 1]);
                Assert.AreEqual(Utilities.InterpolateDates(ThreeQDate, knownDates, knownRates), ThreeQRate, 0.00001, "Not Equal");

                //check all of the quarter points
                DateTime QDate = knownDates[i - 1].AddDays(0.25 * (knownDates[i] - knownDates[i - 1]).TotalDays);
                double QRate = (0.25 * knownRates[i] + 0.75 * knownRates[i - 1]);
                Assert.AreEqual(Utilities.InterpolateDates(QDate, knownDates, knownRates), QRate, 0.00001, "Not Equal");

                //check all of the actual data points
                Assert.AreEqual(Utilities.InterpolateDates(knownDates[i], knownDates, knownRates), knownRates[i], 0.00001);

            }
            //The final data point will not be reached in above iterative loop, so test it here
            Assert.AreEqual(Utilities.InterpolateDates(knownDates[23], knownDates, knownRates), knownRates[23], 0.00001);
        }

        [Test]
        public void TestOutOfRange()
        {
            //OutOfRange values return 0.0 at this stage
            bool thrown1 = false;
            double rate1;
            try
            {   //This is the date too small exception
                rate1 = Utilities.InterpolateDates(new DateTime(2007, 12, 31), knownDates, knownRates);
            }
            catch (DateToSmallException e)
            {
                thrown1 = true; //checking that the exception was correctly t   hrown.
            }
            finally
            {
                Assert.IsTrue(thrown1);
            }
            thrown1 = false; //resetting
            try
            {   //This is the date too large exception
                rate1 = Utilities.InterpolateDates(new DateTime(2009, 12, 2), knownDates, knownRates);
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
        [Test]
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
        [Test]
        public void TestUnsortedExceptions()
        {
            double rate1;
            bool bool1 = false;
            try
            {
                rate1 = Utilities.InterpolateDates(new DateTime(2008, 01, 16), unsortedDates, knownRates);
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

        [Test]
        public void TestUnequalLengthExceptions()
        {
            double rate1;
            bool bool1 = false;
            try
            {
                rate1 = Utilities.InterpolateDates(new DateTime(2008, 01, 16), unequalLengthDates, knownRates);
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
