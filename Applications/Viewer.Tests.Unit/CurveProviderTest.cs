using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using Orion.WebViewer.Curve.Business;
using nab.QDS.FpML.V47;

namespace Viewer.Tests.Unit
{
    /// <summary>
    ///This is a test class for CurveProviderTest and is intended
    ///to contain all CurveProviderTest Unit Tests
    ///</summary>
    [TestClass]
    public class CurveProviderTest
    {
        readonly CurveProvider _curveProvider;
        public const string CurveEmptyFpml = "CurveEmptyFpml";
        public const string CurveInputDiscountZeroIds = "CurveInputDiscountZeroIds";
        public const string CurveInputDiscountZeroNoIds = "CurveInputDiscountZeroNoIds";
        public const string CurveInputDiscount = "CurveInputDiscount";
        public const string CurveInput = "CurveInput";
        public const string CurveDiscount = "CurveDiscount";
        public const string CurveDiscountZero = "CurveDiscountZero";
        public const string CurveFx = "CurveFX";
        public const string Surface = "Surface";
        private const int CurveCount = 10;
        private const int CurveAndSurfaceCount = 11;

        private static Market LoadMarket(string fileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "Viewer.Tests.Unit.Test_Data." + fileName;
            string expectedCurveText = ResourceHelper.GetResource(assembly, resourceName);
            var market = XmlSerializerHelper.DeserializeFromString<Market>(expectedCurveText);
            return market;
        }

        public CurveProviderTest()
        {
            _curveProvider = CreateCurveProvider();
        }

        public static CurveProvider CreateCurveProvider()
        {
            ICoreClient store = new PrivateCore(new TraceLogger(false));

            AddMarket(store, "EmptyMarket.xml", CurveEmptyFpml);
            AddMarket(store, "InputDiscountZeroNoIds.xml", CurveInputDiscountZeroNoIds);
            AddMarket(store, "InputDiscountZeroIds.xml", CurveInputDiscountZeroIds);
            AddMarket(store, "InputDiscount.xml", CurveInputDiscount);
            AddMarket(store, "Input.xml", CurveInput);
            AddMarket(store, "DiscountIds.xml", CurveDiscount);
            AddMarket(store, "DiscountZeroIds.xml", CurveDiscountZero);
            AddMarket(store, "FX.xml", CurveFx);

            #region Add Surface
            Market market = LoadMarket("Surface.xml");

            var properties
                = new Dictionary<string, object>
                      {
                          {CurveProp.PricingStructureType, "VolatilityCube"},
                          {CurveProp.MarketAndDate, "UnitTest"},
                          {CurveProp.BaseDate, DateTime.Today},
                          {CurveProp.CurveName, Surface}
                      };
            store.SaveObject(market, Surface, new NamedValueSet(properties), TimeSpan.MaxValue);

            #endregion

            #region Add invalid curve 3
            string curveName = "Curve3";
            var curve3 = new Market();
            properties
                = new Dictionary<string, object>
                      {
                          {CurveProp.PricingStructureType, "RateCurve"},
                          {CurveProp.MarketAndDate, ""},
                          {CurveProp.BaseDate, DateTime.Today},
                          {CurveProp.CurveName, "curve3"}
                      };
            store.SaveObject(curve3, curveName, new NamedValueSet(properties), TimeSpan.MaxValue);
            #endregion

            #region Add invalid curve 4
            curveName = "Curve4";
            var curve4 = new Market();
            properties
                = new Dictionary<string, object>
                      {
                          {CurveProp.PricingStructureType, "RateCurve"},
                          {CurveProp.MarketAndDate, "UnitTest"},
                          {CurveProp.BaseDate, DateTime.Today},
                          {CurveProp.CurveName, ""}
                      };
            store.SaveObject(curve4, curveName, new NamedValueSet(properties), TimeSpan.MaxValue);
            #endregion

            #region Add invalid curve 5
            curveName = "Curve5";
            var curve5 = new Market();
            properties
                = new Dictionary<string, object>
                      {
                          {CurveProp.PricingStructureType, ""},
                          {CurveProp.MarketAndDate, "UnitTest"},
                          {CurveProp.BaseDate, DateTime.Today},
                          {CurveProp.CurveName, "Curve5"}
                      };
            store.SaveObject(curve5, curveName, new NamedValueSet(properties), TimeSpan.MaxValue);
            #endregion

            #region Add invalid curve 6
            curveName = "Curve6";
            var curve6 = new Market();
            properties
                = new Dictionary<string, object>
                      {
                          {CurveProp.PricingStructureType, "RateCurve"},
                          {CurveProp.BaseDate, DateTime.Today},
                          {CurveProp.CurveName, "curve6"}
                      };
            store.SaveObject(curve6, curveName, new NamedValueSet(properties), TimeSpan.MaxValue);
            #endregion

            #region Add invalid curve 7
            curveName = "Curve7";
            var curve7 = new Market();
            properties
                = new Dictionary<string, object>
                      {
                          {CurveProp.PricingStructureType, "RateCurve"},
                          {CurveProp.MarketAndDate, "UnitTest"},
                          {CurveProp.BaseDate, DateTime.Today},
                      };
            store.SaveObject(curve7, curveName, new NamedValueSet(properties), TimeSpan.MaxValue);
            #endregion

            #region Add invalid curve 8
            curveName = "Curve8";
            var curve8 = new Market();
            properties
                = new Dictionary<string, object>
                      {
                          {CurveProp.MarketAndDate, "UnitTest"},
                          {CurveProp.BaseDate, DateTime.Today},
                          {CurveProp.CurveName, "Curve8"}
                      };
            store.SaveObject(curve8, curveName, new NamedValueSet(properties), TimeSpan.MaxValue);
            #endregion

            var curveProvider = new CurveProvider(store);
            return curveProvider;
        }

