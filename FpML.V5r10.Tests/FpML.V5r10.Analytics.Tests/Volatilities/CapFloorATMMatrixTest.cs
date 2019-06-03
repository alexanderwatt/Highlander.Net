using System;
using Orion.Analytics.Stochastics.Volatilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.Analytics.Tests.Volatilities
{
    [TestClass]
    public class CapFloorATMMatrixTest
    {
        DateTime _valuationDate;
        string _id;
        string[] _header;
        object[][] _data;
        object[][] _settings;

        decimal[][] _testVols;
        string[] _expiries;
        string[] _types;

        [TestInitialize]
        public void Initialize()
        {
            _valuationDate = DateTime.Parse("2008-05-08");
            _id = "TestMatrix_1";

            _header = new string[] { "Expiry", "ATM", "Type" };

            _data = new object[][]
                        {
                            new object[] { "36D", 13.54m, "ETO" }, 
                            new object[] { "127D", 12.34m, "ETO" }, 
                            new object[] { "218D", 13.81m, "ETO" }, 
                            new object[] { "309D", 12.16m, "ETO" }, 
                            new object[] { "2Y", 13.083m, "Cap/Floor" }, 
                            new object[] { "3Y", 13.37m, "Cap/Floor" }, 
                            new object[] { "4Y", 13.49m, "Cap/Floor" }, 
                            new object[] { "5Y", 13.395m, "Cap/Floor" }, 
                            new object[] { "7Y", 13.158m, "Cap/Floor" }, 
                            new object[] { "10Y", 12.013m, "Cap/Floor" }, 
                            new object[] { "15Y", 10.865m, "Cap/Floor" }
                        };

            _settings = new object[][]
                            {
                                new object[] { "Settings", "Value" },
                                new object[] { "Calculation Date", _valuationDate }, 
                                new object[] { "Cap Frequency", "3M" }, 
                                new object[] { "Cap Start Lag", 1 }, 
                                new object[] { "Currency", "AUD" }, 
                                new object[] { "Handle", "CapFloorSettings1" }, 
                                new object[] { "parVolatilityInterpolation", "CubicHermiteSpline" }, 
                                new object[] { "rollConvention", "MODFOLLOWING" } 
                            };

            _testVols = new decimal[][]
                            {
                                new decimal[] { 13.54m },
                                new decimal[] { 12.34m },
                                new decimal[] { 13.81m },
                                new decimal[] { 12.16m },
                                new decimal[] { 13.083m },
                                new decimal[] { 13.37m },
                                new decimal[] { 13.49m },
                                new decimal[] { 13.395m },
                                new decimal[] { 13.158m },
                                new decimal[] { 12.013m },
                                new decimal[] { 10.865m }
                            };

            _expiries = new string[] { "36D", "127D", "218D", "309D", "2Y", "3Y", "4Y", "5Y", "7Y", "10Y", "15Y" };

            _types = new string[] { "ETO", "ETO", "ETO", "ETO", "Cap/Floor", "Cap/Floor", "Cap/Floor", "Cap/Floor", "Cap/Floor", "Cap/Floor", "Cap/Floor" };

        }

        /// <summary>
        /// Test: GetVolatilities()
        /// Get the Volatilities from this matrix
        /// </summary>
        [TestMethod]
        public void GetVolatilitiesTest()
        {
            CapFloorATMMatrix target = new CapFloorATMMatrix(_header, _data, _settings, _valuationDate, _id);

            decimal[][] actual = target.GetVolatilities();

            for (int row = 0; row < _testVols.Length; row++)
            {
                decimal expected = _testVols[row][0];
                Assert.AreEqual(expected, actual[row][0], string.Format("Failure at row {0}.", row));
            }
        }

        /// <summary>
        /// Test: GetHeaders()
        /// Get the Headers from this matrix
        /// </summary>
        [TestMethod]
        public void GetHeadersTest()
        {
            CapFloorATMMatrix target = new CapFloorATMMatrix(_header, _data, _settings, _valuationDate, _id);

            string[] expected = _header;
            string[] actual = target.GetHeaders();
            for (int row = 0; row < expected.Length; row++)
            {
                Assert.AreEqual(expected[row], actual[row], string.Format("Failure at row: {0}.", row));
            }
        }

        /// <summary>
        /// Test: GetExpiries()
        /// Get the Expiries from this matrix
        /// </summary>
        [TestMethod]
        public void GetExpiriesTest()
        {
            CapFloorATMMatrix target = new CapFloorATMMatrix(_header, _data, _settings, _valuationDate, _id);

            string[] expected = _expiries;
            string[] actual = target.GetExpiries();

            for (int row = 0; row < expected.Length; row++)
            {
                Assert.AreEqual(expected[row], actual[row], string.Format("Failure at row: {0}.", row));
            }
        }

        /// <summary>
        /// Test: GetVolatilityTypes()
        /// Get the Volatility Types from this matrix
        /// </summary>
        [TestMethod]
        public void GetVolatilityTypesTest()
        {
            CapFloorATMMatrix target = new CapFloorATMMatrix(_header, _data, _settings, _valuationDate, _id);

            string[] expected = _types;
            string[] actual = target.GetVolatilityTypes();

            for (int row = 0; row < expected.Length; row++)
            {
                Assert.AreEqual(expected[row], actual[row], string.Format("Failure at row: {0}.", row));
            }
        }

        /// <summary>
        /// Test: GetVolatilityTypes()
        /// Get the Volatility Types from this matrix
        /// </summary>
        [TestMethod]
        public void GetVolatilitySettingsTest()
        {
            CapFloorATMMatrix target = new CapFloorATMMatrix(_header, _data, _settings, _valuationDate, _id);

            object[][] expected = _settings;
            object[][] actual = target.GetVolatilitySettings();

            int i = expected[0][0].ToString().ToLower() == "settings" ? 1 : 0;

            for (int row = 0 + i; row < expected.Length; row++)
            {
                for (int column = 0; column < expected[row].Length; column++)
                {
                    Assert.AreEqual(expected[row][column].ToString(), actual[row - i][column].ToString(), string.Format("Failure at row: {0}.", row));
                }
            }
        }
    }
}