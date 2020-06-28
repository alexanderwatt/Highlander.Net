using System;
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

using Highlander.Reporting.Analytics.V5r3.Rates;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Analytics.Tests.V5r3.Rates
{
    /// <summary>
    /// Unit tests for ForwardRates Matrices. These will hold forward rates associated with Volatility Surfaces
    /// SABR will use these to return an ATM price to use in Calibration operations.
    /// </summary>
    [TestClass]
    public class ForwardRatesTest
    {
        private string[] _assetTenors;
        private string[] _assetExpiries;
        private decimal[][] _assetData;

        private const string _id = "TestAssetGrid.1";

        #region Setup Test Data

        [TestInitialize]
        public void Initialise()
        {

            _assetExpiries = new string[] { "1m", "2m", "3m", "6m", "1yr", "2yr", "3yr", "4yr", "5yr", "7yr", "10yr" };
            _assetTenors = new string[] { "1y", "2y", "3y", "4y", "5y", "7y", "10y" };
            _assetData = new decimal[][] {
                                             new decimal[] { 6.8256m/100.0m, 6.8257m/100.0m, 6.8242m/100.0m, 6.8589m/100.0m, 6.8316m/100.0m, 6.7395m/100.0m, 6.6439m / 100.0m },
                                             new decimal[] { 6.8200m/100.0m, 6.8227m/100.0m, 6.8202m/100.0m, 6.8544m/100.0m, 6.8243m/100.0m, 6.7334m/100.0m, 6.6394m / 100.0m  },
                                             new decimal[] { 6.8158m/100.0m, 6.8207m/100.0m, 6.8166m/100.0m, 6.8505m/100.0m, 6.8182m/100.0m, 6.7281m/100.0m, 6.6355m / 100.0m },
                                             new decimal[] { 6.8245m/100.0m, 6.8256m/100.0m, 6.8126m/100.0m, 6.8440m/100.0m, 6.8028m/100.0m, 6.7148m/100.0m, 6.6259m / 100.0m },
                                             new decimal[] { 6.8248m/100.0m, 6.8263m/100.0m, 6.7951m/100.0m, 6.8227m/100.0m, 6.7581m/100.0m, 6.6822m/100.0m, 6.6026m / 100.0m },
                                             new decimal[] { 6.8280m/100.0m, 6.7787m/100.0m, 6.7430m/100.0m, 6.7184m/100.0m, 6.6738m/100.0m, 6.6039m/100.0m, 6.5484m / 100.0m  },
                                             new decimal[] { 6.7263m/100.0m, 6.6961m/100.0m, 6.5945m/100.0m, 6.6113m/100.0m, 6.5807m/100.0m, 6.5072m/100.0m, 6.4826m / 100.0m  },
                                             new decimal[] { 6.6636m/100.0m, 6.5141m/100.0m, 6.4933m/100.0m, 6.5210m/100.0m, 6.4875m/100.0m, 6.4551m/100.0m, 6.4189m / 100.0m  },
                                             new decimal[] { 6.3395m/100.0m, 6.3998m/100.0m, 6.3954m/100.0m, 6.4201m/100.0m, 6.3850m/100.0m, 6.4009m/100.0m, 6.3521m / 100.0m  },
                                             new decimal[] { 6.3856m/100.0m, 6.3351m/100.0m, 6.2844m/100.0m, 6.3705m/100.0m, 6.3761m/100.0m, 6.3450m/100.0m, 6.2872m / 100.0m  },
                                             new decimal[] { 6.4447m/100.0m, 6.3996m/100.0m, 6.3531m/100.0m, 6.3555m/100.0m, 6.3072m/100.0m, 6.2515m/100.0m, 6.1309m / 100.0m  }
                                         };
        }

        #endregion

        /// <summary>
        /// Test the GetForwardRate method.
        /// This method takes an expiry/tenor pair and returns the rate
        /// The expiry/tenors are represented as strings
        /// </summary>
        [TestMethod]
        public void GetForwardRateTest()
        {
            var target = new ForwardRatesMatrix(_assetExpiries, _assetTenors, _assetData, _id);
            // Check for valid expiry/tenor pair
            string expiry = "1yr";
            string tenor = "4y";
            const decimal expected = 6.8227m / 100.0m;
            decimal actual;
            try
            {
                actual = target.GetAssetPrice(expiry, tenor);
                Assert.AreEqual(expected, actual);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid Expiry/Tenor pair supplied.", ex.Message);
            }

            // Check for invalid expiry/tenor pair
            expiry = "6y";
            tenor = "4y";
            try
            {
                actual = target.GetAssetPrice(expiry, tenor);
                Assert.AreEqual(expected, actual);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid Expiry/Tenor pair supplied.", ex.Message);
            }
        }

        /// <summary>
        /// Test the GetForwardRate method.
        /// This method takes an expiry/tenor pair and returns the rate
        /// The expiry/tenors are represented as FpML intervals
        /// </summary>
        [TestMethod]
        public void GetForwardRateTest1()
        {
            var target = new ForwardRatesMatrix(_assetExpiries, _assetTenors, _assetData, _id);
            // Check for valid expiry/tenor pair
            Period expiry = PeriodHelper.Parse("1y");
            Period tenor = PeriodHelper.Parse("4y");
            const decimal expected = 6.8227m / 100.0m;
            decimal actual;
            try
            {
                actual = target.GetAssetPrice(expiry, tenor);
                Assert.AreEqual(expected, actual);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid Expiry/Tenor pair supplied.", ex.Message);
            }

            // Check for invalid expiry/tenor pair
            expiry = PeriodHelper.Parse("6y");
            tenor = PeriodHelper.Parse("4y");

            try
            {
                actual = target.GetAssetPrice(expiry, tenor);
                Assert.AreEqual(expected, actual);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid Expiry/Tenor pair supplied.", ex.Message);
            }
        }

        /// <summary>
        /// Test the GetForwardRate method.
        /// Compare both versions return the same result
        /// </summary>
        [TestMethod]
        public void GetForwardRateTest2()
        {
            var target = new ForwardRatesMatrix(_assetExpiries, _assetTenors, _assetData, _id);
            // Check for valid expiry/tenor pair
            Period expiry = PeriodHelper.Parse("1y");
            Period tenor = PeriodHelper.Parse("4y");
            decimal expected = 6.8227m / 100.0m;
            decimal actual = 0;
            try
            {
                actual = target.GetAssetPrice(expiry, tenor);
                Assert.AreEqual(expected, actual);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid Expiry/Tenor pair supplied.", ex.Message);
            }
            expected = actual;
            // Check for valid expiry/tenor pair
            const string expiry1 = "1y";
            const string tenor1 = "4y";
            try
            {
                actual = target.GetAssetPrice(expiry1, tenor1);
                Assert.AreEqual(expected, actual);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid Expiry/Tenor pair supplied.", ex.Message);
            }
        }
    }
}