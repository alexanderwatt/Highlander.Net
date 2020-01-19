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
using System.Collections.Generic;
using System.Reflection;
using Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Serialisation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.Volatilities
{
    /// <summary>
    ///This is a test class for CapFloorVolatilityMatrixTest and is intended
    ///to contain all CapFloorVolatilityMatrixTest Unit Tests
    ///</summary>
    [TestClass]
    public class CapFloorVolatilityMatrixTest
    {
        private const string _testFile = @"CapFloorVolsTest.xml";

        private string _id;
        private DateTime _valdate;

        private DateTime[] _factorDates;
        private decimal[] _factors;

        private string[] _expiry;
        private string[] _type;
        private decimal[] _strike;

        private decimal[][] _vols;
        private decimal[][] _ATMVols;

        [TestInitialize]
        public void Initialize()
        {
            _id = "CapFloorTest.matrix";
            _valdate = DateTime.Parse("29-Nov-07");

            List<DateTime> dates = new List<DateTime>();
            dates.Add(DateTime.Parse("30-Nov-07"));
            dates.Add(DateTime.Parse("29-Feb-08"));
            dates.Add(DateTime.Parse("30-May-08"));
            dates.Add(DateTime.Parse("29-Aug-08"));
            dates.Add(DateTime.Parse("28-Nov-08"));
            dates.Add(DateTime.Parse("27-Feb-09"));
            dates.Add(DateTime.Parse("29-May-09"));
            dates.Add(DateTime.Parse("31-Aug-09"));
            dates.Add(DateTime.Parse("30-Nov-09"));
            dates.Add(DateTime.Parse("26-Feb-10"));
            dates.Add(DateTime.Parse("31-May-10"));
            dates.Add(DateTime.Parse("30-Aug-10"));
            dates.Add(DateTime.Parse("30-Nov-10"));
            dates.Add(DateTime.Parse("28-Feb-11"));
            dates.Add(DateTime.Parse("30-May-11"));
            dates.Add(DateTime.Parse("30-Aug-11"));
            dates.Add(DateTime.Parse("30-Nov-11"));
            dates.Add(DateTime.Parse("29-Feb-12"));
            dates.Add(DateTime.Parse("30-May-12"));
            dates.Add(DateTime.Parse("30-Aug-12"));
            dates.Add(DateTime.Parse("30-Nov-12"));
            dates.Add(DateTime.Parse("28-Feb-13"));
            dates.Add(DateTime.Parse("30-May-13"));
            dates.Add(DateTime.Parse("30-Aug-13"));
            dates.Add(DateTime.Parse("29-Nov-13"));
            dates.Add(DateTime.Parse("28-Feb-14"));
            dates.Add(DateTime.Parse("30-May-14"));
            dates.Add(DateTime.Parse("29-Aug-14"));
            dates.Add(DateTime.Parse("28-Nov-14"));
            dates.Add(DateTime.Parse("27-Feb-15"));
            dates.Add(DateTime.Parse("29-May-15"));
            dates.Add(DateTime.Parse("31-Aug-15"));
            dates.Add(DateTime.Parse("30-Nov-15"));
            dates.Add(DateTime.Parse("29-Feb-16"));
            dates.Add(DateTime.Parse("30-May-16"));
            dates.Add(DateTime.Parse("30-Aug-16"));
            dates.Add(DateTime.Parse("30-Nov-16"));
            dates.Add(DateTime.Parse("28-Feb-17"));
            dates.Add(DateTime.Parse("30-May-17"));
            dates.Add(DateTime.Parse("30-Aug-17"));
            dates.Add(DateTime.Parse("30-Nov-17"));

            _factorDates = new DateTime[dates.Count];
            dates.CopyTo(_factorDates);

            _factors = new decimal[]
                           {
                               0.999820M, 0.982090M, 0.964480M, 0.946940M, 0.929710M, 0.912800M, 0.896170M, 0.879300M, 0.863230M,
                               0.848120M, 0.832310M, 0.817290M, 0.802400M, 0.788390M, 0.774540M, 0.760830M, 0.747420M, 0.734810M,
                               0.722500M, 0.710360M, 0.698520M, 0.687360M, 0.676360M, 0.665530M, 0.655080M, 0.644890M, 0.634950M,
                               0.625260M, 0.615810M, 0.606170M, 0.596730M, 0.587200M, 0.578180M, 0.569360M, 0.560730M, 0.552190M,
                               0.543840M, 0.535840M, 0.527930M, 0.520110M, 0.512450M
                           };

            _expiry = new string[] { "0D", "8D", "99D", "190D", "281D", "2Y", "3Y", "4Y", "5Y", "7Y", "10Y" };
            _type = new string[] 
                        {
                            "ETO", "ETO", "ETO", "ETO", "ETO", "Cap/Floor", "Cap/Floor", "Cap/Floor",
                            "Cap/Floor", "Cap/Floor", "Cap/Floor"
                        };

            _strike = new decimal[] { 0.07M, 0.07125M, 0.0725M, 0.07375M, 0.075M, 0.07625M, 0.07750M, 0.07875M, 0.08000M };

            _vols = new decimal[][]
                        {
                            new decimal[] { 0.08820M, 0.08820M, 0.08820M, 0.08820M, 0.08820M, 0.08820M, 0.08820M, 0.08820M, 0.08820M },
                            new decimal[] { 0.08820M, 0.08820M, 0.08820M, 0.08820M, 0.08820M, 0.08820M, 0.08820M, 0.08820M, 0.08820M },
                            new decimal[] { 0.09120M, 0.09120M, 0.09120M, 0.09120M, 0.09120M, 0.09120M, 0.09120M, 0.09120M, 0.09120M },
                            new decimal[] { 0.09430M, 0.09430M, 0.09430M, 0.09430M, 0.09430M, 0.09430M, 0.09430M, 0.09430M, 0.09430M },
                            new decimal[] { 0.09710M, 0.09710M, 0.09710M, 0.09710M, 0.09710M, 0.09710M, 0.09710M, 0.09710M, 0.09710M },
                            new decimal[] { 0.103375M, 0.103375M, 0.103375M, 0.103375M, 0.103375M, 0.103375M, 0.103375M, 0.103375M, 0.103375M },
                            new decimal[] { 0.107525M, 0.107525M, 0.107525M, 0.107525M, 0.107525M, 0.107525M, 0.107525M, 0.107525M, 0.107525M },
                            new decimal[] { 0.108400M, 0.108400M, 0.108400M, 0.108400M, 0.108400M, 0.108400M, 0.108400M, 0.108400M, 0.108400M },
                            new decimal[] { 0.109325M, 0.109325M, 0.109325M, 0.109325M, 0.109325M, 0.109325M, 0.109325M, 0.109325M, 0.109325M },
                            new decimal[] { 0.109450M, 0.109450M, 0.109450M, 0.109450M, 0.109450M, 0.109450M, 0.109450M, 0.109450M, 0.109450M },
                            new decimal[] { 0.109900M, 0.109900M, 0.109900M, 0.109900M, 0.109900M, 0.109900M, 0.109900m, 0.109900M, 0.109900M }
                        };

            _ATMVols = new decimal[][]
                           {
                               new decimal[] { 0 },
                               new decimal[] { 0 },
                               new decimal[] { 0 },
                               new decimal[] { 0 },
                               new decimal[] { 0 },
                               new decimal[] { 18.16M },
                               new decimal[] { 17.94M },
                               new decimal[] { 17.62M },
                               new decimal[] { 17.09M },
                               new decimal[] { 15.77M },
                               new decimal[] { 14.21M }
                           };
        }

        ///// <summary>
        /////A test for CapFloorVolatilityMatrix Constructor
        /////</summary>
        //[TestMethod]
        //public void CapFloorVolatilityMatrixConstructorTest()
        //{
        //    string matrixId = _id;
        //    string[] expiry = _expiry;
        //    string[] volType = _type;
        //    decimal[][] vols = _vols;
        //    decimal[] strikes = _strike;
        //    DateTime valueDate = _valdate;
        //    DateTime[] discountFactorDates = _factorDates;
        //    decimal[] discountFactors = _factors;

        //    try
        //    {
        //        CapFloorVolatilityMatrix target = new CapFloorVolatilityMatrix(matrixId, expiry, volType, vols, strikes, discountFactorDates, discountFactors);
        //        Assert.IsNotNull(target);

        //        // Write the document
        //        XmlSerializer oxs = new XmlSerializer(typeof(CapFloorVolatilityMatrix));
        //        FileStream ofs = new FileStream(_testFile, FileMode.Create, FileAccess.Write);

        //        // Load the trade
        //        oxs.Serialize(ofs, target);

        //        ofs.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail(ex.Message);
        //    }
        //}

        ///// <summary>
        /////A test for CapFloorVolatilityMatrix Constructor
        /////</summary>
        //[TestMethod]
        //public void CapFloorVolatilityMatrixConstructorTest1()
        //{
        //    string matrixId = _id;
        //    string[] expiry = _expiry;
        //    string[] volType = _type;
        //    decimal[][] vols = _ATMVols;
        //    decimal[] strikes = null;
        //    DateTime valueDate = _valdate;
        //    DateTime[] discountFactorDates = _factorDates;
        //    decimal[] discountFactors = _factors;

        //    try
        //    {
        //        CapFloorVolatilityMatrix target = new CapFloorVolatilityMatrix(matrixId, expiry, volType, vols, strikes, discountFactorDates, discountFactors);
        //        Assert.IsNotNull(target);

        //        // Write the document
        //        XmlSerializer oxs = new XmlSerializer(typeof(CapFloorVolatilityMatrix));
        //        FileStream ofs = new FileStream(_testFile, FileMode.Create, FileAccess.Write);

        //        // Load the trade
        //        oxs.Serialize(ofs, target);

        //        ofs.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail(ex.Message);
        //    }
        //}

        /// <summary>
        /// A test to load capFloor Matrix and base Vol Matrix
        /// </summary>
        [TestMethod]
        public void CapFloorLoadTest()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string expectedXml = ResourceHelper.GetResourceWithPartialName(assembly, _testFile);

            // Load as a full CapFloor Matrix
            CapFloorVolatilityMatrix cfMatrix = XmlSerializerHelper.DeserializeFromString<CapFloorVolatilityMatrix>(expectedXml);

            Assert.IsNotNull(cfMatrix);
        }

        /// <summary>
        /// Test the discount factor
        /// Check date/factor pair
        /// </summary>
        [TestMethod]
        public void DiscountFactorsTest()
        {
            string matrixId = _id;
            string[] expiry = _expiry;
            string[] volType = _type;
            decimal[][] vols = _vols;
            decimal[] strikes = _strike;
            DateTime valueDate = _valdate;
            DateTime[] discountFactorDates = _factorDates;
            decimal[] discountFactors = _factors;

            try
            {
                CapFloorVolatilityMatrix target = new CapFloorVolatilityMatrix(matrixId, expiry, volType, vols, strikes, discountFactorDates, discountFactors);
                Assert.IsNotNull(target);

                DateTime expectedDate = DateTime.Parse("30-Aug-17");
                decimal expectedDF = 0.520110M;
                SortedList<DateTime, decimal> actual = target.DiscountFactors();

                Assert.AreEqual(true, actual.ContainsValue(expectedDF));
                Assert.AreEqual(true, actual.ContainsKey(expectedDate));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Test the CapVolatilitiesElement retrieval. This version extracts by strike
        /// </summary>
        [TestMethod]
        public void CapVolatilitiesTest()
        {
            string matrixId = _id;
            string[] expiry = _expiry;
            string[] volType = _type;
            decimal[][] vols = _vols;
            decimal[] strikes = _strike;
            DateTime valueDate = _valdate;
            DateTime[] discountFactorDates = _factorDates;
            decimal[] discountFactors = _factors;

            try
            {
                CapFloorVolatilityMatrix target = new CapFloorVolatilityMatrix(matrixId, expiry, volType, vols, strikes, discountFactorDates, discountFactors);
                Assert.IsNotNull(target);

                decimal strike = 7.5m;
                decimal[] expectedVolatilities = new decimal[] { 8.82m, 8.82m, 9.12m, 9.43m, 9.71m, 10.34m, 10.75m, 10.84m, 10.93m, 10.95m, 10.99m };
                int[] term = new int[] { 0, 8, 99, 190, 281, 2, 3, 4, 5, 7, 10 };

                int i = 0;

                List<CapVolatilityDataElement<int>> actual = target.CapVolatilities(strike);

                foreach (CapVolatilityDataElement<int> element in actual)
                {
                    Assert.AreEqual(expectedVolatilities[i], element.Volatility);
                    Assert.AreEqual(term[i++], element.Expiry);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Test the CapVolatilitiesElement retrieval. This version extracts ATM vols
        /// </summary>
        [TestMethod]
        public void CapVolatilitiesTest1()
        {
            CapFloorVolatilityMatrix target = new CapFloorVolatilityMatrix(_id, _expiry, _type, _ATMVols, null, _factorDates, _factors);
            Assert.IsNotNull(target);

            decimal[] expectedVolatilities = new decimal[] { 0, 0, 0, 0, 0, 18.16M, 17.94M, 17.62M, 17.09M, 15.77M, 14.21M };
            int[] term = new int[] { 0, 8, 99, 190, 281, 2, 3, 4, 5, 7, 10 };

            int i = 0;

            List<CapVolatilityDataElement<int>> actual = target.CapVolatilities();

            foreach (CapVolatilityDataElement<int> element in actual)
            {
                Assert.AreEqual(expectedVolatilities[i], element.Volatility);
                Assert.AreEqual(term[i++], element.Expiry);
            }
        }

        /// <summary>
        /// Test to check strikes are able to be retrieved
        /// </summary>
        [TestMethod]
        public void StrikesTest()
        {
            string matrixId = _id;
            string[] expiry = _expiry;
            string[] volType = _type;
            decimal[][] vols = _vols;
            decimal[] strikes = _strike;
            DateTime valueDate = _valdate;
            DateTime[] discountFactorDates = _factorDates;
            decimal[] discountFactors = _factors;

            try
            {
                CapFloorVolatilityMatrix target = new CapFloorVolatilityMatrix(matrixId, expiry, volType, vols, strikes, discountFactorDates, discountFactors);
                Assert.IsNotNull(target);

                int i = 0;

                decimal[] actual = target.Strikes();

                foreach (decimal element in actual)
                {
                    Assert.AreEqual(strikes[i++], element);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

    }
}