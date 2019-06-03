
using System.Diagnostics;
using Orion.Models.Generic.Cashflows;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Orion.Models.Tests.Models.Cashflows
{
    [TestClass]
    public class TestFloatingCashFlowAnalytic
    {
        [TestMethod]
        public void FloatingCashflowAnalyics()
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
            var model = new FloatingCashflowAnalytic();
            model.AnalyticParameters = analyticModelParameters;
            var value = analyticModelParameters.NotionalAmount * (analyticModelParameters.FloatingIndex - analyticModelParameters.StartIndex);
            var result = model.ImpliedQuote;
            var delta1 = model.Delta1;
            var delta0 = model.Delta0;
            var expectedValue = model.ExpectedValue;
            Debug.Print(result.ToString());
            Debug.Print(value.ToString());
            Debug.Print(delta1.ToString());
            Debug.Print(delta0.ToString());
            Debug.Print(expectedValue.ToString());
            Assert.AreEqual(delta1, -1980.0m);
            Assert.AreEqual(delta0, 0.0m);
            Assert.AreEqual(expectedValue, -8000000.0m);
        }
    }
}