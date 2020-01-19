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

using System.Diagnostics;
using System.Globalization;
using Highlander.Reporting.Models.V5r3.Generic.Cashflows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Models.Tests.V5r3.Cashflows
{
    [TestClass]
    public class TestFloatingCashFlowAnalytic
    {
        [TestMethod]
        public void FloatingCashflowAnalytics()
        {
            IFloatingCashflowParameters analyticModelParameters = new FloatingCashflowParameters
                                                                {
                                                                    Multiplier = 1.0m,
                                                                    NotionalAmount = 10000000m,
                                                                    PaymentDiscountFactor = 0.99m,
                                                                    CurveYearFraction = 3.0m,
                                                                    PeriodAsTimesPerYear = 0.25m,
                                                                    StartIndex = 0.8m,
                                                                    FloatingIndex = 1.0m
                                                                };
            var model = new FloatingCashflowAnalytic {AnalyticParameters = analyticModelParameters};
            var value = analyticModelParameters.NotionalAmount * (analyticModelParameters.FloatingIndex - analyticModelParameters.StartIndex);
            var result = model.ImpliedQuote;
            var delta1 = model.Delta1;
            var delta0 = model.Delta0;
            var expectedValue = model.ExpectedValue;
            Debug.Print(result.ToString(CultureInfo.InvariantCulture));
            Debug.Print(value.ToString());
            Debug.Print(delta1.ToString(CultureInfo.InvariantCulture));
            Debug.Print(delta0.ToString(CultureInfo.InvariantCulture));
            Debug.Print(expectedValue.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(delta1, -1980.0m);
            Assert.AreEqual(delta0, 0.0m);
            Assert.AreEqual(expectedValue, -8000000.0m);
        }
    }
}