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
using System.Diagnostics;
using Highlander.Reporting.Models.V5r3.Rates.Swaption;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Models.Tests.V5r3.Rate
{
    [TestClass]
    public class TestSwaptionAnalytics
    {
        private readonly decimal[] _paramRange = {0.0m, 0.25m, 0.5m, 0.75m,1.0m, 2.0m} ;

        [TestMethod]
        public void SwaptionAnalytics()
        {
            foreach (var val in _paramRange)
            {
                ISwaptionInstrumentParameters analyticModelParameters = new SwaptionInstrumentParameters
                                                                              {
                                                                                  SwapBreakEvenRate = .05m,
                                                                                  SwapAccrualFactor = 7000.0m,
                                                                                  IsCall = true,
                                                                                  Volatility = .20m,
                                                                                  Strike = .05m,
                                                                                  //PaymentDiscountFactor = 0.99m,
                                                                                  TimeToExpiry = val,
                                                                              };
                var model = new SimpleIRSwaptionInstrumentAnalytic
                                {
                                    AnalyticParameters = analyticModelParameters
                                };
                var result = model.NPV;
                //var delta0 = model.Delta0;
                Debug.Print("CurveYearFraction : {0} NPV : {1}",
                            analyticModelParameters.TimeToExpiry, result);
            }
        }
    }
}