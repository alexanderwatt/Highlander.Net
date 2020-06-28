
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

#region Using Directives

using System.Collections.Generic;
using Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities;
using Highlander.TestHelpers.V5r3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.CapFloorAnalytics
{
    /// <summary>
    /// Unit tests for the class CapVolatilityDataElement. 
    /// </summary>
    [TestClass]
    public class CapVolatilityDataElementTests
    {
        #region Private Fields

        private int _expiry; // cap expiry
        private decimal _volatility; // cap volatility
        private VolatilityDataType _volatilityType; // ETO or Cap/Floor vol

        private int _actual;
        private int _expected;

        #endregion

        #region Test Accessor Methods

        /// <summary>
        /// Tests all accessor methods.
        /// </summary>
        [TestMethod]
        public void TestAccessorMethods()
        {
            // Instantiate the Cap Volatility Data element.
            _expiry = 2;
            _volatility = 0.096724m;
            _volatilityType = VolatilityDataType.CapFloor;

            CapVolatilityDataElement<int> capObj = 
                new CapVolatilityDataElement<int>
                    (_expiry, _volatility, _volatilityType);
            Assert.IsNotNull(capObj);
            _actual = capObj.Expiry;
            _expected = 2;
            Assert.AreEqual(_expected, _actual);
            Assert.IsTrue(capObj.VolatilityType == VolatilityDataType.CapFloor);
        }

        #endregion

        #region Test: CompareTo Method

        /// <summary>
        /// Tests the CompareTo method.
        /// </summary>
        [TestMethod]
        public void TestCompareTo()
        {
            // Instantiate the container.
            List<CapVolatilityDataElement<int>> container =
                new List<CapVolatilityDataElement<int>>();

            // Add the first element to the container: ETO
            _expiry = 182; // 6M ETO expiry
            _volatility = 0.8820m;
            _volatilityType = VolatilityDataType.ETO;
            CapVolatilityDataElement<int> capObj1 = 
                new CapVolatilityDataElement<int>
                    (_expiry, _volatility, _volatilityType);
            Assert.IsNotNull(capObj1);
            container.Add(capObj1);
            int count = 1;
            Assert.AreEqual(count, container.Count);

            // Add the second element to the container: CAP/FLOOR
            _expiry = 1; // 1Y Cap/Floor expiry
            _volatilityType =
                VolatilityDataType.CapFloor;
            CapVolatilityDataElement<int> capObj2 =
                new CapVolatilityDataElement<int>
                    (_expiry,  _volatility, _volatilityType);
            Assert.IsNotNull(capObj2);
            container.Add(capObj2);
            Assert.AreEqual(++count, container.Count);

            // Add the third element to the container: ETO
            _expiry = 273; // 9M Cap/Floor expiry
            _volatilityType = VolatilityDataType.ETO;
            CapVolatilityDataElement<int> capObj3=
                new CapVolatilityDataElement<int>
                    (_expiry, _volatility, _volatilityType);
            Assert.IsNotNull(capObj3);
            container.Add(capObj3);
            Assert.AreEqual(++count, container.Count);

            // Test that the container is sorted correctly.
            CapVolatilityDataElementComparer myComparer =
                new CapVolatilityDataElementComparer();
            container.Sort(myComparer);
            int numElements = container.Count;
            
            for(int i = 1; i < numElements; ++i)
            {
                AssertExtension.LessOrEqual((int)container[i - 1].VolatilityType,
                                   (int)container[i].VolatilityType);

                if(container[i-1].VolatilityType == container[i].VolatilityType)
                {
                    AssertExtension.Less(container[i - 1].Expiry, container[i].Expiry);
                }
            }
        }

        #endregion
    }
}