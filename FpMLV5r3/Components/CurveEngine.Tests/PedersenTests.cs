#region Using

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Constants;
using Orion.UnitTestEnv;
using Orion.CurveEngine.PricingStructures.Curves;
using Orion.Util.NamedValues;

#endregion

namespace Orion.CurveEngine.Tests 
{
    [TestClass]
    public class PedersenTests1
    {
        #region Properties

        private static UnitTestEnvironment UTE { get; set; }
        private static CurveEngine CurveEngine { get; set; }

        #endregion

        #region Test Initialize and Cleanup

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            UTE = new UnitTestEnvironment();
            //Set the calendar engine
            CurveEngine = new CurveEngine(UTE.Logger, UTE.Cache, UTE.NameSpace);
        }

        [ClassCleanup]
        public static void Teardown()
        {
            // Release resources
            //Logger.Dispose()
            UTE.Dispose();
        }

        #endregion

        #region Calibration Tests

        [Ignore]
        //[TestMethod]
        public void TestPedersen()
        {
            var tempInstruments
            = new[]
                  {
                          "AUD-DEPOSIT-1W",
                          "AUD-DEPOSIT-9M",
                          "AUD-DEPOSIT-1Y",
                          "AUD-DEPOSIT-1M",
                          "AUD-DEPOSIT-2M",
                          "AUD-DEPOSIT-3M",
                          "AUD-DEPOSIT-4M",
                          "AUD-DEPOSIT-5M",
                          "AUD-DEPOSIT-6M",
                          "AUD-IRFUTURE-IR-H0",
                          "AUD-IRFUTURE-IR-M0",
                          "AUD-IRFUTURE-IR-U0",
                          "AUD-IRFUTURE-IR-Z0",
                          "AUD-IRFUTURE-IR-H1",
                          "AUD-IRFUTURE-IR-M1",
                          "AUD-IRFUTURE-IR-U1",
                          "AUD-IRFUTURE-IR-Z1",
                          "AUD-IRSWAP-10Y",
                          "AUD-IRSWAP-15Y",
                          "AUD-IRSWAP-20Y",
                          "AUD-IRSWAP-25Y",
                          "AUD-IRSWAP-2Y",
                          "AUD-IRSWAP-30Y",
                          "AUD-IRSWAP-3Y",
                          "AUD-IRSWAP-4Y",
                          "AUD-IRSWAP-5Y",
                          "AUD-IRSWAP-7Y",
                          "AUD-IRSWAP-9Y",
                          "AUD-IRSWAP-8Y",
                          "AUD-IRSWAP-40Y",
                          "AUD-IRSWAP-6Y",
                          "AUD-IRSWAP-12Y"
                  };

            var rates
                = new[]
                      {
                          0.03795m,
                          0.0453m,
                          0.047m,
                          0.0392m,
                          0.0407m,
                          0.0416m,
                          0.0422m,
                          0.0432m,
                          0.044m,
                          0.04259871m,
                          0.045289684m,
                          0.0481755749999999m,
                          0.0507564959999999m,
                          0.052731378m,
                          0.054293909m,
                          0.0554455260000001m,
                          0.0564841610000001m,
                          0.0615m,
                          0.06229m,
                          0.06181m,
                          0.06052m,
                          0.05035m,
                          0.05901m,
                          0.05284m,
                          0.05567m,
                          0.05712m,
                          0.05967m,
                          0.06105m,
                          0.06047m,
                          0.056819m,
                          0.05851m,
                          0.06188m
                      };

            var baseDate = new DateTime(2019, 2, 22);
            var props
                = new Dictionary<string, object>
                      {
                          {CurveProp.PricingStructureType, "RateCurve"},
                          {CurveProp.Market, "RateCurveBuilderTest2"},
                          {CurveProp.IndexTenor, "3M"},
                          {CurveProp.Currency1, "AUD"},
                          {"Index", "LIBOR-BBA"},
                          {CurveProp.Algorithm, "FastLinearZero"},
                          {CurveProp.BaseDate, baseDate},
                      };

            var valueSet = new NamedValueSet(props);
            if (CurveEngine.CreateCurve(valueSet, tempInstruments, rates, new decimal[rates.Length], null, null) is
                RateCurve curve)
            {
                CurveEngine.SaveCurve(curve);
                CurveEngine.PedersenCalibration(curve.GetPricingStructureId().UniqueIdentifier);
            }
        }

        #endregion
    }
}