        private static void AddMarket(ICoreClient store, string xmlFile, string curveName)
        {
            Market market = LoadMarket(xmlFile);

            var properties
                = new Dictionary<string, object>
                      {
                          {CurveProp.PricingStructureType, "RateCurve"},
                          {CurveProp.MarketAndDate, "UnitTest"},
                          {CurveProp.BaseDate, DateTime.Today},
                          {CurveProp.CurveName, curveName}
                      };
            store.SaveObject(market, curveName, new NamedValueSet(properties), TimeSpan.MaxValue);
        }

        /// <summary>
        ///A test for GetCurvesCount
        ///</summary>
        [TestMethod]
        public void GetCurvesCountTest()
        {
            const bool enabled = true;
            const string marketName = "UnitTest";
            const string pricingStructureType = "";
            const string id = "";
            const int maximumRows = 30;
            const int startRowIndex = 0;

            int actual = _curveProvider.GetCurvesCount(enabled, marketName, pricingStructureType, id, maximumRows, startRowIndex);
            Assert.AreEqual(CurveAndSurfaceCount, actual);
        }

        /// <summary>
        ///A test for GetCurves
        ///</summary>
        [TestMethod]
        public void GetCurvesTest()
        {
            const bool enabled = true;
            const string marketName = "UnitTest";
            const string pricingStructureType = "";
            const string id = "";
            const int maximumRows = 30;
            const int startRowIndex = 0;

            var actual = _curveProvider.GetCurves(enabled, marketName, pricingStructureType, id, maximumRows, startRowIndex);
            Assert.AreEqual(CurveAndSurfaceCount, actual.Count());
        }

        /// <summary>
        /// test don't GetCurves
        ///</summary>
        [TestMethod]
        public void DontGetCurvesTest()
        {
            const bool enabled = false;
            const string marketName = "UnitTest";
            const string pricingStructureType = "RateCurve";
            const string id = "";
            const int maximumRows = 30;
            const int startRowIndex = 0;

            var actual = _curveProvider.GetCurves(enabled, marketName, pricingStructureType, id, maximumRows, startRowIndex);
            Assert.AreEqual(0, actual.Count());
        }

        /// <summary>
        /// test don't GetCurvesCount
        ///</summary>
        [TestMethod]
        public void DontGetCurvesCountTest()
        {
            const bool enabled = false;
            const string marketName = "UnitTest";
            const string pricingStructureType = "RateCurve";
            const string id = "";
            const int maximumRows = 30;
            const int startRowIndex = 0;

            int actual = _curveProvider.GetCurvesCount(enabled, marketName, pricingStructureType, id, maximumRows, startRowIndex);
            Assert.AreEqual(0, actual);
        }

        /// <summary>
        ///A test for GetCurve
        ///</summary>
        [TestMethod]
        public void GetCurveTest()
        {
            const bool enabled = true;
            const string marketName = "UnitTest";
            const string id = "";
            const string pricingStructureType = "RateCurve";
            const int maximumRows = 30;
            const int startRowIndex = 0;

            // Load some curves so that we can get the id
            var curves = _curveProvider.GetCurves(enabled, marketName, pricingStructureType, id, maximumRows, startRowIndex);
            string curveId = curves.First().Id;

            // now load a curve directly
            var curve = _curveProvider.GetCurve(curveId);
            Assert.AreEqual(curveId, curve.Id);

            // also load a curve indirectly
            curves = _curveProvider.GetCurves(true, "", "", curveId, 30, 0);
            Assert.AreEqual(curveId, curves.Single().Id);
        }

        /// <summary>
        /// Don't get a Curve
        ///</summary>
        [TestMethod]
        public void DontGetCurveTest()
        {
            // now load a curve
            var curve = _curveProvider.GetCurve("");
            Assert.IsNull(curve);
        }

        /// <summary>
        ///A test for GetCurves
        ///</summary>
        [TestMethod]
        public void GetSomeCurvesTest()
        {
            const bool enabled = true;
            const string marketName = "UnitTest";
            const string pricingStructureType = "RateCurve";
            const string id = "";
            const int maximumRows = 30;
            const int startRowIndex = 0;

            var actual = _curveProvider.GetCurves(enabled, marketName, pricingStructureType, id, maximumRows, startRowIndex);
            Assert.AreEqual(CurveCount, actual.Count());
        }

        /// <summary>
        ///A test for GetCurve
        ///</summary>
        [TestMethod]
        public void GetCurveSurfaceTest()
        {
            const bool enabled = true;
            const string marketName = "";
            const string pricingStructureType = "VolatilityCube";
            const int maximumRows = 30;
            const int startRowIndex = 0;

            // Load some curves so that we can get the id
            IEnumerable<Curve> curves = _curveProvider.GetCurves(enabled, marketName, pricingStructureType, "", maximumRows, startRowIndex);
            string curveId = curves.Single().Id;

            // now load a curve surface
            Surface surface = _curveProvider.GetCurveSurface(curveId);
            Assert.IsNotNull(surface);
        }

        /// <summary>
        ///A test for GetCurve
        ///</summary>
        [TestMethod]
        public void DontGetCurveSurfaceTest()
        {
            Surface surface = _curveProvider.GetCurveSurface("");
            Assert.IsNull(surface);
        }

        /// <summary>
        ///A test for SupportedPricingStructures
        ///</summary>
        [TestMethod]
        public void SupportedPricingStructuresTest()
        {
            IList<string> structures = CurveProvider.SupportedPricingStructures;
            Assert.AreNotEqual(0, structures.Count);
        }
    }
}