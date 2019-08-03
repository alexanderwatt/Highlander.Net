using System;
using System.Diagnostics;
using FpML.V5r10.Reporting.Models.Rates.Swaption;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FpML.V5r10.Models.Tests.Models.Rate
{
    [TestClass]
    public class TestSwaptionAnalytics
    {
        private readonly Decimal[] paramRange = {0.0m, 0.25m, 0.5m, 0.75m,1.0m, 2.0m} ;

        [TestMethod]
        public void SwaptionAnalyics()
        {
            foreach (var val in paramRange)
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

                //var result = model.NPV;
                //var delta0 = model.Delta0;
                Debug.Print("CurveYearFraction : {0} ",
                            analyticModelParameters.TimeToExpiry);
            }
        }
    }
}